using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeedUploader.Data.Services
{
	public class UserService : IUserService
	{
		private readonly MyDbContext _dbContext;
		public UserService(MyDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task<bool> Create(User entity)
		{
			await _dbContext.Users.AddAsync(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
		public async Task<ICollection<User>> GetAll()
		{
			var users = await _dbContext.Users.ToListAsync();
			return users;
		}
		public async Task<User?> GetById(int id)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
			return user;
		}
		public async Task<User?> GetByEmail(string email)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
			return user;
		}
		public async Task<bool> RemoveById(int id)
		{
			var usertoremove = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
			if (usertoremove == null)
				return false;
			_dbContext.Remove(usertoremove);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
		public async Task<bool> Update(User entity)
		{
			_dbContext.Users.Attach(entity);
			return await _dbContext.SaveChangesAsync() >= 1;
		}
	}
}
