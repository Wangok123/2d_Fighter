namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class AbilitySystem : SystemMainThreadFilter<AbilitySystem.Filter>, ISignalOnActiveAbilityStopped
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public AbilityInventory* AbilityInventory;
            public PlayerLink* PlayerLink;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            SimpleInput2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
            var dic = frame.ResolveDictionary(filter.AbilityInventory->AbilitiesDic);
            
            // 先收集所有要修改的Key
            var keys = new System.Collections.Generic.List<AbilityType>(dic.Count);
            foreach (var abilityPair in dic)
            {
                keys.Add(abilityPair.Key);
            }

            foreach (var abilityType in keys)
            {
                Ability ability = dic[abilityType];
                AbilityData abilityData = frame.FindAsset<AbilityData>(ability.AbilityData.Id);

                abilityData.UpdateAbility(frame, filter.EntityRef, ref ability);
                abilityData.UpdateInput(frame, ref ability, input.GetAbilityInputWasPressed(abilityType));
                abilityData.TryActivateAbility(frame, filter.EntityRef, filter.PlayerLink, ref ability);
                
                dic[abilityType] = ability;
            }
            
        }

        public void OnActiveAbilityStopped(Frame f, EntityRef playerEntityRef)
        {
            AbilityInventory* abilityInventory = f.Unsafe.GetPointer<AbilityInventory>(playerEntityRef);
            
            if (!abilityInventory->HasActiveAbility)
            {
                return;
            }

            var dic = f.ResolveDictionary(abilityInventory->AbilitiesDic);
            foreach (var abilityPair in dic)
            {
                Ability ability = abilityPair.Value;

                if (ability.IsDelayedOrActive)
                {
                    ability.StopAbility(f, playerEntityRef);
                    break;
                }
            }
        }
    }
}
