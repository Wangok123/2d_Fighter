using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.Logic;
using UnityEngine;

namespace UnityCore.GameModule.Battle.View
{
    public class HeroView : MainViewUnit
    {
        private Hero _heroUnit;
        
        public override void Init(LogicUnit logicUnit)
        {
            base.Init(logicUnit);
            
            _heroUnit = logicUnit as Hero;
        }

        protected override Vector3 GetUnitViewDirection()
        {
            return _heroUnit.InputDirection;
        }
    }
}