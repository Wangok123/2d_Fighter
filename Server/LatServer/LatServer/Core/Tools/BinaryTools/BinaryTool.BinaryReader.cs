using System.Net;

namespace LatServer.Core.Tools.BinaryTools;

/// <summary>
/// 扩展 BinaryReader 静态方法
/// </summary>
public static partial class BinaryTool
{
    public static byte[] ReadBytes(BinaryReader reader, int length, bool isNetworkOrder = false)
    {
        byte[] bytes = reader.ReadBytes(length);
        if (isNetworkOrder)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    public static float ReadFloat(BinaryReader reader, bool isNetworkOrder = false)
    {
        int intValue = ReadInt32(reader, isNetworkOrder);
        float value = BitConverter.Int32BitsToSingle(intValue);
        return value;
    }

    public static double ReadDouble(BinaryReader reader, bool isNetworkOrder = false)
    {
        long longValue = ReadInt64(reader, isNetworkOrder);
        double value = BitConverter.Int64BitsToDouble(longValue);
        return value;
    }

    public static short ReadInt16(BinaryReader reader, bool isNetworkOrder = false)
    {
        short value = reader.ReadInt16();
        if (isNetworkOrder)
        {
            value = IPAddress.NetworkToHostOrder(value);
        }

        return value;
    }

    public static ushort ReadUInt16(BinaryReader reader, bool isNetworkOrder = false)
    {
        ushort value = reader.ReadUInt16();
        if (isNetworkOrder)
        {
            value = (ushort)((value >> 8) | (value << 8));
        }

        return value;
    }

    public static int ReadInt32(BinaryReader reader, bool isNetworkOrder = false)
    {
        int value = reader.ReadInt32();
        if (isNetworkOrder)
        {
            value = IPAddress.NetworkToHostOrder(value);
        }

        return value;
    }

    public static uint ReadUInt32(BinaryReader reader, bool isNetworkOrder = false)
    {
        uint value = reader.ReadUInt32();
        if (isNetworkOrder)
        {
            value = (value >> 24) |
                    ((value >> 8) & 0xFF00) |
                    ((value << 8) & 0xFF0000) |
                    (value << 24);
        }

        return value;
    }

    public static long ReadInt64(BinaryReader reader, bool isNetworkOrder = false)
    {
        long value = reader.ReadInt64();
        if (isNetworkOrder)
        {
            value = IPAddress.NetworkToHostOrder(value);
        }

        return value;
    }

    public static ulong ReadUInt64(BinaryReader reader, bool isNetworkOrder = false)
    {
        ulong value = reader.ReadUInt64();
        if (isNetworkOrder)
        {
            value = (value >> 56) |
                    ((value >> 40) & 0xFF00) |
                    ((value >> 24) & 0xFF0000) |
                    ((value >> 8) & 0xFF000000) |
                    ((value << 8) & 0xFF00000000) |
                    ((value << 24) & 0xFF0000000000) |
                    ((value << 40) & 0xFF000000000000) |
                    (value << 56);
        }

        return value;
    }
}