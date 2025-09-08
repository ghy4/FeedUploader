
using Attribute = FeedUploader.Data.Models.Attribute;
namespace AIParser.PromptUtils
{
    public class PromptCategoryConfig
    {

        public string InternalCategory { get; set; } = string.Empty;

       
        public string PromptTemplate { get; set; } = string.Empty;

        public readonly string Marketplace = "eMAG";

        public Dictionary<string, string> FieldMappings { get; set; } = new();

        public List<Attribute> Attributes { get; set; } = new();
    }

}
