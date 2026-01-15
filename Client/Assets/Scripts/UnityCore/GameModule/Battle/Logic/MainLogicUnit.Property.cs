using LATMath;
using UnityCore.GameModule.Battle.Attributes;

namespace UnityCore.GameModule.Battle.Logic
{
    public partial class MainLogicUnit
    {
        public AttributeSet Attributes { get; private set; }
        
        private LATInt _currentHP;
        public LATInt CurrentHP
        {
            get => _currentHP;
            set
            {
                var maxHP = Attributes.GetAttributeValue(AttributeType.MaxHP);
                _currentHP = LATInt.Clamp(value, LATInt.Zero, maxHP);
            }
        }
        
        public LATInt MaxHP => Attributes.GetAttributeValue(AttributeType.MaxHP);
        public LATInt Attack => Attributes.GetAttributeValue(AttributeType.Attack);
        public LATInt Defense => Attributes.GetAttributeValue(AttributeType.Defense);
        public LATInt MoveSpeed => Attributes.GetAttributeValue(AttributeType.MoveSpeed);
        
        private void InitProperty()
        {
            Attributes = new AttributeSet();
            
            Attributes.AddAttribute(AttributeType.MaxHP, LogicUnitData.UnitCfg.Hp);
            Attributes.AddAttribute(AttributeType.Attack, LogicUnitData.UnitCfg.Attack);
            Attributes.AddAttribute(AttributeType.Defense, LogicUnitData.UnitCfg.Def);
            Attributes.AddAttribute(AttributeType.MoveSpeed, LogicUnitData.UnitCfg.MoveSpeed);
            
            Attributes.AddAttribute(AttributeType.CriticalRate, 0);
            Attributes.AddAttribute(AttributeType.CriticalDamage, 15000);
            Attributes.AddAttribute(AttributeType.AttackSpeed, 10000);
            
            CurrentHP = MaxHP;
            
            Attributes.OnAttributeChanged += OnAttributeChanged;
        }
        
        private void OnAttributeChanged(GameAttribute attribute)
        {
            if (attribute.Type == AttributeType.MaxHP)
            {
                if (CurrentHP.Value > attribute.FinalValue.Value)
                {
                    CurrentHP = attribute.FinalValue;
                }
            }
        }
        
        public void AddAttributeModifier(AttributeType type, AttributeModifier modifier)
        {
            Attributes.AddModifier(type, modifier);
        }
        
        public bool RemoveAttributeModifier(AttributeType type, AttributeModifier modifier)
        {
            return Attributes.RemoveModifier(type, modifier);
        }
        
        public void RemoveAllModifiersFromSource(object source)
        {
            Attributes.RemoveAllModifiersFromSource(source);
        }
        
        public void TakeDamage(LATInt damage)
        {
            var actualDamage = CalculateDamage(damage, Defense);
            CurrentHP -= actualDamage;
            
            if (CurrentHP.Value <= 0)
            {
                OnDeath();
            }
        }
        
        private LATInt CalculateDamage(LATInt baseDamage, LATInt defense)
        {
            LATInt defenseConstant = new LATInt { Value = 100000 };
            LATInt reduction = defense * new LATInt { Value = 100 } / (defense + defenseConstant);
            LATInt actualDamage = baseDamage * (LATInt.One - reduction);
            return LATInt.Max(actualDamage, LATInt.One);
        }
        
        private void OnDeath()
        {
            // TODO: 处理单位死亡逻辑
        }
    }
}