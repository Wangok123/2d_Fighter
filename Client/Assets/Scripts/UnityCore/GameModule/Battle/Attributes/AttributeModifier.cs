using System;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes
{
    public enum ModifierType
    {
        Add = 0,
        Multiply = 1,
        Final = 2,
    }
    
    public class AttributeModifier : IComparable<AttributeModifier>
    {
        public readonly LATInt Value;
        public readonly ModifierType Type;
        public readonly ModifierSource Source;
        public readonly int Order;
        public readonly object SourceObject;
        
        public AttributeModifier(LATInt value, ModifierType type, ModifierSource source, int order = 0, object sourceObject = null)
        {
            Value = value;
            Type = type;
            Source = source;
            Order = order;
            SourceObject = sourceObject;
        }
        
        public AttributeModifier(int value, ModifierType type, ModifierSource source, int order = 0, object sourceObject = null)
        {
            Value = new LATInt { Value = value };
            Type = type;
            Source = source;
            Order = order;
            SourceObject = sourceObject;
        }
        
        public int CompareTo(AttributeModifier other)
        {
            if (other == null) return 1;
            
            if (Order != other.Order)
                return Order.CompareTo(other.Order);
            
            return Type.CompareTo(other.Type);
        }
    }
}
