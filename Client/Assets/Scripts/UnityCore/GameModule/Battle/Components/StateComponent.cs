using System.Collections.Generic;
using UnityCore.GameModule.Battle.Data;

namespace UnityCore.GameModule.Battle.Components
{
    public class StateComponent
    {
        public readonly List<StateSlot> slots = new (); // 所有状态槽，按槽Id排序
        public readonly List<StateSlot> activeSlots = new (); // 按状态激活顺序排序的状态槽
        public readonly Dictionary<int, List<State>> stateDic = new ();
    }
}