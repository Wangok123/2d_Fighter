using System.Collections.Generic;
using LATMath;
using UnityCore.GameModule.Battle.Logic;

namespace UnityCore.GameModule.Battle.Buffs
{
    public class BuffManager
    {
        private readonly MainLogicUnit _owner;
        private readonly List<BuffBase> _buffs = new List<BuffBase>();
        
        public BuffManager(MainLogicUnit owner)
        {
            _owner = owner;
        }
        
        public void AddBuff(BuffBase buff, MainLogicUnit caster)
        {
            if (buff == null) return;
            
            var existingBuff = _buffs.Find(b => b.BuffID == buff.BuffID);
            if (existingBuff != null)
            {
                existingBuff.OnStack();
                return;
            }
            
            buff.OnApply(_owner, caster);
            _buffs.Add(buff);
        }
        
        public void RemoveBuff(BuffBase buff)
        {
            if (_buffs.Remove(buff))
            {
                buff.OnRemove();
            }
        }
        
        public void RemoveBuffByID(int buffID)
        {
            var buff = _buffs.Find(b => b.BuffID == buffID);
            if (buff != null)
            {
                RemoveBuff(buff);
            }
        }
        
        public void Tick(LATInt deltaTime)
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.OnTick(deltaTime);
                
                if (buff.IsExpired)
                {
                    RemoveBuff(buff);
                }
            }
        }
        
        public void ClearAllBuffs()
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                RemoveBuff(_buffs[i]);
            }
        }
        
        public BuffBase GetBuff(int buffID)
        {
            return _buffs.Find(b => b.BuffID == buffID);
        }
        
        public bool HasBuff(int buffID)
        {
            return GetBuff(buffID) != null;
        }
        
        public List<BuffBase> GetAllBuffs()
        {
            return new List<BuffBase>(_buffs);
        }
    }
}
