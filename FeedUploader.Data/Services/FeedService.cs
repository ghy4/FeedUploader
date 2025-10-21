using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using EFCore.BulkExtensions;
using System.IO;
using MySqlConnector;
using Microsoft.Extensions.Logging;

namespace FeedUploader.Data.Services
{
	public class FeedService : IFeedService
	{
		private readonly MyDbContext _dbContext;
	private readonly IFeedParsingAdapter _feedParser;
	private readonly IExportAdapter _exporter;
	private readonly ILogger<FeedService> _logger;

		public FeedService(MyDbContext dbContext, IFeedParsingAdapter feedParser, IExportAdapter exporter, ILogger<FeedService> logger)
		{
			_dbContext = dbContext;
            _feedParser = feedParser;
            _exporter = exporter;
			_logger = logger;
		}

		//public async Task<ICollection<Product>> UploadFeedAsync(IFormFile file, int userId)
		//{
		//	if (file == null || file.Length == 0) throw new ArgumentException("No file uploaded");

		//	var user = await _dbContext.Users.FindAsync(userId);
		//	if (user == null) throw new ArgumentException("User not found");

		//	using var reader = new StreamReader(file.OpenReadStream());
		//	using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

		//	csv.Context.RegisterClassMap<ProductCsvMap>();
		//	var products = csv.GetRecords<Product>().ToList();

		//	foreach (var product in products)
		//	{
		//		product.UserId = userId;
		//		product.ExtractedAttributes = await ExtractAttributesAsync(product.Description);

		//		if (product.ExtractedAttributes != null)
		//		{
		//			foreach (var attr in product.ExtractedAttributes)
		//			{
		//				var attribute = await _dbContext.Attributes.FirstOrDefaultAsync(a => a.Code == attr.Key);
		//				if (attribute == null)
		//				{
		//					attribute = new Models.Attribute
		//					{
		//						Code = attr.Key,
		//						Name = GetAttributeName(attr.Key),
		//						IsRequired = IsRequiredAttribute(attr.Key),
		//						IsRestricted = IsRestrictedAttribute(attr.Key),
		//						Unit = GetAttributeUnit(attr.Key),
		//						AllowedValues = GetAllowedValues(attr.Key)
		//					};
		//					await _dbContext.Attributes.AddAsync(attribute);
		//					await _dbContext.SaveChangesAsync();
		//				}

		//				if (attribute.IsRestricted && attribute.AllowedValues != null && !attribute.AllowedValues.Contains(attr.Value))
		//					continue;

		//				product.Attributes.Add(new ProductAttribute
		//				{
		//					AttributeId = attribute.Id,
		//					Value = attr.Value,
		//					IsExtractedByAI = true
		//				});
		//			}
		//		}

		//		await _dbContext.Products.AddAsync(product);
		//	}

		//	return await _dbContext.SaveChangesAsync() >= 1 ? products : throw new Exception("Failed to save products");
		//}


		public async Task<ICollection<Product>> UploadFeedAsync(IFormFile file, int userId)
		{
			if (file == null || file.Length == 0) throw new ArgumentException("No file uploaded");

			var user = await _dbContext.Users.FindAsync(userId);
			if (user == null) throw new ArgumentException("User not found");

			const int batchSize = 500;
			var allProducts = new List<Product>();
			using var stream = file.OpenReadStream();
			_logger.LogInformation("Starting feed upload: file={FileName}, size={Size}, userId={UserId}", file.FileName, file.Length, userId);
			var parsed = await _feedParser.ParseAsync(stream, userId);
			_logger.LogInformation("Parsed {Count} products from feed for userId={UserId}", parsed.Count, userId);
			foreach (var p in parsed) p.UserId = userId;

			var batch = new List<Product>(batchSize);
			foreach (var p in parsed)
			{
				batch.Add(p);
				if (batch.Count >= batchSize)
				{
					_logger.LogInformation("Inserting batch of {BatchSize} products", batch.Count);
					await ProcessAndInsertBatchAsync(batch);
					allProducts.AddRange(batch);
					batch.Clear();
				}
			}

			if (batch.Count > 0)
			{
				_logger.LogInformation("Inserting final batch of {BatchSize} products", batch.Count);
				await ProcessAndInsertBatchAsync(batch);
				allProducts.AddRange(batch);
			}
			_logger.LogInformation("Completed feed upload: inserted {Total} products for userId={UserId}", allProducts.Count, userId);
			return allProducts;
		}

		private async Task BulkInsertProductsAsync(List<Product> products)
		{
			try
			{
				await _dbContext.BulkInsertAsync(products, new BulkConfig
				{
					PreserveInsertOrder = true,
					SetOutputIdentity = true,
					BulkCopyTimeout = 300
				});
				await _dbContext.SaveChangesAsync();
			}
			catch (MySqlException ex) when (ex.Message.Contains("Loading local data is disabled"))
			{
				// Fallback when MySQL LOCAL INFILE is disabled: use regular EF AddRange
				_logger.LogWarning(ex, "MySQL LOCAL INFILE disabled; falling back to AddRange for {Count} products", products.Count);
				await _dbContext.AddRangeAsync(products);
				await _dbContext.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Failed to save products batch (size={Count})", products.Count);
				throw new Exception("Failed to save products: " + ex.Message, ex);
			}
		}

		private async Task ProcessAndInsertBatchAsync(List<Product> batch)
		{
			// If products already contain extracted attributes, persist them
			foreach (var p in batch)
			{
				if (p.ExtractedAttributes != null)
					await ProcessAttributesAsync(p);
			}

			await BulkInsertProductsAsync(batch);
		}

		private async Task ProcessAttributesAsync(Product product)
		{
			foreach (var attr in product.ExtractedAttributes)
			{
				var attribute = await _dbContext.Attributes.FirstOrDefaultAsync(a => a.Code == attr.Key);
				if (attribute == null)
				{
					attribute = new Models.Attribute
					{
						Code = attr.Key,
						Name = GetAttributeName(attr.Key),
						IsRequired = IsRequiredAttribute(attr.Key),
						IsRestricted = IsRestrictedAttribute(attr.Key),
						Unit = GetAttributeUnit(attr.Key),
						AllowedValues = GetAllowedValues(attr.Key)
					};
					_dbContext.Attributes.Add(attribute);
					await _dbContext.SaveChangesAsync();
				}

				if (!(attribute.IsRestricted && attribute.AllowedValues != null && !attribute.AllowedValues.Contains(attr.Value)))
				{
					product.Attributes.Add(new ProductAttribute
					{
						AttributeId = attribute.Id,
						Value = attr.Value,
						IsExtractedByAI = true
					});
				}
			}
		}

		// CSV mapping removed; parsing is delegated to adapter.

		public async Task<byte[]> GenerateExcelAsync(ICollection<int> productIds)
		{
			var products = await _dbContext.Products
				.Include(p => p.Attributes)
				.ThenInclude(pa => pa.Attribute)
				.Where(p => productIds.Contains(p.Id))
				.ToListAsync();

			var map = new Dictionary<string, string>
			{
				{ "part_number", "PartNumber" },
				{ "vendor_ext_id", "Id" },
				{ "name", "Name" },
				{ "description", "Description" },
				{ "brand", "Manufacturer" },
				{ "model", "Model" },
				{ "category", "Category" },
				{ "sale_price", "Price" },
				{ "offer_currency", "Currency" },
				{ "stock", "Quantity" },
				{ "warranty", "Warranty" },
				{ "main_image_url", "MainImage" },
				{ "other_image_url1", "AdditionalImage1" },
				{ "other_image_url2", "AdditionalImage2" },
				{ "other_image_url3", "AdditionalImage3" },
				{ "other_image_url4", "AdditionalImage4" }
			};
			var defaults = new Dictionary<string, string>
			{
				{ "vat_rate", "0.2" },
				{ "status", "1" },
				{ "source_language", "RO_ro" }
			};

			return await _exporter.ExportAsync(products, map, defaults);
		}

		public async Task<bool> ClearDatabaseAsync()
		{
			var conn = _dbContext.Database.GetDbConnection(); 
			await conn.OpenAsync();
			var sql = @"
					SET FOREIGN_KEY_CHECKS=0;
					TRUNCATE TABLE `productattributes`;
					TRUNCATE TABLE `attributes`;
					TRUNCATE TABLE `products`;  
					SET FOREIGN_KEY_CHECKS=1;";

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			await cmd.ExecuteNonQueryAsync();

			_dbContext.ChangeTracker.Clear();
			return true;
		}
		// No manual attribute extraction fallback.

		private string GetAttributeName(string code) => code switch
		{
			"[5704]" => "Tip produs",
			"[8541]" => "Suprafata lucru",
			"[8624]" => "Unealta compatibila",
			"[5401]" => "Culoare",
			_ => "Unknown"
		};

		private bool IsRequiredAttribute(string code) => code is "[5704]" or "[8541]" or "[8624]";

		private bool IsRestrictedAttribute(string code) => code is "[5704]" or "[8541]" or "[8624]" or "[5401]";

		private string? GetAttributeUnit(string code) => code switch
		{
			"[6780]" => "mm",
			"[6862]" => "mm",
			"[6878]" => "Kg",
			"[7115]" => "mm",
			_ => null
		};

		private List<string>? GetAllowedValues(string code) => code switch
		{
			"[5704]" => new List<string> { "Disc", "Set" },
			"[5401]" => new List<string> { "Multicolor", "Negru", "Verde/Mov" },
			_ => null
		};
	}
}
