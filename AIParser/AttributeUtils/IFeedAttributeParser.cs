using FeedUploader.Data.Models;

namespace AIParser.AttributeUtils;

    public interface IFeedAttributeParser
    {
        List<ProductAttribute> Parse(string rawSpecString, int productId);
    }
