using Microsoft.Extensions.Options;
using System.Text;

namespace student_app.Logging
{
    public class RollingTextFileLogger
    {
        private const long BytesPerMegabyte = 1024 * 1024;
        private readonly ActionCallFileLoggingOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private string? _currentFilePath;

        public RollingTextFileLogger(
            IOptions<ActionCallFileLoggingOptions> options,
            IWebHostEnvironment environment)
        {
            _options = options.Value;
            _environment = environment;
        }

        public async Task WriteLineAsync(string message)
        {
            if (!_options.Enabled)
            {
                return;
            }

            await _writeLock.WaitAsync();
            try
            {
                var logFilePath = GetWritableLogFilePath();
                await File.AppendAllTextAsync(logFilePath, message + Environment.NewLine, Encoding.UTF8);
                DeleteOldLogFiles();
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private string GetWritableLogFilePath()
        {
            var logDirectory = GetLogDirectory();
            Directory.CreateDirectory(logDirectory);

            if (_currentFilePath == null || IsFileAtSizeLimit(_currentFilePath))
            {
                _currentFilePath = GetLatestWritableFile(logDirectory) ?? CreateNewLogFilePath(logDirectory);
            }

            return _currentFilePath;
        }

        private string GetLogDirectory()
        {
            if (Path.IsPathRooted(_options.Directory))
            {
                return _options.Directory;
            }

            return Path.Combine(_environment.ContentRootPath, _options.Directory);
        }

        private string? GetLatestWritableFile(string logDirectory)
        {
            var latestFile = Directory
                .GetFiles(logDirectory, $"{_options.FileNamePrefix}-*.txt")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault();

            if (latestFile == null || latestFile.Length >= GetMaxFileSizeBytes())
            {
                return null;
            }

            return latestFile.FullName;
        }

        private string CreateNewLogFilePath(string logDirectory)
        {
            var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff");
            return Path.Combine(logDirectory, $"{_options.FileNamePrefix}-{timestamp}.txt");
        }

        private bool IsFileAtSizeLimit(string filePath)
        {
            return File.Exists(filePath) && new FileInfo(filePath).Length >= GetMaxFileSizeBytes();
        }

        private long GetMaxFileSizeBytes()
        {
            var maxFileSizeMb = Math.Max(1, _options.MaxFileSizeMb);
            return maxFileSizeMb * BytesPerMegabyte;
        }

        private void DeleteOldLogFiles()
        {
            var maxFileCount = Math.Max(1, _options.MaxFileCount);
            var logDirectory = GetLogDirectory();

            var filesToDelete = Directory
                .GetFiles(logDirectory, $"{_options.FileNamePrefix}-*.txt")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Skip(maxFileCount)
                .ToArray();

            foreach (var file in filesToDelete)
            {
                file.Delete();
            }
        }
    }
}
