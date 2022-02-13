using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;

using BinarySerializer;

namespace Tests.SerializationHelpersTests
{
    [TestClass]
    public class SerializationHelpersRoundTrip
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(254)]
        [DataRow(255)]
        public void ByteRoundTrip(int value)
        {
            AssertRoundTrip((byte)value, SerializationHelpers.SerializeByte, SerializationHelpers.DeserializeByte);
        }

        [DataTestMethod]
        [DataRow(-128)]
        [DataRow(-127)]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(126)]
        [DataRow(127)]
        public void SByteRoundTrip(int value)
        {
            AssertRoundTrip((sbyte)value, SerializationHelpers.SerializeSByte, SerializationHelpers.DeserializeSByte);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void BooleanRoundTrip(bool value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeBoolean, SerializationHelpers.DeserializeBoolean);
        }

        [DataTestMethod]
        [DataRow(-32768)]
        [DataRow(-32767)]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(32766)]
        [DataRow(32767)]
        public void ShortRoundTrip(int value)
        {
            AssertRoundTrip((short)value, SerializationHelpers.SerializeShort, SerializationHelpers.DeserializeShort);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(65534)]
        [DataRow(65535)]
        public void UShortRoundTrip(int value)
        {
            AssertRoundTrip((ushort)value, SerializationHelpers.SerializeUShort, SerializationHelpers.DeserializeUShort);
        }

        [DataTestMethod]
        [DataRow((char)0)]
        [DataRow((char)1)]
        [DataRow('a')]
        [DataRow('Z')]
        [DataRow((char)65534)]
        [DataRow((char)65535)]
        public void CharRoundTrip(char value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeChar, SerializationHelpers.DeserializeChar);
        }

        [DataTestMethod]
        [DataRow(-2147483648)]
        [DataRow(-2147483647)]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2147483646)]
        [DataRow(2147483647)]
        public void IntRoundTrip(int value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeInt, SerializationHelpers.DeserializeInt);
        }

        [DataTestMethod]
        [DataRow(0U)]
        [DataRow(1U)]
        [DataRow(4294967294U)]
        [DataRow(4294967295U)]
        public void UIntRoundTrip(uint value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeUInt, SerializationHelpers.DeserializeUInt);
        }

        [DataTestMethod]
        [DataRow(-2147483648f)]
        [DataRow(-2147483647f)]
        [DataRow(-12.345f)]
        [DataRow(-1f)]
        [DataRow(0f)]
        [DataRow(1f)]
        [DataRow(12.345f)]
        [DataRow(2147483646f)]
        [DataRow(2147483647f)]
        public void FloatRoundTrip(float value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeFloat, SerializationHelpers.DeserializeFloat);
        }

        [DataTestMethod]
        [DataRow(-9223372036854775808L)]
        [DataRow(-9223372036854775807L)]
        [DataRow(-1L)]
        [DataRow(0L)]
        [DataRow(1L)]
        [DataRow(9223372036854775806L)]
        [DataRow(9223372036854775807L)]
        public void LongRoundTrip(long value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeLong, SerializationHelpers.DeserializeLong);
        }

        [DataTestMethod]
        [DataRow(0UL)]
        [DataRow(1UL)]
        [DataRow(18446744073709551614UL)]
        [DataRow(18446744073709551615UL)]
        public void ULongRoundTrip(ulong value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeULong, SerializationHelpers.DeserializeULong);
        }

        [DataTestMethod]
        [DataRow(-9223372036854775808D)]
        [DataRow(-9223372036854775807D)]
        [DataRow(-12.345D)]
        [DataRow(-1D)]
        [DataRow(0D)]
        [DataRow(1D)]
        [DataRow(12.345D)]
        [DataRow(9223372036854775806D)]
        [DataRow(9223372036854775807D)]
        public void DoubleRoundTrip(double value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeDouble, SerializationHelpers.DeserializeDouble);
        }

        [DataTestMethod]
        [DataRow(-9223372036854775808D)]
        [DataRow(-9223372036854775807D)]
        [DataRow(-12.345D)]
        [DataRow(-1D)]
        [DataRow(0D)]
        [DataRow(1D)]
        [DataRow(12.345D)]
        [DataRow(9223372036854775806D)]
        [DataRow(9223372036854775807D)]
        public void DecimalRoundTrip(double value)
        {
            AssertRoundTrip((decimal)value, SerializationHelpers.SerializeDecimal, SerializationHelpers.DeserializeDecimal);
        }

        [DataTestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("Test Text")]
        public void StringRoundTrip(string value)
        {
            AssertRoundTrip(value, SerializationHelpers.SerializeString, SerializationHelpers.DeserializeString);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(65534)]
        [DataRow(65535)]
        public void ObjectIdRoundTrip(int value)
        {
            AssertRoundTrip((ushort)value, SerializationHelpers.SerializeObjectId, SerializationHelpers.DeserializeObjectId);
        }

        private void AssertRoundTrip<T>(T expected, Action<T, Stream> serialize, Func<Stream, T> deserialize)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                serialize(expected, ms);
                ms.Seek(0, SeekOrigin.Begin);
                T actual = deserialize(ms);

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
