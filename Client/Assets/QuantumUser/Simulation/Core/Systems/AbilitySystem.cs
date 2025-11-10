using UnityEngine.Scripting;

namespace Quantum
{
    using Photon.Deterministic;

    [Preserve]
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

            foreach (var abilityPair in dic)
            {
                AbilityType abilityType = abilityPair.Key;
                if (!dic.TryGetValuePointer(abilityType, out var ability))
                {
                    continue;
                }

                AbilityData abilityData = frame.FindAsset<AbilityData>(ability->AbilityData.Id);

                abilityData.UpdateAbility(frame, filter.EntityRef, ability);
                abilityData.UpdateInput(frame, filter.EntityRef, abilityType, ability, input);
                abilityData.TryActivateAbility(frame, filter.EntityRef, filter.PlayerLink, abilityType, ref *ability);
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
                    dic[abilityPair.Key] = ability;
                    break;
                }
            }
        }
    }
}
