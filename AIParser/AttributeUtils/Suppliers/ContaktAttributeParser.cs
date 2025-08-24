using FeedUploader.Data.Models;
using Attribute = System.Attribute;

namespace AIParser.AttributeUtils.Suppliers;

public class ContaktAttributeParser : IFeedAttributeParser
{
    public List<ProductAttribute> Parse(string rawSpecString, int productId)
    {
        var result = new List<ProductAttribute>();
        var specs = rawSpecString.Split(';');

        foreach (var spec in specs)
        {
            var parts = spec.Split(':');
            if (parts.Length == 2)
            {
                var name = parts[0].Trim();
                var value = parts[1].Trim();

                result.Add(new ProductAttribute
                {
                    ProductId = productId,
                    Attribute = new FeedUploader.Data.Models.Attribute
                    {
                        Name = name,
                        Code = name.ToLower().Replace(" ", "_")
                    },
                    Value = value,
                    IsExtractedByAI = false
                });
            }
        }

        return result;
    }
}