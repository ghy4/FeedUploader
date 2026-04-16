using AIParser.DataUtils;
using AIParser.PromptUtils;
using FeedUploader.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIParser.Runner.Tests
{
    internal class OutputTest
    {
        public static void Run()
        {
            Console.WriteLine("Export .xlsx test");


            // 1. Читаем JSON из файла
            string jsonString = File.ReadAllText("F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\llm_response2.json");

            List<Product> products = new List<Product>();
            List<string> failedLogs = new List<string>();

            var config = new InMemoryCategoryPromptConfigProvider();
            var categoryConfig = config.GetAllAsync().Result.FirstOrDefault(c => c.InternalCategory == "Huse Smartphone");

            var map = new Dictionary<string, string> { { "part_number", "PartNumber" }, { "vendor_ext_id", "Id" }, { "name", "Name" }, { "description", "Description" }, { "brand", "Manufacturer" }, { "model", "Model" }, { "category", "Category" }, { "sale_price", "Price" }, { "offer_currency", "Currency" }, { "stock", "Quantity" }, { "warranty", "Warranty" }, { "main_image_url", "MainImage" }, { "other_image_url1", "AdditionalImage1" }, { "other_image_url2", "AdditionalImage2" }, { "other_image_url3", "AdditionalImage3" }, { "other_image_url4", "AdditionalImage4" } };
            var defaults = new Dictionary<string, string> { { "vat_rate", "0.2" }, { "status", "1" }, { "source_language", "RO_ro" } };

            if (categoryConfig == null)
            {
                Console.WriteLine("No config found for category 'Huse Smartphone'");
                return;
            }
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                var root = doc.RootElement;

                // Проверка на массив, как в оригинале
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elem in root.EnumerateArray())
                    {
                        // Используем ваш существующий десериализатор
                        // Если для тестов атрибуты не важны, можно передать null или пустой список
                        var desRes = ProductDeserializer.DeserializeFromJsonElement(elem, categoryConfig.Attributes);

                        if (desRes.IsMatchOk && desRes.Product != null)
                        {
                            products.Add(desRes.Product);
                            Console.WriteLine($"{desRes.Product.PartNumber}");
                        }
                        else
                        {
                            var reason = desRes.ErrorReason ?? string.Join("; ", desRes.ValidationErrors);
                            failedLogs.Add($"Row {desRes.RowIndex} failed: {reason}");
                        }
                    }
                }
            }

            // Теперь у вас есть чистый список List<Product> для проверок в тестах

            InternalEmagExporter.ExportOnTemplate(
                products,
                  "F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\sample.xlsx",
                  "F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\output.xlsx",
                map,
                defaults
            );

            Console.WriteLine($"Exported {products.Count} products. {failedLogs.Count} failed.");
            Console.WriteLine($"{products[0].Attributes[0].Value}");// we have attributes!
            Console.WriteLine($"{products[0].PartNumber} part");// problem on input
        }

    }
}
