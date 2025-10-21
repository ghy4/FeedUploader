using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;

namespace FeedUploader.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _logDir;

        public LogsController(IWebHostEnvironment env)
        {
            _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(_logDir);
        }

        [HttpGet("files")]
        public ActionResult<IEnumerable<object>> GetFiles()
        {
            var files = Directory.EnumerateFiles(_logDir, "log_*.log")
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Select(f => new { name = f.Name, size = f.Length, lastWriteUtc = f.LastWriteTimeUtc });
            return Ok(files);
        }

        [HttpGet("recent")]
        public ActionResult<IEnumerable<string>> GetRecent([FromQuery] int maxLines = 500)
        {
            var today = Path.Combine(_logDir, $"log_{DateTime.UtcNow:yyyyMMdd}.log");
            if (!System.IO.File.Exists(today)) return Ok(Array.Empty<string>());
            var lines = Tail(today, maxLines);
            return Ok(lines);
        }

        [HttpGet("download")]
        public IActionResult Download([FromQuery] string? date = null)
        {
            var fileName = date == null ? $"log_{DateTime.UtcNow:yyyyMMdd}.log" : $"log_{date}.log";
            var path = Path.Combine(_logDir, fileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "text/plain", fileName);
        }

        [HttpGet("download-all")]
        public IActionResult DownloadAll()
        {
            var files = Directory.EnumerateFiles(_logDir, "log_*.log").ToList();
            if (files.Count == 0)
            {
                return BadRequest("No logs available");
            }

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = zip.CreateEntry(Path.GetFileName(file), CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fs = System.IO.File.OpenRead(file);
                    fs.CopyTo(entryStream);
                }
            }
            ms.Position = 0;
            var zipName = $"logs_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
            return File(ms.ToArray(), "application/zip", zipName);
        }

        [HttpDelete("clear")]
        public IActionResult Clear()
        {
            foreach (var f in Directory.EnumerateFiles(_logDir, "log_*.log"))
            {
                try { System.IO.File.Delete(f); } catch { }
            }
            return NoContent();
        }

        private static IEnumerable<string> Tail(string file, int maxLines)
        {
            var lines = new LinkedList<string>();
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                lines.AddLast(line);
                if (lines.Count > maxLines) lines.RemoveFirst();
            }
            return lines.ToArray();
        }
    }
}
