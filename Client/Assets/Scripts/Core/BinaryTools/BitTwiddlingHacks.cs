using LATLog;

namespace Core.BinaryTools
{
    /// <summary>
/// 位操作技巧
/// https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
/// </summary>
public static class BitTwiddlingHacks
{

    #region Byte

    // Byte 位运算
    public const int ByteBitsSize = 8;

    /// <summary>
    /// 设置 byte 中指定索引的位。
    /// </summary>
    public static byte SetBit(byte map, int index)
    {
        if (!IsValidIndex(index, ByteBitsSize))
        {
            GameDebug.LogError("SetBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (byte)(map | (1 << index));
    }

    /// <summary>
    /// 清除 byte 中指定索引的位。
    /// </summary>
    public static byte ClearBit(byte map, int index)
    {
        if (!IsValidIndex(index, ByteBitsSize))
        {
            GameDebug.LogError("ClearBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (byte)(map & ~(1 << index));
    }

    /// <summary>
    /// 检查 byte 中指定索引的位是否被设置。
    /// </summary>
    public static bool IsBitSet(byte map, int index)
    {
        if (!IsValidIndex(index, ByteBitsSize))
        {
            GameDebug.LogError("IsBitSet: 索引超出范围 map:{0} index:{1}", map, index);
            return false;
        }

        return (map & (1 << index)) != 0;
    }

    /// <summary>
    /// 反转 byte 中指定索引的位。
    /// </summary>
    public static byte ToggleBit(byte map, int index)
    {
        if (!IsValidIndex(index, ByteBitsSize))
        {
            GameDebug.LogError("ToggleBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (byte)(map ^ (1 << index));
    }

    /// <summary>
    /// 计算 byte 中设置的位数。
    /// 使用 Kernighan's 算法。
    /// </summary>
    public static int CountSetBits(byte map)
    {
        int count = 0;
        while (map != 0)
        {
            map &= (byte)(map - 1);
            count++;
        }

        return count;
    }

    /// <summary>
    /// 计算 byte 的奇偶校验（奇数个1返回 true，偶数个1返回 false）。
    /// </summary>
    public static bool Parity(byte map)
    {
        map ^= (byte)(map >> 4);
        map ^= (byte)(map >> 2);
        map ^= (byte)(map >> 1);
        return (map & 1) != 0;
    }

    #endregion

    #region short

    // Short 位运算
    public const int ShortBitsSize = 16;

    public static short SetBit(short map, int index)
    {
        if (!IsValidIndex(index, ShortBitsSize))
        {
            GameDebug.LogError("SetBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (short)(map | (1 << index));
    }

    public static short ClearBit(short map, int index)
    {
        if (!IsValidIndex(index, ShortBitsSize))
        {
            GameDebug.LogError("ClearBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (short)(map & ~(1 << index));
    }

    public static bool IsBitSet(short map, int index)
    {
        if (!IsValidIndex(index, ShortBitsSize))
        {
            GameDebug.LogError("IsBitSet: 索引超出范围 map:{0} index:{1}", map, index);
            return false;
        }

        return (map & (1 << index)) != 0;
    }

    public static short ToggleBit(short map, int index)
    {
        if (!IsValidIndex(index, ShortBitsSize))
        {
            GameDebug.LogError("ToggleBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return (short)(map ^ (1 << index));
    }

    public static int CountSetBits(short map)
    {
        int count = 0;
        while (map != 0)
        {
            map &= (short)(map - 1);
            count++;
        }
        return count;
    }

    #endregion

    #region float
// Float 位运算
    public static int FloatToIntBits(float value)
    {
        return global::System.BitConverter.ToInt32(global::System.BitConverter.GetBytes(value), 0);
    }

    public static float IntBitsToFloat(int value)
    {
        return global::System.BitConverter.ToSingle(global::System.BitConverter.GetBytes(value), 0);
    }

    public static int SetBit(float map, int index)
    {
        int intMap = FloatToIntBits(map);
        intMap = SetBit(intMap, index);
        return intMap;
    }

    public static int ClearBit(float map, int index)
    {
        int intMap = FloatToIntBits(map);
        intMap = ClearBit(intMap, index);
        return intMap;
    }

    public static bool IsBitSet(float map, int index)
    {
        int intMap = FloatToIntBits(map);
        return IsBitSet(intMap, index);
    }

    public static float ToggleBit(float map, int index)
    {
        int intMap = FloatToIntBits(map);
        intMap = ToggleBit(intMap, index);
        return IntBitsToFloat(intMap);
    }

    public static int CountSetBits(float map)
    {
        int intMap = FloatToIntBits(map);
        return CountSetBits(intMap);
    }
    

    #endregion

    #region double 

    // Double 位运算
    public static long DoubleToLongBits(double value)
    {
        return global::System.BitConverter.ToInt64(global::System.BitConverter.GetBytes(value), 0);
    }

    public static double LongBitsToDouble(long value)
    {
        return global::System.BitConverter.ToDouble(global::System.BitConverter.GetBytes(value), 0);
    }

    public static long SetBit(double map, int index)
    {
        long longMap = DoubleToLongBits(map);
        longMap = SetBit(longMap, index);
        return longMap;
    }

    public static long ClearBit(double map, int index)
    {
        long longMap = DoubleToLongBits(map);
        longMap = ClearBit(longMap, index);
        return longMap;
    }

    public static bool IsBitSet(double map, int index)
    {
        long longMap = DoubleToLongBits(map);
        return IsBitSet(longMap, index);
    }

    public static double ToggleBit(double map, int index)
    {
        long longMap = DoubleToLongBits(map);
        longMap = ToggleBit(longMap, index);
        return LongBitsToDouble(longMap);
    }

    public static int CountSetBits(double map)
    {
        long longMap = DoubleToLongBits(map);
        return CountSetBits(longMap);
    }

    #endregion
    
    #region int 32

    // Int32 位运算
    public const int IntBitsSize = 32;

    /// <summary>
    /// 设置 int 中指定索引的位。
    /// </summary>
    public static int SetBit(int map, int index)
    {
        if (!IsValidIndex(index, IntBitsSize))
        {
            GameDebug.LogError("SetBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map | (1 << index);
    }

    /// <summary>
    /// 清除 int 中指定索引的位。
    /// </summary>
    public static int ClearBit(int map, int index)
    {
        if (!IsValidIndex(index, IntBitsSize))
        {
            GameDebug.LogError("ClearBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map & ~(1 << index);
    }

    /// <summary>
    /// 检查 int 中指定索引的位是否被设置。
    /// </summary>
    public static bool IsBitSet(int map, int index)
    {
        if (!IsValidIndex(index, IntBitsSize))
        {
            GameDebug.LogError("IsBitSet: 索引超出范围 map:{0} index:{1}", map, index);
            return false;
        }

        return (map & (1 << index)) != 0;
    }

    /// <summary>
    /// 反转 int 中指定索引的位。
    /// </summary>
    public static int ToggleBit(int map, int index)
    {
        if (!IsValidIndex(index, IntBitsSize))
        {
            GameDebug.LogError("ToggleBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map ^ (1 << index);
    }

    /// <summary>
    /// 计算 int 中设置的位数。
    /// 使用 Kernighan's 算法。
    /// </summary>
    public static int CountSetBits(int map)
    {
        int count = 0;
        while (map != 0)
        {
            map &= map - 1;
            count++;
        }

        return count;
    }

    /// <summary>
    /// 计算 int 的奇偶校验（奇数个1返回 true，偶数个1返回 false）。
    /// </summary>
    public static bool Parity(int map)
    {
        map ^= map >> 16;
        map ^= map >> 8;
        map ^= map >> 4;
        map ^= map >> 2;
        map ^= map >> 1;
        return (map & 1) != 0;
    }

    /// <summary>
    /// 计算一个整数的对数（以2为底），返回最高设置位的位置。
    /// 如果x为0或负数，返回-1。
    /// </summary>
    public static int Log2(int x)
    {
        if (x <= 0) return -1;

        int log = 0;
        while ((x >>= 1) != 0) // 添加括号以确保先进行右移赋值操作
        {
            log++;
        }

        return log;
    }

    #endregion

    #region long

    // Long 位运算
    public const int LongBitsSize = 64;

    /// <summary>
    /// 设置 long 中指定索引的位。
    /// </summary>
    public static long SetBit(long map, int index)
    {
        if (!IsValidIndex(index, LongBitsSize))
        {
            GameDebug.LogError("SetBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map | (1L << index);
    }

    /// <summary>
    /// 清除 long 中指定索引的位。
    /// </summary>
    public static long ClearBit(long map, int index)
    {
        if (!IsValidIndex(index, LongBitsSize))
        {
            GameDebug.LogError("ClearBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map & ~(1L << index);
    }

    /// <summary>
    /// 检查 long 中指定索引的位是否被设置。
    /// </summary>
    public static bool IsBitSet(long map, int index)
    {
        if (!IsValidIndex(index, LongBitsSize))
        {
            GameDebug.LogError("IsBitSet: 索引超出范围 map:{0} index:{1}", map, index);
            return false;
        }

        return (map & (1L << index)) != 0;
    }

    /// <summary>
    /// 反转 long 中指定索引的位。
    /// </summary>
    public static long ToggleBit(long map, int index)
    {
        if (!IsValidIndex(index, LongBitsSize))
        {
            GameDebug.LogError("ToggleBit: 索引超出范围 map:{0} index:{1}", map, index);
            return map;
        }

        return map ^ (1L << index);
    }

    /// <summary>
    /// 计算 long 中设置的位数。
    /// 使用 Kernighan's 算法。
    /// </summary>
    public static int CountSetBits(long map)
    {
        int count = 0;
        while (map != 0)
        {
            map &= map - 1;
            count++;
        }

        return count;
    }

    /// <summary>
    /// 计算 long 的奇偶校验（奇数个1返回 true，偶数个1返回 false）。
    /// </summary>
    public static bool Parity(long map)
    {
        map ^= map >> 32;
        map ^= map >> 16;
        map ^= map >> 8;
        map ^= map >> 4;
        map ^= map >> 2;
        map ^= map >> 1;
        return (map & 1) != 0;
    }

    /// <summary>
    /// 计算一个长整数的对数（以2为底），返回最高设置位的位置。
    /// 如果x为0或负数，返回-1。
    /// </summary>
    public static int Log2(long x)
    {
        if (x <= 0) return -1;

        int log = 0;
        while ((x >>= 1) != 0) // 添加括号以确保先进行右移赋值操作
        {
            log++;
        }

        return log;
    }

    #endregion
  
    // 通用辅助方法

    /// <summary>
    /// 检查位索引是否在有效范围内。
    /// </summary>
    private static bool IsValidIndex(int index, int bitSize)
    {
        return index >= 0 && index < bitSize;
    }

    // 高级位操作方法

    /// <summary>
    /// 反转 byte 中的所有位。
    /// 使用查找表方式实现。
    /// </summary>
    private static readonly byte[] ByteReverseTable = new byte[256];

    /// <summary>
    /// 计算 int 的汉明重量（设置的位数）。
    /// 使用查找表方式实现。
    /// </summary>
    private static readonly int[] HammingWeightTable = new int[256];

    // 静态构造函数，用于初始化查找表
    static BitTwiddlingHacks()
    {
        for (int i = 0; i < 256; i++)
        {
            ByteReverseTable[i] = ReverseBitsInByte((byte)i);
            HammingWeightTable[i] = CountBitsInByte((byte)i);
        }
    }

    private static byte ReverseBitsInByte(byte b)
    {
        b = (byte)((b * 0x0802u & 0x22110u) | (b * 0x8020u & 0x88440u));
        b = (byte)((b * 0x10101u) >> 16);
        return b;
    }

    private static int CountBitsInByte(byte b)
    {
        int count = 0;
        while (b != 0)
        {
            count += b & 1;
            b >>= 1;
        }

        return count;
    }

    /// <summary>
    /// 反转 byte 的所有位。
    /// </summary>
    public static byte ReverseBits(byte b)
    {
        return ByteReverseTable[b];
    }

    /// <summary>
    /// 反转 int 的所有位。
    /// 使用逐字节反转和查找表。
    /// </summary>
    public static int ReverseBits(int x)
    {
        return (ByteReverseTable[x & 0xFF] << 24) |
               (ByteReverseTable[(x >> 8) & 0xFF] << 16) |
               (ByteReverseTable[(x >> 16) & 0xFF] << 8) |
               (ByteReverseTable[(x >> 24) & 0xFF]);
    }

    /// <summary>
    /// 反转 long 的所有位。
    /// 使用逐字节反转和查找表。
    /// </summary>
    public static long ReverseBits(long x)
    {
        return ((long)ReverseBits((int)x) << 32) | ((long)ReverseBits((int)(x >> 32)) & 0xFFFFFFFFL);
    }

    /// <summary>
    /// 使用查找表计算 byte 的汉明重量。
    /// </summary>
    public static int GetHammingWeight(byte b)
    {
        return HammingWeightTable[b];
    }

    /// <summary>
    /// 使用查找表计算 int 的汉明重量。
    /// </summary>
    public static int GetHammingWeight(int x)
    {
        uint u = (uint)x;
        return HammingWeightTable[u & 0xFF] +
               HammingWeightTable[(u >> 8) & 0xFF] +
               HammingWeightTable[(u >> 16) & 0xFF] +
               HammingWeightTable[(u >> 24) & 0xFF];
    }

    /// <summary>
    /// 使用查找表计算 long 的汉明重量。
    /// </summary>
    public static int GetHammingWeight(long x)
    {
        ulong u = (ulong)x;
        return HammingWeightTable[u & 0xFF] +
               HammingWeightTable[(u >> 8) & 0xFF] +
               HammingWeightTable[(u >> 16) & 0xFF] +
               HammingWeightTable[(u >> 24) & 0xFF] +
               HammingWeightTable[(u >> 32) & 0xFF] +
               HammingWeightTable[(u >> 40) & 0xFF] +
               HammingWeightTable[(u >> 48) & 0xFF] +
               HammingWeightTable[(u >> 56) & 0xFF];
    }

    /// <summary>
    /// 位旋转 - 左旋转指定数量的位。
    /// </summary>
    public static int RotateLeft(int x, int n)
    {
        n &= 31; // 确保旋转位数在0-31之间
        return (x << n) | (x >> (IntBitsSize - n));
    }

    /// <summary>
    /// 位旋转 - 右旋转指定数量的位。
    /// </summary>
    public static int RotateRight(int x, int n)
    {
        n &= 31; // 确保旋转位数在0-31之间
        return (x >> n) | (x << (IntBitsSize - n));
    }

    /// <summary>
    /// 位旋转 - 左旋转指定数量的位。
    /// </summary>
    public static long RotateLeft(long x, int n)
    {
        n &= 63; // 确保旋转位数在0-63之间
        return (x << n) | (x >> (LongBitsSize - n));
    }

    /// <summary>
    /// 位旋转 - 右旋转指定数量的位。
    /// </summary>
    public static long RotateRight(long x, int n)
    {
        n &= 63; // 确保旋转位数在0-63之间
        return (x >> n) | (x << (LongBitsSize - n));
    }

    /// <summary>
    /// 位选取 - 提取指定范围内的位。
    /// </summary>
    /// <param name="x">源数。</param>
    /// <param name="start">起始位（包含）。</param>
    /// <param name="count">要提取的位数。</param>
    /// <returns>提取后的位。</returns>
    public static int ExtractBits(int x, int start, int count)
    {
        if (start < 0 || start + count > IntBitsSize || count <= 0)
        {
            GameDebug.LogError("ExtractBits: 无效的范围 start:{0} count:{1}", start, count);
            return 0;
        }

        int mask = (1 << count) - 1;
        return (x >> start) & mask;
    }

    /// <summary>
    /// 位选取 - 提取指定范围内的位。
    /// </summary>
    public static long ExtractBits(long x, int start, int count)
    {
        if (start < 0 || start + count > LongBitsSize || count <= 0)
        {
            GameDebug.LogError("ExtractBits: 无效的范围 start:{0} count:{1}", start, count);
            return 0;
        }

        long mask = (1L << count) - 1;
        return (x >> start) & mask;
    }

    /// <summary>
    /// 位合并 - 将两个 int 的特定位合并为一个 int。
    /// </summary>
    public static int MergeBits(int x, int y, int xStart, int yStart, int count)
    {
        if (xStart < 0 || xStart + count > IntBitsSize ||
            yStart < 0 || yStart + count > IntBitsSize || count <= 0)
        {
            GameDebug.LogError("MergeBits: 无效的范围 xStart:{0} yStart:{1} count:{2}", xStart, yStart, count);
            return 0;
        }

        int xBits = ExtractBits(x, xStart, count);
        int yBits = ExtractBits(y, yStart, count);
        return (xBits << yStart) | yBits;
    }

    /// <summary>
    /// Morton 编码 - 将两个 short 数的位交错合并生成一个 int。
    /// </summary>
    public static int MortonEncode(short x, short y)
    {
        int morton = 0;
        for (int i = 0; i < 16; i++)
        {
            morton |= ((x >> i) & 1) << (2 * i);
            morton |= ((y >> i) & 1) << (2 * i + 1);
        }

        return morton;
    }

    /// <summary>
    /// Morton 解码 - 从 Morton 编码中提取 x 和 y 的值。
    /// </summary>
    public static void MortonDecode(int morton, out short x, out short y)
    {
        int tempX = 0;
        int tempY = 0;
        for (int i = 0; i < 16; i++)
        {
            tempX |= ((morton >> (2 * i)) & 1) << i;
            tempY |= ((morton >> (2 * i + 1)) & 1) << i;
        }
        x = (short)tempX;
        y = (short)tempY;
    }
    /// <summary>
    /// 计算整数的连续尾部零位数。
    /// 如果x为0，返回整数位数。
    /// </summary>
    public static int CountTrailingZeros(int x)
    {
        if (x == 0) return IntBitsSize;
        int count = 0;
        while ((x & 1) == 0)
        {
            count++;
            x >>= 1;
        }

        return count;
    }

    /// <summary>
    /// 计算长整数的连续尾部零位数。
    /// 如果x为0，返回长整数位数。
    /// </summary>
    public static int CountTrailingZeros(long x)
    {
        if (x == 0) return LongBitsSize;
        int count = 0;
        while ((x & 1L) == 0)
        {
            count++;
            x >>= 1;
        }

        return count;
    }
    
    /// <summary>
    /// 并行计算位集：你已经实现了CountSetBits方法，但它只采用了Kernighan算法。可以增加一个并行算法，用于处理更大的数据类型或多个数据集。特别是在使用64位数据时，使用并行计算可以提高效率。
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public static int CountSetBitsParallel(int map)
    {
        map -= (map >> 1) & 0x55555555;
        map = (map & 0x33333333) + ((map >> 2) & 0x33333333);
        map = (map + (map >> 4)) & 0x0F0F0F0F;
        map += map >> 8;
        map += map >> 16;
        return map & 0x3F;
    }
    /// <summary>
    /// 计算汉明距离：可以添加一个计算两个整数之间汉明距离的方法，用于计算两个二进制数中不同位的个数。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int HammingDistance(int x, int y)
    {
        return CountSetBits(x ^ y);
    }
    /// <summary>
    /// 位操作集合：你可以考虑添加一些其他常见的位操作功能，例如：
    /// 获取整数的二进制表示：通过一个方法返回整数的二进制字符串表示。
    /// 检查一个数是否为2的幂：你可以直接使用(x & (x - 1)) == 0来检查。
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static bool IsPowerOfTwo(int x)
    {
        return x > 0 && (x & (x - 1)) == 0;
    }
    
    /// <summary>
    /// 快速计算以2为底的对数，返回最高设置位的位置。
    /// 如果x为0或负数，返回-1。
    /// 尝试使用 BitOperations，如果不可用，则使用备用实现。
    /// </summary>
    public static int FastLog2(int x)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        if (x <= 0) return -1;
        return 31 - BitOperations.LeadingZeroCount((uint)x);
#else
        return Log2(x); // 使用已有的 Log2 实现
#endif
    }

    /// <summary>
    /// 快速计算以2为底的对数，返回最高设置位的位置。
    /// 如果x为0或负数，返回-1。
    /// 尝试使用 BitOperations，如果不可用，则使用备用实现。
    /// </summary>
    public static int FastLog2(long x)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        if (x <= 0) return -1;
        return 63 - BitOperations.LeadingZeroCount((ulong)x);
#else
        return Log2(x); // 使用已有的 Log2 实现
#endif
    }
}
}

