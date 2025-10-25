using System.Collections.Generic;
using System.Linq;
using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class LoadProgressNotificationHandler : MessageHandler<LoadProgressNotification>
    {
        public LoadProgressNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(LoadProgressNotification message)
        {
            int[] percentList = message.PercentList.ToArray();
            ResponseEventDefine.LoadProgressNotificationArgs.SendEventMessage(percentList);
        }
    }
}