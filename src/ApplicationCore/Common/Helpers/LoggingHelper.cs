using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationCore.Common.Helpers
{
    public class LoggingOptions
    {
        public string ServiceUrl { get; set; }
    }

    public class LoggingContext
    {
        public int ConfigurationId { get; set; }
        public string Subprocess { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public interface ILoggingHelper
    {
        void SetContext(LoggingContext context);
        void Information(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null);
        void Warning(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null);
        void Error(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null);
        void Exception(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null);
        void Commit();
    }

    public class LoggingHelper : ILoggingHelper
    {
        private readonly List<LogEntry> _logEntries = [];
        private readonly string _logServiceUrl;
        private readonly HttpClient httpClient;
        private readonly ILogger<LoggingHelper> logger;
        private LoggingContext _context;

        const string ServiceLog = "/AddEntry";
        const string ServiceLogList = "/AddEntries";

        public LoggingHelper(HttpClient httpClient, IOptions<LoggingOptions> options, ILogger<LoggingHelper> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            _logServiceUrl = options.Value.ServiceUrl;
        }

        public void SetContext(LoggingContext context)
        {
            _context = context;
        }

        public void Information(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null)
        {
            Log(message, "Information", actionType, addEntryToList, storeName, tillName);
        }

        public void Warning(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null)
        {
            Log(message, "Warning", actionType, addEntryToList, storeName, tillName);
        }

        public void Error(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null)
        {
            Log(message, "Error", actionType, addEntryToList, storeName, tillName);
        }

        public void Exception(string message, string actionType, bool addEntryToList = false, string storeName = null, string tillName = null)
        {
            Log(message, "Critical", actionType, addEntryToList, storeName, tillName);
        }

        private void Log(string message, string level, string actionType, bool addEntryToList, string storeName, string tillName)
        {
            var logEntry = new LogEntry
            {
                Message = message,
                Level = level,
                StoreName = storeName,
                TillName = tillName,
                ActionType = actionType,
                ConfigurationId = _context.ConfigurationId,
                Subprocess = _context.Subprocess,
                UserId = _context.UserId,
                UserName = _context.UserName,
                RoleId = 0
            };

            if (addEntryToList)
            {
                _logEntries.Add(logEntry);
            }
            else
            {
                LogAsync(logEntry).ConfigureAwait(false);
            }
        }

        public void Commit()
        {
            if (_logEntries.Count > 0)
            {
                LogAsync(_logEntries).ConfigureAwait(false);
                _logEntries.Clear();
            }
        }

        private async Task LogAsync(LogEntry logEntry)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(_logServiceUrl + ServiceLog, logEntry);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                logger.LogError($"Logging failed: {ex.Message}");
            }
        }

        private async Task LogAsync(List<LogEntry> logEntries)
        {
            try
            {
                var logEntriesCopy = JsonSerializer.Deserialize<List<LogEntry>>(JsonSerializer.Serialize(logEntries));
                var response = await httpClient.PostAsJsonAsync(_logServiceUrl + ServiceLogList, logEntriesCopy);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                logger.LogError($"Logging failed: {ex.Message}");
            }
        }
    }

    public class LogEntry
    {
        public string Message { get; set; }
        public string Level { get; set; }
        public string StoreName { get; set; }
        public string TillName { get; set; }
        public string ActionType { get; set; }
        public int ConfigurationId { get; set; }
        public string Subprocess { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
    }
}
