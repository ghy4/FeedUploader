using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IUserService : IService<User>
	{
		public Task<ICollection<User>> GetAll();
		public Task<bool> Create(User user);
		public Task<User?> GetByEmail(string email);
	}
}
