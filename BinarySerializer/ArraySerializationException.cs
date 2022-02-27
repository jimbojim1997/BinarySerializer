using System;

namespace BinarySerializer
{
    [Serializable]
    public class ArraySerializationException : Exception
    {
        public object CurrentObject { get; }
        public Type ObjectType { get; }
        public int Index { get; }
        public Type ElementType { get; }

        internal ArraySerializationException(Array currentObject, Type objectType, int index, Type elementType, string message) : base(message)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
            Index = index;
            ElementType = elementType;
        }

        internal ArraySerializationException(Array currentObject, Type objectType, int index, Type elementType, string message, Exception innerException) : base(message, innerException)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
            Index = index;
            ElementType = elementType;
        }

        protected ArraySerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            CurrentObject = info.GetValue(nameof(CurrentObject), typeof(object));
            ObjectType = info.GetValue(nameof(ObjectType), typeof(Type)) as Type;
            Index = (int)info.GetValue(nameof(Index), typeof(int));
            ElementType = (Type)info.GetValue(nameof(ElementType), typeof(Type));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(CurrentObject), CurrentObject);
            info.AddValue(nameof(ObjectType), ObjectType);
            info.AddValue(nameof(Index), Index);
            info.AddValue(nameof(ElementType), ElementType);
        }
    }
}
