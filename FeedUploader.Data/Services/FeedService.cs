using ClosedXML.Excel;
using CsvHelper;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using EFCore.BulkExtensions;
using OfficeOpenXml;

namespace FeedUploader.Data.Services
{
	public class FeedService : IFeedService
	{
		private readonly MyDbContext _dbContext;

		public FeedService(MyDbContext dbContext)
		{
			_dbContext = dbContext;
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

			var currentBatch = new List<Product>();
			var allProducts = new List<Product>();
			const int batchSize = 500; 

			try
			{
				using var stream = file.OpenReadStream();
				using var reader = new StreamReader(stream);
				using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

				csv.Context.RegisterClassMap<ProductCsvMap>();
				await csv.ReadAsync();
				csv.ReadHeader();

				while (await csv.ReadAsync())
				{
					var product = csv.GetRecord<Product>();
					if (product == null) continue;

					product.UserId = userId;
					product.ExtractedAttributes = await ExtractAttributesAsync(product.Description);

					if (product.ExtractedAttributes != null)
					{
						await ProcessAttributesAsync(product);
					}

					currentBatch.Add(product);

					if (currentBatch.Count >= batchSize)
					{
						await BulkInsertProductsAsync(currentBatch);
						allProducts.AddRange(currentBatch);
						currentBatch.Clear();
					}
				}

				if (currentBatch.Any())
				{
					await BulkInsertProductsAsync(currentBatch);
					allProducts.AddRange(currentBatch);
				}

				return allProducts;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to save products: " + ex.Message, ex);
			}
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
			catch (DbUpdateException ex)
			{
				throw new Exception("Failed to save products: " + ex.Message, ex);
			}
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

		private sealed class ProductCsvMap : CsvHelper.Configuration.ClassMap<Product>
		{
			public ProductCsvMap()
			{
				Map(m => m.Id).Name("id");
				Map(m => m.Name).Name("name");
				Map(m => m.Description).Name("description");
				Map(m => m.Model).Name("model");
				Map(m => m.Manufacturer).Name("manufacturer");
				Map(m => m.Category).Name("category");
				Map(m => m.Price).Name("price");
				Map(m => m.SalePrice).Name("sale_price");
				Map(m => m.Currency).Name("currency");
				Map(m => m.Quantity).Name("quantity");
				Map(m => m.Warranty).Name("warranty");
				Map(m => m.MainImage).Name("image");
				Map(m => m.AdditionalImage1).Name("additional_image_1");
				Map(m => m.AdditionalImage2).Name("additional_image_2");
				Map(m => m.AdditionalImage3).Name("additional_image_3");
				Map(m => m.AdditionalImage4).Name("additional_image_4");
				Map(m => m.Type).Name("type");
			}
		}

		public async Task<byte[]> GenerateExcelAsync(ICollection<int> productIds)
		{
			using var package = new ExcelPackage();
			var worksheet = package.Workbook.Worksheets.Add("Template");

			// eMAG template headers
			worksheet.Cells[1, 1].Value = "part_number";
			worksheet.Cells[1, 2].Value = "name";
			worksheet.Cells[1, 3].Value = "main_image_url";
			worksheet.Cells[1, 4].Value = "Tip produs: [5704]";
			worksheet.Cells[1, 5].Value = "Suprafata lucru: [8541]";
			worksheet.Cells[1, 6].Value = "Unealta compatibila: [8624]";
			worksheet.Cells[1, 7].Value = "Culoare: [5401]";

			var products = await _dbContext.Products
				.Include(p => p.Attributes)
				.ThenInclude(pa => pa.Attribute)
				.Where(p => productIds.Contains(p.Id))
				.ToListAsync();

			for (int i = 0; i < products.Count; i++)
			{
				var product = products[i];
				worksheet.Cells[i + 2, 1].Value = product.Model;
				worksheet.Cells[i + 2, 2].Value = product.Name;
				worksheet.Cells[i + 2, 3].Value = product.MainImage;
				worksheet.Cells[i + 2, 4].Value = product.Attributes.FirstOrDefault(a => a.Attribute?.Code == "[5704]")?.Value ?? "Disc";
				worksheet.Cells[i + 2, 5].Value = product.Attributes.FirstOrDefault(a => a.Attribute?.Code == "[8541]")?.Value ?? "Metal";
				worksheet.Cells[i + 2, 6].Value = product.Attributes.FirstOrDefault(a => a.Attribute?.Code == "[8624]")?.Value ?? "Polizor unghiular";
				worksheet.Cells[i + 2, 7].Value = product.Attributes.FirstOrDefault(a => a.Attribute?.Code == "[5401]")?.Value ?? "Multicolor";
			}

			return package.GetAsByteArray();
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

		private async Task<Dictionary<string, string>> ExtractAttributesAsync(string description)
		{
			// Placeholder for AI service (e.g., Azure NLP)
			return await Task.FromResult(new Dictionary<string, string>
			{
				{ "[5704]", "Disc" },
				{ "[8541]", "Metal" },
				{ "[8624]", "Polizor unghiular" },
				{ "[5401]", "Multicolor" }
			});
		}

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
