using System;
using System.Runtime.Serialization;

namespace BinarySerializer
{
    [Serializable]
    public class UnsupportedTypeException : Exception
    {
        public Type Type { get; }

        internal UnsupportedTypeException(Type type, string message) : base(message)
        {
            Type = type;
        }

        protected UnsupportedTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Type = info.GetValue(nameof(Type), typeof(Type)) as Type;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Type), Type);
        }
    }
}
