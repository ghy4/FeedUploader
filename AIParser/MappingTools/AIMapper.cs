namespace AIParser.MappingTools
{
    public class AIMapper : IMapper
    {
        private readonly AIService _aiService;
        public AIMapper(AIService aiService)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        }
        public async Task<MapModel> MapModel(string model, List<string> markets)
        {
            string prompt = $"Please map the model '{model}' to the following markets: {string.Join(", ", markets)}. Split the answer by : symbol. Example of answer - Calculatoare > Laptopuri Gaming : Electronica > Laptops > Laptop Gaming ";
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
                    Market = mappings[1].Trim(),
                    Model = model
                };
            }
            else if (mappings[0].Contains(model))
            {
                return new MapModel
                {
                    Market = mappings[0].Trim(),
                    Model = model
                };
            }
            else
            {
                throw new Exception($"Model '{model}' not found in AI response.");
            }
        }
        public async Task<MapModel> MapModel(List<string> models, string market)
        {
            string prompt = $"Please map the following models to the market '{market}': {string.Join(", ", models)}. Split the answer by : symbol. Example of answer - Calculatoare > Laptopuri Gaming : Electronica > Laptops > Laptop Gaming ";
            return await GetMapping(prompt, string.Join(", ", models));
        }
    }
}
