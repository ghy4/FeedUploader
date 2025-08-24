using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedUploader.Data.Models;
using System.Globalization;
using AIParser.AttributeUtils;
using AIParser.DataUtils.Suppliers;
using AIParser.MappingTools;

namespace AIParser.DataUtils
{
    public static class FeedExtractor
    {
        public static List<Product>? ExtractProducts(RawFeedData rawData, List<List<MapModel>> maps, User user)
        {
            if (rawData == null || rawData.Headers.Count == 0 || rawData.Rows.Count == 0)
            {
                throw new ArgumentException("Raw feed data is empty or invalid.");
            }
            var keys = rawData.Headers;
            var result = FeedStrategyFactory.DetectStrategy(keys, maps);
            if (result.IsSuccessful)
            {
                var products = rawData.Rows.Select(row => result.Strategy.Parse(row)).ToList();
                foreach (var p in products)
                {
                    p.User = user;
                    p.UserId = user.Id;
                    p.Id = GetId();
                }
                return products;
            }
            else
            {
                //Start manual mapping
                return null;
            }
           
        }
        
        public static int GetId()
        {
            return Random.Shared.Next();
        }
    }
}

