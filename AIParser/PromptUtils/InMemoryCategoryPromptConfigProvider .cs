using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attribute = FeedUploader.Data.Models.Attribute;

namespace AIParser.PromptUtils
{
    public class InMemoryCategoryPromptConfigProvider : ICategoryPromptConfigProvider
    {
        private readonly List<PromptCategoryConfig> _configs;

        public InMemoryCategoryPromptConfigProvider()
        {
            _configs = new List<PromptCategoryConfig>
        {
            new PromptCategoryConfig
            {
                InternalCategory = "Huse Smartphone",
                PromptTemplate = "You have the following feed line(s) in Romanian: \r\n{rowsText}\r\n\r\nFeed category (original): \"\"{internalCategory}\"\"\r\n\r\nTask:\r\nReturn a VALID JSON array of objects, each object must match the Product model described below, all text should be on Romanian(tranlate if needed)\r\nDo NOT return any explanatory text — only JSON.\r\n\r\nProduct fields:\r\n{{\r\n  \"\"RowIndex\"\": integer,\r\n  \"\"Name\"\": string,\r\n  \"\"Description\"\": string,\r\n  \"\"Model\"\": string,\r\n  \"\"Manufacturer\"\": string,\r\n  \"\"Category\"\": string,\r\n  \"\"Price\"\": number,\r\n  \"\"SalePrice\"\": number,\r\n  \"\"Currency\"\": string,\r\n  \"\"Quantity\"\": integer,\r\n  \"\"Warranty\"\": integer|null,\r\n  \"\"MainImage\"\": string,\r\n  \"\"AdditionalImage1\"\": string,\r\n  \"\"AdditionalImage2\"\": string,\r\n  \"\"AdditionalImage3\"\": string,\r\n  \"\"AdditionalImage4\"\": string,\r\n  \"\"Type\"\": string,\r\n  \"\"Attributes\"\": [ {{ \"\"Name\"\": \"\"Color\"\", \"\"Value\"\": \"\"Red\"\" }} ],\r\n  \"\"MatchStatus\"\": \"\"ok\"\" | \"\"error\"\",\r\n  \"\"ErrorReason\"\": string|null\r\n}}\r\n\r\nConstraints:\r\n- Required attributes for each product: {req}\r\n- Other attributes (optional, but strongly recommended to complete): {other}\r\n- If a product cannot match required attributes,try put exact estimated value(for model IPhone 15, Brand is Apple), or set \"\"MatchStatus\"\":\"\"error\"\" and provide \"\"ErrorReason\"\".\r\n- Use numbers for Price/SalePrice(1 required, cannot be 0, return error if missing) and integers for Quantity/Warranty (or null).Return same number of objects, even with error. \r\n\";",
                Attributes = new List<Attribute>
                {
                   new Attribute {
                Name = "Material",
                Code = "6372",
                IsRequired = true,
             //   IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Brand compatibil",
                Code = "8927",
                IsRequired = true,
             //   IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Model compatibil",
                Code = "8928",
                IsRequired = true,
              //  IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Tip",
                Code = "8930",
                IsRequired = true,
               // IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Culoare",
                Code = "5401",
                IsRequired = false,
               // IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Functii",
                Code = "7235",
                IsRequired = false,
              //  IsRestrictive = true,
                Unit = null
            },
            new Attribute {
                Name = "Continut pachet",
                Code = "6556",
                IsRequired = false,
              //  IsRestrictive = false,
                Unit = null
            },
                }
            }
           
        };
        }

        public Task<List<PromptCategoryConfig>> GetAllAsync() =>
            Task.FromResult(_configs);

        
    }


}
