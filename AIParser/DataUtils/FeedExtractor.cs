using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedUploader.Data.Models;
using System.Globalization;

namespace AIParser
{


    internal class FeedExtractor
    {
        public static Product ExtractProduct(List<string> keys, List<string> values)
        {
            if (IsContaktFeed(keys))
            {
                return ContaktStrategy(values);
            }
            else if (IsInterlinkFeed(keys))
            {
                return InterlinkStrategy(values);
            }
            else
            {
                throw new NotSupportedException("Format de feed necunoscut.");
            }
        }

        private static bool IsContaktFeed(List<string> keys)
        {
            return keys.Contains("Cod produs") && keys.Contains("Brand") && keys.Contains("Denumire produs");
        }

        private static bool IsInterlinkFeed(List<string> keys)
        {
            return keys.Contains("id") && keys.Contains("name") && keys.Contains("description");
        }

        private static Product ContaktStrategy(List<string> values)
        {
            return new Product
            {
                Model = values[0],
                Manufacturer = values[1],
                Name = values[2],
                MainImage = values[3],
                Description = values[4],
                Category = $"{values[5]} > {values[6]}",
                AdditionalImage1 = values[7],
                Temps = new Dictionary<string, string> { { "Specificatii", values[8] } },
                Quantity = ParseInt(values[9]),
                Price = ParseDecimal(values[11]),
                Currency = "LEI",
                Type = "new"
            };
        }

        private static Product InterlinkStrategy(List<string> values)
        {
            return new Product
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
        }


        private static string SafeGet(List<string> values, int index)
        {
            return index < values.Count ? values[index] : string.Empty;
        }

        private static decimal ParseDecimal(string input)
        {
            decimal.TryParse(input.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }

        private static int ParseInt(string input)
        {
            int.TryParse(input, out var result);
            return result;
        }
    }
}

