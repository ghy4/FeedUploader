using AIParser.AttributeUtils;
using FeedUploader.Data.Models;

namespace AIParser.DataUtils.Suppliers;

public class ContaktStrategy : IFeedStrategy
{
    public Product Parse(List<string> values)
    {
        var product = new Product
        {
            Model = values[0],
            Manufacturer = values[1],
            Name = values[2],
            MainImage = values[3],
            Description = values[4],
            Category = $"{values[5]} > {values[6]}",
            AdditionalImage1 = values[7],
            Quantity = ParseInt(values[9]),
            Price = ParseDecimal(values[11]),
            Currency = "LEI",
            Type = "new"
        };

        // Atributele le delegăm către parserul specific
        var parser = FeedAttributeParserFactory.GetParser("Contakt");
        product.Attributes = parser.Parse(values[8], product.Id);

        return product;
    }

    private int ParseInt(string input) => int.TryParse(input, out var n) ? n : 0;
    private decimal ParseDecimal(string input) => decimal.TryParse(input, out var d) ? d : 0m;
}