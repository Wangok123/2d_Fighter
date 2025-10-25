using System.Linq;
using GameProtocol;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.GameModule.GameFlow;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class LoadResourceNotificationHandler : MessageHandler<LoadResourceNotification>
    {
        public LoadResourceNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(LoadResourceNotification message)
        {
            uint mapId = message.MapId;
            int posIndex = message.PosIndex;
            BattleHeroDto[] heroDataList = message.HeroDataList.ToArray();
            ResponseEventDefine.LoadResNotificationArgs.SendEventMessage(mapId, posIndex, heroDataList);
        }
    }
}