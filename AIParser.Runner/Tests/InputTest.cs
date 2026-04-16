using AIParser.DataUtils;
using AIParser.Internal_Tests;
using AIParser.PromptUtils;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedUploader.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.Runner.Tests
{
    internal class InputTest
    {
        public static void Run() {
            
            AIService service = new AIService("key");
            InternalFeedExtractor extractor = new InternalFeedExtractor(service,new InMemoryCategoryPromptConfigProvider());
 
            var map = new Dictionary<string, string> { { "part_number", "PartNumber" }, { "vendor_ext_id", "Id" }, { "name", "Name" }, { "description", "Description" }, { "brand", "Manufacturer" }, { "model", "Model" }, { "category", "Category" }, { "sale_price", "Price" }, { "offer_currency", "Currency" }, { "stock", "Quantity" }, { "warranty", "Warranty" }, { "main_image_url", "MainImage" }, { "other_image_url1", "AdditionalImage1" }, { "other_image_url2", "AdditionalImage2" }, { "other_image_url3", "AdditionalImage3" }, { "other_image_url4", "AdditionalImage4" } };
            var defaults = new Dictionary<string, string> { { "vat_rate", "0.2" }, { "status", "1" }, { "source_language", "RO_ro" } };


            var inputPath = "F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\husetest.csv";
            var interres = RawDataExtractor.ExtractFromCsv(inputPath);
            var res = extractor.ProcessRawFeedAsync(interres, new FeedUploader.Data.Models.User());
            List<Product> products = res.Result.Products;
            Console.WriteLine(products[0].PartNumber);

            InternalEmagExporter.ExportOnTemplate(
              products,
                "F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\sample.xlsx",
                "F:\\Visual Studio repo\\FeedUploader\\AIParser.Runner\\TestData\\output.xlsx",
              map,
              defaults
          );
        }
    }
}
