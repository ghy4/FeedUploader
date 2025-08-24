using AIParser.AttributeUtils;
using AIParser.DataUtils.Suppliers;
using AIParser.MappingTools;


namespace AIParser.DataUtils
{
    public static class FeedStrategyFactory
    {
        private const string ERROR_MESSAGE_NO_MAP = "No saved maps";

        public static FeedStrategyResult DetectStrategy(List<string> keys, List<List<MapModel>> maps)
        {
            foreach (var map in maps)
            {
                if (IsFullyMapped(keys, map))
                    return new FeedStrategyResult
                    {
                        IsSuccessful = true,
                        Strategy = new DbMappedStrategy(map)
                    };

              
            }

            if (IsContaktFeed(keys))
                return new FeedStrategyResult
                {
                    Strategy = new ContaktStrategy(),
                    IsSuccessful = true
                };
            if (IsInterlinkFeed(keys))
                return new FeedStrategyResult
                {
                    IsSuccessful = true,
                    Strategy = new InterlinkStrategy()
                };
            return new FeedStrategyResult
            {
                IsSuccessful = false,
                ErrorMessage = ERROR_MESSAGE_NO_MAP
            };
        }

        private static bool IsFullyMapped(List<string> keys, List<MapModel> maps)
        {
            var mappedKeys = maps.Select(m => m.SourceField).ToHashSet(StringComparer.OrdinalIgnoreCase);
            bool allCovered = keys.All(k => mappedKeys.Contains(k));

            if (!allCovered) return false;

            var duplicates = maps
                .GroupBy(m => m.DestinationField)
                .Any(g => g.Count() > 1);

            return !duplicates;
        }

        private static bool IsContaktFeed(List<string> keys)
            => keys.Contains("Cod produs") && keys.Contains("Brand") && keys.Contains("Denumire produs");

        private static bool IsInterlinkFeed(List<string> keys)
            => keys.Contains("id") && keys.Contains("name") && keys.Contains("description");
    }
}