using GameProtocol;
using UnityCore.EventDefine;
using UnityCore.Network.Dispatcher;

namespace UnityCore.Network.Handler
{
    public class MatchHandler : MessageHandler<MatchResponse>
    {
        public MatchHandler(uint cmdId) : base(cmdId)
        {
        }

        protected override void Process(MatchResponse message)
        {
            ResponseEventDefine.MatchStartArgs.SendEventMessage(message.PredictionTime);
        }
    }
}