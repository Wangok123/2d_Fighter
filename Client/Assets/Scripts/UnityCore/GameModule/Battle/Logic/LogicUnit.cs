using LATMath;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Logic
{
    public abstract class LogicUnit : ILogic
    {
        #region Properties

        public Transform UnitRoot { get; set; }
        public string UnitName { get; set; }
        
        public bool IsLogicDirDirty { get; set; }
        public bool IsLogicPosDirty { get; set; }
        private LATVector3 _logicPosition;
        public LATVector3 LogicPosition
        {
            get => _logicPosition;
            set
            {
                _logicPosition = value;
                IsLogicPosDirty = true;
            }
        }

        private LATVector3 _logicDirection;
        public LATVector3 LogicDirection
        {
            get => _logicDirection;
            set
            {
                _logicDirection = value;
                IsLogicDirDirty = true;
            }
        }

        private LATInt _logicSpeed;
        public LATInt LogicSpeed
        {
            get => _logicSpeed;
            set => _logicSpeed = value;
        }

        public LATInt MoveSpeedBase;
        
        #endregion

        public abstract void LogicInit();
        public abstract void LogicTick();
        public abstract void LogicUnInit();
    }
}