using System.Collections.Generic;

namespace BinarySerializer.Tests.TestStructures
{
    internal struct ExampleStruct
    {
        public int A;
        public string B;
        public decimal D;
    }

    internal class ExampleStructEqualityComparer : IEqualityComparer<ExampleStruct>
    {
        public bool Equals(ExampleStruct x, ExampleStruct y)
        {
            return x.A == y.A && x.B == y.B && x.D == y.D;
        }

        public int GetHashCode(ExampleStruct obj)
        {
            return obj.GetHashCode();
        }
    }
}
