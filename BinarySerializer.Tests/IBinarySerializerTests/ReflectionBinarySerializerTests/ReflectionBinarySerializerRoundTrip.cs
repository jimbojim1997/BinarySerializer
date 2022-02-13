using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

namespace BinarySerializer.Tests.IBinarySerializerTests.ReflectionBinarySerializerTests
{
    [TestClass]
    public class ReflectionBinarySerializerRoundTrip : BinarySerializerRountTripTestsBase
    {
        protected override void AssertRoundTrip<T>(T expected)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                serializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = serializer.Deserialize<T>(ms);

                Assert.AreEqual(expected, actual);
            }
        }

        protected override void AssertRoundTrip<T>(T expected, Func<T, T, bool> areEqual)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                serializer.Serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = serializer.Deserialize<T>(ms);

                Assert.IsTrue(areEqual(expected, actual));
            }
        }
    }
}
