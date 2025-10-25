using System.Collections.Generic;
using LATLog;
using LATMath;
using LATPhysics;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager.MainGame;

namespace UnityCore.GameModule.Battle.Logic
{
    public partial class MainLogicUnit
    {
        private LATVector3 _inputDirection;
        public LATVector3 InputDirection
        {
            get => _inputDirection;
            private set => _inputDirection = value;
        }
        
        public LATCircleCollider Collider { get; private set; }
        
        private List<LATColliderBase> _envColliders = new List<LATColliderBase>();
        
        private void InitMove()
        {
            LogicPosition = LogicUnitData.BornPosition;
            LogicSpeed = LogicUnitData.UnitCfg.MoveSpeed;
            MoveSpeedBase = LogicUnitData.UnitCfg.MoveSpeed;
            var gameManager = GameModuleManager.GetModule<MainGameManager>();
            _envColliders = gameManager.GetEnvColliders();
            
            Collider = new LATCircleCollider(LogicUnitData.UnitCfg.ColliderConfig)
            {
                Position = LogicPosition
            };
        }

        private void TickMove()
        {
            LATVector3 moveDirection = InputDirection;
            // 预测更新
            Collider.Position += moveDirection * LogicSpeed * (LATInt)LatProtocol.Configs.ClientLogicFrameDeltaSec;
            LATVector3 adj = LATVector3.Zero;
            Collider.CalcColliders(_envColliders, ref moveDirection, ref adj);
            if (LogicDirection != moveDirection)
            {
                LogicDirection = moveDirection;
            }

            if (LogicDirection != LATVector3.Zero)
            {
                LogicPosition = Collider.Position + adj;
            }
            
            Collider.Position = LogicPosition;
            GameDebug.Log($"{UnitName} pos : {Collider.Position}");
        }

        private void UnInitMove()
        {
            // Clean up movement logic here
        }

        public void InputMoveKey(LATVector3 dir)
        {
            InputDirection = dir;
            GameDebug.Log("Input Move Key: " + dir);
        }
    }
}