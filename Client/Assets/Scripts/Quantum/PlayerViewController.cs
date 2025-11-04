using Photon.Deterministic;
using Quantum.QuantumView.Base;
using UnityCore.AnimationSystem;
using UnityEngine;

namespace Quantum.QuantumView
{
    public unsafe class PlayerViewController : QuantumEntityViewComponent<CustomViewContext>
    {
        private readonly Vector3 _rightRotation = Vector3.zero;
        private readonly Vector3 _leftRotation = new(0, 180, 0);
        
        [HideInInspector] public int LookDirection; 
        [SerializeField] private Transform _playerCenterTransform;
        [SerializeField] private WarriorAnimationManager _manager;

        private bool _isPlayingAbilityAnimation;
        
        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventAbilityActivated>(this, OnAbilityActivated);
            QuantumEvent.Subscribe<EventAbilityCancelled>(this, OnAbilityCancelled);
            QuantumEvent.Subscribe<EventAbilityEnded>(this, OnAbilityEnded);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        public override void OnUpdateView()
        {
            KCC2D* kcc = VerifiedFrame.Unsafe.GetPointer<KCC2D>(EntityRef);
            KCC2DConfig config = VerifiedFrame.FindAsset(kcc->Config);
            UpdateRightFace();
            if (!_isPlayingAbilityAnimation)
            {
                UpdateAnimatorMovementSpeed(kcc, config);
                UpdateAnimatorJumpState(kcc);
            }
        }

        private void UpdateRightFace()
        {
            bool isRight = VerifiedFrame.Get<MovementData>(EntityRef).IsFacingRight;
            if (isRight)
            {
                _playerCenterTransform.localRotation = Quaternion.Euler(_rightRotation);
                LookDirection = 1;
            }
            else
            {
                _playerCenterTransform.localRotation = Quaternion.Euler(_leftRotation);
                LookDirection = -1;
            }
        }
        
        private void UpdateAnimatorMovementSpeed(KCC2D* kcc, KCC2DConfig config)
        {
            var isGrounded = kcc->State == KCCState.GROUNDED;
            FP normalizedSpeed = kcc->_kinematicVelocity.Magnitude / config.BaseSettings.MaxBaseSpeed;
            if (isGrounded) 
            {
                if (normalizedSpeed <= 0.5f.ToFP())
                {
                    _manager.PlayIdle();
                }
                else
                {
                    _manager.PlayRun();
                }
            }
        }

        private void UpdateAnimatorJumpState(KCC2D* kcc)
        {
            var isGrounded = kcc->State == KCCState.GROUNDED;
            if (!isGrounded)
            {
                if (kcc->_kinematicVelocity.Y > 0)
                {
                    _manager.PlayJump();
                }
                else
                {
                    _manager.PlayFall();
                }
            }
        }

        private void OnAbilityActivated(EventAbilityActivated e)
        {
            if (e.Entity != EntityRef) return;

            switch (e.AbilityType)
            {
                case AbilityType.MovementJump:
                case AbilityType.MovementDoubleJump:
                    _manager.PlayJump();
                    break;
                    
                case AbilityType.MovementDash:
                case AbilityType.MovementAirDash:
                    _isPlayingAbilityAnimation = true;
                    _manager.PlayDash();
                    break;
                    
                case AbilityType.MovementWallSlide:
                    _isPlayingAbilityAnimation = true;
                    _manager.PlayWallSlide();
                    break;
                    
                case AbilityType.MovementWallJump:
                    _manager.PlayJump();
                    break;
                    
                case AbilityType.AttackLight:
                    _isPlayingAbilityAnimation = true;
                    _manager.PlayAttack();
                    break;
            }
        }

        private void OnAbilityCancelled(EventAbilityCancelled e)
        {
            if (e.Entity != EntityRef) return;

            switch (e.AbilityType)
            {
                case AbilityType.MovementWallSlide:
                    _isPlayingAbilityAnimation = false;
                    break;
                    
                case AbilityType.MovementDash:
                case AbilityType.MovementAirDash:
                case AbilityType.AttackLight:
                    _isPlayingAbilityAnimation = false;
                    break;
            }
        }

        private void OnAbilityEnded(EventAbilityEnded e)
        {
            if (e.Entity != EntityRef) return;

            switch (e.AbilityType)
            {
                case AbilityType.MovementDash:
                case AbilityType.MovementAirDash:
                case AbilityType.AttackLight:
                case AbilityType.MovementWallSlide:
                    _isPlayingAbilityAnimation = false;
                    break;
            }
        }

    }
}