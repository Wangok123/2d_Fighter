namespace Quantum
{
    public unsafe partial struct AbilityInventory
    {
        public bool HasActiveAbility => ActiveAbilityInfo.ActiveAbilityType != AbilityType.None;

        public bool TryGetAbility(Frame f, AbilityType type, out Ability ability)
        {
            var dic = f.ResolveDictionary(AbilitiesDic);
            if (dic.TryGetValue(type, out ability))
            {
                return ability.AbilityData.Id.IsValid;
            }
            
            ability = default;
            return false;
        }

        public bool TryGetActiveAbility(Frame f, out Ability ability)
        {
            if (!HasActiveAbility)
            {
                ability = default;
                return false;
            }

            var dic = f.ResolveDictionary(AbilitiesDic);
            if (dic.TryGetValue(ActiveAbilityInfo.ActiveAbilityType, out ability))
            {
                return true;
            }
            
            ability = default;
            return false;
        }
    } 
}
