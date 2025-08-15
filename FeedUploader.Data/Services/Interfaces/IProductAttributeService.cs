using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IProductAttributeService : IService<ProductAttribute>
	{
		public Task<bool> Create(ProductAttribute productAttribute);
	}
}
