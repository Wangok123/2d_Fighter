using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class SelectNotificationHandler : MessageHandler<SelectNotification>
    {
        public SelectNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(SelectNotification message)
        {
            ResponseEventDefine.SelectNotificationArgs.SendEventMessage();
        }
    }
}