

namespace AIParser.MappingTools
{
    public class AIMapper : IMapperHelper
    {
        private readonly AIService _aiService;


        public AIMapper(AIService aiService)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        }

        public async Task<MapModel> MapModel(string model, List<string> markets)
        {
            string prompt =
                $"Please map the model '{model}' to the following markets: {string.Join(", ", markets)}. Split the answer by : symbol. Example of answer - Calculatoare > Laptopuri Gaming : Electronica > Laptops > Laptop Gaming ";
            return await GetMapping(prompt, model);
        }

        private async Task<MapModel> GetMapping(string prompt, string model)
        {
            var aiRequest = new AiRequest
            {
                Prompt = prompt,
                MaxTokens = 1000
            };
            var response = await _aiService.GetResponseAsync(aiRequest);
            string[] mappings = response.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (mappings.Length < 2)
            {
                throw new Exception("Invalid response format from AI service.");
            }

            if (mappings[1].Contains(model))
            {
                return new MapModel
                {
                    DestinationField = mappings[1].Trim(),
                    SourceField = model
                };
            }
            else if (mappings[0].Contains(model))
            {
                return new MapModel
                {
                    DestinationField = mappings[0].Trim(),
                    SourceField = model
                };
            }
            else
            {
                throw new Exception($"Model '{model}' not found in AI response.");
            }
        }

        public async Task<MapModel> MapCategory(List<string> models, string market)
        {
            string prompt =
                $"Please map the following models to the market '{market}': {string.Join(", ", models)}. Split the answer by : symbol. Example of answer - Calculatoare > Laptopuri Gaming : Electronica > Laptops > Laptop Gaming ";
            return await GetMapping(prompt, string.Join(", ", models));
        }


        public Task<MapModel> FeedMap(List<string> sources, string model)
        {
            throw new NotImplementedException();
        }

        public Task<MapModel> MarketMap(List<string> marketFields, string model)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SuggestAttributeAsync(string sourceAttribute, List<string> marketplaceAttributes)
        {
            var prompt = $@"
Tu ești un asistent care mapează câmpuri între feed-uri de produse și marketplace-uri.

Atribut feed: ""{sourceAttribute}""
Lista de atribute marketplace:
- {string.Join("\n- ", marketplaceAttributes)}

Răspunde DOAR cu numele atributului marketplace cel mai apropiat.
Dacă nu există corespondent, răspunde ""UNKNOWN"".
";

            var response = await _aiService.GetResponseAsync(new AiRequest{MaxTokens = 2500, Prompt = prompt});
            return response.Trim();
        }

        public async Task<string> SuggestValueAsync(string attributeName, string sourceValue,
            List<string> allowedValues)
        {
            var prompt = $@"
Tu ești un asistent care normalizează valori de atribute pentru marketplace.

Atribut: ""{attributeName}""
Valoare feed: ""{sourceValue}""

Lista de valori permise:
- {string.Join("\n- ", allowedValues)}

Răspunde DOAR cu valoarea cea mai apropiată din listă.
Dacă nu există potrivire, răspunde ""UNKNOWN"".
";

            var response = await _aiService.GetResponseAsync(new AiRequest{MaxTokens = 2500, Prompt = prompt});
            return response.Trim();
        }


        public async Task<MapModel> MapMarket(List<string> modelFields, string market)
        {
            // de terminat
            string prompt =
                $"Please map the following models to the market '{market}': {string.Join(", ", modelFields)}. Split the answer by : symbol. Example of answer - Warranty : Garantie ";
            return await GetMapping(prompt, string.Join(", ", modelFields));
        }
    }
}

