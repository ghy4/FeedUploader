using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IUserService : IService<User>
	{
		public Task<User?> GetByEmail(string email);
	}
}
