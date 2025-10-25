using System.Net;

namespace LatServer.Core.Tools.BinaryTools
{
    /// <summary>
    /// 扩展 BinaryWriter 静态方法
    /// </summary>
    public static partial class BinaryTool
    {
        public static void Write(BinaryWriter writer, short value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                value = IPAddress.HostToNetworkOrder(value);
            }

            writer.Write(value);
        }

        public static void Write(BinaryWriter writer, ushort value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                byte b1 = (byte)(value & 0xFF);
                byte b2 = (byte)((value >> 8) & 0xFF);
                writer.Write(b1);
                writer.Write(b2);
            }
            else
            {
                writer.Write(value);
            }
        }

        public static void Write(BinaryWriter writer, int value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                value = IPAddress.HostToNetworkOrder(value);
            }

            writer.Write(value);
        }

        public static void Write(BinaryWriter writer, uint value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                byte b1 = (byte)(value & 0xFF);
                byte b2 = (byte)((value >> 8) & 0xFF);
                byte b3 = (byte)((value >> 16) & 0xFF);
                byte b4 = (byte)((value >> 24) & 0xFF);
                writer.Write(b1);
                writer.Write(b2);
                writer.Write(b3);
                writer.Write(b4);
            }
            else
            {
                writer.Write(value);
            }
        }

        public static void Write(BinaryWriter writer, long value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                value = IPAddress.HostToNetworkOrder(value);
            }

            writer.Write(value);
        }

        public static void Write(BinaryWriter writer, ulong value, bool isToNetworkOrder = true)
        {
            if (isToNetworkOrder)
            {
                byte b1 = (byte)(value & 0xFF);
                byte b2 = (byte)((value >> 8) & 0xFF);
                byte b3 = (byte)((value >> 16) & 0xFF);
                byte b4 = (byte)((value >> 24) & 0xFF);
                byte b5 = (byte)((value >> 32) & 0xFF);
                byte b6 = (byte)((value >> 40) & 0xFF);
                byte b7 = (byte)((value >> 48) & 0xFF);
                byte b8 = (byte)((value >> 56) & 0xFF);
                writer.Write(b1);
                writer.Write(b2);
                writer.Write(b3);
                writer.Write(b4);
                writer.Write(b5);
                writer.Write(b6);
                writer.Write(b7);
                writer.Write(b8);
            }
            else
            {
                writer.Write(value);
            }
        }

        public static void Write(BinaryWriter writer, float value, bool isToNetworkOrder = true)
        {
            int intValue = BitConverter.SingleToInt32Bits(value);
            if (isToNetworkOrder)
            {
                intValue = IPAddress.HostToNetworkOrder(intValue);
            }
            writer.Write(intValue);
        }

        public static void Write(BinaryWriter writer, double value, bool isToNetworkOrder = true)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);
            if (isToNetworkOrder)
            {
                longValue = IPAddress.HostToNetworkOrder(longValue);
            }
            writer.Write(longValue);
        }

        public static void WriteBytes(BinaryWriter writer, byte[] bytes, int length, bool isToNetworkOrder)
        {
            if (isToNetworkOrder)
            {
                Array.Reverse(bytes);
            }

            writer.Write(bytes, 0, length);
        }
    }
}