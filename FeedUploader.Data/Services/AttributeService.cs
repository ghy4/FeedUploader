using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeedUploader.Data.Services
{
	public class AttributeService : IAttributeService
	{
		private readonly MyDbContext _dbContext;
		public AttributeService(MyDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task<bool> Create(Models.Attribute entity)
		{
			await _dbContext.Attributes.AddAsync(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<ICollection<Models.Attribute>> GetAll()
		{
			return await _dbContext.Attributes.ToListAsync();
		}

		public async Task<Models.Attribute?> GetById(int id)
		{
			return await _dbContext.Attributes.FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task<Models.Attribute?> GetByCode(string code)
		{
			return await _dbContext.Attributes.FirstOrDefaultAsync(a => a.Code == code);
		}

		public async Task<bool> Update(Models.Attribute entity)
		{
			_dbContext.Attributes.Attach(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}

		public async Task<bool> RemoveById(int id)
		{
			var attribute = await _dbContext.Attributes.FirstOrDefaultAsync(a => a.Id == id);
			if (attribute == null) return false;
			_dbContext.Attributes.Remove(attribute);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
	}
}
