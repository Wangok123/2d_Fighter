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

        // 添加：当前播放的攻击段数，防止重复播放
        private int _currentAttackStep = -1;
        private int _lastComboEventFrame = -1;
        private int _lastComboStep = -1;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventAbilityActivated>(this, OnAbilityActivated);
            QuantumEvent.Subscribe<EventAbilityCancelled>(this, OnAbilityCancelled);
            QuantumEvent.Subscribe<EventAbilityEnded>(this, OnAbilityEnded);

            // 订阅攻击事件
            QuantumEvent.Subscribe<EventComboAttackStarted>(this, OnComboAttackStarted);
            QuantumEvent.Subscribe<EventChargingStarted>(this, OnChargeAttackStarted);
            QuantumEvent.Subscribe<EventChargingCancelled>(this, OnChargeAttackCancelled);
            QuantumEvent.Subscribe<EventChargeAttackReleased>(this, OnChargeAttackReleased);
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
            bool isRight = VerifiedFrame.Get<MovementComponent>(EntityRef).IsFacingRight;
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
                    break;

                case AbilityType.AttackHeavy:
                    _isPlayingAbilityAnimation = true;
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
                case AbilityType.AttackHeavy:
                    _isPlayingAbilityAnimation = false;
                    _currentAttackStep = -1; // 重置攻击段数
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
                case AbilityType.AttackHeavy:
                case AbilityType.MovementWallSlide:
                    _isPlayingAbilityAnimation = false;
                    _currentAttackStep = -1; // 重置攻击段数
                    break;
            }
        }

        // 轻攻击连击事件
        private void OnComboAttackStarted(EventComboAttackStarted e)
        {
            if (e.Entity != EntityRef) return;

            // 防止Quantum回滚导致的重复事件触发
            int currentFrame = VerifiedFrame.Number;
            if (_lastComboEventFrame == currentFrame && _lastComboStep == e.Step)
            {
                return;
            }

            _lastComboEventFrame = currentFrame;
            _lastComboStep = e.Step;

            // 添加：防止同一段攻击重复播放
            if (_currentAttackStep == e.Step)
            {
                return;
            }

            _currentAttackStep = e.Step;

            // 根据连击段数播放动画
            switch (e.Step)
            {
                case 1:
                    _manager.PlayAttack1(); // Attack_1
                    break;
                case 2:
                    _manager.PlayAttack2(); // Attack_2
                    break;
            }
        }

        private void OnChargeAttackStarted(EventChargingStarted e)
        {
            if (e.Entity != EntityRef) return;

            _isPlayingAbilityAnimation = true;
            // 播放蓄力开始动画/特效
            _manager.PlayChargeStart(); // ChargeStart
        }

        // 蓄力取消事件
        private void OnChargeAttackCancelled(EventChargingCancelled e)
        {
            if (e.Entity != EntityRef) return;

            _isPlayingAbilityAnimation = false;
            // 播放蓄力取消动画/特效
            Debug.Log("蓄力被取消");
        }

        // 重攻击释放事件
        private void OnChargeAttackReleased(EventChargeAttackReleased e)
        {
            if (e.Entity != EntityRef) return;

            // 根据是否满蓄力播放不同动画
            if (e.IsFullyCharged)
            {
                _manager.PlayHeavyAttack(); // Attack_3（满蓄力重攻击）
            }
            else
            {
                _manager.PlayHeavyAttack(); // Attack（普通重攻击）
            }
        }
    }
}