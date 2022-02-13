using System.Collections.Generic;

namespace Tests.TestStructures
{
    internal struct ExampleStruct
    {
        public int A;
        public string B;
    }

    internal class ExampleStructEqualityComparer : IEqualityComparer<ExampleStruct>
    {
        public bool Equals(ExampleStruct x, ExampleStruct y)
        {
            return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(ExampleStruct obj)
        {
            return obj.GetHashCode();
        }
    }
}
