using LATNet;

namespace KCPExampleProtocol;

[Serializable]
public class NetMsg : KCPMsg
{
    public CMD Cmd { get; set; }
    public NetPing Ping { get; set; }
    public string Info { get; set; }
}

[Serializable]
public class NetPing
{
    public bool IsOver;
}

public enum CMD
{
    None, 
    NetPing,
}