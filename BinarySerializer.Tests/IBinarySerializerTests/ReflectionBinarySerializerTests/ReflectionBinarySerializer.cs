using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

using BinarySerializer.Tests.Extensions;
using BinarySerializer.Tests.TestStructures;

namespace BinarySerializer.Tests.IBinarySerializerTests.ReflectionBinarySerializerTests
{
    [TestClass]
    public class ReflectionBinarySerializer
    {
        [TestMethod]
        public void SerializeThrowSerializationException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.ThrowsException<BinarySerializer.SerializationException>(() => serializer.Serialize((IntPtr)10, ms));
            }
        }

        [TestMethod]
        public void SerializeThrowArraySerializationException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.ArraySerializationException>(() => serializer.Serialize(new IntPtr[] { (IntPtr)10 }, ms));
            }
        }

        [TestMethod]
        public void SerializeThrowStructFieldSerializationException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.FieldSerializationException>(() => serializer.Serialize(new UnsupportedStruct { A = (IntPtr)10 }, ms));
            }
        }

        [TestMethod]
        public void SerializeThrowClassFieldSerializationException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.FieldSerializationException>(() => serializer.Serialize(new UnsupportedClass { A = (IntPtr)10 }, ms));
            }
        }

        [TestMethod]
        public void SerializeThrowUnsupportedTypeException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.UnsupportedTypeException>(() => serializer.Serialize((IntPtr)10, ms));
            }
        }

        [TestMethod]
        public void SerializeThrowStreamArgumentNullException()
        {
            var serializer = new BinarySerializer.ReflectionBinarySerializer();
            Assert.ThrowsException<ArgumentNullException>(() => serializer.Serialize(10, null));
        }

        [TestMethod]
        public void SerializeThrowStreamArgumentException()
        {
            var serializer = new BinarySerializer.ReflectionBinarySerializer();
            Assert.ThrowsException<ArgumentException>(() => serializer.Serialize(10, new StubStreamNonReadWrite()));
        }

        [TestMethod]
        public void DeserializeThrowSerializationException()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.ThrowsException<BinarySerializer.SerializationException>(() => serializer.Deserialize<ExampleStruct>(ms));
            }
        }

        [TestMethod]
        public void DeserializeThrowArraySerializationException()
        {
            byte[] buffer = new byte[] {
                1,0,0,0, //objectId (1)
                4,0,0,0, //Array count (4)
                1,2 //Only two array values
            };
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.ArraySerializationException>(() => serializer.Deserialize<byte[]>(ms));
            }
        }

        [TestMethod]
        public void DeserializeThrowStructFieldSerializationException()
        {
            byte[] buffer = new byte[] {
                0,1,2,3,4,5,6,7,8
            };
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.FieldSerializationException>(() => serializer.Deserialize<UnsupportedStruct>(ms));
            }
        }

        [TestMethod]
        public void DeserializeThrowClassFieldSerializationException()
        {
            byte[] buffer = new byte[] {
                0,1,2,3,4,5,6,7,8
            };
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.FieldSerializationException>(() => serializer.Deserialize<UnsupportedClass>(ms));
            }
        }

        [TestMethod]
        public void DeserializeThrowUnsupportedTypeException()
        {
            byte[] buffer = new byte[] {
                0,1,2,3,4,5,6,7,8
            };
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                var serializer = new BinarySerializer.ReflectionBinarySerializer();
                Assert.That.ThrowsInnerException<BinarySerializer.UnsupportedTypeException>(() => serializer.Deserialize<IntPtr>(ms));
            }
        }

        [TestMethod]
        public void DeserializeThrowStreamArgumentNullException()
        {
            var serializer = new BinarySerializer.ReflectionBinarySerializer();
            Assert.ThrowsException<ArgumentNullException>(() => serializer.Serialize(10, null));
        }

        [TestMethod]
        public void DeserializeThrowStreamArgumentException()
        {
            var serializer = new BinarySerializer.ReflectionBinarySerializer();
            Assert.ThrowsException<ArgumentException>(() => serializer.Serialize(10, new StubStreamNonReadWrite()));
        }
    }
}
