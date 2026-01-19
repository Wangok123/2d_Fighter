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
        
        private IAttributeCalculator _calculator;
        
        public event Action<GameAttribute> OnValueChanged;
        
        public GameAttribute(AttributeType type, LATInt baseValue) 
            : this(type, baseValue, null)
        {
        }
        
        public GameAttribute(AttributeType type, int baseValue) 
            : this(type, new LATInt { Value = baseValue }, null)
        {
        }
        
        public GameAttribute(AttributeType type, LATInt baseValue, IAttributeCalculator calculator)
        {
            Type = type;
            _baseValue = baseValue;
            _calculator = calculator ?? AttributeCalculatorFactory.GetCalculator(type);
            _isDirty = true;
        }
        
        public GameAttribute(AttributeType type, int baseValue, IAttributeCalculator calculator) 
            : this(type, new LATInt { Value = baseValue }, calculator)
        {
        }
        
        public void AddModifier(AttributeModifier modifier)
        {
            if (modifier == null) return;
            
            int insertIndex = _modifiers.Count;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                if (_modifiers[i].CompareTo(modifier) > 0)
                {
                    insertIndex = i;
                    break;
                }
            }
            
            _modifiers.Insert(insertIndex, modifier);
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
        
        public void SetCalculator(IAttributeCalculator calculator)
        {
            _calculator = calculator ?? AttributeCalculatorFactory.GetCalculator(Type);
            _isDirty = true;
        }
        
        public IReadOnlyList<AttributeModifier> GetModifiers()
        {
            return _modifiers.AsReadOnly();
        }
        
        private void CalculateFinalValue()
        {
            LATInt previousValue = _finalValue;
            
            _finalValue = _calculator.Calculate(_baseValue, _modifiers);
            
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
