using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class LoginEvent : IEvent
{
    public ServerSession Session { get;}
    public string Account { get; }
    public string Password { get; }

    public LoginEvent()
    {
    }
    
    public LoginEvent(ServerSession session ,string account, string password)
    {
        Session = session;
        Account = account;
        Password = password;
    }
}