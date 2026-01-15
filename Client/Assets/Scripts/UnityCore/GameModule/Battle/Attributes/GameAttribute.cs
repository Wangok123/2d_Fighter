using System;
using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes
{
    public class GameAttribute
    {
        public AttributeType Type { get; private set; }
        
        private LATInt _baseValue;
        public LATInt BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue.Value != value.Value)
                {
                    _baseValue = value;
                    _isDirty = true;
                }
            }
        }
        
        private LATInt _finalValue;
        public LATInt FinalValue
        {
            get
            {
                if (_isDirty)
                {
                    CalculateFinalValue();
                    _isDirty = false;
                }
                return _finalValue;
            }
        }
        
        private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();
        private bool _isDirty = true;
        
        public event Action<GameAttribute> OnValueChanged;
        
        public GameAttribute(AttributeType type, LATInt baseValue)
        {
            Type = type;
            _baseValue = baseValue;
            _isDirty = true;
        }
        
        public GameAttribute(AttributeType type, int baseValue)
        {
            Type = type;
            _baseValue = new LATInt { Value = baseValue };
            _isDirty = true;
        }
        
        public void AddModifier(AttributeModifier modifier)
        {
            if (modifier == null) return;
            
            _modifiers.Add(modifier);
            _modifiers.Sort();
            _isDirty = true;
        }
        
        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _isDirty = true;
                return true;
            }
            return false;
        }
        
        public bool RemoveModifiersFromSource(object source)
        {
            int removedCount = _modifiers.RemoveAll(mod => mod.SourceObject == source);
            if (removedCount > 0)
            {
                _isDirty = true;
                return true;
            }
            return false;
        }
        
        public void ClearModifiers()
        {
            _modifiers.Clear();
            _isDirty = true;
        }
        
        private void CalculateFinalValue()
        {
            LATInt addSum = LATInt.Zero;
            LATInt multiplySum = LATInt.Zero;
            LATInt finalSum = LATInt.Zero;
            
            foreach (var modifier in _modifiers)
            {
                switch (modifier.Type)
                {
                    case ModifierType.Add:
                        addSum += modifier.Value;
                        break;
                    
                    case ModifierType.Multiply:
                        multiplySum += modifier.Value;
                        break;
                    
                    case ModifierType.Final:
                        finalSum += modifier.Value;
                        break;
                }
            }
            
            LATInt previousValue = _finalValue;
            
            _finalValue = (_baseValue + addSum) * (LATInt.One + multiplySum) + finalSum;
            
            if (previousValue.Value != _finalValue.Value)
            {
                OnValueChanged?.Invoke(this);
            }
        }
        
        public override string ToString()
        {
            return $"{Type}: Base={_baseValue.Value}, Final={FinalValue.Value}, Modifiers={_modifiers.Count}";
        }
    }
}
