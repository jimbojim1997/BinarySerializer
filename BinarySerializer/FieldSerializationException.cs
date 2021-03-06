using System;

namespace BinarySerializer
{
    [Serializable]
    public class FieldSerializationException : Exception
    {
        public object CurrentObject { get; }
        public Type ObjectType { get; }
        public string FieldName { get; }
        public Type FieldType { get; }

        internal FieldSerializationException(object currentObject, Type objectType, string fieldName, Type fieldType, string message) : base(message)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
            FieldName = fieldName;
            FieldType = fieldType;
        }

        internal FieldSerializationException(object currentObject, Type objectType, string fieldName, Type fieldType, string message, Exception innerException) :base(message, innerException)
        {
            CurrentObject = currentObject;
            ObjectType = objectType;
            FieldName = fieldName;
            FieldType = fieldType;
        }

        protected FieldSerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            CurrentObject = info.GetValue(nameof(CurrentObject), typeof(object));
            ObjectType = info.GetValue(nameof(ObjectType), typeof(Type)) as Type;
            FieldName = (string)info.GetValue(nameof(FieldName), typeof(string));
            FieldType = (Type)info.GetValue(nameof(FieldType), typeof(Type));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(CurrentObject), CurrentObject);
            info.AddValue(nameof(ObjectType), ObjectType);
            info.AddValue(nameof(FieldName), FieldName);
            info.AddValue(nameof(FieldType), FieldType);
        }
    }
}
