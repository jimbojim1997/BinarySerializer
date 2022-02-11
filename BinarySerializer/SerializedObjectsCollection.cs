using System.Collections.Generic;

namespace BinarySerializer
{
    internal class SerializedObjectsCollection
    {
        private const uint NULL_OBJECT_ID = 0;
        private uint _nextObjectId = NULL_OBJECT_ID + 1;
        private Dictionary<object, uint> _objects = new Dictionary<object, uint>();


        public uint Add<T>(T obj) where T : class
        {
            if(obj is null)
            {
                return NULL_OBJECT_ID;
            }
            else
            {
                uint objectId = _nextObjectId;
                _nextObjectId++;
                _objects.Add(obj, objectId);
                return objectId;
            }
        }

        public bool TryGetObjectId<T>(T obj, out uint objectId) where T:class
        {
            if (obj is null)
            {
                objectId = NULL_OBJECT_ID;
                return true;
            }
            else
            {
                return _objects.TryGetValue(obj, out objectId);
            }
        }
    }
}
