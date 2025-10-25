using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Core.BinaryTools
{
    public partial class BufferWriter : IDisposable, IAsyncDisposable
    {
        private MemoryStream stream = null;
        private BinaryWriter writer = null;

        private bool isNetworkData = false;

        /// <summary>
        /// 是否网络序 Unity为小端，而Jave需要大端
        ///     小端字节序,其中高字节在前，低字节在后
        ///     大端字节序,其中低字节在前，高字节在后
        /// </summary>
        public bool IsNetworkData
        {
            get => isNetworkData;
            set => isNetworkData = value;
        }

        public BufferWriter()
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public BufferWriter(bool isNetworkData) : this()
        {
            // IsNetworkData = BitConverter.IsLittleEndian;
            IsNetworkData = isNetworkData;
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
        /// 将当前写入的内容转为 byte[]（复制一份新的数组）。
        /// </summary>
        public byte[] ToArray()
        {
            writer?.Flush();
            return stream.ToArray();
        }

        /// <summary>
        /// 获取内部 buffer，不会拷贝，只是返回底层的引用。
        /// 注意：此数组可能大于实际内容长度，使用时要小心 length。
        /// </summary>
        public byte[] GetBuffer()
        {
            writer?.Flush();
            return stream.GetBuffer();
        }

        /// <summary>
        /// 刷新 Writer 缓冲区。
        /// </summary>
        public void Flush()
        {
            writer?.Flush();
        }

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
            writer?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (stream != null) await stream.DisposeAsync();
            if (writer != null) await writer.DisposeAsync();
        }

        #endregion

        #region 基础数据类型

        /// <summary>确保 writer 已初始化，否则无法写</summary>
        private void EnsureWriter()
        {
            if (writer == null)
                throw new InvalidOperationException("ByteBuffer is not in write mode (writer is null).");
        }

        /// <summary>写 float，使用大端序（network order）。</summary>
        public void WriteFloat(float value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>写 double，使用大端序（network order）。</summary>
        public void WriteDouble(double value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>写单个字节（无大小端差异）。</summary>
        public void WriteByte(byte value)
        {
            EnsureWriter();
            writer.Write(value);
        }

        /// <summary>写 bool，内部用 0/1 表示。</summary>
        public void WriteBool(bool value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            writer.Write(bytes);
            // DataConverter.ToBytes(value, stream.GetBuffer(), (int)stream.Position);
        }

        /// <summary>写 short，使用大端序（network order）。</summary>
        public void WriteShort(short value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>写 ushort，使用大端序（network order）。</summary>
        public void WriteUShort(ushort value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>写 int，使用大端序（network order）。</summary>
        public void WriteInt(int value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        public void WriteUInt(uint value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData)
            {
                Array.Reverse(bytes);
            }

            writer.Write(bytes);
        }

        /// <summary>写 long，使用大端序（network order）。</summary>
        public void WriteLong(long value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        /// <summary>写 ulong，使用大端序。</summary>
        public void WriteULong(ulong value)
        {
            EnsureWriter();
            byte[] bytes = BitConverter.GetBytes(value);
            if (IsNetworkData) Array.Reverse(bytes);
            writer.Write(bytes);
        }

        #endregion

        #region 泛型处理
        public void Write(byte[] data)
        {
            writer.Write(data, 0, data.Length);
        }
        public void Write(byte[] data, int startIndex, int lenght)
        {
            writer.Write(data, startIndex, lenght);
        }

        public void Write<T>(T data) where T : unmanaged
        {
            var type = typeof(T);
            switch (type)
            {
                // ---------- 基本类型 ----------
                case var t when t == typeof(byte):
                    WriteByte((byte)Convert.ToByte(data));
                    break;
                case var t when t == typeof(bool):
                    WriteBool(Convert.ToBoolean(data));
                    break;
                case var t when t == typeof(int):
                    WriteInt(Convert.ToInt32(data));
                    break;
                case var t when t == typeof(uint):
                    WriteUInt(Convert.ToUInt32(data));
                    break;
                case var t when t == typeof(short):
                    WriteShort(Convert.ToInt16(data));
                    break;
                case var t when t == typeof(ushort):
                    WriteUShort(Convert.ToUInt16(data));
                    break;
                case var t when t == typeof(long):
                    WriteLong(Convert.ToInt64(data));
                    break;
                case var t when t == typeof(ulong):
                    WriteULong(Convert.ToUInt64(data));
                    break;
                case var t when t == typeof(float):
                    WriteFloat(Convert.ToSingle(data));
                    break;
                case var t when t == typeof(double):
                    WriteDouble(Convert.ToDouble(data));
                    break;
                case var t when t == typeof(string):
                    WriteString(data as string);
                    break;


                // ---------- 数组类型 ----------
                case var t when t == typeof(byte[]):
                    WriteByteArray(data as byte[]);
                    break;
                case var t when t == typeof(short[]):
                    WriteShortArray(data as short[]);
                    break;
                case var t when t == typeof(ushort[]):
                    WriteUShortArray(data as ushort[]);
                    break;
                case var t when t == typeof(int[]):
                    WriteIntArray(data as int[]);
                    break;
                case var t when t == typeof(uint[]):
                    WriteUIntArray(data as uint[]);
                    break;
                case var t when t == typeof(long[]):
                    WriteLongArray(data as long[]);
                    break;
                case var t when t == typeof(ulong[]):
                    WriteULongArray(data as ulong[]);
                    break;
                case var t when t == typeof(float[]):
                    WriteFloatArray(data as float[]);
                    break;
                case var t when t == typeof(double[]):
                    WriteDoubleArray(data as double[]);
                    break;

                // ---------- Unity类型 ----------
                // case var t when t == typeof(Vector2):
                //     break;
                // case var t when t == typeof(Vector3):
                //     break;
                // case var t when t == typeof(Vector4):
                //     break;
                // case var t when t == typeof(Color):
                //     break;
                // case var t when t == typeof(Quaternion):
                //     break;
                // case var t when t == typeof(Rect):
                //     break;
                // case var t when t == typeof(Bounds):
                //     break;
                // case var t when t == typeof(Matrix4x4):
                //     break;
                // case var t when t == typeof(Vector2Int):
                //     break;
                // case var t when t == typeof(Vector3Int):
                //     break;
                // case var t when t == typeof(RangeInt):
                //     break;
                // case var t when t == typeof(RectInt):
                //     break;
                // case var t when t == typeof(Color32):
                //     break;
                // case var t when t == typeof(BoundsInt):
                //     break;
                // case var t when t == typeof(Pose):
                //     break;
                // case var t when t == typeof(DateTime):
                //     break;
                default:
                    throw new DataException($"Write<T> 不支持的类型: {type}");
            }
        }

        public void WriteArray<T>(T[] values) where T : unmanaged
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            int size = Marshal.SizeOf<T>();
            byte[] bytes = new byte[values.Length * size];
            Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);

            if (IsNetworkData)
            {
                for (int i = 0; i < bytes.Length; i += size)
                {
                    Array.Reverse(bytes, i, size);
                }
            }

            writer.Write(bytes);
        }

        #endregion
    }
}