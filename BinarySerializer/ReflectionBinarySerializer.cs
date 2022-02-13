using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

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
            try
            {
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
                    else throw new UnsupportedTypeException(type, $"Primitive type \"{type.FullName}\" not supported.");
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
                        SerializationHelpers.SerializeString(str, stream);
                    }
                }
                else if (obj is decimal de)
                {
                    SerializationHelpers.SerializeDecimal(de, stream);
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

                        for (int i = 0; i < array.Length; i++)
                        {
                            try
                            {
                                elementSerialize.Invoke(this, new[] { array.GetValue(i), stream, serializedObjects });
                            }
                            catch (Exception ex)
                            {
                                throw new ArraySerializationException(array, type, i, type.GetElementType(), $"Unable to serialize index {i} of type \"{type.GetElementType().FullName}\" of array \"{type.FullName}\".", ex);
                            }
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
                            try
                            {
                                if (field.IsLiteral || field.IsStatic) continue;
                                object value = field.GetValue(obj);
                                MethodInfo valueSerialize = _serialize.MakeGenericMethod(field.FieldType);
                                valueSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                            }
                            catch (Exception ex)
                            {
                                throw new FieldSerializationException(obj, type, field.Name, field.FieldType, $"Unable to serialize field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                            }
                        }
                    }
                }
                else if (type.IsValueType)
                {
                    foreach (FieldInfo field in type.GetRuntimeFields())
                    {
                        try
                        {
                            if (field.IsLiteral || field.IsStatic) continue;
                            object value = field.GetValue(obj);
                            MethodInfo valueSerialize = _serialize.MakeGenericMethod(field.FieldType);
                            valueSerialize.Invoke(this, new[] { value, stream, serializedObjects });
                        }
                        catch (Exception ex)
                        {
                            throw new FieldSerializationException(obj, type, field.Name, field.FieldType, $"Unable to serialize field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                        }
                    }
                }
                else
                {
                    throw new UnsupportedTypeException(type, $"Type \"{type.FullName}\" not supported.");
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(obj, type, $"Unable to serialize type \"{type.FullName}\".", ex);
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("The stream must be readable.", nameof(stream));

            return Deserialize<T>(stream, new DeserializedObjectsCollection());
        }

        private T Deserialize<T>(Stream stream, DeserializedObjectsCollection deserializedObjects)
        {
            Type type = typeof(T);
            try
            {
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
                    else throw new UnsupportedTypeException(type, $"Primitive type \"{type.FullName}\" not supported.");
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
                        obj = SerializationHelpers.DeserializeString(stream);
                        deserializedObjects.Add(objectId, obj);
                        return (T)obj;
                    }
                }
                else if (type.Equals(typeof(decimal)))
                {
                    return (T)(object)SerializationHelpers.DeserializeDecimal(stream);
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
                            try
                            {
                                obj = elementDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                                array.SetValue(obj, i);
                            }
                            catch (Exception ex)
                            {
                                throw new ArraySerializationException(array, type, i, type.GetElementType(), $"Unable to deserialize index {i} of type \"{type.GetElementType().FullName}\" of array \"{type.FullName}\".", ex);
                            }
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
                            try
                            {
                                if (field.IsLiteral || field.IsStatic) continue;
                                MethodInfo valueDeserialize = _deserialize.MakeGenericMethod(field.FieldType);
                                object value = valueDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                                field.SetValue(obj, value);
                            }
                            catch (Exception ex)
                            {
                                throw new FieldSerializationException(obj, type, field.Name, field.FieldType, $"Unable to deserialize field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                            }
                        }
                        return (T)obj;
                    }
                }
                else if (type.IsValueType)
                {
                    object obj = FormatterServices.GetUninitializedObject(type);

                    foreach (FieldInfo field in type.GetRuntimeFields())
                    {
                        try
                        {
                            if (field.IsLiteral || field.IsStatic) continue;
                            MethodInfo valueDeserialize = _deserialize.MakeGenericMethod(field.FieldType);
                            object value = valueDeserialize.Invoke(this, new object[] { stream, deserializedObjects });
                            field.SetValue(obj, value);
                        }
                        catch (Exception ex)
                        {
                            throw new FieldSerializationException(obj, type, field.Name, field.FieldType, $"Unable to deserialize field \"{type.FullName}.{field.Name}\" of type \"{field.FieldType.FullName}\".", ex);
                        }
                    }
                    return (T)obj;
                }
                else
                {
                    throw new UnsupportedTypeException(type, $"Type \"{type.FullName}\" not supported.");
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(null, type, $"Unable to deserialize type \"{type.FullName}\".", ex);
            }
        }
    }
}
