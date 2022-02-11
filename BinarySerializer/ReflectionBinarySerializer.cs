using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace BinarySerializer
{
    public class ReflectionBinarySerializer : IBinarySerializer
    {
        private static readonly MethodInfo _serialize = typeof(ReflectionBinarySerializer).GetMethod("Serialize", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Serialize<T>(T obj, Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("The stream must be writeable.", nameof(stream));

            Serialize(obj, stream, new SerializedObjectsCollection());
        }

        private void Serialize<T>(T obj, Stream stream, SerializedObjectsCollection serializedObjects)
        {
            Type type = typeof(T);

            if (type.IsPrimitive)
            {
                if (obj is byte b) SerializationHelpers.SerializeByte(b, stream);
                else if (obj is sbyte sb) SerializationHelpers.SerializeSByte(sb, stream);
                else if (obj is bool bo) SerializationHelpers.SerializeBoolean(bo, stream);
                else if (obj is short s) SerializationHelpers.SerializeShort(s, stream);
                else if (obj is ushort us) SerializationHelpers.SerializeUShort(us, stream);
                else if (obj is char c) SerializationHelpers.SerializeChar(c, stream);
                else if (obj is int i) SerializationHelpers.SerializeInt(i, stream);
                else if (obj is uint ui) SerializationHelpers.SerializeUInt(ui, stream);
                else if (obj is float f) SerializationHelpers.SerializeFloat(f, stream);
                else if (obj is long l) SerializationHelpers.SerializeLong(l, stream);
                else if (obj is ulong ul) SerializationHelpers.SerializeULong(ul, stream);
                else if (obj is double d) SerializationHelpers.SerializeDouble(d, stream);
                else if (obj is decimal de) SerializationHelpers.SerializeDecimal(de, stream);
                else throw new Exception($"Primitive type \"{type.FullName}\" not supported.");
            }
            else if(obj is string str)
            {
                uint objectId;
                if (serializedObjects.TryGetObjectId((object)obj, out objectId))
                {
                    SerializationHelpers.SerializeObjectId(objectId, stream);
                }
                else
                {
                    objectId = serializedObjects.Add((object)obj);
                    SerializationHelpers.SerializeObjectId(objectId, stream);

                    byte[] bytes = Encoding.UTF8.GetBytes(str);
                    SerializationHelpers.SerializeUInt((uint)bytes.Length, stream);

                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            else if (type.IsArray)
            {
                uint objectId;
                if (serializedObjects.TryGetObjectId((object)obj, out objectId))
                {
                    SerializationHelpers.SerializeObjectId(objectId, stream);
                }
                else
                {
                    objectId = serializedObjects.Add((object)obj);
                    SerializationHelpers.SerializeObjectId(objectId, stream);

                    Array array = obj as Array;
                    SerializationHelpers.SerializeUInt((uint)array.Length, stream);
                    MethodInfo elementSerialize = _serialize.MakeGenericMethod(type.GetElementType());

                    foreach (object value in array)
                    {
                        elementSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                    }
                }
            }
            else if (type.IsClass)
            {
                uint objectId;
                if (serializedObjects.TryGetObjectId((object)obj, out objectId))
                {
                    SerializationHelpers.SerializeObjectId(objectId, stream);
                }
                else
                {
                    objectId = serializedObjects.Add((object)obj);
                    SerializationHelpers.SerializeObjectId(objectId, stream);
                    foreach (FieldInfo field in typeof(T).GetRuntimeFields())
                    {
                        object value = field.GetValue(obj);
                        MethodInfo valueSerialize = _serialize.MakeGenericMethod(field.FieldType);
                        valueSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                    }
                }
            }
            else if (type.IsValueType)
            {
                foreach (FieldInfo field in typeof(T).GetRuntimeFields())
                {
                    object value = field.GetValue(obj);
                    MethodInfo valueSerialize = _serialize.MakeGenericMethod(field.FieldType);
                    valueSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                }
            }
            else
            {
                throw new Exception($"Type \"{type.FullName}\" not supported.");
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
