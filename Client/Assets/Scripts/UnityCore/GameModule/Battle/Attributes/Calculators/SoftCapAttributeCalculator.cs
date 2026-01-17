using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes.Calculators
{
    /// <summary>
    /// 计算属性值并在超过软上限后进行递减的计算器。
    /// </summary>
    public class SoftCapAttributeCalculator : IAttributeCalculator
    {
        private readonly IAttributeCalculator _baseCalculator;
        private readonly LATInt _softCap;
        private readonly LATInt _diminishRate;
        
        public SoftCapAttributeCalculator(LATInt softCap, LATInt diminishRate, IAttributeCalculator baseCalculator = null)
        {
            _softCap = softCap;
            _diminishRate = diminishRate;
            _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
        }
        
        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            var result = _baseCalculator.Calculate(baseValue, modifiers);
            
            if (result <= _softCap)
                return result;
            
            var excess = result - _softCap;
            var diminishedExcess = excess * _diminishRate / LATInt.One;
            
            return _softCap + diminishedExcess;
        }
    }
}
