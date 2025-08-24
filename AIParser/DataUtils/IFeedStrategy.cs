using FeedUploader.Data.Models;

namespace AIParser.DataUtils;

public interface IFeedStrategy
{
    Product Parse(List<string> values);
}