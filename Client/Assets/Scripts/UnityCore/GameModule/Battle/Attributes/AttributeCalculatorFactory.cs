using LATMath;
using UnityCore.GameModule.Battle.Attributes.Calculators;

namespace UnityCore.GameModule.Battle.Attributes
{
    public static class AttributeCalculatorFactory
    {
        private static readonly IAttributeCalculator DefaultCalculator = new DefaultAttributeCalculator();
        
        public static IAttributeCalculator GetCalculator(AttributeType attributeType)
        {
            return attributeType switch
            {
                AttributeType.CurrentHP => new NonNegativeAttributeCalculator(),
                AttributeType.MaxHP => new NonNegativeAttributeCalculator(),
                
                AttributeType.CriticalRate => new PercentageAttributeCalculator(),
                AttributeType.CriticalDamage => new PercentageAttributeCalculator(),
                
                AttributeType.MoveSpeed => new SoftCapAttributeCalculator(
                    softCap: new LATInt { Value = 10000 },
                    diminishRate: new LATInt { Value = 5000 }
                ),
                
                AttributeType.AttackSpeed => new ClampedAttributeCalculator(
                    minValue: new LATInt { Value = 100 },
                    maxValue: new LATInt { Value = 20000 }
                ),
                
                AttributeType.PhysicalResistance => new PercentageAttributeCalculator(),
                AttributeType.MagicResistance => new PercentageAttributeCalculator(),
                
                _ => DefaultCalculator
            };
        }
        
        public static IAttributeCalculator CreateClamped(LATInt min, LATInt max)
        {
            return new ClampedAttributeCalculator(min, max);
        }
        
        public static IAttributeCalculator CreateSoftCap(LATInt softCap, LATInt diminishRate)
        {
            return new SoftCapAttributeCalculator(softCap, diminishRate);
        }
        
        public static IAttributeCalculator CreateNonNegative()
        {
            return new NonNegativeAttributeCalculator();
        }
        
        public static IAttributeCalculator CreatePercentage()
        {
            return new PercentageAttributeCalculator();
        }
        
        public static IAttributeCalculator GetDefault()
        {
            return DefaultCalculator;
        }
    }
}
