using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    public enum AbilityPriority
    {
        Passive = 0,
        Low = 1,
        Normal = 2,
        High = 3,
        Ultimate = 4
    }
    
    public unsafe class AbilityData : AssetObject
    {
        public FP InputBuffer = FP._0_10 + FP._0_05;
        public FP Delay = FP._0_10 + FP._0_05;
        public FP Duration = FP._0_25;
        public FP Cooldown = 5;
        
        public AbilityCastDirectionType CastDirectionType = AbilityCastDirectionType.FacingDirection;
        public bool FaceCastDirection = true;
        public bool KeepVelocity = false;
        public bool StartCooldownAfterDelay = false;
        
        [Header("Movement Settings")]
        public bool DisableMovementDuringAbility = false;
        
        [Header("Cancel Settings")]
        public AbilityPriority Priority = AbilityPriority.Normal;
        public bool CanBeCancelledByHigherPriority = true;
        public bool CanCancelLowerPriority = true;
        
        [Header("Unity")] [SerializeField] private GameObject _uiAbilityPrefab;
        
        public bool HasUIPrefab => _uiAbilityPrefab != null;
        public GameObject UIAbilityPrefab => _uiAbilityPrefab;
        
        public virtual Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            return ability->Update(frame, entityRef);
        }

        public virtual void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            if (ShouldBufferInput(frame, entityRef, abilityType, ability, input))
            {
                ability->BufferInput(frame);
            }
        }

        protected virtual bool ShouldBufferInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            if (abilityType == AbilityType.None || !ability->AbilityData.Id.IsValid)
            {
                return false;
            }

            return input.GetAbilityInputWasPressed(abilityType);
        }

        public virtual bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            if (ability.HasBufferedInput)
            {
                if (TryActivateAbilityInternal(frame, entityRef, playerLink, abilityType, ref ability))
                {
                    return true;
                }
            }

            return false;
        }
        
        protected bool TryActivateAbilityInternal(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);

            if (abilityInventory->HasActiveAbility)
            {
                if (!TryCancelActiveAbility(frame, entityRef, abilityInventory))
                {
                    return false;
                }
            }

            bool activated = ability.TryActivateAbility(frame, entityRef, playerLink->Player, abilityType);
            
            if (activated)
            {
                frame.Events.AbilityActivated(entityRef, abilityType);
            }
            
            return activated;
        }
        
        protected virtual bool CanCancelAbility(Frame frame, EntityRef entityRef, AbilityData otherAbility)
        {
            if (!CanCancelLowerPriority)
            {
                return false;
            }

            if (!otherAbility.CanBeCancelledByHigherPriority)
            {
                return false;
            }

            return Priority > otherAbility.Priority;
        }

        private bool TryCancelActiveAbility(Frame frame, EntityRef entityRef, AbilityInventory* abilityInventory)
        {
            AbilityType activeAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;
            
            if (activeAbilityType == AbilityType.None)
            {
                return true;
            }

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValuePointer(activeAbilityType, out Ability* activeAbility))
            {
                return true;
            }

            AbilityData activeAbilityData = frame.FindAsset<AbilityData>(activeAbility->AbilityData.Id);
            
            if (CanCancelAbility(frame, entityRef, activeAbilityData))
            {
                activeAbility->StopAbility(frame, entityRef);
                
                frame.Events.AbilityCancelled(entityRef, activeAbilityType);
                OnAbilityCancelled(frame, entityRef, activeAbilityType);
                
                return true;
            }

            return false;
        }

        protected virtual void OnAbilityCancelled(Frame frame, EntityRef entityRef, AbilityType cancelledAbilityType)
        {
        }
        
        public virtual void OnCommandInputDetected(Frame frame, EntityRef entityRef)
        {
        }
    }
}
