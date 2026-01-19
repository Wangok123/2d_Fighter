using System.Collections.Generic;
using Wjybxx.Commons.Fx;

namespace UnityCore.GameModule.Components
{
    [ComponentDefine(Shared = true)]
    public class SharedComponent : GComponent
    {
        public List<GameUnit> Members = new();
    
        public void AddMember(GameUnit unit)
        {
            if (!Members.Contains(unit))
            {
                Members.Add(unit);
            }
        }
    
        public bool RemoveMember(GameUnit unit)
        {
            return Members.Remove(unit);
        }
    
        public override void Reset()
        {
            base.Reset();
            Members.Clear();
        }
    }
}