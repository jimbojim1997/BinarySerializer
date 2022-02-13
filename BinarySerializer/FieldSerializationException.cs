using System;

namespace BinarySerializer
{
    [Serializable]
    public class FieldSerializationException : SerializationException
    {
        public string FieldName { get; }
        public Type FieldType { get; }

        internal FieldSerializationException(object currentObject, Type objectType, string fieldName, Type fieldType, string message) : base(currentObject, objectType, message)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }

        internal FieldSerializationException(object currentObject, Type objectType, string fieldName, Type fieldType, string message, Exception innerException) : base(currentObject, objectType, message, innerException)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }

        protected FieldSerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            FieldName = (string)info.GetValue(nameof(FieldName), typeof(string));
            FieldType = (Type)info.GetValue(nameof(FieldType), typeof(Type));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(FieldName), FieldName);
            info.AddValue(nameof(FieldType), FieldType);
        }
    }
}
