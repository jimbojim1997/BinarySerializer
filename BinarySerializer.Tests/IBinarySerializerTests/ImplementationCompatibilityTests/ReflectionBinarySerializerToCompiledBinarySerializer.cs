using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

namespace BinarySerializer.Tests.IBinarySerializerTests.ImplementationCompatibilityTests
{
    [TestClass]
    public class ReflectionBinarySerializerToCompiledBinarySerializer : BinarySerializerRountTripTestsBase
    {
        protected override void AssertRoundTrip<T>(T expected)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                var compiledBinarySerializer = new CompiledBinarySerializer();
                var reflectionBinarySerializer = new ReflectionBinarySerializer();

                reflectionBinarySerializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = compiledBinarySerializer.Deserialize<T>(ms);
                Assert.AreEqual(expected, actual);
            }
        }

        protected override void AssertRoundTrip<T>(T expected, Func<T, T, bool> areEqual)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var compiledBinarySerializer = new CompiledBinarySerializer();
                var reflectionBinarySerializer = new ReflectionBinarySerializer();

                reflectionBinarySerializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = compiledBinarySerializer.Deserialize<T>(ms);

                Assert.IsTrue(areEqual(expected, actual));
            }
        }
    }
}
