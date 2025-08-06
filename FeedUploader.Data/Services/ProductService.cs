using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeedUploader.Data.Services
{
	public class ProductService : IProductService
	{
		private readonly MyDbContext _dbContext;

		public ProductService(MyDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<bool> Create(Product entity)
		{
			await _dbContext.Products.AddAsync(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<ICollection<Product>> GetAll()
		{
			var products = await _dbContext.Products.ToListAsync();
			return products;
		}

		public async Task<Product?> GetById(int id)
		{
			var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
			return product;
		}

		public async Task<ICollection<Product>> GetProductsByCategory(string category)
		{
			var products = await _dbContext.Products.Where(x => x.Category == category).ToListAsync();
			return products;
		}

		public async Task<bool> RemoveById(int id)
		{
			var productToRemove = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
			if (productToRemove == null)
				return false;
			_dbContext.Remove(productToRemove);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<bool> Update(Product entity)
		{
			_dbContext.Products.Attach(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
	}
}
