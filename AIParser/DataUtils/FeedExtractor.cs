using AIParser.DataUtils;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIParser
{


    public class FeedExtractor
    {

        private readonly AIService _aiService;
        private readonly UserService userService;
        private readonly ProductService productService;
        private readonly FeedBatchScheduler _batchScheduler;
        private readonly AttributeService attributeService;
        public FeedExtractor(AIService aiService, UserService userService, ProductService productService, AttributeService attributeService)
        {
            _aiService = aiService;
            _batchScheduler = new FeedBatchScheduler(2000); 
            this.userService = userService;
            this.productService = productService;
            this.attributeService = attributeService;
        }

        public async Task<List<Product>> ProcessRawFeedAsync(RawFeedData rawData, int userId)
        {
         /*   var user = await userService.GetById(userId);
            if (user == null) throw new ArgumentException("User not found");

            var products = new List<Product>();

            foreach (var batch in _batchScheduler.SplitIntoBatches(rawData))
            {
                try
                {
                    // AI получает сразу headers + batch строк
                    var productJsonArray = await _aiService.NormalizeProductsBatchAsync(rawData.Headers, batch);

                    
                   
                    products.AddRange(batchProducts);
                }
                catch (Exception ex)
                {
                    // если весь батч не удался → можно fallback по одной строке
                    Console.WriteLine($"Batch processing failed: {ex.Message}");
                    continue;
                }
            }

            if (products.Any())
            {
                await _dbContext.Products.AddRangeAsync(products);
                await _dbContext.SaveChangesAsync();
            }
         */   var products = new List<Product>();
            return products;
        }
    }
}

