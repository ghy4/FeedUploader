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

		public async Task<bool> Create(Product entity, int userId)
		{
			var user = await _dbContext.Users.FindAsync(userId);
			if (user == null) return false;

			entity.UserId = userId;
			await _dbContext.Products.AddAsync(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<ICollection<Product>> GetAll()
		{
			return await _dbContext.Products
				.Include(p => p.Attributes)
				.ThenInclude(pa => pa.Attribute)
				.ToListAsync();
		}

		public async Task<ICollection<Product>> GetByUserId(int userId)
		{
			return await _dbContext.Products
				.Include(p => p.Attributes)
				.ThenInclude(pa => pa.Attribute)
				.Where(p => p.UserId == userId)
				.ToListAsync();
		}

		public async Task<Product?> GetById(int id)
		{
			return await _dbContext.Products
				.Include(p => p.Attributes)
				.ThenInclude(pa => pa.Attribute)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<bool> Update(Product entity)
		{
			_dbContext.Products.Attach(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<bool> RemoveById(int id)
		{
			var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product == null) return false;
			_dbContext.Products.Remove(product);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
	}
}