using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes.Calculators
{
    /// <summary>
    /// 计算属性值并确保其不为负数的计算器。
    /// </summary>
    public class NonNegativeAttributeCalculator : IAttributeCalculator
    {
        private readonly IAttributeCalculator _baseCalculator;
        
        public NonNegativeAttributeCalculator(IAttributeCalculator baseCalculator = null)
        {
            _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
        }
        
        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            var result = _baseCalculator.Calculate(baseValue, modifiers);
            
            return result < LATInt.Zero ? LATInt.Zero : result;
        }
    }
}
