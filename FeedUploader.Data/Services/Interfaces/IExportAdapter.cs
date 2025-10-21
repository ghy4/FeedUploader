using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
    public interface IExportAdapter
    {
        Task<byte[]> ExportAsync(
            IEnumerable<Product> products,
            IDictionary<string, string> columnToPropertyMap,
            IDictionary<string, string> defaultValues,
            CancellationToken ct = default);
    }
}