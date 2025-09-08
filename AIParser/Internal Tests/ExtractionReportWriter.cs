using AIParser.DataUtils;
using FeedUploader.Data.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace AIParser.Internal_Tests
{

public static class ExtractionReportWriter
    {
        /// <summary>
        /// Сохраняет текстовый отчёт о результатах InternalFeedExtractionResult в файл.
        /// Возвращает путь к файлу отчёта.
        /// </summary>
        public static async Task<string> SaveExtractionReportAsync(
            InternalFeedExtractionResult result,
            RawFeedData rawData,
            string outputFilePath)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (rawData == null) throw new ArgumentNullException(nameof(rawData));
            if (string.IsNullOrWhiteSpace(outputFilePath)) throw new ArgumentNullException(nameof(outputFilePath));

            var sb = new StringBuilder();

            sb.AppendLine("=== Feed Extraction Report ===");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            sb.AppendLine("---- Summary ----");
            sb.AppendLine($"Input rows : {rawData.Rows?.Count ?? 0}");
            sb.AppendLine($"Products   : {result.Products?.Count ?? 0}");
            sb.AppendLine($"Failed     : {result.Failed?.Count ?? 0}");
            sb.AppendLine($"Logs count : {result.Logs?.Count ?? 0}");
            sb.AppendLine();

            sb.AppendLine("---- Category mapping ----");
            if (result.CategoryMapping != null && result.CategoryMapping.Count > 0)
            {
                foreach (var kv in result.CategoryMapping)
                    sb.AppendLine($"{kv.Key} => {kv.Value}");
            }
            else
            {
                sb.AppendLine("(no category mapping)");
            }
            sb.AppendLine();

            sb.AppendLine("---- Logs ----");
            if (result.Logs != null && result.Logs.Count > 0)
            {
                foreach (var l in result.Logs)
                    sb.AppendLine($"- {l}");
            }
            else
            {
                sb.AppendLine("(no logs)");
            }
            sb.AppendLine();

            sb.AppendLine("---- Products (detailed) ----");
            if (result.Products != null && result.Products.Count > 0)
            {
                for (int i = 0; i < result.Products.Count; i++)
                {
                    var p = result.Products[i];
                    sb.AppendLine($"[{i}] Name: {Safe(p.Name)}, Price: {p.Price} {Safe(p.Currency)}, Category: {Safe(p.Category)}");
                    sb.AppendLine($"     Model: {Safe(p.Model)}, Manufacturer: {Safe(p.Manufacturer)}, Type: {Safe(p.Type)}");
                    sb.AppendLine($"     Quantity: {p.Quantity}, Warranty: {(p.Warranty?.ToString() ?? "null")}");
                    sb.AppendLine($"     MainImage: {Safe(p.MainImage)}");
                    if (p.Attributes != null && p.Attributes.Count > 0)
                    {
                        sb.AppendLine($"     Attributes ({p.Attributes.Count}):");
                        foreach (var pa in p.Attributes)
                        {
                            var attrName = pa.Attribute?.Name ?? "(unknown)";
                            sb.AppendLine($"       - {attrName}: {Safe(pa.Value)} (AI: {pa.IsExtractedByAI})");
                        }
                    }
                    else
                    {
                        sb.AppendLine("     Attributes: (none)");
                    }
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("(no products)");
                sb.AppendLine();
            }

            sb.AppendLine("---- Failed items ----");
            if (result.Failed != null && result.Failed.Count > 0)
            {
                for (int i = 0; i < result.Failed.Count; i++)
                {
                    var f = result.Failed[i];
                    sb.AppendLine($"[{i}] RowIndex: {(f.RowIndex.HasValue ? f.RowIndex.Value.ToString() : "null")}");
                    sb.AppendLine($"     IsMatchOk: {f.IsMatchOk}");
                    sb.AppendLine($"     ErrorReason: {Safe(f.ErrorReason)}");
                    if (f.ValidationErrors != null && f.ValidationErrors.Count > 0)
                    {
                        sb.AppendLine("     ValidationErrors:");
                        foreach (var ve in f.ValidationErrors)
                            sb.AppendLine($"       * {ve}");
                    }
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("(no failures)");
                sb.AppendLine();
            }

            // Ensure directory exists
            var folder = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Write the text report
            await File.WriteAllTextAsync(outputFilePath, sb.ToString(), Encoding.UTF8);

            // Also save JSON dump of products for easy machine parsing
            try
            {
                var jsonPath = Path.ChangeExtension(outputFilePath, ".products.json");
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(result.Products ?? new System.Collections.Generic.List<Product>(), jsonOptions), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Do not fail the whole operation if JSON dump fails; log to Console
                Console.WriteLine($"Warning: failed to write products JSON dump: {ex.Message}");
            }

            return outputFilePath;
        }

        private static string Safe(string? s) => string.IsNullOrEmpty(s) ? string.Empty : s;
    }

}
