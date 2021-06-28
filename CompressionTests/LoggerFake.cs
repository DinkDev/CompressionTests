namespace CompressionTests
{
    using System;
    using System.Text;
    using Microsoft.Extensions.Logging;

    public class LoggerFake : ILogger
    {
        private readonly string _logEntryHeader;

        public LoggerFake(string logEntryHeader)
        {
            _logEntryHeader = string.IsNullOrEmpty(logEntryHeader) ? string.Empty : $"{logEntryHeader}: ";

            LoggedData = new StringBuilder();
        }

        public StringBuilder LoggedData { get; set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggedData.AppendLine($"{_logEntryHeader}{logLevel}: {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}