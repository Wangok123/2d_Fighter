using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class LoadingFinishNotificationHandler : MessageHandler<LoadingFinishNotification>
    {
        public LoadingFinishNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(LoadingFinishNotification message)
        {
            ResponseEventDefine.LoadFinishNotificationArgs.SendEventMessage();
        }
    }
}