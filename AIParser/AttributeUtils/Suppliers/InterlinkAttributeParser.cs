using FeedUploader.Data.Models;

namespace AIParser.AttributeUtils.Suppliers;

public class InterlinkAttributeParser : IFeedAttributeParser
{
    public List<ProductAttribute> Parse(string rawSpecString, int productId)
    {
        // aici folosim AttributeExtractor doar dacă nu găsim delimitatori ":" sau ";"
        if (!rawSpecString.Contains(":"))
        {
            return AttributeExtractor.ExtractFromDescription(rawSpecString, productId);
        }

        var result = new List<ProductAttribute>();
        var specs = rawSpecString.Split('\n'); // la Interlink pot fi pe linii separate

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