using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IService<T>
	{
		public Task<T?> GetById(int id);
		public Task<bool> RemoveById(int id);
		public Task<bool> Update(T entity);
	}
}
