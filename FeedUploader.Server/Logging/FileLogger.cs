using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace FeedUploader.Server.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
        private readonly object _sync = new();

        public FileLoggerProvider(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(_logDirectory, name));
        }

        public void Dispose()
        {
        }

        private class FileLogger : ILogger
        {
            private readonly string _logDirectory;
            private readonly string _category;

            public FileLogger(string logDirectory, string category)
            {
                _logDirectory = logDirectory;
                _category = category;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                var filePath = Path.Combine(_logDirectory, $"log_{DateTime.UtcNow:yyyyMMdd}.log");
                var line = $"{DateTime.UtcNow:O}\t{logLevel}\t{_category}\t{eventId.Id}\t{formatter(state, exception)}";
                if (exception != null)
                {
                    line += $"\t{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
                }
                try
                {
                    lock (filePath)
                    {
                        File.AppendAllText(filePath, line + Environment.NewLine);
                    }
                }
                catch
                {
                    // swallow logging errors
                }
            }
        }
    }
}
