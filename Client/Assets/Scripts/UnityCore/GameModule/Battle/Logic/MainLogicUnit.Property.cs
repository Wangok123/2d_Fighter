using LATMath;

namespace UnityCore.GameModule.Battle.Logic
{
    public partial class MainLogicUnit
    {
        private LATInt _hp;
        public LATInt HP
        {
            get => _hp;
            set => _hp = value;
        }

        private LATInt _def;
        public LATInt Def
        {
            get => _def;
            set => _def = value;
        }
        
        
        private void InitProperty()
        {
            HP = LogicUnitData.UnitCfg.Hp;
            Def = LogicUnitData.UnitCfg.Def;
        }
    }
}