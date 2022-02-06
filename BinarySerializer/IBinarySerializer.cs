using System.IO;

namespace BinarySerializer
{
    public interface IBinarySerializer
    {
        void Serialize<T>(T obj, Stream stream);
        T Deserialize<T>(Stream stream);
    }
}
