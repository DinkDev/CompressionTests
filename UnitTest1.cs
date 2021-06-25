using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompressionTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Stubs;

    [TestClass]
    public class UnitTest1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestMethod1()
        {
            var tokenSource = new CancellationTokenSource();
            Stubs.Stream.TestContext = TestContext;
            var sut = new CompleteServerStub(tokenSource.Token, () => tokenSource.Cancel());
            sut.Logger = new fm.Extensions.Logging.TestContext.TestContextLogger(nameof(TestMethod1), TestContext);
            
            await sut.ExecuteAsync();

            TestContext.WriteLine($"{nameof(UnitTest1)}.{nameof(TestMethod1)} completed!");
        }
    }
}
