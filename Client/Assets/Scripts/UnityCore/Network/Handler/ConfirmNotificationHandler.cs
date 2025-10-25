using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class ConfirmNotificationHandler : MessageHandler<ConfirmNotification>
    {
        public ConfirmNotificationHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(ConfirmNotification message)
        {
            ConfirmDto[] confirmArr = new ConfirmDto[message.ConfirmData.Count];
            for (int i = 0; i < message.ConfirmData.Count; i++)
            {
                confirmArr[i] = new ConfirmDto
                {
                    IconIndex = message.ConfirmData[i].IconIndex,
                    ConfirmDone = message.ConfirmData[i].ConfirmDone,
                };
            }

            ResponseEventDefine.ConfirmNotificationArgs.SendEventMessage(message.RoomId, message.Dismiss, confirmArr);
        }
    }
}