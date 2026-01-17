using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes.Calculators
{
    public class DefaultAttributeCalculator : IAttributeCalculator
    {
        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            LATInt addSum = LATInt.Zero;
            LATInt multiplySum = LATInt.Zero;
            LATInt finalSum = LATInt.Zero;
            
            foreach (var modifier in modifiers)
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
            
            return (baseValue + addSum) * (LATInt.One + multiplySum) + finalSum;
        }
    }
}
