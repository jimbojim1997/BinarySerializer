using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinarySerializer
{
    public class CompiledBinarySerializer : IBinarySerializer
    {
        public void Serialize<T>(T obj, Stream stream)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
