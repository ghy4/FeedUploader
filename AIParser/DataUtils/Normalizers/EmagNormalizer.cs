using FeedUploader.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using AIParser.DataUtils;
using AIParser.MappingTools;
using 

namespace AIParser.DataUtils.Normalizers
{
    public class EmagNormalizer : IProductNormalizer
    {
        private readonly List<Attribute> _emagAttributes;
        private readonly AIMapper _aiMapper;

        public EmagNormalizer(List<Attribute> emagAttributes, AIMapper aiMapper)
        {
            _emagAttributes = emagAttributes;
            _aiMapper = aiMapper;
        }

        public async Task<Product> NormalizeAsync(Product product)
        {
            foreach (var pa in product.Attributes)
            {
                var attr = _emagAttributes.FirstOrDefault(a =>
                    string.Equals(a.Name, pa.Attribute?.Name, StringComparison.OrdinalIgnoreCase));

                if (attr == null)
                {
                    var suggestion = await _aiMapper.SuggestAttributeAsync(
                        pa.Attribute?.Name ?? "",
                        _emagAttributes.Select(a => a.Name).ToList()
                    );

                    if (suggestion != "UNKNOWN")
                        attr = _emagAttributes.First(a => a.Name == suggestion);
                }

                if (attr != null)
                {
                    pa.Attribute = attr;

                    if (attr.IsRestricted && attr.AllowedValues != null)
                    {
                        if (!attr.AllowedValues.Contains(pa.Value))
                        {
                            var suggestion = await _aiMapper.SuggestValueAsync(attr.Name, pa.Value, attr.AllowedValues);
                            if (suggestion != "UNKNOWN")
                                pa.Value = suggestion;
                        }
                    }
                }
            }

            return product;
        }
    }
}