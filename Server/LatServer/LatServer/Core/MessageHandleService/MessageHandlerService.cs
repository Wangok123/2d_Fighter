using GameProtocol;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.MessageHandleService.Handler;
using LatServer.Core.Service.NetService;

namespace LatServer.Core.MessageHandleService;

public class MessageHandlerService : Singleton<MessageHandlerService>
{
    MessageDispatcher _dispatcher = new MessageDispatcher();

    public override void Init()
    {
        _dispatcher.RegisterHandler(new LoginHandler((ushort)ProtocolID.LoginRequest));
        _dispatcher.RegisterHandler(new MatchHandler((ushort)ProtocolID.MatchRequest));
        _dispatcher.RegisterHandler(new SendConfirmHandler((ushort)ProtocolID.SendConfirmRequest));
        _dispatcher.RegisterHandler(new SendSelectHandler((ushort)ProtocolID.SendSelectRequest));
        _dispatcher.RegisterHandler(new SendLoadProgressHandler((ushort)ProtocolID.SendLoadProgressRequest));
        _dispatcher.RegisterHandler(new SendLoadFinishHandler((ushort)ProtocolID.LoadingFinishRequest));
    }
    
    public void Dispatch(MsgPack message)
    {
        _dispatcher.Dispatch(message);
    }
}