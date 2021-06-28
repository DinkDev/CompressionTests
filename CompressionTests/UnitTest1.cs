using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompressionTests
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ApprovalTests;
    using ApprovalTests.Reporters;
    using Microsoft.Extensions.Logging;

    [TestClass]
    [UseReporter(typeof(WinMergeReporter))]
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestMethod1()
        {
            var tokenSource = new CancellationTokenSource();
            var loggerFake = new LoggerFake(nameof(TestMethod1));
            Stubs.Stream.Logger = (ILogger)loggerFake;
            var sut = new CompleteServerStub(tokenSource.Token, () => tokenSource.Cancel());
            sut.Logger = loggerFake;
            sut.CurrentDateTime = () => new DateTime(2021, 6, 28, 01, 0, 0);

            await sut.ExecuteAsync();

            loggerFake.LogTrace("Test completed!");

            var output = loggerFake.LoggedData.ToString();

            Approvals.Verify(output);

        }
    }

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
