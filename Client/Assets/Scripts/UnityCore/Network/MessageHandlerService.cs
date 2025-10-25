using LatProtocol;
using UnityCore.Network.Dispatcher;
using UnityCore.Network.Handler;

namespace UnityCore.Network
{
    public class MessageHandlerService
    {
        private MessageDispatcher _dispatcher = new MessageDispatcher();
        
        private static MessageHandlerService _instance;
        public static MessageHandlerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageHandlerService();
                }
                return _instance;
            }
        }
            
        public void Init()
        {
            _dispatcher.RegisterHandler(new LoginHandler((ushort)ProtocolID.LoginResponse));
            _dispatcher.RegisterHandler(new MatchHandler((ushort)ProtocolID.MatchResponse));
            _dispatcher.RegisterHandler(new ConfirmNotificationHandler((ushort)ProtocolID.ConfirmNotification));
            _dispatcher.RegisterHandler(new SelectNotificationHandler((ushort)ProtocolID.SelectNotification));
            _dispatcher.RegisterHandler(new LoadResourceNotificationHandler((ushort)ProtocolID.LoadResourceNotification));
            _dispatcher.RegisterHandler(new LoadProgressNotificationHandler((ushort)ProtocolID.LoadProgressNotification));
            _dispatcher.RegisterHandler(new LoadingFinishNotificationHandler((ushort)ProtocolID.LoadingFinishNotification));
            _dispatcher.RegisterHandler(new OperationKeyNotificationHandler((ushort)ProtocolID.OperationKeyNotification));
        }
    
        public void Dispatch(MsgPack message)
        {
            _dispatcher.Dispatch(message);
        }
    }
}