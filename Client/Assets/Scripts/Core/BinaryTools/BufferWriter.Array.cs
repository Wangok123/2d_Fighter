using System.Text;
using LATLog;

namespace Core.BinaryTools
{ 
    /// <summary>
    /// 数组处理
    /// </summary>
    public partial class BufferWriter
    {
        /// <summary>
        /// 写字符串：先写长度(int)，再写 UTF8 内容。
        /// 若字符串为 null，长度写 -1。
        /// </summary>
        public void WriteString(string value)
        {
            EnsureWriter();
            if (value == null)
            {
                WriteInt(-1);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length); // 长度（大端序）
            writer.Write(bytes); // 原样写出
        }

        public void WriteByteArray(byte[] data, bool isWriteLength = true)
        {
            EnsureWriter();
            if (data == null || data.Length == 0)
            {
                GameDebug.LogError("WriteByteArray data is null");
                WriteInt(-1);
                return;
            }

            WriteInt(data.Length);
            writer.Write(data);
        }

        public void WriteFloatArray(float[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var f in values)
            {
                WriteFloat(f);
            }
        }

        public void WriteDoubleArray(double[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var d in values)
            {
                WriteDouble(d);
            }
        }

        public void WriteShortArray(short[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteShort(v);
            }
        }

        public void WriteUShortArray(ushort[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteUShort(v);
            }
        }


        public void WriteIntArray(int[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteInt(v);
            }
        }

        public void WriteUIntArray(uint[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteUInt(v);
            }
        }

        public void WriteLongArray(long[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteLong(v);
            }
        }

        public void WriteULongArray(ulong[] values)
        {
            EnsureWriter();
            if (values == null)
            {
                WriteInt(-1);
                return;
            }

            WriteInt(values.Length);
            foreach (var v in values)
            {
                WriteULong(v);
            }
        }
    }

   
}