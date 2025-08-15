using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeedUploader.Data.Services
{
	public class ProductAttributeService : IProductAttributeService
	{
		private readonly MyDbContext _dbContext;

		public ProductAttributeService(MyDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<bool> Create(ProductAttribute entity)
		{
			var product = await _dbContext.Products.FindAsync(entity.ProductId);
			var attribute = await _dbContext.Attributes.FindAsync(entity.AttributeId);
			if (product == null || attribute == null) return false;

			if (attribute.IsRestricted && attribute.AllowedValues != null && !attribute.AllowedValues.Contains(entity.Value))
				return false;

			await _dbContext.ProductAttributes.AddAsync(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<ICollection<ProductAttribute>> GetByProductId(int productId)
		{
			return await _dbContext.ProductAttributes
				.Include(pa => pa.Attribute)
				.Where(pa => pa.ProductId == productId)
				.ToListAsync();
		}

		public async Task<ProductAttribute?> GetById(int id)
		{
			return await _dbContext.ProductAttributes
				.Include(pa => pa.Attribute)
				.FirstOrDefaultAsync(pa => pa.Id == id);
		}

		public async Task<bool> Update(ProductAttribute entity)
		{
			var attribute = await _dbContext.Attributes.FindAsync(entity.AttributeId);
			if (attribute == null) return false;

			if (attribute.IsRestricted && attribute.AllowedValues != null && !attribute.AllowedValues.Contains(entity.Value))
				return false;

			_dbContext.ProductAttributes.Attach(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<bool> RemoveById(int id)
		{
			var productAttribute = await _dbContext.ProductAttributes.FirstOrDefaultAsync(pa => pa.Id == id);
			if (productAttribute == null) return false;
			_dbContext.ProductAttributes.Remove(productAttribute);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
	}
}
