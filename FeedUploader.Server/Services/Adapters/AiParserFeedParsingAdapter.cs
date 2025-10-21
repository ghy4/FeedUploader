using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIParser;
using AIParser.DataUtils;
using AIParser.Internal_Tests;
using AIParser.PromptUtils;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FeedUploader.Server.Services.Adapters
{
    public class AiParserFeedParsingAdapter : IFeedParsingAdapter
    {
        private readonly IConfiguration _config;
        private readonly MyDbContext _db;

        public AiParserFeedParsingAdapter(IConfiguration config, MyDbContext db)
        {
            _config = config;
            _db = db;
        }

        public async Task<List<Product>> ParseAsync(Stream feedStream, int userId, CancellationToken ct = default)
        {
            // Persist stream temporarily to detect format and reuse RawDataExtractor
            var tempDir = Path.Combine(Path.GetTempPath(), "feeduploader");
            Directory.CreateDirectory(tempDir);
            var tmpPath = Path.Combine(tempDir, $"upload_{userId}_{System.Guid.NewGuid():N}");

            await using (var fs = File.Create(tmpPath))
            {
                await feedStream.CopyToAsync(fs, ct);
            }

            // Detect format by content (XLSX ZIP magic) or XML first char, else CSV
            RawFeedData raw;
            using (var check = File.OpenRead(tmpPath))
            {
                var header = new byte[4];
                _ = await check.ReadAsync(header, 0, header.Length, ct);
                check.Position = 0;
                var firstChar = (char)header[0];

                if (header[0] == 0x50 && header[1] == 0x4B) // PK => ZIP => XLSX
                {
                    raw = RawDataExtractor.ExtractFromXlsx(tmpPath);
                }
                else if (firstChar == '<')
                {
                    raw = RawDataExtractor.ExtractFromXml(tmpPath);
                }
                else
                {
                    raw = RawDataExtractor.ExtractFromCsv(tmpPath);
                }
            }

            try
            {
                var apiKey = _config["OpenAI:ApiKey"] ?? string.Empty;
                var ai = new AIService(apiKey);
                var cfg = new InMemoryCategoryPromptConfigProvider();
                var extractor = new InternalFeedExtractor(ai, cfg);

                var user = await _db.Users.FirstAsync(u => u.Id == userId, ct);
                var result = await extractor.ProcessRawFeedAsync(raw, user);
                return result.Products;
            }
            finally
            {
                try { File.Delete(tmpPath); } catch { /* ignore */ }
            }
        }
    }
}
