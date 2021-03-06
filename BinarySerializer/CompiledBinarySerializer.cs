using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace BinarySerializer
{
    public class CompiledBinarySerializer : ICompiledBinarySerializer
    {
        #region Statics
        private static readonly MethodInfo _serializeByte = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeByte), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeSByte = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeSByte), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeBoolean = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeBoolean), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeShort = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeShort), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeUShort = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeUShort), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeChar = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeChar), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeInt = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeInt), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeUInt = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeUInt), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeFloat = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeFloat), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeLong = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeLong), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeULong = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeULong), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeDouble = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeDouble), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeDecimal = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeDecimal), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeString = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeString), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _serializeObjectId = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.SerializeObjectId), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _deserializeByte = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeByte), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeSByte = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeSByte), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeBoolean = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeBoolean), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeShort = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeShort), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeUShort = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeUShort), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeChar = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeChar), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeInt = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeInt), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeUInt = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeUInt), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeFloat = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeFloat), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeLong = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeLong), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeULong = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeULong), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeDouble = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeDouble), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeDecimal = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeDecimal), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeString = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeString), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _deserializeObjectId = typeof(SerializationHelpers).GetMethod(nameof(SerializationHelpers.DeserializeObjectId), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _serializedObjectsCollectionTryGetObjectId = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.TryGetObjectId));
        private static readonly MethodInfo _serializedObjectsCollectionTryGetObjectIdString = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.TryGetObjectId)).MakeGenericMethod(typeof(string));
        private static readonly MethodInfo _serializedObjectsCollectionAdd = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.Add));
        private static readonly MethodInfo _serializedObjectsCollectionAddString = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.Add)).MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _deserializedObjectsCollectionTryGetObjectId = typeof(DeserializedObjectsCollection).GetMethod(nameof(DeserializedObjectsCollection.TryGetObject));
        private static readonly MethodInfo _deserializedObjectsCollectionAdd = typeof(DeserializedObjectsCollection).GetMethod(nameof(DeserializedObjectsCollection.Add));

        private static readonly MethodInfo _getUninitializedObject = typeof(FormatterServices).GetMethod(nameof(FormatterServices.GetUninitializedObject));
        private static readonly MethodInfo _getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

        private static readonly MethodInfo _createSerializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateSerializationException), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _createArraySerializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateArraySerializationException), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _createFieldSerializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateFieldSerializationException), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _createDeserializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateDeserializationException), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _createArrayDeserializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateArrayDeserializationException), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _createFieldDeserializationException = typeof(CompiledBinarySerializer).GetMethod(nameof(CompiledBinarySerializer.CreateFieldDeserializationException), BindingFlags.Static | BindingFlags.NonPublic);
        #endregion

        private readonly object _serializeLock = new object();
        private readonly ConcurrentDictionary<Type, MethodInfo> _serializeMethodInfoCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ConcurrentDictionary<Type, Delegate> _serializeDelegateCache = new ConcurrentDictionary<Type, Delegate>();

        private readonly object _deserializeLock = new object();
        private readonly ConcurrentDictionary<Type, MethodInfo> _deserializeMethodInfoCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ConcurrentDictionary<Type, Delegate> _deserializeDelegateCache = new ConcurrentDictionary<Type, Delegate>();

        public void Serialize<T>(T obj, Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("The stream must be writeable.", nameof(stream));

            try
            {
                SerializeObjectDelegate<T> serialize = (SerializeObjectDelegate<T>)GetOrRegisterSerializeDelegate(typeof(T));
                serialize(obj, stream, new SerializedObjectsCollection());
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SerializationException(obj, typeof(T), $"Unable to serialize type \"{typeof(T).FullName}\".", ex);
            }
        }

        public void RegisterSerialize<T>()
        {
            GetOrRegisterSerializeMethodInfo(typeof(T));
        }

        public void RegisterSerialize(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            GetOrRegisterSerializeMethodInfo(type);
        }

        private MethodInfo GetOrRegisterSerializeMethodInfo(Type type)
        {
            //Do a double get to reduce the number of locks.
            //The first get without the lock because the value will exist more often than not.
            //The second get within the lock because the value may have been added by another thread, make sure it hasn't before creating the DynamicMethod.
            if (_serializeMethodInfoCache.TryGetValue(type, out MethodInfo methodInfo))
            {
                return methodInfo;
            }
            else
            {
                lock (_serializeLock)
                {
                    if (_serializeMethodInfoCache.TryGetValue(type, out methodInfo))
                    {
                        return methodInfo;
                    }
                    else
                    {
                        CreateSerializeDynamicMethod(type);
                        return _serializeMethodInfoCache[type];
                    }
                }
            }
        }

        private Delegate GetOrRegisterSerializeDelegate(Type type)
        {
            //Do a double get to reduce the number of locks.
            //The first get without the lock because the value will exist more often than not.
            //The second get within the lock because the value may have been added by another thread, make sure it hasn't before creating the DynamicMethod.
            if (_serializeDelegateCache.TryGetValue(type, out Delegate del))
            {
                return del;
            }
            else
            {
                lock (_serializeLock)
                {
                    if (_serializeDelegateCache.TryGetValue(type, out del))
                    {
                        return del;
                    }
                    else
                    {
                        CreateSerializeDynamicMethod(type);
                        return _serializeDelegateCache[type];
                    }
                }
            }
        }

        private void CreateSerializeDynamicMethod(Type type)
        {
            DynamicMethod method = new DynamicMethod("Serialize", typeof(void), new[] { type, typeof(Stream), typeof(SerializedObjectsCollection) }, typeof(CompiledBinarySerializer), true);
            _serializeMethodInfoCache[type] = method;

            if (type.IsPrimitive || type.IsEnum) EmitPrimitiveSerialize(type, method);
            else if (type.Equals(typeof(string))) EmitStringSerialize(method);
            else if (type.Equals(typeof(decimal))) EmitDecimalSerialize(method);
            else if (type.IsArray) EmitArraySerialize(type, method);
            else if (type.IsClass) EmitClassSerialize(type, method);
            else if (type.IsValueType) EmitStructSerialize(type, method);
            else
            {
                _serializeMethodInfoCache.TryRemove(type, out _);
                throw new UnsupportedTypeException(type, $"Type \"{type.FullName}\" not supported.");
            }

            Type delegateType = typeof(SerializeObjectDelegate<>).MakeGenericType(type);
            _serializeDelegateCache[type] = method.CreateDelegate(delegateType);
        }

        private void EmitPrimitiveSerialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_0); //T
            il.Emit(OpCodes.Ldarg_1); //Stream
            EmitPrimitiveSerializeMethodCall(type, il);
            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldarg_0); //T
            il.Emit(OpCodes.Box, type);
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createSerializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Expected execution stack: T, Stream.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="il"></param>
        /// <exception cref="UnsupportedTypeException"></exception>
        private void EmitPrimitiveSerializeMethodCall(Type type, ILGenerator il)
        {
            Type actualType;
            if (type.IsEnum) actualType = type.GetEnumUnderlyingType();
            else actualType = type;

            if (actualType.Equals(typeof(byte))) il.Emit(OpCodes.Call, _serializeByte);
            else if (actualType.Equals(typeof(sbyte))) il.Emit(OpCodes.Call, _serializeSByte);
            else if (actualType.Equals(typeof(bool))) il.Emit(OpCodes.Call, _serializeBoolean);
            else if (actualType.Equals(typeof(short))) il.Emit(OpCodes.Call, _serializeShort);
            else if (actualType.Equals(typeof(ushort))) il.Emit(OpCodes.Call, _serializeUShort);
            else if (actualType.Equals(typeof(char))) il.Emit(OpCodes.Call, _serializeChar);
            else if (actualType.Equals(typeof(int))) il.Emit(OpCodes.Call, _serializeInt);
            else if (actualType.Equals(typeof(uint))) il.Emit(OpCodes.Call, _serializeUInt);
            else if (actualType.Equals(typeof(float))) il.Emit(OpCodes.Call, _serializeFloat);
            else if (actualType.Equals(typeof(long))) il.Emit(OpCodes.Call, _serializeLong);
            else if (actualType.Equals(typeof(ulong))) il.Emit(OpCodes.Call, _serializeULong);
            else if (actualType.Equals(typeof(double))) il.Emit(OpCodes.Call, _serializeDouble);
            else throw new UnsupportedTypeException(type, $"Primitive type \"{type.FullName}\" not supported.");
        }

        private void EmitStringSerialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();

            var objectId = il.DeclareLocal(typeof(uint));
            var objectExists = il.DefineLabel();
            var endOfMethod = il.DefineLabel();
            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //string
            il.Emit(OpCodes.Ldloca, objectId);
            il.Emit(OpCodes.Call, _serializedObjectsCollectionTryGetObjectIdString);
            il.Emit(OpCodes.Brtrue, objectExists);

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //string
            il.Emit(OpCodes.Call, _serializedObjectsCollectionAddString);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            il.Emit(OpCodes.Ldarg_0); //string
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeString);
            il.Emit(OpCodes.Br, endOfMethod);

            il.MarkLabel(objectExists);
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            il.MarkLabel(endOfMethod);
            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldarg_0); //string
            il.Emit(OpCodes.Ldtoken, typeof(string));
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createSerializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
        }

        private void EmitDecimalSerialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_0); //decimal
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeDecimal);

            il.Emit(OpCodes.Leave, exceptionEnd);
            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldarg_0); //decimal
            il.Emit(OpCodes.Box, typeof(decimal));
            il.Emit(OpCodes.Ldtoken, typeof(decimal));
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createSerializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ret);
        }

        private void EmitArraySerialize(Type type, DynamicMethod method)
        {
            Type elementType = type.GetElementType();

            ILGenerator il = method.GetILGenerator();
            var objectId = il.DeclareLocal(typeof(uint));
            var index = il.DeclareLocal(typeof(int));
            var objectExists = il.DefineLabel();
            var startOfLoop = il.DefineLabel();
            var endOfLoop = il.DefineLabel();
            var endOfMethod = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldloca, objectId);
            il.Emit(OpCodes.Call, _serializedObjectsCollectionTryGetObjectIdString);
            il.Emit(OpCodes.Brtrue, objectExists);

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Call, _serializedObjectsCollectionAddString);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeUInt);

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc, index);
            il.MarkLabel(startOfLoop);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Clt);
            il.Emit(OpCodes.Brfalse, endOfLoop);

            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldelem, elementType);
            il.Emit(OpCodes.Ldarg_1); //Stream
            if (elementType.IsPrimitive)
            {
                try
                {
                    EmitPrimitiveSerializeMethodCall(elementType, il);
                }
                catch (Exception ex)
                {
                    throw new ArraySerializationException(null, type, 0, type.GetElementType(), $"Unable to create serialization method for array of type \"{type.FullName}\".", ex);
                }
            }
            else if (elementType.Equals(typeof(decimal)))
            {
                il.Emit(OpCodes.Call, _serializeDecimal);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
                il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(elementType));
            }

            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createArraySerializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, index);
            il.Emit(OpCodes.Br, startOfLoop);
            il.MarkLabel(endOfLoop);
            il.Emit(OpCodes.Br, endOfMethod);

            il.MarkLabel(objectExists);
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            il.MarkLabel(endOfMethod);
            il.Emit(OpCodes.Ret);
        }

        private void EmitClassSerialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var objectId = il.DeclareLocal(typeof(uint));
            var objectExists = il.DefineLabel();
            var endOfMethod = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //T
            il.Emit(OpCodes.Ldloca, objectId);
            il.Emit(OpCodes.Call, _serializedObjectsCollectionTryGetObjectIdString);
            il.Emit(OpCodes.Brtrue, objectExists);

            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Ldarg_0); //T
            il.Emit(OpCodes.Call, _serializedObjectsCollectionAddString);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsLiteral || field.IsStatic) continue;

                var exceptionEnd = il.BeginExceptionBlock();

                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1); //Stream
                if (field.FieldType.IsPrimitive)
                {
                    try
                    {
                        EmitPrimitiveSerializeMethodCall(field.FieldType, il);
                    }
                    catch (Exception ex)
                    {
                        throw new FieldSerializationException(null, type, field.Name, field.FieldType, $"Unable to create serialization method for field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                    }
                }
                else if (field.FieldType.Equals(typeof(decimal)))
                {
                    il.Emit(OpCodes.Call, _serializeDecimal);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
                    il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(field.FieldType));
                }

                il.Emit(OpCodes.Leave, exceptionEnd);

                il.BeginCatchBlock(typeof(Exception));
                var exception = il.DeclareLocal(typeof(Exception));
                il.Emit(OpCodes.Stloc, exception);
                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldloc, exception);
                il.Emit(OpCodes.Call, _createFieldSerializationException);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();
            }
            il.Emit(OpCodes.Br, endOfMethod);

            il.MarkLabel(objectExists);
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeObjectId);

            il.MarkLabel(endOfMethod);
            il.Emit(OpCodes.Ret);
        }

        private void EmitStructSerialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();

            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsLiteral || field.IsStatic) continue;

                var exceptionEnd = il.BeginExceptionBlock();

                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1); //Stream
                if (field.FieldType.IsPrimitive)
                {
                    try
                    {
                        EmitPrimitiveSerializeMethodCall(field.FieldType, il);
                    }
                    catch (UnsupportedTypeException ex)
                    {
                        throw new FieldSerializationException(null, type, field.Name, field.FieldType, $"Unable to create serialization method for field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                    }
                }
                else if (field.FieldType.Equals(typeof(decimal)))
                {
                    il.Emit(OpCodes.Call, _serializeDecimal);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
                    il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(field.FieldType));
                }

                il.Emit(OpCodes.Leave, exceptionEnd);

                il.BeginCatchBlock(typeof(Exception));
                var exception = il.DeclareLocal(typeof(Exception));
                il.Emit(OpCodes.Stloc, exception);
                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldloc, exception);
                il.Emit(OpCodes.Call, _createFieldSerializationException);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();
            }

            il.Emit(OpCodes.Ret);
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("The stream must be readable.", nameof(stream));

            try
            {
                DeserializeObjectDelegate<T> deserialize = (DeserializeObjectDelegate<T>)GetOrRegisterDeserializeDelegate(typeof(T));
                return deserialize(stream, new DeserializedObjectsCollection());
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SerializationException(null, typeof(T), $"Unable to deserialize type \"{typeof(T).FullName}\".", ex);
            }
        }

        public void RegisterDeserialize<T>()
        {
            GetOrRegisterDeserializeMethodInfo(typeof(T));
        }

        public void RegisterDeserialize(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            GetOrRegisterDeserializeMethodInfo(type);
        }

        private MethodInfo GetOrRegisterDeserializeMethodInfo(Type type)
        {
            //Do a double get to reduce the number of locks.
            //The first get without the lock because the value will exist more often than not.
            //The second get within the lock because the value may have been added by another thread, make sure it hasn't before creating the DynamicMethod.
            if (_deserializeMethodInfoCache.TryGetValue(type, out MethodInfo methodInfo))
            {
                return methodInfo;
            }
            else
            {
                lock (_deserializeLock)
                {
                    if (_deserializeMethodInfoCache.TryGetValue(type, out methodInfo))
                    {
                        return methodInfo;
                    }
                    else
                    {
                        CreateDeserializeDynamicMethod(type);
                        return _deserializeMethodInfoCache[type];
                    }
                }
            }
        }

        private Delegate GetOrRegisterDeserializeDelegate(Type type)
        {
            //Do a double get to reduce the number of locks.
            //The first get without the lock because the value will exist more often than not.
            //The second get within the lock because the value may have been added by another thread, make sure it hasn't before creating the DynamicMethod.
            if (_deserializeDelegateCache.TryGetValue(type, out Delegate del))
            {
                return del;
            }
            else
            {
                lock (_deserializeLock)
                {
                    if (_deserializeDelegateCache.TryGetValue(type, out del))
                    {
                        return del;
                    }
                    else
                    {
                        CreateDeserializeDynamicMethod(type);
                        return _deserializeDelegateCache[type];
                    }
                }
            }
        }

        private void CreateDeserializeDynamicMethod(Type type)
        {
            DynamicMethod method = new DynamicMethod("Deserialize", type, new[] { typeof(Stream), typeof(DeserializedObjectsCollection) }, typeof(CompiledBinarySerializer), true);
            _deserializeMethodInfoCache[type] = method;

            if (type.IsPrimitive || type.IsEnum) EmitPrimitiveDeserialize(type, method);
            else if (type.Equals(typeof(string))) EmitStringDeserialize(method);
            else if (type.Equals(typeof(decimal))) EmitDecimalDeserialize(method);
            else if (type.IsArray) EmitArrayDeserialize(type, method);
            else if (type.IsClass) EmitClassDeserialize(type, method);
            else if (type.IsValueType) EmitStructDeserialize(type, method);
            else
            {
                _deserializeMethodInfoCache.TryRemove(type, out _);
                throw new UnsupportedTypeException(type, $"Type \"{type.FullName}\" not supported.");
            }

            Type delegateType = typeof(DeserializeObjectDelegate<>).MakeGenericType(type);
            _deserializeDelegateCache[type] = method.CreateDelegate(delegateType);
        }

        private void EmitPrimitiveDeserialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var value = il.DeclareLocal(type);

            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_0); //Stream
            EmitPrimitiveDeserializeMethodCall(type, il);
            il.Emit(OpCodes.Stloc, value);
            il.Emit(OpCodes.Leave, exceptionEnd);
            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createDeserializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Expected execution stack: Stream.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="il"></param>
        /// <exception cref="UnsupportedTypeException"></exception>
        private void EmitPrimitiveDeserializeMethodCall(Type type, ILGenerator il)
        {
            Type actualType;
            if (type.IsEnum) actualType = type.GetEnumUnderlyingType();
            else actualType = type;

            if (actualType.Equals(typeof(byte))) il.Emit(OpCodes.Call, _deserializeByte);
            else if (actualType.Equals(typeof(sbyte))) il.Emit(OpCodes.Call, _deserializeSByte);
            else if (actualType.Equals(typeof(bool))) il.Emit(OpCodes.Call, _deserializeBoolean);
            else if (actualType.Equals(typeof(short))) il.Emit(OpCodes.Call, _deserializeShort);
            else if (actualType.Equals(typeof(ushort))) il.Emit(OpCodes.Call, _deserializeUShort);
            else if (actualType.Equals(typeof(char))) il.Emit(OpCodes.Call, _deserializeChar);
            else if (actualType.Equals(typeof(int))) il.Emit(OpCodes.Call, _deserializeInt);
            else if (actualType.Equals(typeof(uint))) il.Emit(OpCodes.Call, _deserializeUInt);
            else if (actualType.Equals(typeof(float))) il.Emit(OpCodes.Call, _deserializeFloat);
            else if (actualType.Equals(typeof(long))) il.Emit(OpCodes.Call, _deserializeLong);
            else if (actualType.Equals(typeof(ulong))) il.Emit(OpCodes.Call, _deserializeULong);
            else if (actualType.Equals(typeof(double))) il.Emit(OpCodes.Call, _deserializeDouble);
            else throw new UnsupportedTypeException(type, $"Primitive type \"{type.FullName}\" not supported.");
        }

        private void EmitStringDeserialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();

            var objectId = il.DeclareLocal(typeof(uint));
            var value = il.DeclareLocal(typeof(string));
            var valueExists = il.DefineLabel();

            var exceptionEnd = il.BeginExceptionBlock();
            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeObjectId);
            il.Emit(OpCodes.Stloc, objectId);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloca, value);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionTryGetObjectId);
            il.Emit(OpCodes.Brtrue, valueExists);

            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeString);
            il.Emit(OpCodes.Stloc, value);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionAdd);

            il.MarkLabel(valueExists);
            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldtoken, typeof(string));
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createDeserializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ret);
        }

        private void EmitDecimalDeserialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var value = il.DeclareLocal(typeof(decimal));
            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeDecimal);
            il.Emit(OpCodes.Stloc, value);
            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldarg_0); //decimal
            il.Emit(OpCodes.Box, typeof(decimal));
            il.Emit(OpCodes.Ldtoken, typeof(decimal));
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createDeserializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ret);
        }

        private void EmitArrayDeserialize(Type type, DynamicMethod method)
        {
            Type elementType = type.GetElementType();
            ILGenerator il = method.GetILGenerator();
            var objectId = il.DeclareLocal(typeof(uint));
            var array = il.DeclareLocal(type);
            var index = il.DeclareLocal(typeof(int));
            var valueExists = il.DefineLabel();
            var startOfLoop = il.DefineLabel();
            var endOfLoop = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeObjectId);
            il.Emit(OpCodes.Stloc, objectId);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloca, array);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionTryGetObjectId);
            il.Emit(OpCodes.Brtrue, valueExists);

            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeUInt);
            il.Emit(OpCodes.Newarr, elementType);
            il.Emit(OpCodes.Stloc, array);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloc, array);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionAdd);

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc, index);
            il.MarkLabel(startOfLoop);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldloc, array);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Clt);
            il.Emit(OpCodes.Brfalse, endOfLoop);

            var exceptionEnd = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldloc, array);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldarg_0); //Stream;
            if (elementType.IsPrimitive)
            {
                try
                {
                    EmitPrimitiveDeserializeMethodCall(elementType, il);
                }
                catch (Exception ex)
                {
                    throw new ArraySerializationException(null, type, 0, type.GetElementType(), $"Unable to create deserialization method for array of type \"{type.FullName}\".", ex);
                }
            }
            else if (elementType.Equals(typeof(decimal)))
            {
                il.Emit(OpCodes.Call, _deserializeDecimal);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_1); //DeserializedObjectsCollection
                il.Emit(OpCodes.Call, GetOrRegisterDeserializeMethodInfo(elementType));
            }
            il.Emit(OpCodes.Stelem, elementType);
            il.Emit(OpCodes.Leave, exceptionEnd);

            il.BeginCatchBlock(typeof(Exception));
            var exception = il.DeclareLocal(typeof(Exception));
            il.Emit(OpCodes.Stloc, exception);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldloc, exception);
            il.Emit(OpCodes.Call, _createArrayDeserializationException);
            il.Emit(OpCodes.Throw);
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, index);
            il.Emit(OpCodes.Br, startOfLoop);

            il.MarkLabel(endOfLoop);
            il.MarkLabel(valueExists);
            il.Emit(OpCodes.Ldloc, array);

            il.Emit(OpCodes.Ret);
        }

        private void EmitClassDeserialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var objectId = il.DeclareLocal(typeof(uint));
            var value = il.DeclareLocal(typeof(string));
            var valueExists = il.DefineLabel();

            //TODO wrap in exception handler

            il.Emit(OpCodes.Ldarg_0); //Stream
            il.Emit(OpCodes.Call, _deserializeObjectId);
            il.Emit(OpCodes.Stloc, objectId);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloca, value);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionTryGetObjectId);
            il.Emit(OpCodes.Brtrue, valueExists);

            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, _getTypeFromHandle);
            il.Emit(OpCodes.Call, _getUninitializedObject);
            il.Emit(OpCodes.Stloc, value);

            il.Emit(OpCodes.Ldarg_1); //DeserializedObjectCollection
            il.Emit(OpCodes.Ldloc, objectId);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Call, _deserializedObjectsCollectionAdd);

            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsLiteral || field.IsStatic) continue;

                var exceptionEnd = il.BeginExceptionBlock();

                il.Emit(OpCodes.Ldloc, value);
                il.Emit(OpCodes.Ldarg_0); //Stream
                if (field.FieldType.IsPrimitive)
                {
                    try
                    {
                        EmitPrimitiveDeserializeMethodCall(field.FieldType, il);
                    }
                    catch (UnsupportedTypeException ex)
                    {
                        throw new FieldSerializationException(null, type, field.Name, field.FieldType, $"Unable to create deserialization method for field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                    }
                }
                else if (field.FieldType.Equals(typeof(decimal)))
                {
                    il.Emit(OpCodes.Call, _deserializeDecimal);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_1); //DeserializedObjectsCollection
                    il.Emit(OpCodes.Call, GetOrRegisterDeserializeMethodInfo(field.FieldType));
                }
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Leave, exceptionEnd);

                il.BeginCatchBlock(typeof(Exception));
                var exception = il.DeclareLocal(typeof(Exception));
                il.Emit(OpCodes.Stloc, exception);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldloc, exception);
                il.Emit(OpCodes.Call, _createFieldDeserializationException);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();
            }

            il.MarkLabel(valueExists);
            il.Emit(OpCodes.Ldloc, value);

            il.Emit(OpCodes.Ret);
        }

        private void EmitStructDeserialize(Type type, DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var value = il.DeclareLocal(type);

            il.Emit(OpCodes.Ldloca, value);
            il.Emit(OpCodes.Initobj, type);

            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsLiteral || field.IsStatic) continue;

                var exceptionEnd = il.BeginExceptionBlock();

                il.Emit(OpCodes.Ldloca, value);
                il.Emit(OpCodes.Ldarg_0); //Stream
                if (field.FieldType.IsPrimitive)
                {
                    try
                    {
                        EmitPrimitiveDeserializeMethodCall(field.FieldType, il);
                    }
                    catch (UnsupportedTypeException ex)
                    {
                        throw new FieldSerializationException(null, type, field.Name, field.FieldType, $"Unable to create deserialization method for field \"{type.FullName}.{field.Name}\", type \"{field.FieldType.FullName}\".", ex);
                    }
                }
                else if (field.FieldType.Equals(typeof(decimal)))
                {
                    il.Emit(OpCodes.Call, _deserializeDecimal);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_1); //DeserializedObjectsCollection
                    il.Emit(OpCodes.Call, GetOrRegisterDeserializeMethodInfo(field.FieldType));
                }
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Leave, exceptionEnd);

                il.BeginCatchBlock(typeof(Exception));
                var exception = il.DeclareLocal(typeof(Exception));
                il.Emit(OpCodes.Stloc, exception);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                il.Emit(OpCodes.Call, _getTypeFromHandle);
                il.Emit(OpCodes.Ldloc, exception);
                il.Emit(OpCodes.Call, _createFieldDeserializationException);
                il.Emit(OpCodes.Throw);
                il.EndExceptionBlock();
            }

            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ret);
        }

        private static SerializationException CreateSerializationException(object obj, Type objectType, Exception innerException)
        {
            return new SerializationException(obj, objectType, $"Unable to serialize type \"{objectType.FullName}\".", innerException);
        }

        private static ArraySerializationException CreateArraySerializationException(Array array, Type type, int index, Exception innerException)
        {
            return new ArraySerializationException(array, type, index, type.GetElementType(), $"Unable to serialize index {index} of type \"{type.GetElementType().FullName}\" of array \"{type.FullName}\".", innerException);
        }

        private static FieldSerializationException CreateFieldSerializationException(object obj, Type objectType, string fieldName, Type fieldType, Exception innerException)
        {
            return new FieldSerializationException(obj, objectType, fieldName, fieldType, $"Unable to serialize field \"{objectType.FullName}.{fieldName}\", type \"{fieldType.FullName}\".", innerException);
        }

        private static SerializationException CreateDeserializationException(object obj, Type objectType, Exception innerException)
        {
            return new SerializationException(obj, objectType, $"Unable to deserialize type \"{objectType.FullName}\".", innerException);
        }

        private static ArraySerializationException CreateArrayDeserializationException(Array array, Type type, int index, Exception innerException)
        {
            return new ArraySerializationException(array, type, index, type.GetElementType(), $"Unable to deserialize index {index} of type \"{type.GetElementType().FullName}\" of array \"{type.FullName}\".", innerException);
        }

        private static FieldSerializationException CreateFieldDeserializationException(object obj, Type objectType, string fieldName, Type fieldType, Exception innerException)
        {
            return new FieldSerializationException(obj, objectType, fieldName, fieldType, $"Unable to deserialize field \"{objectType.FullName}.{fieldName}\", type \"{fieldType.FullName}\".", innerException);
        }

        private delegate void SerializeObjectDelegate<T>(T obj, Stream stream, SerializedObjectsCollection serializedObjects);
        private delegate T DeserializeObjectDelegate<T>(Stream stream, DeserializedObjectsCollection serializedObjects);
    }
}
