using FeedUploader.Data.Models;
using AIParser.MappingTools;
using Attribute = FeedUploader.Data.Models.Attribute;

namespace AIParser.DataUtils.Suppliers
{
    public class DbMappedStrategy : IFeedStrategy
    {
        private readonly List<MapModel> _maps;
        private readonly List<string> _keys;
        public DbMappedStrategy(List<MapModel> maps)
        {
            _maps = maps;
        }

        public Product Parse(List<string> values)
        {
            var product = new Product();
            for (int i = 0; i < _maps.Count; i++)
            {
                for (int j = 0; j < _keys.Count; j++)
                {
                    if (_maps[i].DestinationField == _keys[j])
                    {
                        ApplyMapping(product,_maps[i].SourceField,values[j]);
                    }
                }
            }
            return product;
        }
        

        private void ApplyMapping(Product product, string destinationField, string value)
        {
            switch (destinationField)
            {
                case "Id": product.Id = int.Parse(value); break;
                case "Name": product.Name = value; break;
                case "Description": product.Description = value; break;
                case "Model": product.Model = value; break;
                case "Manufacturer": product.Manufacturer = value; break;
                case "Category": product.Category = value; break;
                case "Price": product.Price = decimal.TryParse(value, out var p) ? p : 0m; break;
                case "SalePrice": product.SalePrice = decimal.TryParse(value, out var sp) ? sp : 0m; break;
                case "Currency": product.Currency = value; break;
                case "Quantity": product.Quantity = int.TryParse(value, out var q) ? q : 0; break;
                case "Warranty": product.Warranty = int.TryParse(value, out var w) ? w : 0; break;
                case "MainImage": product.MainImage = value; break;
                case "AdditionalImage1": product.AdditionalImage1 = value; break;
                case "AdditionalImage2": product.AdditionalImage2 = value; break;
                case "AdditionalImage3": product.AdditionalImage3 = value; break;
                case "AdditionalImage4": product.AdditionalImage4 = value; break;
                case "Type": product.Type = value; break;

                default:
                    // dacă maparea duce într-un câmp necunoscut -> punem ca atribut extra
                    product.Attributes.Add(new ProductAttribute
                    {
                        Attribute = new Attribute { Name = destinationField },
                        Value = value,
                        IsExtractedByAI = false
                    });
                    break;
            }
        }
    }
}