using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
    public interface IFeedParsingAdapter
    {
        Task<List<Product>> ParseAsync(Stream feedStream, int userId, CancellationToken ct = default);
    }
}