using System.Collections.Generic;
using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class OperationKeyNotificationHandler : MessageHandler<OperationKeyNotification>
    {
        public OperationKeyNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(OperationKeyNotification message)
        {
            BattleEventDefine.OperationKeyNotificationArgs.SendEventMessage(message);
        }
    }
}