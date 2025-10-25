using UnityEngine;

namespace Combat
{
    public class AttackDefinition :ScriptableObject
    {
        public float CoolDown;

        public float Range;
        public float MinDamage;
        public float MaxDamage;
        public float CriticalMultiplier;
        public float CriticalChance;
    }
}