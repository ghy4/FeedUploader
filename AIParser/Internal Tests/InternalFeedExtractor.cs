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
            ICategoryPromptConfigProvider configProvider,
            CategoryMapper categoryMapper,
            FeedBatchScheduler? scheduler = null)
        {
            _ai = aiService;
            _configProvider = configProvider;
            _categoryMapper = categoryMapper;
            _scheduler = scheduler ?? new FeedBatchScheduler(2000);
        }

        /// <summary>
        /// Process raw feed for a given externalCategory (Romanian). 
        /// availableAttributes - список атрибутов, которые допустимы для выбранной внутренней категории.
        /// </summary>
        public async Task<InternalFeedExtractionResult> ProcessRawFeedAsync(
            RawFeedData raw,
            List<Attribute> availableAttributes)
        {
            var result = new InternalFeedExtractionResult();
            var configs = await _configProvider.GetAllAsync();
            string externalCategory;
            var externalCategories = await CategoryFieldDetector.GetAllExternalCategories(raw, _ai);
            externalCategory = externalCategories[0];
            var maps = await _categoryMapper.MapCategories(externalCategories, configs);
            string internalCategory = maps[externalCategory];
            // 2) Получаем конфиг для этой внутренней категории (если есть)
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

                // Build prompt (используем предложенный строгий промпт)
                //var prompt = BuildProductExtractionPrompt(rowsText, externalCategory, reqAttrs, otherAttrs);
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

                // For each element in array -> use ProductDeserializer overload
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

        private string BuildProductExtractionPrompt(string rowsText, string externalCategory, List<string> requiredAttributes, List<string> otherAttributes)
        {
            // Используем рекомендованный строгий промпт (обновлённый)
            var req = requiredAttributes.Any() ? string.Join(", ", requiredAttributes) : "none";
            var other = otherAttributes.Any() ? string.Join(", ", otherAttributes) : "none";

            return $@"
You have the following feed line(s) in Romanian: 
{rowsText}

Feed category (original): ""{externalCategory}""

Task:
Return a VALID JSON array of objects, each object must match the Product model described below.
Do NOT return any explanatory text — only JSON.

Product fields:
{{
  ""RowIndex"": integer,
  ""Name"": string,
  ""Description"": string,
  ""Model"": string,
  ""Manufacturer"": string,
  ""Category"": string,
  ""Price"": number,
  ""SalePrice"": number,
  ""Currency"": string,
  ""Quantity"": integer,
  ""Warranty"": integer|null,
  ""MainImage"": string,
  ""AdditionalImage1"": string,
  ""AdditionalImage2"": string,
  ""AdditionalImage3"": string,
  ""AdditionalImage4"": string,
  ""Type"": string,
  ""Attributes"": [ {{ ""Name"": ""Color"", ""Value"": ""Red"" }} ],
  ""MatchStatus"": ""ok"" | ""error"",
  ""ErrorReason"": string|null
}}

Constraints:
- Required attributes for each product: {req}
- Other attributes (optional): {other}
- If a product cannot match required attributes, set ""MatchStatus"":""error"" and provide ""ErrorReason"".
- Use numbers for Price/SalePrice and integers for Quantity/Warranty (or null).
";
        }
    }
    }
