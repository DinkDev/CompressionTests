namespace CompressionTests
{
    using System;

    internal class CrcStub
    {
        public int Compute(byte[] packageBytes)
        {
            return new Random().Next(1000);
        }
    }
}