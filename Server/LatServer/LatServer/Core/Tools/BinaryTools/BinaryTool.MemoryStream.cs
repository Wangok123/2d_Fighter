using System.Text;

namespace LatServer.Core.Tools.BinaryTools
{
    /// <summary>
    /// 二进制工具类
    ///     提供 MemoryStream 读取扩展
    /// </summary>
    public static partial class BinaryTool
    {
        public static string PrintBytes(string tag, byte[] bytes, int startIndex, int endIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(tag);
            sb.Append("] ");
            sb.AppendFormat("len:{0} >>> ", bytes.Length);

            for (var i = 0; i < bytes.Length; i++)
            {
                if (i < startIndex)
                    continue;
                if (i > endIndex)
                    break;
                sb.Append(bytes[i]);
                sb.Append(",");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 注意 从当前Postion开始读取 读取完毕后自动偏移 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static byte ReadByte(ref MemoryStream ms)
        {
            if (ms == null)
            {
                throw new NullReferenceException("StreamHelper.ReadByte , MemoryStream=null");
            }

            int num = (int)ms.Position + 1;
            if (num > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadByte , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            ms.Position += 1;
            byte[] buffer = ms.GetBuffer();
            return buffer[num - 1];
        }

        /// <summary>
        /// BinaryReader.ReadBytes
        /// 实测 执行10000000次
        ///     自己写的ReadBytes ~= 453ms  节约new BinaryReader
        ///     BinaryReader.ReadBytes ~= 703ms
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static byte[] ReadBytes(ref MemoryStream ms, int count)
        {
            if (ms == null)
            {
                throw new NullReferenceException("BReader.ReadByte , MemoryStream=null");
            }

            int allLength = (int)ms.Position + count;
            if (allLength > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"BReader.ReadByte , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }
            else
            {
                // BinaryReader.ReadBytes
                byte[] numArray = new byte[count];
                int length = 0;
                while (count > 0)
                {
                    int num = ms.Read(numArray, length, count);
                    if (num == 0)
                    {
                        break; // 没有读取到字节时跳出
                    }

                    length += num;
                    count -= num;
                }

                if (length != numArray.Length)
                {
                    byte[] dst = new byte[length];
                    // Buffer.InternalBlockCopy((Array) numArray, 0, (Array) dst, 0, length);
                    Buffer.BlockCopy((Array)numArray, 0, (Array)dst, 0, length);
                    numArray = dst;
                }

                return numArray;
            }
        }

        /// <summary>
        /// 无额外内存分配地读取 2 字节 short 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static short ReadShort(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            short value;
            if (isNetworkOrder)
            {
                // 数据为大端（网络序）：高字节在前
                value = (short)((buffer[pos] << 8) |
                                (buffer[pos + 1] & 0xFF));
            }
            else
            {
                // 数据为小端：低字节在前
                value = (short)((buffer[pos] & 0xFF) |
                                ((buffer[pos + 1] & 0xFF) << 8));
            }

            ms.Position += 2;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 2 字节 ushort 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static ushort ReadUShort(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            ushort value;
            if (isNetworkOrder)
            {
                value = (ushort)(((buffer[pos] & 0xFF) << 8) |
                                 (buffer[pos + 1] & 0xFF));
            }
            else
            {
                value = (ushort)(((buffer[pos] & 0xFF)) |
                                 ((buffer[pos + 1] & 0xFF) << 8));
            }

            ms.Position += 2;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 4 字节 int 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static int ReadInt(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            int value;
            if (isNetworkOrder)
            {
                // 大端：高位字节先
                value = (buffer[pos] << 24) |
                        (buffer[pos + 1] << 16) |
                        (buffer[pos + 2] << 8) |
                        buffer[pos + 3];
            }
            else
            {
                // 小端：低位字节先
                value = buffer[pos] |
                        (buffer[pos + 1] << 8) |
                        (buffer[pos + 2] << 16) |
                        (buffer[pos + 3] << 24);
            }
            ms.Position += 4;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 4 字节 uint 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static uint ReadUInt(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            uint value;
            if (isNetworkOrder)
            {
                value = ((uint)(buffer[pos] & 0xFF) << 24) |
                        ((uint)(buffer[pos + 1] & 0xFF) << 16) |
                        ((uint)(buffer[pos + 2] & 0xFF) << 8) |
                        ((uint)buffer[pos + 3] & 0xFF);
            }
            else
            {
                value = ((uint)(buffer[pos] & 0xFF)) |
                        ((uint)(buffer[pos + 1] & 0xFF) << 8) |
                        ((uint)(buffer[pos + 2] & 0xFF) << 16) |
                        ((uint)(buffer[pos + 3] & 0xFF) << 24);
            }

            ms.Position += 4;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 8 字节 long 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static long ReadLong(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            long value;
            if (isNetworkOrder)
            {
                value = ((long)(buffer[pos] & 0xFFL) << 56) |
                        ((long)(buffer[pos + 1] & 0xFFL) << 48) |
                        ((long)(buffer[pos + 2] & 0xFFL) << 40) |
                        ((long)(buffer[pos + 3] & 0xFFL) << 32) |
                        ((long)(buffer[pos + 4] & 0xFFL) << 24) |
                        ((long)(buffer[pos + 5] & 0xFFL) << 16) |
                        ((long)(buffer[pos + 6] & 0xFFL) << 8) |
                        ((long)(buffer[pos + 7] & 0xFFL));
            }
            else
            {
                value = ((long)(buffer[pos] & 0xFFL)) |
                        ((long)(buffer[pos + 1] & 0xFFL) << 8) |
                        ((long)(buffer[pos + 2] & 0xFFL) << 16) |
                        ((long)(buffer[pos + 3] & 0xFFL) << 24) |
                        ((long)(buffer[pos + 4] & 0xFFL) << 32) |
                        ((long)(buffer[pos + 5] & 0xFFL) << 40) |
                        ((long)(buffer[pos + 6] & 0xFFL) << 48) |
                        ((long)(buffer[pos + 7] & 0xFFL) << 56);
            }

            ms.Position += 8;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 8 字节 ulong 数值，根据 IsNetworkData 判断字节序。
        /// </summary>
        public static ulong ReadULong(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            ulong value;
            if (isNetworkOrder)
            {
                value = ((ulong)(buffer[pos] & 0xFFUL) << 56) |
                        ((ulong)(buffer[pos + 1] & 0xFFUL) << 48) |
                        ((ulong)(buffer[pos + 2] & 0xFFUL) << 40) |
                        ((ulong)(buffer[pos + 3] & 0xFFUL) << 32) |
                        ((ulong)(buffer[pos + 4] & 0xFFUL) << 24) |
                        ((ulong)(buffer[pos + 5] & 0xFFUL) << 16) |
                        ((ulong)(buffer[pos + 6] & 0xFFUL) << 8) |
                        ((ulong)(buffer[pos + 7] & 0xFFUL));
            }
            else
            {
                value = ((ulong)(buffer[pos] & 0xFFUL)) |
                        ((ulong)(buffer[pos + 1] & 0xFFUL) << 8) |
                        ((ulong)(buffer[pos + 2] & 0xFFUL) << 16) |
                        ((ulong)(buffer[pos + 3] & 0xFFUL) << 24) |
                        ((ulong)(buffer[pos + 4] & 0xFFUL) << 32) |
                        ((ulong)(buffer[pos + 5] & 0xFFUL) << 40) |
                        ((ulong)(buffer[pos + 6] & 0xFFUL) << 48) |
                        ((ulong)(buffer[pos + 7] & 0xFFUL) << 56);
            }

            ms.Position += 8;
            return value;
        }

        /// <summary>
        /// 无额外内存分配地读取 4 字节 float 数值，根据 IsNetworkData 判断字节序。
        /// 先直接从缓冲区读取 4 字节组成一个 int，再用 BitConverter.Int32BitsToSingle 转换为 float。
        /// </summary>
        public static float ReadFloat(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            int bits;
            if (isNetworkOrder)
            {
                bits = (buffer[pos] << 24) | (buffer[pos + 1] << 16) | (buffer[pos + 2] << 8) | buffer[pos + 3];
            }
            else
            {
                bits = buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
            }

            ms.Position += 4;
            return BitConverter.Int32BitsToSingle(bits);
        }

        /// <summary>
        /// 无额外内存分配地读取 8 字节 double 数值，根据 IsNetworkData 判断字节序。
        /// 先直接从缓冲区读取 8 字节组成一个 long，再用 BitConverter.Int64BitsToDouble 转换为 double。
        /// </summary>
        public static double ReadDouble(ref MemoryStream ms, bool isNetworkOrder = false)
        {
            byte[] buffer = ms.GetBuffer();
            int pos = (int)ms.Position;
            long bits;
            if (isNetworkOrder)
            {
                bits = ((long)(buffer[pos] & 0xFFL) << 56) |
                       ((long)(buffer[pos + 1] & 0xFFL) << 48) |
                       ((long)(buffer[pos + 2] & 0xFFL) << 40) |
                       ((long)(buffer[pos + 3] & 0xFFL) << 32) |
                       ((long)(buffer[pos + 4] & 0xFFL) << 24) |
                       ((long)(buffer[pos + 5] & 0xFFL) << 16) |
                       ((long)(buffer[pos + 6] & 0xFFL) << 8) |
                       ((long)(buffer[pos + 7] & 0xFFL));
            }
            else
            {
                bits = ((long)(buffer[pos] & 0xFFL)) |
                       ((long)(buffer[pos + 1] & 0xFFL) << 8) |
                       ((long)(buffer[pos + 2] & 0xFFL) << 16) |
                       ((long)(buffer[pos + 3] & 0xFFL) << 24) |
                       ((long)(buffer[pos + 4] & 0xFFL) << 32) |
                       ((long)(buffer[pos + 5] & 0xFFL) << 40) |
                       ((long)(buffer[pos + 6] & 0xFFL) << 48) |
                       ((long)(buffer[pos + 7] & 0xFFL) << 56);
            }

            ms.Position += 8;
            return BitConverter.Int64BitsToDouble(bits);
        }
    }
}