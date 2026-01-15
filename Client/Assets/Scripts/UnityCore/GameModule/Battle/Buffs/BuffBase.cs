using System.Collections.Generic;
using LATMath;
using UnityCore.GameModule.Battle.Attributes;
using UnityCore.GameModule.Battle.Logic;

namespace UnityCore.GameModule.Battle.Buffs
{
    public enum BuffType
    {
        Buff = 0,
        Debuff = 1,
    }
    
    public abstract class BuffBase
    {
        public int BuffID { get; protected set; }
        public string BuffName { get; protected set; }
        public BuffType Type { get; protected set; }
        
        public MainLogicUnit Target { get; private set; }
        public MainLogicUnit Caster { get; private set; }
        
        public LATInt Duration { get; protected set; }
        public LATInt RemainingTime { get; protected set; }
        
        public int StackCount { get; private set; }
        public int MaxStack { get; protected set; }
        
        protected List<AttributeModifier> _modifiers = new List<AttributeModifier>();
        
        public bool IsExpired => RemainingTime.Value <= 0;
        
        public virtual void OnApply(MainLogicUnit target, MainLogicUnit caster)
        {
            Target = target;
            Caster = caster;
            RemainingTime = Duration;
            StackCount = 1;
            
            ApplyModifiers();
        }
        
        public virtual void OnStack()
        {
            if (StackCount < MaxStack)
            {
                StackCount++;
                RefreshDuration();
            }
        }
        
        public virtual void OnRemove()
        {
            RemoveModifiers();
        }
        
        public virtual void OnTick(LATInt deltaTime)
        {
            RemainingTime -= deltaTime;
        }
        
        public void RefreshDuration()
        {
            RemainingTime = Duration;
        }
        
        protected virtual void ApplyModifiers()
        {
            foreach (var modifier in _modifiers)
            {
                Target.Attributes.AddModifier(GetModifierAttributeType(), modifier);
            }
        }
        
        protected virtual void RemoveModifiers()
        {
            Target.RemoveAllModifiersFromSource(this);
        }
        
        protected abstract AttributeType GetModifierAttributeType();
    }
}
