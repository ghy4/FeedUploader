using AIParser.DataUtils;
using FeedUploader.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIParser.PromptUtils
{
    public static class CategoryFieldDetector
    {
        /// <summary>
        /// Формирует строгий промпт для модели: список заголовков и (опционально) одна строка значений.
        /// Модель должна вернуть только список 1-based индексов колонок, разделённых запятой (например "4,5")
        /// либо JSON-массив чисел: [4,5]. Никаких объяснений — только данные.
        /// </summary>
        public static string BuildCategoryFieldDetectionPrompt(IEnumerable<string> headers, string? sampleRowValues = null)
        {
            var headersList = headers?.ToList() ?? new List<string>();
            var headerLines = string.Join("\n", headersList.Select((h, i) => $"{i + 1}. {h}"));

            var sampleSection = string.IsNullOrWhiteSpace(sampleRowValues)
                ? string.Empty
                : $"\nSample row values (optional):\n{sampleRowValues}\n";

            return $@"
You are an assistant. Given the feed headers (keys) and an optional example row of values, identify which header positions correspond to category information (category, subcategory, department, group, etc.).
Return ONLY the list of 1-based indices which contain category information. The output must be one of the two forms only:

1) Plain CSV of indices, no extra text. Example:
4,5

OR

2) JSON array of integers. Example:
[4,5]

Do NOT include any explanations or other text.

Headers:
{headerLines}
{sampleSection}
Rules:
- Use 1-based indexing (first header is 1).
- If there are multiple category-related columns (e.g. category + subcategory), return them in the natural order they appear in headers.
- If no category-like column is present, return an empty array [] or an empty line.
Examples:
Headers: 1. Name 2. Price 3. Description 4. Category 5. Subcategory
Answer: 4,5
";
        }

        
            public static async Task<List<int>> DetectCategoryFieldIndicesAsync(
            IEnumerable<string> headers,
            AIService aiService,
            string? sampleRowValues = null)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (aiService == null) throw new ArgumentNullException(nameof(aiService));

            var headersList = headers.ToList();
            var prompt = BuildCategoryFieldDetectionPrompt(headersList, sampleRowValues);

            string aiResponse = await aiService.GetResponseAsync(new AiRequest { MaxTokens = 500, Prompt = prompt });

            var parsed = ParseIndicesFromAiResponse(aiResponse, headersList.Count);

            return parsed;
        }

        public static List<int> ParseIndicesFromAiResponse(string aiResponse, int headerCount)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(aiResponse) || headerCount <= 0)
                return result;

            aiResponse = aiResponse.Trim();

            // Попытка распарсить JSON-массив чисел
            if (aiResponse.StartsWith("[") && aiResponse.EndsWith("]"))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<int>>(aiResponse);
                    if (arr != null)
                    {
                        foreach (var n in arr)
                        {
                            if (n >= 1 && n <= headerCount && !result.Contains(n))
                                result.Add(n);
                        }
                        return result;
                    }
                }
                catch
                {
                    // fallback к обычному парсингу ниже
                }
            }

            // Иначе: ищем числа в тексте (порядок появлений)
            // Регекс захватывает последовательности цифр
            var matches = Regex.Matches(aiResponse, @"\d+");
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Value, out var n))
                {
                    if (n >= 1 && n <= headerCount && !result.Contains(n))
                        result.Add(n);
                }
            }

            // В редких случаях модель может вернуть что-то вроде "4,5" вместе с текстом —
            // наш регекс уже обработает это. Если ничего не найдено, вернём пустой список.
            return result;
        }

        public static string FormatIndicesAsPath(IEnumerable<int> indices, IList<string> headers)
        {
            if (indices == null) return string.Empty;
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            var parts = new List<string>();
            foreach (var idx in indices)
            {
                if (idx >= 1 && idx <= headers.Count)
                {
                    parts.Add(headers[idx - 1]);
                }
            }

            return string.Join(" > ", parts);
        }

        public async static Task<List<string>> GetAllExternalCategories(RawFeedData data, AIService service)
        {
          
            var cs = await DetectCategoryFieldIndicesAsync(data.Headers, service);

          if (cs.Count == 0) throw new Exception();

          var categories = new List<string>();
            foreach (var row in data.Rows)
            {
                Console.WriteLine($"{row[0]}");
                var parts = new List<string>();
                foreach (var c in cs)
                {
                    if (c - 1 < row.Count)
                        parts.Add(row[c - 1]);
                }
                var category = string.Join(" > ", parts);
                if (!string.IsNullOrWhiteSpace(category) && !categories.Contains(category))
                {
                    categories.Add(category);
                    Console.WriteLine($"Detected category: {category} from columns {string.Join(",", cs)}");
                }
                   
            }
            return categories;
        }
    }
}
