using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes.Calculators
{
    /// <summary>
    /// 计算属性值并将其限制在指定范围内的计算器。
    /// </summary>
    public class ClampedAttributeCalculator : IAttributeCalculator
    {
        private readonly IAttributeCalculator _baseCalculator;
        private readonly LATInt _minValue;
        private readonly LATInt _maxValue;
        
        public ClampedAttributeCalculator(LATInt minValue, LATInt maxValue, IAttributeCalculator baseCalculator = null)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
        }
        
        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            var result = _baseCalculator.Calculate(baseValue, modifiers);
            
            if (result < _minValue)
                return _minValue;
            
            if (result > _maxValue)
                return _maxValue;
            
            return result;
        }
    }
}
