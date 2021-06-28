using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompressionTests
{
    using System;
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
        public async Task OriginalCompleteServerTest1()
        {
            var tokenSource = new CancellationTokenSource();
            var loggerFake = new LoggerFake(nameof(OriginalCompleteServerTest1));
            Stubs.Stream.Logger = (ILogger)loggerFake;
            var sut = new CompleteServerStub(tokenSource.Token, () => tokenSource.Cancel());
            sut.Logger = loggerFake;
            sut.CurrentDateTime = () => new DateTime(2021, 6, 28, 01, 0, 0);

            await sut.ExecuteAsync();

            loggerFake.LogTrace("Test completed!");

            var output = loggerFake.LoggedData.ToString();

            Approvals.Verify(output);
        }

        [TestMethod]
        public async Task RefactoredCompleteServerTest1()
        {
            var tokenSource = new CancellationTokenSource();
            var loggerFake = new LoggerFake(nameof(OriginalCompleteServerTest1));
            Stubs.Stream.Logger = (ILogger)loggerFake;
            var sut = new RefactoredCompleteServerStub(tokenSource.Token, () => tokenSource.Cancel());
            sut.Logger = loggerFake;
            sut.CurrentDateTime = () => new DateTime(2021, 6, 28, 02, 0, 0);

            await sut.ExecuteAsync();

            loggerFake.LogTrace("Test completed!");

            var output = loggerFake.LoggedData.ToString();

            Approvals.Verify(output);
        }
    }
}
