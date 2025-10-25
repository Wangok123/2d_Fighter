using System.Collections.Generic;
using Core.ReferencePool;
using GameProtocol;
using UnityCore.EventSystem;

namespace UnityCore.EventDefine
{
    public class BattleEventDefine
    {
        public class OperationKeyNotificationArgs : EventDefineBase
        {
            public OperationKeyNotification Notification { get; set; }

            public static void SendEventMessage(OperationKeyNotification notification)
            {
                var args = ReferencePool.Acquire<OperationKeyNotificationArgs>();
                args.Notification = notification;
                UniEvent.SendMessage(args);
                ReferencePool.Release(args);
            }
        }
    }
}