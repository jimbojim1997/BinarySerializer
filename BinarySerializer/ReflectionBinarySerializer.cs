using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace BinarySerializer
{
    public class ReflectionBinarySerializer : IBinarySerializer
    {
        private static readonly MethodInfo _serialize = typeof(ReflectionBinarySerializer).GetMethod(nameof(Serialize), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _deserialize = typeof(ReflectionBinarySerializer).GetMethod(nameof(Deserialize), BindingFlags.NonPublic | BindingFlags.Instance);

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
            else if (obj is string str)
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
                    foreach (FieldInfo field in type.GetRuntimeFields())
                    {
                        if (field.IsLiteral || field.IsStatic) continue;
                        object value = field.GetValue(obj);
                        MethodInfo valueSerialize = _serialize.MakeGenericMethod(field.FieldType);
                        valueSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                    }
                }
            }
            else if (type.IsValueType)
            {
                foreach (FieldInfo field in type.GetRuntimeFields())
                {
                    if (field.IsLiteral || field.IsStatic) continue;
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
            return Deserialize<T>(stream, new DeserializedObjectsCollection());
        }

        private T Deserialize<T>(Stream stream, DeserializedObjectsCollection deserializedObjects)
        {
            Type type = typeof(T);

            if (type.IsPrimitive)
            {
                object obj;
                if (type.Equals(typeof(byte))) obj = SerializationHelpers.DeserializeByte(stream);
                else if (type.Equals(typeof(sbyte))) obj = SerializationHelpers.DeserializeSByte(stream);
                else if (type.Equals(typeof(bool))) obj = SerializationHelpers.DeserializeBoolean(stream);
                else if (type.Equals(typeof(short))) obj = SerializationHelpers.DeserializeShort(stream);
                else if (type.Equals(typeof(ushort))) obj = SerializationHelpers.DeserializeUShort(stream);
                else if (type.Equals(typeof(char))) obj = SerializationHelpers.DeserializeChar(stream);
                else if (type.Equals(typeof(int))) obj = SerializationHelpers.DeserializeInt(stream);
                else if (type.Equals(typeof(uint))) obj = SerializationHelpers.DeserializeUInt(stream);
                else if (type.Equals(typeof(float))) obj = SerializationHelpers.DeserializeFloat(stream);
                else if (type.Equals(typeof(long))) obj = SerializationHelpers.DeserializeLong(stream);
                else if (type.Equals(typeof(ulong))) obj = SerializationHelpers.DeserializeULong(stream);
                else if (type.Equals(typeof(double))) obj = SerializationHelpers.DeserializeDouble(stream);
                else if (type.Equals(typeof(decimal))) obj = SerializationHelpers.DeserializeDecimal(stream);
                else throw new Exception($"Primitive type \"{type.FullName}\" not supported.");
                return (T)obj;
            }
            else if (type.Equals(typeof(string)))
            {
                uint objectId = SerializationHelpers.DeserializeObjectId(stream);
                if (deserializedObjects.TryGetObject(objectId, out object obj))
                {
                    return (T)obj;
                }
                else
                {
                    uint length = SerializationHelpers.DeserializeUInt(stream);
                    byte[] bytes = new byte[(int)length];
                    stream.Read(bytes, 0, bytes.Length);
                    obj = Encoding.UTF8.GetString(bytes);
                    deserializedObjects.Add(objectId, obj);
                    return (T)obj;
                }
            }
            else if (type.IsArray)
            {
                uint objectId = SerializationHelpers.DeserializeObjectId(stream);
                if (deserializedObjects.TryGetObject(objectId, out object obj))
                {
                    return (T)obj;
                }
                else
                {
                    uint length = SerializationHelpers.DeserializeUInt(stream);
                    Array array = Array.CreateInstance(type.GetElementType(), (int)length);
                    deserializedObjects.Add(objectId, array);

                    MethodInfo elementDeserialize = _deserialize.MakeGenericMethod(type.GetElementType());

                    for (int i = 0; i < array.Length; i++)
                    {
                        obj = elementDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                        array.SetValue(obj, i);
                    }
                    return (T)(object)array;
                }
            }
            else if (type.IsClass)
            {
                uint objectId = SerializationHelpers.DeserializeObjectId(stream);
                if (deserializedObjects.TryGetObject(objectId, out object obj))
                {
                    return (T)obj;
                }
                else
                {
                    obj = FormatterServices.GetUninitializedObject(type);
                    deserializedObjects.Add(objectId, obj);

                    foreach (FieldInfo field in type.GetRuntimeFields())
                    {
                        if (field.IsLiteral || field.IsStatic) continue;
                        MethodInfo valueDeserialize = _deserialize.MakeGenericMethod(field.FieldType);
                        object value = valueDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                        field.SetValue(obj, value);
                    }
                    return (T)obj;
                }
            }
            else if (type.IsValueType)
            {
                object obj = FormatterServices.GetUninitializedObject(type);

                foreach (FieldInfo field in type.GetRuntimeFields())
                {
                    if (field.IsLiteral || field.IsStatic) continue;
                    MethodInfo valueDeserialize = _deserialize.MakeGenericMethod(field.FieldType);
                    object value = valueDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                    field.SetValue(obj, value);
                }
                return (T)obj;
            }
            else
            {
                throw new Exception($"Type \"{type.FullName}\" not supported.");
            }
        }
    }
}
