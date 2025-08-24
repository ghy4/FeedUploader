using FeedUploader.Data.Models;

namespace AIParser.DataUtils
{
    public interface IProductNormalizer
    {
        Task<Product> NormalizeAsync(Product product);
    }
}