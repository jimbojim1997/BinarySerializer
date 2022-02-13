using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

using BinarySerializer;

namespace Tests.SerializationHelpersTests
{
    [TestClass]
    public class SerializationEndOfStreamException
    {
        [TestMethod]
        public void ByteEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeByte);

        [TestMethod]
        public void SByteEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeSByte);

        [TestMethod]
        public void BooleanEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeBoolean);

        [TestMethod]
        public void ShortEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeShort);

        [TestMethod]
        public void UShortEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeUShort);

        [TestMethod]
        public void CharEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeChar);

        [TestMethod]
        public void IntEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeInt);

        [TestMethod]
        public void UIntEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeUInt);

        [TestMethod]
        public void FloatEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeFloat);

        [TestMethod]
        public void LongEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeLong);

        [TestMethod]
        public void ULongEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeULong);

        [TestMethod]
        public void DoubleEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeDouble);

        [TestMethod]
        public void DecimalEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeDecimal);

        [TestMethod]
        public void StringEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeString);

        [TestMethod]
        public void ObjectIdEndOfStreamException() => AssertEndOfStreamException(SerializationHelpers.DeserializeUInt);

        private void AssertEndOfStreamException<T>(Func<Stream, T> deserialize)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                Assert.ThrowsException<EndOfStreamException>(() => deserialize(ms));
            }
        }
    }
}
