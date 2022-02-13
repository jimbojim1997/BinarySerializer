using System;
using System.Runtime.Serialization;

namespace BinarySerializer
{
    [Serializable]
    public class SerializationException : Exception
    {
        public object CurrentObject { get; }
        public Type ObjectType { get; }

        internal SerializationException(object currentObject, Type objectType, string message) : base(message)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
        }

        internal SerializationException(object currentObject, Type objectType, string message, Exception innerException) : base(message, innerException)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
        }

        protected SerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CurrentObject = info.GetValue(nameof(CurrentObject), typeof(object));
            ObjectType = info.GetValue(nameof(ObjectType), typeof(Type)) as Type;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(CurrentObject), CurrentObject);
            info.AddValue(nameof(ObjectType), ObjectType);
        }
    }
}
