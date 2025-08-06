using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IProductService : IService<Product>
	{
		public Task<ICollection<Product>> GetProductsByCategory(string category);
	}
}
