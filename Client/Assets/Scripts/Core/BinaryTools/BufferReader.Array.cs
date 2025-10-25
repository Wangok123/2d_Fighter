using System.Text;

namespace Core.BinaryTools
{
    /// <summary>
    /// 数组处理
    /// </summary>
    public partial class BufferReader
    {
        /// <summary>
        /// 读字符串：先读长度(int)，若为 -1 则返回 null，否则读对应长度字节并用 UTF8 解码。
        /// </summary>
        public string ReadString()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null; // -1 表示 null
            byte[] buffer = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 读取指定长度的 byte[]。
        /// </summary>
        public byte[] ReadByteArray()
        {
            EnsureReader();
            int length = ReadInt();
            if (length < 0) return null;
            return reader.ReadBytes(length);
        }

        public float[] ReadFloatArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            float[] arr = new float[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadFloat();
            }

            return arr;
        }

        public double[] ReadDoubleArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            double[] arr = new double[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadDouble();
            }

            return arr;
        }

        public short[] ReadShortArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            short[] arr = new short[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadShort();
            }

            return arr;
        }

        public ushort[] ReadUShortArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            ushort[] arr = new ushort[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadUShort();
            }

            return arr;
        }

        public int[] ReadIntArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            int[] arr = new int[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadInt();
            }

            return arr;
        }

        public uint[] ReadUIntArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            uint[] arr = new uint[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadUInt();
            }

            return arr;
        }

        public long[] ReadLongArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            long[] arr = new long[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadLong();
            }

            return arr;
        }

        public ulong[] ReadULongArray()
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;
            ulong[] arr = new ulong[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = ReadULong();
            }

            return arr;
        }
    }
}