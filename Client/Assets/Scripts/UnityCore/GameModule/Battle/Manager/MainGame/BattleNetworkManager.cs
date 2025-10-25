using GameProtocol;
using LATMath;
using LatProtocol;
using UnityCore.Base;
using UnityCore.GameModule.GM;

namespace UnityCore.GameModule.Battle.Manager.MainGame
{
    public class BattleNetworkManager
    {
        private uint _keyId = 0;
        private uint KeyId => ++_keyId;

        private MatchManager _matchManager;
        private GmManager _gmManager;
        
        public bool SendMoveKey(LATVector3 logicDir)
        {
            if (_matchManager == null)
            {
                _matchManager = GameModuleManager.GetModule<MatchManager>();
            }
            
            if (_gmManager == null)
            {
                _gmManager = GameModuleManager.GetModule<GmManager>();
            }
            
            SendOperationKeyRequest request = new SendOperationKeyRequest
            {
                RoomId = 0,
                // RoomId = _matchManager.GetRoomId(),
            };
            
            OperationKey operationKey = new OperationKey
            {
                KeyType = OperationKeyType.Move,
            };
            
            operationKey.MoveKey = new MoveKey
            {
                X = logicDir.x.Value,
                Z = logicDir.z.Value,
                KeyId = this.KeyId
            };
            request.OpKey = operationKey;

            _gmManager.SendMsgGM((ushort)ProtocolID.SendOperationKeyRequest, request);
            
            return true;
        }
    }
}