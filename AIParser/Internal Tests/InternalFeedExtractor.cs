using AIParser.DataUtils;
using AIParser.PromptUtils;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Attribute = FeedUploader.Data.Models.Attribute;

namespace AIParser.Internal_Tests
{
    public class InternalFeedExtractionResult
    {
        public List<Product> Products { get; } = new();
        public List<ProductDeserializationResult> Failed { get; } = new();
        public Dictionary<string, string> CategoryMapping { get; } = new();
        public List<string> Logs { get; } = new();
    }
    public class InternalFeedExtractor
    {
        private readonly AIService _ai;
        private readonly ICategoryPromptConfigProvider _configProvider;
        private readonly CategoryMapper _categoryMapper;
        private readonly FeedBatchScheduler _scheduler;

        public InternalFeedExtractor(
            AIService aiService,
            ICategoryPromptConfigProvider configProvider)
        {
            _ai = aiService;
            _configProvider = configProvider;
            _categoryMapper = new CategoryMapper(_ai);
            _scheduler = new FeedBatchScheduler(2000);
        }

        public async Task<InternalFeedExtractionResult> ProcessRawFeedAsync(
            RawFeedData raw,
             User user)
        {
            var availableAttributes = _configProvider.GetAllAsync().Result.SelectMany(c => c.Attributes).ToList();
            var result = new InternalFeedExtractionResult();
            var configs = await _configProvider.GetAllAsync();
            string externalCategory;
            var externalCategories = await CategoryFieldDetector.GetAllExternalCategories(raw, _ai);
            externalCategory = externalCategories[0];
            var maps = await _categoryMapper.MapCategories(externalCategories, configs);
            string internalCategory = maps[externalCategory];// later, it will be a list, now just 1to1
            // aici trebu pus la sciotcic goiu, da pe urma 
            var config = configs.FirstOrDefault(c => string.Equals(c.InternalCategory, internalCategory, StringComparison.OrdinalIgnoreCase));
            if (config == null)
            {
                result.Logs.Add($"No prompt config for internal category '{internalCategory}'. Aborting.");
                return result;
            }

            // 3) Разбиваем на батчи
            foreach (var batch in _scheduler.SplitIntoBatches(raw))
            {
                // Build rows text for prompt
                var rowsText = BuildRowsText(batch, raw.Headers);

                // Required and other attributes names (by Name)
                var reqAttrs = config.Attributes.Where(a => a.IsRequired).Select(a => a.Name).ToList();
                var otherAttrs = config.Attributes.Where(a => !a.IsRequired).Select(a => a.Name).ToList();
                var prompt = config.PromptTemplate
                    .Replace("{rowsText}", rowsText)
                    .Replace("{internalCategory}", internalCategory)
                    .Replace("{req}", reqAttrs.Any() ? string.Join(", ", reqAttrs) : "none")
                    .Replace("{other}", otherAttrs.Any() ? string.Join(", ", otherAttrs) : "none");
                // Call AI
                string aiResponse;
                try
                {
                    aiResponse = await _ai.GetResponseAsync(new AiRequest { MaxTokens = 4000, Prompt = prompt });

                }
                catch (Exception ex)
                {
                    result.Logs.Add($"AI call failed for a batch: {ex.Message}");
                    // можно реализовать fallback по строкам — но для простоты записываем ошибку и продолжаем
                    continue;
                }

                // Try parse AI response as JSON array
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(aiResponse);
                }
                catch (JsonException)
                {
                    result.Logs.Add("AI returned invalid JSON for batch. Skipping batch.");
                    continue;
                }

                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Array)
                {
                    result.Logs.Add("AI did not return JSON array. Skipping batch.");
                    continue;
                }

                foreach (var elem in root.EnumerateArray())
                {
                    var desRes = ProductDeserializer.DeserializeFromJsonElement(elem, config.Attributes);

                    if (desRes.IsMatchOk && desRes.Product != null)
                    {
                        result.Products.Add(desRes.Product);
                    }
                    else
                    {
                        result.Failed.Add(desRes);
                        // Also log reason
                        var reason = desRes.ErrorReason ?? string.Join("; ", desRes.ValidationErrors);
                        result.Logs.Add($"Row {(desRes.RowIndex?.ToString() ?? "?")} failed: {reason}");
                    }
                }
            }
            foreach(var p in result.Products)
            {
                p.User = user;
                p.UserId = user.Id;
            }


            return result;
        }

        private string BuildRowsText(List<List<string>> batch, List<string> headers)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < batch.Count; i++)
            {
                var row = batch[i];
                var rowParts = new List<string>();
                for (int j = 0; j < headers.Count && j < row.Count; j++)
                {
                    var h = headers[j];
                    var v = row[j];
                    rowParts.Add($"{h}={v}");
                }

                sb.AppendLine($"RowIndex:{i} -> {string.Join("; ", rowParts)}");
            }
            return sb.ToString().Trim();
        }


    }
}
