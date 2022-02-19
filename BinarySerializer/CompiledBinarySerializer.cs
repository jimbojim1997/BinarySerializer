using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BinarySerializer
{
    public class CompiledBinarySerializer : IBinarySerializer
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

        private static readonly MethodInfo _serializedObjectsCollectionTryGetObjectId = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.TryGetObjectId));
        private static readonly MethodInfo _serializedObjectsCollectionTryGetObjectIdString = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.TryGetObjectId)).MakeGenericMethod(typeof(string));
        private static readonly MethodInfo _serializedObjectsCollectionAdd = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.Add));
        private static readonly MethodInfo _serializedObjectsCollectionAddString = typeof(SerializedObjectsCollection).GetMethod(nameof(SerializedObjectsCollection.Add)).MakeGenericMethod(typeof(string));
        #endregion


        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Type, MethodInfo> _serializeMethodInfoCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ConcurrentDictionary<Type, Delegate> _serializeDelegateCache = new ConcurrentDictionary<Type, Delegate>();

        public void Serialize<T>(T obj, Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("The stream must be writeable.", nameof(stream));

            SerializeObjectDelegate<T> serialize = (SerializeObjectDelegate<T>)GetOrRegisterSerializeDelegate(typeof(T));
            serialize(obj, stream, new SerializedObjectsCollection());
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
                lock (_lock)
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
                lock (_lock)
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

            if (type.IsPrimitive) EmitPrimitiveSerialize(type, method);
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
            il.Emit(OpCodes.Ldarg_0); //T
            il.Emit(OpCodes.Ldarg_1); //Stream

            //TODO wrap in exception handler
            if (type.Equals(typeof(byte))) il.Emit(OpCodes.Call, _serializeByte);
            else if (type.Equals(typeof(sbyte))) il.Emit(OpCodes.Call, _serializeSByte);
            else if (type.Equals(typeof(bool))) il.Emit(OpCodes.Call, _serializeBoolean);
            else if (type.Equals(typeof(short))) il.Emit(OpCodes.Call, _serializeShort);
            else if (type.Equals(typeof(ushort))) il.Emit(OpCodes.Call, _serializeUShort);
            else if (type.Equals(typeof(char))) il.Emit(OpCodes.Call, _serializeChar);
            else if (type.Equals(typeof(int))) il.Emit(OpCodes.Call, _serializeInt);
            else if (type.Equals(typeof(uint))) il.Emit(OpCodes.Call, _serializeUInt);
            else if (type.Equals(typeof(float))) il.Emit(OpCodes.Call, _serializeFloat);
            else if (type.Equals(typeof(long))) il.Emit(OpCodes.Call, _serializeLong);
            else if (type.Equals(typeof(ulong))) il.Emit(OpCodes.Call, _serializeULong);
            else if (type.Equals(typeof(double))) il.Emit(OpCodes.Call, _serializeDouble);
            else throw new UnsupportedTypeException(type, $"Primitive type \"{type.FullName}\" not supported.");

            il.Emit(OpCodes.Ret);
        }

        private void EmitStringSerialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            var objectId = il.DeclareLocal(typeof(uint));
            var objectExists = il.DefineLabel();
            var endOfMethod = il.DefineLabel();

            //TODO wrap in exception handler
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
            il.Emit(OpCodes.Ret);
        }

        private void EmitDecimalSerialize(DynamicMethod method)
        {
            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //decimal
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Call, _serializeDecimal);
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

            //TODO wrap in exception handler
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

            //TODO wrap loop inner in exception handler
            il.Emit(OpCodes.Ldarg_0); //T[]
            il.Emit(OpCodes.Ldloc, index);
            il.Emit(OpCodes.Ldelem, elementType);
            il.Emit(OpCodes.Ldarg_1); //Stream
            il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
            il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(elementType));

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

            //TODO wrap in exception handler
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

            foreach(FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsLiteral || field.IsStatic) continue;
                //TODO wrap field serialize in exception handler
                //TODO Maybe emit primitive serialization inline?
                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1); //Stream
                il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
                il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(field.FieldType));
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
                //TODO wrap field serialize in exception handler
                //TODO Maybe emit primitive serialization inline?
                il.Emit(OpCodes.Ldarg_0); //T
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1); //Stream
                il.Emit(OpCodes.Ldarg_2); //SerializedObjectsCollection
                il.Emit(OpCodes.Call, GetOrRegisterSerializeMethodInfo(field.FieldType));
            }

            il.Emit(OpCodes.Ret);
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        private delegate void SerializeObjectDelegate<T>(T obj, Stream stream, SerializedObjectsCollection serializedObjects);
    }
}
