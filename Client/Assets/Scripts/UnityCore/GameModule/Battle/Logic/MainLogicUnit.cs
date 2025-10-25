using GameProtocol;
using LATLog;
using LATMath;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Config;
using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.View;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Logic
{
    public abstract partial class MainLogicUnit : LogicUnit
    {
        public LogicUnitData LogicUnitData { get; private set; }
        public UnitState UnitState { get; private set; }
        public UnitType UnitType { get; set; }
        public MainViewUnit ViewUnit { get; private set; }

        public MainLogicUnit(LogicUnitData logicUnitData)
        {
            LogicUnitData = logicUnitData;
            UnitName = logicUnitData.UnitCfg.UnitName;
        }

        public override void LogicInit()
        {
            InitProperty();
            InitSkill();
            InitMove();
            InitGameObject();

            UnitState = UnitState.Alive;
        }

        public override void LogicTick()
        {
            TickSkill();
            TickMove();
        }

        public override void LogicUnInit()
        {
            UnInitSkill();
            UnInitMove();
        }

        public void InputKey(OperationKey key)
        {
            switch (key.KeyType)
            {
                case OperationKeyType.Skill:
                    break;
                case OperationKeyType.Move:
                    LATInt x = LATInt.Zero;
                    x.Value = key.MoveKey.X;
                    LATInt z = LATInt.Zero;
                    z.Value = key.MoveKey.Z;
                    InputMoveKey(new LATVector3(x, 0, z));
                    break;
                case OperationKeyType.None:
                default:
                    GameDebug.LogError("InputKey: Unsupported key type. KeyType = " + key.KeyType);
                    break;
            }
        }

        private void InitGameObject()
        {
            var resName = LogicUnitData.UnitCfg.ResourceName;
            GameObject gameObject = Game.YooAsset.LoadGameObjectSync(resName);
            var go = Object.Instantiate(gameObject, UnitRoot);
            ViewUnit = go.GetComponent<MainViewUnit>();
            if (ViewUnit == null)
            {
                GameDebug.LogError("MainLogicUnit InitGameObject: ViewUnit is null. ResourceName = " + resName);
            }
            else
            {
                ViewUnit.Init(this);
            }
        }
    }
}