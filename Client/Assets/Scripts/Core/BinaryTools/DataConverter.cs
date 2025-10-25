using System;

namespace Core.BinaryTools
{
    /// <summary>
    /// 部分优化 BitConverter 部分可以使用 BitConverter 方法
    ///   大端转小端字节序,其中高字节在前，低字节在后
    ///   小端转大端字节序,其中低字节在前，高字节在后
    ///   例如:Unity为小端，而Jave需要大端
    /// 字节数组 转换 基本数据类型（小端:高字节在前，低字节在后）
    ///     字节数组：isToNetworkOrder=true 代表 字节数组是大端数据，false=小端数据
    /// 
    /// c#发送java数据 是小端 需要 调用IPAddress.HostToNetworkOrder 转换为大端
    /// c#接受java数据是大端 需要 调用IPAddress.NetworkToHostOrder 转换为小端
    /// </summary>
    public static class DataConverter
    {
        public const int SIZE_OF_BOOL = 1;
        public const int SIZE_OF_CHAR = 2;
        public const int SIZE_OF_SHORT = 2;
        public const int SIZE_OF_INT = 4;
        public const int SIZE_OF_LONG = 8;
        public const int SIZE_OF_FLOAT = 4;
        public const int SIZE_OF_DOUBLE = 8;

        private static bool IsToNetworkOrder => BitConverter.IsLittleEndian;

        #region 从字节数组转换为基本类型

        public static bool ToBool(byte[] data, int startIndex = 0)
        {
            bool value = BitConverter.ToBoolean(data, startIndex);
            return value;
        }

        public static char ToChar(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // data 为大端数据，转换为小端：交换两个字节
                return (char)((data[startIndex + 1] & 0xFF) | ((data[startIndex] & 0xFF) << 8));
            }
            else
            {
                // data 为小端数据，直接构造数值
                return (char)((data[startIndex] & 0xFF) | ((data[startIndex + 1] & 0xFF) << 8));
            }
        }

        public static short ToShort(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // data 为大端数据，交换字节：大端 [高,低] → 小端 [低,高]
                return (short)((data[startIndex + 1] & 0xFF) | ((data[startIndex] & 0xFF) << 8));
            }
            else
            {
                // data 为小端数据
                return (short)((data[startIndex] & 0xFF) | ((data[startIndex + 1] & 0xFF) << 8));
            }
        }

        public static ushort ToUShort(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                return (ushort)((data[startIndex + 1] & 0xFF) | ((data[startIndex] & 0xFF) << 8));
            }
            else
            {
                return (ushort)((data[startIndex] & 0xFF) | ((data[startIndex + 1] & 0xFF) << 8));
            }
        }

        public static int ToInt(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // data 为大端数据，字节顺序 [b0, b1, b2, b3] → 小端：交换顺序为 [b3, b2, b1, b0]
                return ((data[startIndex + 3] & 0xFF)) |
                       ((data[startIndex + 2] & 0xFF) << 8) |
                       ((data[startIndex + 1] & 0xFF) << 16) |
                       ((data[startIndex] & 0xFF) << 24);
            }
            else
            {
                // data 为小端数据
                return ((data[startIndex] & 0xFF)) |
                       ((data[startIndex + 1] & 0xFF) << 8) |
                       ((data[startIndex + 2] & 0xFF) << 16) |
                       ((data[startIndex + 3] & 0xFF) << 24);
            }
        }

        public static uint ToUInt(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                return ((uint)(data[startIndex + 3] & 0xFF)) |
                       ((uint)(data[startIndex + 2] & 0xFF) << 8) |
                       ((uint)(data[startIndex + 1] & 0xFF) << 16) |
                       ((uint)(data[startIndex] & 0xFF) << 24);
            }
            else
            {
                return ((uint)(data[startIndex] & 0xFF)) |
                       ((uint)(data[startIndex + 1] & 0xFF) << 8) |
                       ((uint)(data[startIndex + 2] & 0xFF) << 16) |
                       ((uint)(data[startIndex + 3] & 0xFF) << 24);
            }
        }

        public static long ToLong(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // data 为大端数据，交换字节顺序
                return ((long)(data[startIndex + 7] & 0xFFL)) |
                       ((long)(data[startIndex + 6] & 0xFFL) << 8) |
                       ((long)(data[startIndex + 5] & 0xFFL) << 16) |
                       ((long)(data[startIndex + 4] & 0xFFL) << 24) |
                       ((long)(data[startIndex + 3] & 0xFFL) << 32) |
                       ((long)(data[startIndex + 2] & 0xFFL) << 40) |
                       ((long)(data[startIndex + 1] & 0xFFL) << 48) |
                       ((long)(data[startIndex] & 0xFFL) << 56);
            }
            else
            {
                // data 为小端数据
                return ((long)(data[startIndex] & 0xFFL)) |
                       ((long)(data[startIndex + 1] & 0xFFL) << 8) |
                       ((long)(data[startIndex + 2] & 0xFFL) << 16) |
                       ((long)(data[startIndex + 3] & 0xFFL) << 24) |
                       ((long)(data[startIndex + 4] & 0xFFL) << 32) |
                       ((long)(data[startIndex + 5] & 0xFFL) << 40) |
                       ((long)(data[startIndex + 6] & 0xFFL) << 48) |
                       ((long)(data[startIndex + 7] & 0xFFL) << 56);
            }
        }

        public static ulong ToULong(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                return ((ulong)(data[startIndex + 7] & 0xFFUL)) |
                       ((ulong)(data[startIndex + 6] & 0xFFUL) << 8) |
                       ((ulong)(data[startIndex + 5] & 0xFFUL) << 16) |
                       ((ulong)(data[startIndex + 4] & 0xFFUL) << 24) |
                       ((ulong)(data[startIndex + 3] & 0xFFUL) << 32) |
                       ((ulong)(data[startIndex + 2] & 0xFFUL) << 40) |
                       ((ulong)(data[startIndex + 1] & 0xFFUL) << 48) |
                       ((ulong)(data[startIndex] & 0xFFUL) << 56);
            }
            else
            {
                return ((ulong)(data[startIndex] & 0xFFUL)) |
                       ((ulong)(data[startIndex + 1] & 0xFFUL) << 8) |
                       ((ulong)(data[startIndex + 2] & 0xFFUL) << 16) |
                       ((ulong)(data[startIndex + 3] & 0xFFUL) << 24) |
                       ((ulong)(data[startIndex + 4] & 0xFFUL) << 32) |
                       ((ulong)(data[startIndex + 5] & 0xFFUL) << 40) |
                       ((ulong)(data[startIndex + 6] & 0xFFUL) << 48) |
                       ((ulong)(data[startIndex + 7] & 0xFFUL) << 56);
            }
        }

        public static float ToFloat(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // 如果数据为大端格式，则反转指定范围的字节顺序后再转换
                Array.Reverse(data, startIndex, SIZE_OF_FLOAT);
            }

            return BitConverter.ToSingle(data, startIndex);
        }

        public static double ToDouble(byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                Array.Reverse(data, startIndex, SIZE_OF_DOUBLE);
            }

            return BitConverter.ToDouble(data, startIndex);
        }

        #endregion

        #region 从基本类型转换为字节数组

        public static byte[] ToBytes(bool value, byte[] data, int startIndex = 0)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, startIndex, bytes.Length);
            return data;
        }

        public static byte[] ToBytes(char value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            // char 为 2 字节（UInt16）
            if (isToNetworkOrder)
            {
                // 大端：高字节先
                data[startIndex] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 1] = (byte)(value & 0xFF);
            }
            else
            {
                // 小端：低字节先
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
            }

            return data;
        }


        public static byte[] ToBytes(short value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // 大端：高字节先
                data[startIndex] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 1] = (byte)(value & 0xFF);
            }
            else
            {
                // 小端：低字节先
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(ushort value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                data[startIndex] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 1] = (byte)(value & 0xFF);
            }
            else
            {
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
            }

            return data;
        }


        public static byte[] ToBytes(float value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            // 获取 float 的位表示（32位整数）
            // BitConverter.GetBytes 会产生一个新的byte[]归墟 避免额外堆内存分配 减轻GC压力
            int bits = BitConverter.SingleToInt32Bits(value);
            if (isToNetworkOrder)
            {
                // 需要大端（网络）格式：最高有效字节放前面
                data[startIndex] = (byte)((bits >> 24) & 0xFF);
                data[startIndex + 1] = (byte)((bits >> 16) & 0xFF);
                data[startIndex + 2] = (byte)((bits >> 8) & 0xFF);
                data[startIndex + 3] = (byte)(bits & 0xFF);
            }
            else
            {
                // 小端格式：低有效字节放前面
                data[startIndex] = (byte)(bits & 0xFF);
                data[startIndex + 1] = (byte)((bits >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((bits >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((bits >> 24) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(double value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            // 获取 double 的位表示（64位整数）
            // BitConverter.GetBytes 会产生一个新的byte[]归墟 避免额外堆内存分配 减轻GC压力
            long bits = BitConverter.DoubleToInt64Bits(value);

            if (isToNetworkOrder)
            {
                // 大端格式
                data[startIndex] = (byte)((bits >> 56) & 0xFF);
                data[startIndex + 1] = (byte)((bits >> 48) & 0xFF);
                data[startIndex + 2] = (byte)((bits >> 40) & 0xFF);
                data[startIndex + 3] = (byte)((bits >> 32) & 0xFF);
                data[startIndex + 4] = (byte)((bits >> 24) & 0xFF);
                data[startIndex + 5] = (byte)((bits >> 16) & 0xFF);
                data[startIndex + 6] = (byte)((bits >> 8) & 0xFF);
                data[startIndex + 7] = (byte)(bits & 0xFF);
            }
            else
            {
                // 小端格式
                data[startIndex] = (byte)(bits & 0xFF);
                data[startIndex + 1] = (byte)((bits >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((bits >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((bits >> 24) & 0xFF);
                data[startIndex + 4] = (byte)((bits >> 32) & 0xFF);
                data[startIndex + 5] = (byte)((bits >> 40) & 0xFF);
                data[startIndex + 6] = (byte)((bits >> 48) & 0xFF);
                data[startIndex + 7] = (byte)((bits >> 56) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(int value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // 大端：最高字节先
                data[startIndex] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 1] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 3] = (byte)(value & 0xFF);
            }
            else
            {
                // 小端：低字节先
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 24) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(uint value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                data[startIndex] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 1] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 3] = (byte)(value & 0xFF);
            }
            else
            {
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 24) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(long value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                // 大端：最高有效字节先
                data[startIndex] = (byte)((value >> 56) & 0xFF);
                data[startIndex + 1] = (byte)((value >> 48) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 40) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 32) & 0xFF);
                data[startIndex + 4] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 5] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 6] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 7] = (byte)(value & 0xFF);
            }
            else
            {
                // 小端：低有效字节先
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 4] = (byte)((value >> 32) & 0xFF);
                data[startIndex + 5] = (byte)((value >> 40) & 0xFF);
                data[startIndex + 6] = (byte)((value >> 48) & 0xFF);
                data[startIndex + 7] = (byte)((value >> 56) & 0xFF);
            }

            return data;
        }

        public static byte[] ToBytes(ulong value, byte[] data, int startIndex = 0, bool isToNetworkOrder = false)
        {
            if (isToNetworkOrder)
            {
                data[startIndex] = (byte)((value >> 56) & 0xFF);
                data[startIndex + 1] = (byte)((value >> 48) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 40) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 32) & 0xFF);
                data[startIndex + 4] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 5] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 6] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 7] = (byte)(value & 0xFF);
            }
            else
            {
                data[startIndex] = (byte)(value & 0xFF);
                data[startIndex + 1] = (byte)((value >> 8) & 0xFF);
                data[startIndex + 2] = (byte)((value >> 16) & 0xFF);
                data[startIndex + 3] = (byte)((value >> 24) & 0xFF);
                data[startIndex + 4] = (byte)((value >> 32) & 0xFF);
                data[startIndex + 5] = (byte)((value >> 40) & 0xFF);
                data[startIndex + 6] = (byte)((value >> 48) & 0xFF);
                data[startIndex + 7] = (byte)((value >> 56) & 0xFF);
            }

            return data;
        }

        #endregion

    }
}