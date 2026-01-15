using System;
using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes
{
    public class AttributeSet
    {
        private readonly Dictionary<AttributeType, GameAttribute> _attributes = new Dictionary<AttributeType, GameAttribute>();
        
        public event Action<GameAttribute> OnAttributeChanged;
        
        public void AddAttribute(AttributeType type, LATInt baseValue)
        {
            if (_attributes.ContainsKey(type))
            {
                _attributes[type].BaseValue = baseValue;
            }
            else
            {
                var attribute = new GameAttribute(type, baseValue);
                attribute.OnValueChanged += OnAttributeValueChanged;
                _attributes[type] = attribute;
            }
        }
        
        public void AddAttribute(AttributeType type, int baseValue)
        {
            AddAttribute(type, new LATInt { Value = baseValue });
        }
        
        public GameAttribute GetAttribute(AttributeType type)
        {
            return _attributes.TryGetValue(type, out var attribute) ? attribute : null;
        }
        
        public LATInt GetAttributeValue(AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.FinalValue ?? LATInt.Zero;
        }
        
        public LATInt GetAttributeBaseValue(AttributeType type)
        {
            var attribute = GetAttribute(type);
            return attribute?.BaseValue ?? LATInt.Zero;
        }
        
        public void SetAttributeBaseValue(AttributeType type, LATInt value)
        {
            var attribute = GetAttribute(type);
            if (attribute != null)
            {
                attribute.BaseValue = value;
            }
        }
        
        public void AddModifier(AttributeType type, AttributeModifier modifier)
        {
            var attribute = GetAttribute(type);
            attribute?.AddModifier(modifier);
        }
        
        public bool RemoveModifier(AttributeType type, AttributeModifier modifier)
        {
            var attribute = GetAttribute(type);
            return attribute != null && attribute.RemoveModifier(modifier);
        }
        
        public bool RemoveModifiersFromSource(AttributeType type, object source)
        {
            var attribute = GetAttribute(type);
            return attribute != null && attribute.RemoveModifiersFromSource(source);
        }
        
        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var attribute in _attributes.Values)
            {
                attribute.RemoveModifiersFromSource(source);
            }
        }
        
        public void ClearAllModifiers()
        {
            foreach (var attribute in _attributes.Values)
            {
                attribute.ClearModifiers();
            }
        }
        
        private void OnAttributeValueChanged(GameAttribute attribute)
        {
            OnAttributeChanged?.Invoke(attribute);
        }
        
        public Dictionary<AttributeType, GameAttribute>.ValueCollection GetAllAttributes()
        {
            return _attributes.Values;
        }
    }
}
