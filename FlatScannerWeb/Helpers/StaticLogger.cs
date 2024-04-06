using FlatScannerWeb.Entities;
using System.Collections.Concurrent;
using System.Text;

namespace FlatScannerWeb
{
    public class StaticLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new StaticLogger();
        }

        public void Dispose() { }
    }

    public class StaticLogger : ILogger
    {
        public static ConcurrentDictionary<DateTime, string> LogsData = new();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var date = DateTime.Now;
            RemoveOldLogs(date);

            var log = $"[{date.ToString("dd.MM.yy HH:mm:ss:fff")}] {formatter(state, exception)}";

            if (exception != null)
            {
                log += Environment.NewLine + "Exception:" + Environment.NewLine + exception;
            }

            LogsData.TryAdd(date, log);
        }

        private void RemoveOldLogs(DateTime currentTime)
        {
            foreach (var data in LogsData.Keys)
            {
                if ((currentTime - data) > TimeSpan.FromHours(Constants.OldLogsTimeoutHours))
                {
                    LogsData.Remove(data, out _);
                }
            }
        }

        public static string GetLogs()
        {
            var logs = new StringBuilder();
            foreach (var data in LogsData.OrderBy(p => p.Key))
            {
                logs.AppendLine(data.Value);
            }
            return logs.ToString();
        }
    }

    public class StaticLogData
    {
        public DateTime Timestamp { get; set; }
        public string Log { get; set; } = string.Empty;
    }

}