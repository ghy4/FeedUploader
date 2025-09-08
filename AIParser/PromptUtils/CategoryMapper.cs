using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIParser.PromptUtils
{


    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class CategoryMapper
    {
        private readonly AIService aIService;

        public CategoryMapper(AIService aiService)
        {
            this.aIService = aiService;
        }

        private string BuildCategoryMappingPrompt(
            IEnumerable<string> externalCategories,
            IEnumerable<PromptCategoryConfig> internalConfigs)
        {
            // Формируем списки; каждый элемент в кавычках, без лишних ведущих запятых
            var externalList = string.Join("\n", externalCategories.Select(c => $"\"{c}\""));
            var internalList = string.Join("\n", internalConfigs.Select(c => $"\"{c.InternalCategory}\""));

            // Жёсткий JSON-only prompt с примером
            return $@"
System:
You are an AI assistant that MUST return mapping only, with NO explanations or extra text. Be deterministic.

User:
External feed categories (each is a full path string; use the whole path as a single key):
{externalList}

Internal system categories (available targets):
{internalList}

Task:
Return a SINGLE JSON object where keys are the EXACT external path strings (including '>' if present)
and values are EXACT internal categories.

Format requirements (must be followed exactly):
- Return a single JSON object only, for example:
{{ 
  ""Huse telefoane > Huse Apple"": ""Huse Smartphone"", 
  ""Huse telefoane > Huse Huawei"": ""Huse Smartphone"" 
}}
- Use the exact strings provided as keys (including spaces and >).
- If you cannot match exactly, map to ""Uncategorized"".
- Do NOT return any plain text, no explanations, no extra characters, no leading tokens.

Examples:
External: [""A > B"", ""C""] Internal: [""X"",""Y""]
Answer:
{{ ""A > B"": ""X"", ""C"": ""Y"" }}

Now produce the JSON object for the given lists.
";
        }

        /// <summary>
        /// Robust parser: принимает список ожидаемых внешних категорий (exact path strings) и raw aiResponse,
        /// возвращает маппинг expectedExternal -> internalCategory, применяя правила:
        /// 1) exact match,
        /// 2) prefix/parent match (AI returned a parent path),
        /// 3) token-overlap heuristic (max overlap),
        /// 4) fallback "Uncategorized".
        /// Параллельно пишутся поясняющие логи.
        /// </summary>
        public Dictionary<string, string> ParseMappingResponse(
            IEnumerable<string> externalCategories,
            string aiResponse)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var expected = externalCategories?.ToList() ?? new List<string>();

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                foreach (var ext in expected)
                {
                    Console.WriteLine($"No mapping response for '{ext}', defaulting to 'Uncategorized'");
                    result[ext] = "Uncategorized";
                }
                return result;
            }

            // 1) Парсим AI ответ в словарь AIkey -> AIvalue (AI keys остаются нормализованными)
            var aiMap = ParseAiResponseToDict(aiResponse);
            Console.WriteLine($"Parsed AI keys: {string.Join(" | ", aiMap.Keys)}");

            // Подготовим токены для ai keys
            var aiTokenMap = aiMap.ToDictionary(kv => kv.Key, kv => Tokenize(kv.Key), StringComparer.OrdinalIgnoreCase);

            // Для каждого ожидаемого external path ищем соответствие
            foreach (var expectedRaw in expected)
            {
                var expectedNorm = NormalizePath(expectedRaw);
                string chosen = "Uncategorized";
                string reason = "none";

                // 1) Exact match (normalized)
                var exactKey = aiMap.Keys.FirstOrDefault(k => string.Equals(k, expectedNorm, StringComparison.OrdinalIgnoreCase));
                if (exactKey != null)
                {
                    chosen = aiMap[exactKey];
                    reason = $"exact match key='{exactKey}'";
                }
                else
                {
                    // 2) Prefix/parent match: aiKey segments are prefix of expected segments
                    var parentKey = aiMap.Keys.FirstOrDefault(k => IsPrefixPath(k, expectedNorm));
                    if (parentKey != null)
                    {
                        chosen = aiMap[parentKey];
                        reason = $"parent/prefix match aiKey='{parentKey}'";
                    }
                    else
                    {
                        // 3) Token-overlap heuristic: choose aiKey with max token intersection (>=1)
                        var expectedTokens = Tokenize(expectedNorm);
                        int bestOverlap = 0;
                        string? bestAiKey = null;
                        foreach (var kv in aiTokenMap)
                        {
                            var overlap = expectedTokens.Intersect(kv.Value).Count();
                            if (overlap > bestOverlap)
                            {
                                bestOverlap = overlap;
                                bestAiKey = kv.Key;
                            }
                        }

                        if (bestAiKey != null && bestOverlap > 0)
                        {
                            chosen = aiMap[bestAiKey];
                            reason = $"token-overlap match aiKey='{bestAiKey}', overlap={bestOverlap}";
                        }
                        else
                        {
                            chosen = "Uncategorized";
                            reason = "no match found";
                        }
                    }
                }

                result[expectedRaw] = chosen;
                Console.WriteLine($"Mapped '{expectedRaw}' -> '{chosen}' ({reason})");
            }

            return result;
        }

        /// <summary>
        /// Основной метод: строит промпт, вызывает AIService и возвращает уже распарсенный словарь.
        /// </summary>
        public async Task<Dictionary<string, string>> MapCategories(
            IEnumerable<string> externalCategories,
            IEnumerable<PromptCategoryConfig> internalConfigs)
        {
            var prompt = BuildCategoryMappingPrompt(externalCategories, internalConfigs);
            var aiRequest = new AiRequest
            {
                Prompt = prompt,
                MaxTokens = 2000
                // при необходимости можно добавить другие параметры temperature и т.п., если AiRequest их поддерживает
            };

            string aiResponse;
            try
            {
                aiResponse = await aIService.GetResponseAsync(aiRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI request failed: {ex.Message}. Defaulting all to 'Uncategorized'.");
                return externalCategories.ToDictionary(e => e, e => "Uncategorized", StringComparer.OrdinalIgnoreCase);
            }

            var parsed = ParseMappingResponse(externalCategories, aiResponse);

            // Гарантируем, что для всех external будет ключ
            foreach (var external in externalCategories)
            {
                if (!parsed.ContainsKey(external))
                {
                    parsed[external] = "Uncategorized";
                    Console.WriteLine($"No mapping for '{external}', defaulting to 'Uncategorized'");
                }
            }

            return parsed;
        }

        // -----------------вспомогательные методы----------------

        private static string NormalizePath(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var tmp = s.Trim().Trim('"', '\'');
            tmp = Regex.Replace(tmp, @"\s*>\s*", " > ");
            tmp = Regex.Replace(tmp, @"\s+", " ").Trim();
            return tmp;
        }

        private static HashSet<string> Tokenize(string s)
        {
            var tokens = Regex.Split(s.ToLowerInvariant(), @"\W+")
                .Where(t => !string.IsNullOrWhiteSpace(t) && t.Length > 2)
                .ToHashSet();
            return tokens;
        }

        private static bool IsPrefixPath(string candidate, string expectedPath)
        {
            if (string.IsNullOrWhiteSpace(candidate) || string.IsNullOrWhiteSpace(expectedPath)) return false;

            var candSegs = NormalizePath(candidate).Split('>').Select(x => x.Trim().ToLowerInvariant()).ToArray();
            var expSegs = NormalizePath(expectedPath).Split('>').Select(x => x.Trim().ToLowerInvariant()).ToArray();

            if (candSegs.Length > expSegs.Length) return false;

            for (int i = 0; i < candSegs.Length; i++)
            {
                if (!string.Equals(candSegs[i], expSegs[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Парсит AI-ответ в словарь (AIkey(normalized) -> value).
        /// Поддерживает JSON object и разные варианты plain text "key : value".
        /// </summary>
        private static Dictionary<string, string> ParseAiResponseToDict(string aiResponse)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(aiResponse))
                return dict;

            var trimmed = aiResponse.Trim();

            // Попытка JSON object
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            {
                try
                {
                    var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(trimmed);
                    if (obj != null)
                    {
                        foreach (var kv in obj)
                        {
                            var k = NormalizePath(kv.Key);
                            var v = kv.Value?.Trim() ?? string.Empty;
                            dict[k] = v;
                        }
                        return dict;
                    }
                }
                catch
                {
                    // fallthrough to plain parsing
                }
            }

            // Plain text parsing: ищем пары "key" : "value" или key : value или key:value
            var rx = new Regex(@"['""]?(?<k>[^:'""]+?)['""]?\s*:\s*['""]?(?<v>[^'""]+?)['""]?(?:\r?\n|$)", RegexOptions.Multiline);
            var matches = rx.Matches(aiResponse);
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    var k = NormalizePath(m.Groups["k"].Value);
                    var v = m.Groups["v"].Value.Trim();
                    if (!string.IsNullOrEmpty(k))
                        dict[k] = v;
                }
                return dict;
            }

            // Fallback: построчный разбор "key:value"
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var idx = raw.IndexOf(':');
                if (idx > 0)
                {
                    var k = NormalizePath(raw.Substring(0, idx));
                    var v = raw.Substring(idx + 1).Trim().Trim('"', '\'');
                    if (!string.IsNullOrEmpty(k))
                        dict[k] = v;
                }
            }

            return dict;
        }
    }

}
