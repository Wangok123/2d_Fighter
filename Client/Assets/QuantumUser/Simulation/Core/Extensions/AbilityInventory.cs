namespace Quantum
{
    public unsafe partial struct AbilityInventory
    {
        public bool HasActiveAbility => ActiveAbilityInfo.ActiveAbilityType != AbilityType.None;

        public bool TryGetAbility(Frame f, AbilityType type, out Ability ability)
        {
            var dic = f.ResolveDictionary(AbilitiesDic);
            ability = dic[type];
            return ability.AbilityData.Id.IsValid;
        }

        public bool TryGetActiveAbility(Frame f, out Ability ability)
        {
            if (!HasActiveAbility)
            {
                ability = default;
                return false;
            }

            var dic = f.ResolveDictionary(AbilitiesDic);
            ability = dic[ActiveAbilityInfo.ActiveAbilityType];
            return true;
        }
    } 
}