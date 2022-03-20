# .NET Binary Serializer

This project contains two binary serializer implementations and supporting tests: [CompiledBinarySerializer](#compiled-binary-serializer) and [ReflectionBinarySerializer](#reflection-binary-serializer). Both implementations are compatible with each other and serialize to the format specified in the [schema](schema.md);

## Compiled Binary Serializer
This implementations creates serialization and deserialization methods at runtime using `System.Reflection.Emit.DynamicMethod`; these methods are then cached for higher performance (compared to traditional reflection-only methods). This implementation is the main focus of this repository.

When calling `Serialize` or `Deserialize` the relevant methods are created for the types that are being de/serialized; alternatively `RegisterSerialize` and `RegisterDeserialize` can be called in advance (e.g. at start-up) to create the relevant methods.

### Examples

#### Serialization
```C#
using BinarySerializer;
//...
using (var stream = new MemoryStream())
{
    var data = new List<DateTime>()
    {
        DateTime.Now,
        new DateTime(1970, 1, 1)
    };

    var serializer = new CompiledBinarySerializer();
    serializer.Serialize(data, stream);
}
```

#### Deserialization
```C#
using BinarySerializer;
//...
using (var stream = new MemoryStream())
{
    var serializer = new CompiledBinarySerializer();
    var data = serializer.Deserialize<List<DateTime>>(stream);
}
```

#### Type Registration
```C#
using BinarySerializer;
//...
var serializer = new CompiledBinarySerializer();
serializer.RegisterSerialize<List<DateTime>>();
serializer.RegisterSerialize(typeof(List<DateTime>));

serializer.RegisterDeserialize<List<DateTime>>();
serializer.RegisterDeserialize(typeof(List<DateTime>));
```

## Reflection Binary Serializer
This implementation was created first due to its easier and faster development time, it was then used to test the [CompiledBinarySerializer](#compiled-binary-serializer) implementation. Performance isn't a concern for this implementation and it's unlikely to have any further development other than to implement future tests.

### Examples

#### Serialization
```C#
using BinarySerializer;
//...
using(var stream = new MemoryStream())
{
    var data = new List<DateTime>()
    {
        DateTime.Now,
        new DateTime(1970, 1, 1)
    };

    var serializer = new ReflectionBinarySerializer();
    serializer.Serialize(data, stream);
}
```

#### Deserialization
```C#
using BinarySerializer;
//...
using (var stream = new MemoryStream())
{
    var serializer = new ReflectionBinarySerializer();
    var data = serializer.Deserialize<List<DateTime>>(stream);
}
```

## Future Development
- Add attributes to explicitly set which fields will be serialized and serialization order.
- Add support for multi-dimension arrays. Currently only jagged arrays are supported.
- Improve performance for `struct` in [CompiledBinarySerializer](#compiled-binary-serializer). `DynamicMethod` doesn't support `ref` parameters or `ref return` so there's always a copy of the `struct` when de/serializing. This can be resolved by either in-lining the de/serialization or using pointers.
- Create a Roslyn source generator that follows the [schema](schema.md). This will allow for compile time stability assertions, but is limited to primitives, arrays, and user defined types.
