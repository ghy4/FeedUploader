using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IAttributeService : IService<FeedUploader.Data.Models.Attribute>
	{
		public Task<ICollection<Models.Attribute>> GetAll();
		public Task<bool> Create(FeedUploader.Data.Models.Attribute attribute);
	}
}
