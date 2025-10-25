namespace LatProtocol;

public enum ProtocolID
{
    BattleStartRequest = 1,
    SendOperationKeyRequest = 2,
    OperationKeyNotification = 3,
    LoginRequest = 4,
    LoginResponse = 5,
    MatchRequest = 6,
    MatchResponse = 7,
    ConfirmNotification = 8,
    SendConfirmRequest = 9,
    SelectNotification = 10,
    SendSelectRequest = 11,
    SendLoadProgressRequest = 12,
    LoadProgressNotification = 13,
    LoadResourceNotification = 14,
    LoadingFinishRequest = 15,
    LoadingFinishNotification = 16,
}
