using Quantum.QuantumView.Base;
using UnityEngine;

namespace Quantum.QuantumView
{
    public class CharacterView : QuantumEntityViewComponent<CustomViewContext>
    {
        private static readonly int IsFacingRight = Animator.StringToHash("IsFacingRight");
        private readonly Vector3 _rightRotation = Vector3.zero;
        private readonly Vector3 _leftRotation = new(0, 180, 0);
        
        /// <summary>
        /// Direction the character is currently facing: 1 for right, -1 for left.
        /// </summary>
        [HideInInspector] public int LookDirection;
        public Transform Body;
        public Animator CharacterAnimator;
        
        public override void OnUpdateView()
        {
            bool isRight = VerifiedFrame.Get<MovementData>(EntityRef).IsFacingRight;
            if (isRight)
            { 
                Body.localRotation = Quaternion.Euler(_rightRotation);
                LookDirection = 1; 
            }
            else
            {
                Body.localRotation = Quaternion.Euler(_leftRotation);
                LookDirection = -1;
            }
        }
    }
}