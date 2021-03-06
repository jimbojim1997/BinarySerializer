using System;
using System.Collections.Generic;

namespace BinarySerializer.Tests.TestStructures
{
    internal class ExampleClass
    {
        public int A;
        public string B;
        public ExampleClass C;
        public decimal D;
    }
    internal class ExampleClassEqualityComparer : IEqualityComparer<ExampleClass>
    {
        public bool Equals(ExampleClass x, ExampleClass y)
        {
            return x?.A == y?.A && x?.B == y?.B && x?.D == y?.D;
        }

        public int GetHashCode(ExampleClass obj)
        {
            throw new NotImplementedException();
        }
    }
}
