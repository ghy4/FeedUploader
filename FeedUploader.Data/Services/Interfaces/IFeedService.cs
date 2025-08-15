using FeedUploader.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IFeedService
	{
		Task<ICollection<Product>> UploadFeedAsync(IFormFile file, int userId);
		Task<byte[]> GenerateExcelAsync(ICollection<int> productIds);
	}
}
