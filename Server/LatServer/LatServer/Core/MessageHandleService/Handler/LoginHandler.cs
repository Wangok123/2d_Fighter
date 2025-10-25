using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class LoginHandler : MessageHandler<LoginRequest>
{
    public LoginHandler(uint cmdId) : base(cmdId)
    {
    }
    
    protected override void Process(ServerSession session, LoginRequest message)
    {
        string account = message.Account;
        string password = message.Password;
        
        LoginEvent loginEvent = new LoginEvent(session, account, password);
        
        EventManager.Publish(loginEvent);
    }
}