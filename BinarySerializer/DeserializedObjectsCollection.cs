using System.Collections.Generic;

namespace BinarySerializer
{
    internal class DeserializedObjectsCollection
    {
        private const uint NULL_OBJECT_ID = 0;
        private Dictionary<uint, object> _objects = new Dictionary<uint, object>()
        {
            {NULL_OBJECT_ID, null}
        };


        public void Add(uint objectId, object obj)
        {
            _objects.Add(objectId, obj);
        }

        public bool TryGetObject(uint objectId, out object obj)
        {
            return _objects.TryGetValue(objectId, out obj);
        }
    }
}
