using AIParser.AttributeUtils;
using FeedUploader.Data.Models;

namespace AIParser.DataUtils.Suppliers;

public class InterlinkStrategy : IFeedStrategy
{
    public Product Parse(List<string> values)
    {
        var product = new Product
        {
            Model = values[3],
            Name = values[1],
            Description = values[2],
            Manufacturer = values[4],
            Category = values[5],
            Price = ParseDecimal(values[6]),
            SalePrice = ParseDecimal(values[7]),
            Currency = values[8],
            Quantity = ParseInt(values[9]),
            Warranty = ParseInt(values[10]),
            MainImage = values[11],
            AdditionalImage1 = SafeGet(values, 12),
            AdditionalImage2 = SafeGet(values, 13),
            AdditionalImage3 = SafeGet(values, 14),
            AdditionalImage4 = SafeGet(values, 15),
            Type = SafeGet(values, 16) ?? "new"
        };

        // apelăm parserul de atribute pentru Interlink
        var parser = FeedAttributeParserFactory.GetParser("Interlink");
        product.Attributes = parser.Parse(product.Description, product.Id);

        return product;
    }

    private int ParseInt(string input) => int.TryParse(input, out var n) ? n : 0;
    private decimal ParseDecimal(string input) => decimal.TryParse(input, out var d) ? d : 0m;
    private string SafeGet(List<string> values, int index) => index < values.Count ? values[index] : string.Empty;
}