using AIParser.AttributeUtils.Suppliers;

namespace AIParser.AttributeUtils;

public static class FeedAttributeParserFactory
{
    public static IFeedAttributeParser GetParser(string supplierName)
    {
        return supplierName switch
        {
            "Contakt"  => new ContaktAttributeParser(),
            "Interlink"=> new InterlinkAttributeParser(),
            _          => throw new NotSupportedException($"No parser for supplier {supplierName}")
        };
    }

    public static IFeedAttributeParser GetParser(List<string> keys)
    {
        if (IsInterlinkFeed(keys))
            return new InterlinkAttributeParser();
        else if (IsContaktFeed(keys))
            return new ContaktAttributeParser();
        
        
    }
    
    private static bool IsContaktFeed(List<string> keys)
    {
        return keys.Contains("Cod produs") && keys.Contains("Brand") && keys.Contains("Denumire produs");
    }

    private static bool IsInterlinkFeed(List<string> keys)
    {
        return keys.Contains("id") && keys.Contains("name") && keys.Contains("description");
    }

}