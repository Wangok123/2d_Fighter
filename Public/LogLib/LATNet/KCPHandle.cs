using System.Buffers;
using System.Net.Sockets.Kcp;

namespace LATNet;

public class KCPHandle : IKcpCallback
{
    public Action<Memory<byte>>? OutputAction { get; set; }
    public Action<byte[]> ReceiveAction { get; set; }
    public void Output(IMemoryOwner<byte> buffer, int avalidLength)
    {
        using (buffer)
        {
            OutputAction?.Invoke(buffer.Memory.Slice(0, avalidLength));
        }
    }
    
    public void Receive(byte[] buffer)
    {
        ReceiveAction?.Invoke(buffer);
    }
}