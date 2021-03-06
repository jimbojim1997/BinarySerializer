# Schema

## Primitives

All primitives are stored in [Little Endian byte order](https://en.wikipedia.org/wiki/Endianness).

`LO`: Low Order Byte. `HI`: High Order Byte. `-`: Intermediate bytes.

| Type | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
|------|---|---|---|---|---|---|---|---|
| `byte` | `LO` | | | | | | | |
| `sbyte` | `LO` | | | | | | | |
| `boolean` [^1] | `LO` | | | | | | | |
| `short`/`int16` | `LO` | `HI` | | | | | | |
| `ushort`/`uint16` | `LO` | `HI` | | | | | | |
| `char` [^2] | `LO` | `HI` | | | | | | |
| `int`/`int32` | `LO` | - | - | `HI` | | | | |
| `uint`/`uint32` | `LO` | - | - | `HI` | | | | |
| `float`/`single` | `LO` | - | - | `HI` | | | | |
| `long`/`int64` | `LO` | - | - | - | - | - | - | `HI` |
| `ulong`/`uint64` | `LO` | - | - | - | - | - | - | `HI` |
| `double` | `LO` | - | - | - | - | - | - | `HI` |

## Non-primitives

| Type | | | | | |
|------|---|---|---|---|---|
| `struct` | struct data inline | | | | |
| `object` [^3] | `uint32` object ID | object data inline | | | |
| `null` [^4] | `uint32` object ID `0` (zero) | | | | |
| `string (UTF-8)` [^3] | `uint32` object ID | `uint32` byte length | `byte` data inline | | |
| `array` [^3] [^5] | `uint32` object ID | `uint32` item count | data inline | | |
| `decimal` [^3] [^6] | `uint32` object ID | `int32` `[0]` |`int32` `[1]` |`int32` `[2]` |`int32` `[3]` |

[^1]: A `0` (zero) value represents `false`. Any none `0` (zero) value represents `true`.

[^2]: A .NET [`char`](https://docs.microsoft.com/en-us/dotnet/api/system.char) is stored internally as UTF-16.

[^3]: The first time an object is serialized both a unique object ID and the object data is written, subsequent references to this object only write the object ID.

[^4]: A `null` object will have an object ID of `0` (zero).

[^5]: Only single dimension `0` (zero) indexed arrays will be implemented.

[^6]: A `decimal` will be stored as `4` `int32` values that are retrieved using `decimal.GetBits(decimal)`, and reconstructed into a `decimal` using `new decimal(int[])`.