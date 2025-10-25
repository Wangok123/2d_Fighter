using Google.Protobuf;

namespace LatProtocol
{
    public class ProtocolProcessor
    {
        // 发送消息
        // protocolId: 协议号
        // message: 消息体
        // 返回值: byte[] 数组
        // 注意: 这里使用了 MemoryStream 来避免多次分配内存，提高性能
        public static byte[] SendMessage<T>(ushort protocolId, T message) where T : IMessage
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            // 写入协议头 (2字节)
            writer.Write(protocolId);

            int len = message.CalculateSize();
            // 写入消息长度 (4字节)
            writer.Write(len);
            // 写入Protobuf消息
            message.WriteTo(ms);

            return ms.ToArray();
        }

        // 处理单个数据包
        public static void ProcessPacket(byte[] packet, out ushort cmdId, out byte[] message)
        {
            try
            {
                using var ms = new MemoryStream(packet);
                using var reader = new BinaryReader(ms);
                // 读取协议号 类型为uint 
                ushort protocolId = reader.ReadUInt16();

                // 读取消息长度 (4字节)
                int messageLength = reader.ReadInt32();

                // 验证数据完整性
                if (messageLength > ms.Length - ms.Position)
                {
                    throw new InvalidDataException(
                        $"Invalid message length. Expected: {messageLength}, Available: {ms.Length - ms.Position}");
                }

                // 读取消息体
                byte[] messageData = reader.ReadBytes(messageLength);
                cmdId = protocolId;
                message = messageData;
            }
            catch (Exception ex)
            {
                // 在实际项目中应该记录日志
                Console.WriteLine($"Error processing packet: {ex.Message}");
                cmdId = 0;
                message = Array.Empty<byte>();
            }
        }
    }
}