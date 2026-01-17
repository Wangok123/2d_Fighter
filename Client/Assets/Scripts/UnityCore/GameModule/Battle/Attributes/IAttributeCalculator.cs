using System.Collections.Generic;
using LATMath;

namespace UnityCore.GameModule.Battle.Attributes
{
    public interface IAttributeCalculator
    {
        LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers);
    }
}
