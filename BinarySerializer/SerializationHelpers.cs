using System.IO;

namespace BinarySerializer
{
    internal static class SerializationHelpers
    {
        internal static void SerializeByte(byte value, Stream stream)
        {
            stream.WriteByte(value);
        }

        internal static byte DeserializeByte(Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        internal static void SerializeSByte(sbyte value, Stream stream)
        {
            SerializeByte((byte)value, stream);
        }

        internal static sbyte DeserializeSByte(Stream stream)
        {
            return (sbyte)DeserializeByte(stream);
        }

        internal static void SerializeBoolean(bool value, Stream stream)
        {
            stream.WriteByte((byte)(value ? 1 : 0));
        }

        internal static bool DeserializeBoolean(Stream stream)
        {
            return stream.ReadByte() != 0;
        }

        internal static void SerializeShort(short value, Stream stream)
        {
            byte[] bytes = new byte[] {
                (byte)(value),
                (byte)(value >> 8)
            };
            stream.Write(bytes, 0, bytes.Length);
        }

        internal static short DeserializeShort(Stream stream)
        {
            byte[] bytes = new byte[2];
            stream.Read(bytes, 0, bytes.Length);
            return (short)(bytes[0] | bytes[1] << 8);
        }

        internal static void SerializeUShort(ushort value, Stream stream)
        {
            SerializeShort((short)value, stream);
        }

        internal static ushort DeserializeUShort(Stream stream)
        {
            return (ushort)DeserializeShort(stream);
        }

        internal static void SerializeChar(char value, Stream stream)
        {
            SerializeShort((short)value, stream);
        }

        internal static char DeserializeChar(Stream stream)
        {
            return (char)DeserializeShort(stream);
        }

        internal static void SerializeInt(int value, Stream stream)
        {
            byte[] bytes = new byte[] {
                (byte)(value),
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24)
            };
            stream.Write(bytes, 0, bytes.Length);
        }

        internal static int DeserializeInt(Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);
            return (int)(bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24);
        }

        internal static void SerializeUInt(uint value, Stream stream)
        {
            SerializeInt((int)value, stream);
        }

        internal static uint DeserializeUInt(Stream stream)
        {
            return (uint)DeserializeInt(stream);
        }

        internal static void SerializeFloat(float value, Stream stream)
        {
            unsafe
            {
                SerializeInt(*(int*)&value, stream);
            }
        }

        internal static float DeserializeFloat(Stream stream)
        {
            unsafe
            {
                int value = DeserializeInt(stream);
                return *(float*)&value;
            }
        }

        internal static void SerializeLong(long value, Stream stream)
        {
            byte[] bytes = new byte[] {
                (byte)(value),
                (byte)(value >> 8),
                (byte)(value >> 16),
                (byte)(value >> 24),
                (byte)(value >> 32),
                (byte)(value >> 40),
                (byte)(value >> 48),
                (byte)(value >> 56)
            };
            stream.Write(bytes, 0, bytes.Length);
        }

        internal static long DeserializeLong(Stream stream)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, bytes.Length);
            long low = ((long)bytes[0] | (long)bytes[1] << 8 | (long)bytes[2] << 16 | (long)bytes[3] << 24);
            long high = ((long)bytes[4] | (long)bytes[5] << 8 | (long)bytes[6] << 16 | (long)bytes[7] << 24);
            return low | high << 32;
        }

        internal static void SerializeULong(ulong value, Stream stream)
        {
            SerializeLong((long)value, stream);
        }

        internal static ulong DeserializeULong(Stream stream)
        {
            return (ulong)DeserializeLong(stream);
        }

        internal static void SerializeDouble(double value, Stream stream)
        {
            unsafe
            {
                SerializeLong(*(long*)&value, stream);
            }
        }

        internal static double DeserializeDouble(Stream stream)
        {
            unsafe
            {
                long value = DeserializeLong(stream);
                return *(double*)&value;
            }
        }

        internal static void SerializeObjectId(uint objectId, Stream stream)
        {
            SerializeUInt(objectId, stream);
        }

        internal static uint DeserializeObjectId(Stream stream)
        {
            return DeserializeUInt(stream);
        }

    }
}
