using LATMath;
using UnityCore.GameModule.Battle.Buffs;

namespace UnityCore.GameModule.Battle.Logic
{
    public partial class MainLogicUnit
    {
        public BuffManager BuffManager { get; private set; }
        
        private void InitBuff()
        {
            BuffManager = new BuffManager(this);
        }
        
        private void TickBuff()
        {
            if (BuffManager != null)
            {
                LATInt deltaTime = new LATInt { Value = (long)(LatProtocol.Configs.ClientLogicFrameDeltaSec * 1000) };
                BuffManager.Tick(deltaTime);
            }
        }
        
        private void UnInitBuff()
        {
            BuffManager?.ClearAllBuffs();
        }
        
        public void ApplyBuff(BuffBase buff, MainLogicUnit caster)
        {
            BuffManager?.AddBuff(buff, caster);
        }
        
        public void RemoveBuff(int buffID)
        {
            BuffManager?.RemoveBuffByID(buffID);
        }
        
        public bool HasBuff(int buffID)
        {
            return BuffManager?.HasBuff(buffID) ?? false;
        }
    }
}

