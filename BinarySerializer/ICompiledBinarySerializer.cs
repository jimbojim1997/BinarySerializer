using System;
using System.IO;

namespace BinarySerializer
{
    public interface ICompiledBinarySerializer : IBinarySerializer
    {
        void RegisterDeserialize(Type type);
        void RegisterDeserialize<T>();
        void RegisterSerialize(Type type);
        void RegisterSerialize<T>();
    }
}