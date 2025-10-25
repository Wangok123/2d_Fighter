using System.Data;
using System.Runtime.InteropServices;

namespace LatProtocol.Utils
{
    public partial class BufferReader : IDisposable, IAsyncDisposable
    {
        private MemoryStream stream;
        private BinaryReader reader;
        private bool _isNetworkData = false;

        /// <summary>
        /// 是否网络序 Unity为小端，而Jave需要大端
        ///     小端字节序,其中高字节在前，低字节在后
        ///     大端字节序,其中低字节在前，高字节在后
        /// </summary>
        public bool IsNetworkData
        {
            get => _isNetworkData;
            set => _isNetworkData = value;
        }

        public BufferReader(byte[] data, bool isNetworkData) : this(data)
        {
            // IsNetworkData = BitConverter.IsLittleEndian;
            IsNetworkData = isNetworkData;
        }

        public BufferReader(byte[] data)
        {
            stream = new MemoryStream(data);
            reader = new BinaryReader(stream);
        }

        public void Update(byte[] data)
        {
            stream.Position = 0L;
            stream.SetLength(data.Length);
            stream.Write(data, 0, data.Length);
        }

        public void Update(byte[] data, int offset, int length)
        {
            stream.Position = 0L;
            stream.SetLength(length);
            stream.Write(data, offset, length);
        }

        #region 基本的属性/方法

        /// <summary>
        /// 暴露内部的 MemoryStream，若不希望外部修改可改为 internal 或去掉。
        /// </summary>
        public MemoryStream Stream => stream;

        /// <summary>
        /// 当前流的位置（通常是读或写的指针）。
        /// </summary>
        public int Position
        {
            get => (int)stream.Position;
            set => stream.Position = value;
        }

        /// <summary>
        /// 返回当前流的长度。
        /// </summary>
        public int Length => (int)stream.Length;

        /// <summary>
        /// 重置流到 0 长度，并根据选项是否清除底层缓冲区。
        /// isClearArray = true 时只清空已用长度部分，也可改成清除 buffer.Length。
        /// </summary>
        /// <param name="isClearArray">是否清除底层已用内存</param>
        public void Reset(bool isClearArray = false)
        {
            if (stream == null) return;

            if (isClearArray)
            {
                byte[] buffer = stream.GetBuffer();
                // 清除当前长度范围内的数据
                Array.Clear(buffer, 0, (int)stream.Length);

                // 如果要彻底清除整个底层缓冲区，可改为：
                // Array.Clear(buffer, 0, buffer.Length);
            }

            stream.Position = 0L;
            stream.SetLength(0);
        }

        /// <summary>
        /// 重置长度到指定 length（并清零当前已用的数据）。
        /// </summary>
        public void ResetLength(int length)
        {
            if (stream == null) return;

            byte[] buffer = stream.GetBuffer();
            if (buffer.Length >= length)
            {
                Array.Clear(buffer, 0, length);
            }
            else
            {
                Array.Clear(buffer, 0, (int)stream.Length);
            }

            stream.Position = 0L;
            stream.SetLength(length);
        }

        public void Dispose()
        {
            stream?.Dispose();
            reader?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (stream != null) await stream.DisposeAsync();
            if (reader != null) reader.Dispose();
        }

        #endregion

        #region 基础数据类型

        /// <summary>确保 reader 已初始化，否则无法读</summary>
        private void EnsureReader()
        {
            if (reader == null)
                throw new InvalidOperationException("ByteBuffer is not in read mode (reader is null).");
        }

        /// <summary>读单个字节。</summary>
        public byte ReadByte()
        {
            EnsureReader();
            return reader.ReadByte();
        }

        public bool ReadBool()
        {
            EnsureReader();
            bool value = reader.ReadBoolean();
            return value;
        }

        public short ReadShort()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            short value;
            if (IsNetworkData)
            {
                // 大端（网络序）：高字节在前
                value = (short)((buffer[pos] << 8) |
                                (buffer[pos + 1] & 0xFF));
            }
            else
            {
                // 小端：低字节在前
                value = (short)((buffer[pos] & 0xFF) |
                                ((buffer[pos + 1] & 0xFF) << 8));
            }

            stream.Position += 2;
            return value;
        }

        public ushort ReadUShort()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            ushort value;
            if (IsNetworkData)
            {
                value = (ushort)(((buffer[pos] & 0xFF) << 8) |
                                 (buffer[pos + 1] & 0xFF));
            }
            else
            {
                value = (ushort)(((buffer[pos] & 0xFF)) |
                                 ((buffer[pos + 1] & 0xFF) << 8));
            }

            stream.Position += 2;
            return value;
        }

        public int ReadInt()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            int value;
            if (IsNetworkData)
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

            stream.Position += 4;
            return value;
        }

        public uint ReadUInt()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            uint value;
            if (IsNetworkData)
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

            stream.Position += 4;
            return value;
        }

        public long ReadLong()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            long value;
            if (IsNetworkData)
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

            stream.Position += 8;
            return value;
        }

        public ulong ReadULong()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            ulong value;
            if (IsNetworkData)
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

            stream.Position += 8;
            return value;
        }

        public float ReadFloat()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            int bits;
            if (IsNetworkData)
            {
                bits = (buffer[pos] << 24) |
                       (buffer[pos + 1] << 16) |
                       (buffer[pos + 2] << 8) |
                       buffer[pos + 3];
            }
            else
            {
                bits = buffer[pos] |
                       (buffer[pos + 1] << 8) |
                       (buffer[pos + 2] << 16) |
                       (buffer[pos + 3] << 24);
            }

            stream.Position += 4;
            return BitConverter.Int32BitsToSingle(bits);
        }

        public double ReadDouble()
        {
            EnsureReader();
            byte[] buffer = stream.GetBuffer();
            int pos = (int)stream.Position;
            long bits;
            if (IsNetworkData)
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

            stream.Position += 8;
            return BitConverter.Int64BitsToDouble(bits);
        }

        #endregion

        #region 泛型处理

        public T Read<T>()
        {
            var type = typeof(T);
            return (T)(object)(type switch
            {
                // ------ 基本类型 ------
                var t when t == typeof(byte) => ReadByte(),
                var t when t == typeof(bool) => ReadBool(),
                var t when t == typeof(int) => ReadInt(),
                var t when t == typeof(uint) => ReadUInt(),
                var t when t == typeof(short) => ReadShort(),
                var t when t == typeof(ushort) => ReadUShort(),
                var t when t == typeof(long) => ReadLong(),
                var t when t == typeof(ulong) => ReadULong(),
                var t when t == typeof(float) => ReadFloat(),
                var t when t == typeof(double) => ReadDouble(),
                var t when t == typeof(string) => ReadString(),

                // ------ 数组类型 ------
                var t when t == typeof(byte[]) => ReadByteArray(), // 先读长度, 再 read
                var t when t == typeof(short[]) => ReadShortArray(),
                var t when t == typeof(ushort[]) => ReadUShortArray(),
                var t when t == typeof(int[]) => ReadIntArray(),
                var t when t == typeof(uint[]) => ReadUIntArray(),
                var t when t == typeof(long[]) => ReadLongArray(),
                var t when t == typeof(ulong[]) => ReadULongArray(),
                var t when t == typeof(float[]) => ReadFloatArray(),
                var t when t == typeof(double[]) => ReadDoubleArray(),

                _ => throw new DataException($"Read<T> 不支持的类型: {type}")
            });
        }

        public T[] ReadArray<T>() where T : unmanaged
        {
            EnsureReader();
            int len = ReadInt();
            if (len < 0) return null;

            int size = Marshal.SizeOf<T>();


            byte[] bytes = reader.ReadBytes(len * size);

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < bytes.Length; i += size)
                {
                    Array.Reverse(bytes, i, size);
                }
            }

            T[] arr = new T[len];
            Buffer.BlockCopy(bytes, 0, arr, 0, bytes.Length);
            return arr;
        }

        #endregion
    }
}