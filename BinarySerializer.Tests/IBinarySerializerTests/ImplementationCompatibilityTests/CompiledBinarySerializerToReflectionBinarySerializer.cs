using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

namespace BinarySerializer.Tests.IBinarySerializerTests.ImplementationCompatibilityTests
{
    [TestClass]
    public class CompiledBinarySerializerToReflectionBinarySerializer : BinarySerializerRountTripTestsBase
    {
        protected override void AssertRoundTrip<T>(T expected)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                var compiledBinarySerializer = new CompiledBinarySerializer();
                var reflectionBinarySerializer = new ReflectionBinarySerializer();

                compiledBinarySerializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = reflectionBinarySerializer.Deserialize<T>(ms);
                Assert.AreEqual(expected, actual);
            }
        }

        protected override void AssertRoundTrip<T>(T expected, Func<T, T, bool> areEqual)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var compiledBinarySerializer = new CompiledBinarySerializer();
                var reflectionBinarySerializer = new ReflectionBinarySerializer();

                compiledBinarySerializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = reflectionBinarySerializer.Deserialize<T>(ms);

                Assert.IsTrue(areEqual(expected, actual));
            }
        }
    }
}
