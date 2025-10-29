using Quantum.QuantumView.Base;
using UnityEngine;

namespace Quantum.QuantumView
{
    public unsafe class PlayerViewController : QuantumEntityViewComponent<CustomViewContext>
    {
        private static readonly int IsFacingRight = Animator.StringToHash("IsFacingRight");
        private readonly Vector3 _rightRotation = Vector3.zero;
        private readonly Vector3 _leftRotation = new(0, 180, 0);
        
        /// <summary>
        /// Direction the character is currently facing: 1 for right, -1 for left.
        /// </summary>
        [HideInInspector] public int LookDirection; 
        [SerializeField] private Transform _playerCenterTransform;
        [SerializeField] private Animator _animator;
        
        public override void OnUpdateView()
        {
            UpdateRightFace();
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
        
        // private void UpdateAnimatorMovementSpeed(KCC2D* kcc, KCC2DConfig config)
        // {
        //     float normalizedSpeed = kcc->Ve->Velocity.ToUnityVector3().X0Z().magnitude / defaultKCCConfig.MaxSpeed.AsFloat;
        //     _animator.SetFloat(NORMALIZED_SPEED_ANIM_HASH, normalizedSpeed);
        // }
        //
        // private void UpdateAnimatorMovementSpeed(CharacterController3D* kcc, CharacterController2DConfig defaultKCCConfig)
        // {
        //     float normalizedSpeed = kcc->Velocity.ToUnityVector3().X0Z().magnitude / defaultKCCConfig.MaxSpeed.AsFloat;
        //     _animator.SetFloat(NORMALIZED_SPEED_ANIM_HASH, normalizedSpeed);
        // }

    }
}