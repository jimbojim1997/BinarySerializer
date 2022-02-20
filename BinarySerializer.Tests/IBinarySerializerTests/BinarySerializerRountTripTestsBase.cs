using BinarySerializer.Tests.TestStructures;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;

namespace BinarySerializer.Tests.IBinarySerializerTests
{
    public abstract class BinarySerializerRountTripTestsBase
    {
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(254)]
        [DataRow(255)]
        public void ByteRoundTrip(int value)
        {
            AssertRoundTrip((byte)value);
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
            AssertRoundTrip((sbyte)value);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void BooleanRoundTrip(bool value)
        {
            AssertRoundTrip(value);
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
            AssertRoundTrip((short)value);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(65534)]
        [DataRow(65535)]
        public void UShortRoundTrip(int value)
        {
            AssertRoundTrip((ushort)value);
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
            AssertRoundTrip(value);
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
            AssertRoundTrip(value);
        }

        [DataTestMethod]
        [DataRow(0U)]
        [DataRow(1U)]
        [DataRow(4294967294U)]
        [DataRow(4294967295U)]
        public void UIntRoundTrip(uint value)
        {
            AssertRoundTrip(value);
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
            AssertRoundTrip(value);
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
            AssertRoundTrip(value);
        }

        [DataTestMethod]
        [DataRow(0UL)]
        [DataRow(1UL)]
        [DataRow(18446744073709551614UL)]
        [DataRow(18446744073709551615UL)]
        public void ULongRoundTrip(ulong value)
        {
            AssertRoundTrip(value);
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
            AssertRoundTrip(value);
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
            AssertRoundTrip((decimal)value);
        }

        [DataTestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("Test Text")]
        public void StringRoundTrip(string value)
        {
            AssertRoundTrip(value);
        }

        [TestMethod]
        public void StructRoundTrip()
        {
            AssertRoundTrip(new ExampleStruct()
            {
                A = 123,
                B = "Test Text",
                D = 456.789M
            }, (a, b) => a.A == b.A && a.B == b.B);
        }

        [TestMethod]
        public void ClassRoundTrip()
        {
            ExampleClass e = new ExampleClass()
            {
                A = 123,
                B = "Test Text",
                D = 123.456M
            };
            e.C = e;
            AssertRoundTrip(e, (a, b) => a.A == b.A && a.B == b.B && b == b.C && b == b.C.C);
        }

        [TestMethod]
        public void NullRoundTrip()
        {
            AssertRoundTrip((object)null);
        }

        [TestMethod]
        public void NullArrayRoundTrip()
        {
            AssertRoundTrip((int[])null);
        }

        [TestMethod]
        public void EmptyArrayRoundTrip()
        {
            AssertRoundTrip(new int[0], (a, b) => a.SequenceEqual(b));
        }

        [TestMethod]
        public void PrimitiveArrayRoundTrip()
        {
            AssertRoundTrip(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, (a, b) => a.SequenceEqual(b));
        }

        [TestMethod]
        public void StructArrayRoundTrip()
        {
            AssertRoundTrip(new ExampleStruct[] { new ExampleStruct() { A = 123, B = "Test Text", D = 123.456M }, new ExampleStruct() { A = 456, B = "Hello, World!", D = 456.789M } }, (a, b) => a.SequenceEqual(b, new ExampleStructEqualityComparer()));
        }

        [TestMethod]
        public void ClassArrayRoundTrip()
        {
            AssertRoundTrip(new ExampleClass[] { new ExampleClass() { A = 123, B = "Test Text", D = 123.456M }, new ExampleClass() { A = 456, B = "Hello, World!", D = 456.789M } }, (a, b) => a.SequenceEqual(b, new ExampleClassEqualityComparer()));
        }

        [TestMethod]
        public void ClassArrayOfNullRoundTrip()
        {
            AssertRoundTrip(new ExampleClass[] { null, null }, (a, b) => a.SequenceEqual(b));
        }

        protected abstract void AssertRoundTrip<T>(T expected);
        protected abstract void AssertRoundTrip<T>(T expected, Func<T, T, bool> areEqual);
    }
}
