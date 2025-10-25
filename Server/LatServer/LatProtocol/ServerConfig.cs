namespace LatProtocol
{
    public class ServerConfig
    {
        // 倒计时匹配设置
        public const int ConfirmationTime = 15; // 秒
        public const int SelectionTime = 15; // 秒
        
        public const string LocalDevInnerIp = "172.16.1.31";
        public const int UdpPort = 17666;
    }

    public class Configs
    {
        public const float ClientLogicFrameDeltaSec = 0.066f;
    }
}