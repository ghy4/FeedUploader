namespace AIParser.MappingTools;

public interface IMapperHelper
{
    public Task<MapModel> FeedMap(List<string> sources, string model);

    public Task<MapModel> MarketMap(List<string> marketFields, string model);

    public Task<MapModel> MapMarket(List<string> modelFields, string market);
}