namespace AIParser.DataUtils;

public class FeedStrategyResult
{
    public bool IsSuccessful { get; set; }
    public IFeedStrategy? Strategy { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? MissingMappings { get; set; }
}