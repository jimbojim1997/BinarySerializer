using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinarySerializer.Tests.IBinarySerializerTests.ReflectionBinarySerializerTests
{
    [TestClass]
    public class ReflectionBinarySerializer : BinarySerializerTestsBase
    {
        protected override IBinarySerializer GetImplementation()
        {
            return new BinarySerializer.ReflectionBinarySerializer();
        }
    }
}
