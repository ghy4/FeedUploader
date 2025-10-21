using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using ClosedXML.Excel;

namespace FeedUploader.Server.Services.Adapters
{
    public class AiParserEmagExportAdapter : IExportAdapter
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public AiParserEmagExportAdapter(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task<byte[]> ExportAsync(
            IEnumerable<Product> products,
            IDictionary<string, string> columnToPropertyMap,
            IDictionary<string, string> defaultValues,
            CancellationToken ct = default)
        {
            // Resolve template path from configuration
            // Set in appsettings.json:  "Export": { "EmagTemplatePath": "C:\\path\\to\\emag_template.xlsx" }
            var template = _config["Export:EmagTemplatePath"];
            if (!string.IsNullOrWhiteSpace(template) && !Path.IsPathRooted(template))
            {
                template = Path.Combine(_env.ContentRootPath, template);
            }

            // Use InternalEmagExporter which writes to disk; create a temp output file and return its bytes
            var tempDir = Path.Combine(Path.GetTempPath(), "feeduploader");
            Directory.CreateDirectory(tempDir);
            var output = Path.Combine(tempDir, $"export_{System.Guid.NewGuid():N}.xlsx");

            // If template is missing, generate a minimal one on the fly (sheet "Template", codes on row 3)
            string? dynamicTemplate = null;
            if (string.IsNullOrWhiteSpace(template) || !File.Exists(template))
            {
                dynamicTemplate = Path.Combine(tempDir, $"emag_template_{System.Guid.NewGuid():N}.xlsx");
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.AddWorksheet("Template");
                    int c = 1;
                    foreach (var code in columnToPropertyMap.Keys)
                        ws.Cell(3, c++).Value = code;
                    foreach (var code in defaultValues.Keys)
                        ws.Cell(3, c++).Value = code;
                    wb.SaveAs(dynamicTemplate);
                }
                template = dynamicTemplate;
            }

            InternalEmagExporter.ExportOnTemplate(
                products.ToList(),
                template,
                output,
                new Dictionary<string, string>(columnToPropertyMap),
                new Dictionary<string, string>(defaultValues));

            try
            {
                return await File.ReadAllBytesAsync(output, ct);
            }
            finally
            {
                try { File.Delete(output); } catch { }
                if (dynamicTemplate != null)
                {
                    try { File.Delete(dynamicTemplate); } catch { }
                }
            }
        }
    }
}
