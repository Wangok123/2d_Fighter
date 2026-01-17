using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes.Calculators
{
    /// <summary>
    ///  计算属性值并将其限制在0%到100%范围内的计算器。
    /// </summary>
    public class PercentageAttributeCalculator : IAttributeCalculator
    {
        private readonly IAttributeCalculator _baseCalculator;

        public PercentageAttributeCalculator(IAttributeCalculator baseCalculator = null)
        {
            _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
        }

        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            var result = _baseCalculator.Calculate(baseValue, modifiers);

            var minValue = LATInt.Zero;
            var maxValue = new LATInt { Value = 10000 };

            if (result < minValue)
                return minValue;

            if (result > maxValue)
                return maxValue;

            return result;
        }
    }
}