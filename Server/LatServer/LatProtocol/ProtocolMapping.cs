using GameProtocol;

namespace LatProtocol;

public static class ProtocolMapping
{
    public static Dictionary<ushort, Type> ProtocolMap = new Dictionary<ushort, Type>
    {
        { (ushort)ProtocolID.BattleStartRequest, typeof(BattleStartRequest) },
        { (ushort)ProtocolID.SendOperationKeyRequest, typeof(SendOperationKeyRequest) },
        { (ushort)ProtocolID.OperationKeyNotification, typeof(OperationKeyNotification) },
        { (ushort)ProtocolID.LoginRequest, typeof(LoginRequest) },
        { (ushort)ProtocolID.LoginResponse, typeof(LoginResponse) },
        { (ushort)ProtocolID.MatchRequest, typeof(MatchRequest) },
        { (ushort)ProtocolID.MatchResponse, typeof(MatchResponse) },
        { (ushort)ProtocolID.ConfirmNotification, typeof(ConfirmNotification) },
        { (ushort)ProtocolID.SendConfirmRequest, typeof(SendConfirmRequest) },
        { (ushort)ProtocolID.SelectNotification, typeof(SelectNotification) },
        { (ushort)ProtocolID.SendSelectRequest, typeof(SendSelectRequest) },
        { (ushort)ProtocolID.SendLoadProgressRequest, typeof(SendLoadProgressRequest) },
        { (ushort)ProtocolID.LoadProgressNotification, typeof(LoadProgressNotification) },
        { (ushort)ProtocolID.LoadResourceNotification, typeof(LoadResourceNotification) },
        { (ushort)ProtocolID.LoadingFinishRequest, typeof(LoadingFinishRequest) },
        { (ushort)ProtocolID.LoadingFinishNotification, typeof(LoadingFinishNotification) },
    };
}
