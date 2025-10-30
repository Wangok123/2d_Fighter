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
        
        /// <summary>
        /// Direction the character is currently facing: 1 for right, -1 for left.
        /// </summary>
        [HideInInspector] public int LookDirection; 
        [SerializeField] private Transform _playerCenterTransform;
        [SerializeField] private WarriorAnimationManager _manager;

        public override void OnActivate(Frame frame)
        {
            base.OnActivate(frame);
        }

        public override void OnUpdateView()
        {
            KCC2D* kcc = VerifiedFrame.Unsafe.GetPointer<KCC2D>(EntityRef);
            KCC2DConfig config = VerifiedFrame.FindAsset(kcc->Config);
            UpdateRightFace();
            UpdateAnimatorMovementSpeed(kcc, config);
            UpdateAnimatorJumpState(kcc);
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

    }
}