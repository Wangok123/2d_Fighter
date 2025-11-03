using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

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
        
        [Header("Unity")] [SerializeField] private GameObject _uiAbilityPrefab;
        
        public bool HasUIPrefab => _uiAbilityPrefab != null;
        public GameObject UIAbilityPrefab => _uiAbilityPrefab;
        
        public virtual Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, ref Ability ability)
        {
            return ability.Update(frame, entityRef);
        }

        public virtual void UpdateInput(Frame frame, ref Ability ability, bool inputWasPressed)
        {
            if (inputWasPressed)
            {
                ability.BufferInput(frame);
            }
        }

        public virtual bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerStatus, ref Ability ability)
        {
            
            if (ability.HasBufferedInput)
            {
                if (ability.TryActivateAbility(frame, entityRef, playerStatus->Player))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
