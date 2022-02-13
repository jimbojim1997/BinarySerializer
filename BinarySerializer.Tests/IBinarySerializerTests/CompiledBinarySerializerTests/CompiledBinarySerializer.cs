using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinarySerializer.Tests.IBinarySerializerTests.ReflectionBinarySerializerTests
{
    [TestClass]
    public class CompiledBinarySerializer : BinarySerializerTestsBase
    {
        protected override IBinarySerializer GetImplementation()
        {
            return new BinarySerializer.CompiledBinarySerializer();
        }
    }
}
