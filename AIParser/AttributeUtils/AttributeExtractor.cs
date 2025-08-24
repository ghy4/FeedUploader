
using FeedUploader.Data.Models;
using Attribute = System.Attribute;

namespace AIParser.AttributeUtils
{


    public static class AttributeExtractor
    {
        /// <summary>
        /// Extrage atribute dintr-un text liber (descriere).
        /// Folosește reguli simple (regex sau heuristici).
        /// Dacă nu găsește nimic, returnează listă goală.
        /// </summary>
        public static List<ProductAttribute> ExtractFromDescription(string description, int productId)
        {
            var result = new List<ProductAttribute>();

            if (string.IsNullOrWhiteSpace(description))
                return result;

            // Simplu: căutăm pattern-uri de tip "Cheie: Valoare"
            var lines = description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        result.Add(new ProductAttribute
                        {
                            ProductId = productId,
                            Attribute = new FeedUploader.Data.Models.Attribute
                            {
                                Name = parts[0].Trim(),
                                Code = parts[0].Trim().ToLower().Replace(" ", "_")
                            },
                            Value = parts[1].Trim(),
                            IsExtractedByAI = false
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Helper pentru cazurile în care ai un șir delimitat de semicolon (ex. Contakt).
        /// </summary>
        public static List<ProductAttribute> ExtractFromDelimited(string rawSpecString, int productId,
            char delimiter = ';')
        {
            var result = new List<ProductAttribute>();
            var specs = rawSpecString.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            foreach (var spec in specs)
            {
                var parts = spec.Split(':', 2);
                if (parts.Length == 2)
                {
                    result.Add(new ProductAttribute
                    {
                        ProductId = productId,
                        Attribute = new FeedUploader.Data.Models.Attribute
                        {
                            Name = parts[0].Trim(),
                            Code = parts[0].Trim().ToLower().Replace(" ", "_")
                        },
                        Value = parts[1].Trim(),
                        IsExtractedByAI = false
                    });
                }
            }

            return result;
        }
    }
}