using LATMath;
using UnityCore.GameModule.Battle.Attributes;

namespace UnityCore.GameModule.Battle.Buffs.Examples
{
    public class AttackBoostBuff : BuffBase
    {
        public AttackBoostBuff(int buffID, LATInt duration, LATInt attackBoost)
        {
            BuffID = buffID;
            BuffName = "攻击力提升";
            Type = BuffType.Buff;
            Duration = duration;
            MaxStack = 3;
            
            _modifiers.Add(new AttributeModifier(
                attackBoost, 
                ModifierType.Add, 
                ModifierSource.Buff, 
                0, 
                this
            ));
        }
        
        protected override AttributeType GetModifierAttributeType()
        {
            return AttributeType.Attack;
        }
    }
    
    public class AttackPercentBoostBuff : BuffBase
    {
        public AttackPercentBoostBuff(int buffID, LATInt duration, LATInt percentBoost)
        {
            BuffID = buffID;
            BuffName = "攻击力百分比提升";
            Type = BuffType.Buff;
            Duration = duration;
            MaxStack = 1;
            
            _modifiers.Add(new AttributeModifier(
                percentBoost,
                ModifierType.Multiply,
                ModifierSource.Buff,
                0,
                this
            ));
        }
        
        protected override AttributeType GetModifierAttributeType()
        {
            return AttributeType.Attack;
        }
    }
    
    public class DefenseDebuff : BuffBase
    {
        public DefenseDebuff(int buffID, LATInt duration, LATInt defenseReduction)
        {
            BuffID = buffID;
            BuffName = "防御力降低";
            Type = BuffType.Debuff;
            Duration = duration;
            MaxStack = 1;
            
            _modifiers.Add(new AttributeModifier(
                -defenseReduction,
                ModifierType.Add,
                ModifierSource.Debuff,
                0,
                this
            ));
        }
        
        protected override AttributeType GetModifierAttributeType()
        {
            return AttributeType.Defense;
        }
    }
}
