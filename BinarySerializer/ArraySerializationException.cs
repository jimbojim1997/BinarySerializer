using System;

namespace BinarySerializer
{
    [Serializable]
    public class ArraySerializationException : SerializationException
    {
        public int Index { get; }
        public Type ElementType { get; }

        internal ArraySerializationException(Array currentObject, Type objectType, int index, Type elementType, string message) : base(currentObject, objectType, message)
        {
            Index = index;
            ElementType = elementType;
        }

        internal ArraySerializationException(Array currentObject, Type objectType, int index, Type elementType, string message, Exception innerException) : base(currentObject, objectType, message, innerException)
        {
            Index = index;
            ElementType = elementType;
        }

        protected ArraySerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            Index = (int)info.GetValue(nameof(Index), typeof(int));
            ElementType = (Type)info.GetValue(nameof(ElementType), typeof(Type));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Index), Index);
            info.AddValue(nameof(ElementType), ElementType);
        }
    }
}
