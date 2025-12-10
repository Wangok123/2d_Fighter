using Photon.Deterministic;
using Quantum.QuantumView.Base;
using UnityCore.AnimationSystem;
using UnityEngine;

namespace Quantum.QuantumView
{
    public unsafe class PlayerViewController : QuantumEntityViewComponent<CustomViewContext>
    {
        [Header("同步优化配置")]
        [Tooltip("是否为本地玩家")]
        [SerializeField] private bool _isLocalPlayer = false;
        
        [Tooltip("本地玩家使用预测帧（灵敏），远程玩家使用验证帧（稳定）")]
        [SerializeField] private bool _useHybridFrameMode = true;

        [Tooltip("动画防抖间隔（秒）")]
        [SerializeField] private float _animationDebounceTime = 0.08f;

        [Tooltip("是否启用帧去重")]
        [SerializeField] private bool _enableFrameDeduplication = true;

        [Header("位置平滑 - 仅远程玩家")]
        [Tooltip("自动为远程玩家添加平滑组件")]
        [SerializeField] private bool _autoAddSmoothingForRemote = true;

        [Header("视图引用")]
        [SerializeField] private Transform _playerCenterTransform;
        [SerializeField] private WarriorAnimationManager _manager;

        [Header("调试信息")]
        [SerializeField] private bool _showDebugLog = false;

        private readonly Vector3 _rightRotation = Vector3.zero;
        private readonly Vector3 _leftRotation = new(0, 180, 0);

        [HideInInspector] public int LookDirection;

        private int _currentAttackStep = -1;
        
        private int _lastProcessedFrame = -1;
        private int _lastComboEventFrame = -1;
        private int _lastComboStep = -1;
        
        private float _lastAnimationTime;
        private string _currentAnimationState;

        private QuantumSmoothTransform _smoothTransform;
        private bool _useSmoothTransform;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventAbilityActivated>(this, OnAbilityActivated);
            QuantumEvent.Subscribe<EventAbilityCancelled>(this, OnAbilityCancelled);
            QuantumEvent.Subscribe<EventAbilityEnded>(this, OnAbilityEnded);

            QuantumEvent.Subscribe<EventComboAttackStarted>(this, OnComboAttackStarted);
            QuantumEvent.Subscribe<EventChargingStarted>(this, OnChargeAttackStarted);
            QuantumEvent.Subscribe<EventChargingCancelled>(this, OnChargeAttackCancelled);
            QuantumEvent.Subscribe<EventChargeAttackReleased>(this, OnChargeAttackReleased);
            QuantumEvent.Subscribe<EventCommandAttackExecuted>(this, OnCommandAttackExecuted);

            QuantumEvent.Subscribe<EventOnPlayerRespawned>(this, OnPlayerRespawned);
            QuantumEvent.Subscribe<EventOnPlayerKnockedBack>(this, OnPlayerKnockedBack);

            _lastProcessedFrame = -1;
            _currentAnimationState = null;
            _currentAttackStep = -1;

            if (frame.TryGet<PlayerLink>(EntityRef, out var playerLink))
            {
                if (Game != null)
                {
                    _isLocalPlayer = Game.PlayerIsLocal(playerLink.Player);
                }
                
                if (_showDebugLog)
                {
                    Debug.Log($"[PlayerViewController] Entity {EntityRef} - IsLocal: {_isLocalPlayer}, PlayerRef: {playerLink.Player}");
                }
            }

            InitializeSmoothTransform(frame);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void InitializeSmoothTransform(Frame frame)
        {
            if (_autoAddSmoothingForRemote && !_isLocalPlayer)
            {
                _smoothTransform = GetComponent<QuantumSmoothTransform>();
                if (_smoothTransform == null)
                {
                    _smoothTransform = gameObject.AddComponent<QuantumSmoothTransform>();
                }

                if (frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out var transform2D))
                {
                    _smoothTransform.Initialize(transform2D->Position.ToUnityVector3());
                }

                _useSmoothTransform = true;

                if (_showDebugLog)
                {
                    Debug.Log($"[PlayerViewController] QuantumSmoothTransform enabled for REMOTE player");
                }
            }
            else
            {
                _smoothTransform = GetComponent<QuantumSmoothTransform>();
                _useSmoothTransform = _smoothTransform != null && _smoothTransform.IsEnabled;

                if (_useSmoothTransform && _showDebugLog)
                {
                    Debug.Log($"[PlayerViewController] Using existing QuantumSmoothTransform");
                }
            }
        }

        public override void OnUpdateView()
        {
            Frame frameToUse = GetFrameToUse();
            if (frameToUse == null)
            {
                return;
            }

            bool isRollback = frameToUse.Number < _lastProcessedFrame;
            
            if (_enableFrameDeduplication && !isRollback && frameToUse.Number == _lastProcessedFrame)
            {
                return;
            }

            _lastProcessedFrame = frameToUse.Number;

            if (!frameToUse.Unsafe.TryGetPointer<KCC2D>(EntityRef, out var kcc))
            {
                return;
            }

            if (frameToUse.Unsafe.TryGetPointer<Transform2D>(EntityRef, out var transform2D))
            {
                UpdatePosition(transform2D->Position.ToUnityVector3(), isRollback);
            }

            KCC2DConfig config = frameToUse.FindAsset(kcc->Config);
            
            UpdateRightFace(frameToUse);
            
            bool isPlayingAbility = IsPlayingAbility(frameToUse);
            
            if (!isPlayingAbility)
            {
                UpdateAnimatorMovementSpeed(kcc, config);
                UpdateAnimatorJumpState(kcc);
            }
        }

        private void UpdatePosition(Vector3 newTargetPosition, bool isRollback)
        {
            if (_useSmoothTransform && _smoothTransform != null)
            {
                _smoothTransform.SetTargetPosition(newTargetPosition, isRollback);
            }
            else
            {
                transform.position = newTargetPosition;
            }
        }

        private bool IsPlayingAbility(Frame frame)
        {
            if (frame.Unsafe.TryGetPointer<KnockbackComponent>(EntityRef, out var knockback))
            {
                if (knockback->StatusEffect.DurationTimer.IsRunning)
                {
                    if (_showDebugLog)
                    {
                        Debug.Log($"[PlayerViewController] Protecting animation - Knockback active at Frame {frame.Number}");
                    }
                    return true;
                }
            }
            
            if (frame.Unsafe.TryGetPointer<AttackComponent>(EntityRef, out var attackComponent))
            {
                if (attackComponent->IsChargingHeavy)
                {
                    if (_showDebugLog)
                    {
                        Debug.Log($"[PlayerViewController] Protecting animation - IsCharging at Frame {frame.Number}");
                    }
                    return true;
                }
            }

            if (!frame.Unsafe.TryGetPointer<AbilityInventory>(EntityRef, out var abilityInventory))
            {
                return false;
            }

            if (!abilityInventory->HasActiveAbility)
            {
                return false;
            }

            AbilityType activeAbility = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;
            
            switch (activeAbility)
            {
                case AbilityType.AttackLight:
                case AbilityType.AttackHeavy:
                case AbilityType.MovementDash:
                case AbilityType.MovementAirDash:
                case AbilityType.MovementWallSlide:
                    if (_showDebugLog)
                    {
                        Debug.Log($"[PlayerViewController] Protecting animation - Active Ability: {activeAbility} at Frame {frame.Number}");
                    }
                    return true;

                case AbilityType.MovementJump:
                case AbilityType.MovementDoubleJump:
                case AbilityType.MovementWallJump:
                    return false;

                default:
                    return false;
            }
        }

        private Frame GetFrameToUse()
        {
            if (_useHybridFrameMode)
            {
                if (_isLocalPlayer)
                {
                    return PredictedFrame ?? VerifiedFrame;
                }
                else
                {
                    return VerifiedFrame;
                }
            }
            
            return VerifiedFrame;
        }

        private void UpdateRightFace(Frame frame)
        {
            if (!frame.TryGet<MovementComponent>(EntityRef, out var movement))
            {
                return;
            }

            bool isRight = movement.IsFacingRight;
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
                    PlayAnimationSafe("Idle", () => _manager.PlayIdle());
                }
                else
                {
                    PlayAnimationSafe("Run", () => _manager.PlayRun());
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
                    PlayAnimationSafe("Jump", () => _manager.PlayJump());
                }
                else
                {
                    PlayAnimationSafe("Fall", () => _manager.PlayFall());
                }
            }
        }

        private void PlayAnimationSafe(string animName, System.Action playAction)
        {
            if (_currentAnimationState == animName && 
                Time.time - _lastAnimationTime < _animationDebounceTime)
            {
                return;
            }

            _currentAnimationState = animName;
            _lastAnimationTime = Time.time;
            
            if (_showDebugLog)
            {
                Debug.Log($"[PlayerViewController] {(_isLocalPlayer ? "LOCAL" : "REMOTE")} Playing: {animName} at Frame {_lastProcessedFrame}");
            }
            
            playAction?.Invoke();
        }

        private void OnAbilityActivated(EventAbilityActivated e)
        {
            if (e.Entity != EntityRef) return;

            switch (e.AbilityType)
            {
                case AbilityType.MovementJump:
                case AbilityType.MovementDoubleJump:
                    PlayAnimationSafe("Jump", () => _manager.PlayJump());
                    break;

                case AbilityType.MovementDash:
                case AbilityType.MovementAirDash:
                    PlayAnimationSafe("Dash", () => _manager.PlayDash());
                    break;

                case AbilityType.MovementWallSlide:
                    PlayAnimationSafe("WallSlide", () => _manager.PlayWallSlide());
                    break;

                case AbilityType.MovementWallJump:
                    PlayAnimationSafe("WallJump", () => _manager.PlayJump());
                    break;

                case AbilityType.AttackLight:
                case AbilityType.AttackHeavy:
                    break;
            }
        }

        private void OnAbilityCancelled(EventAbilityCancelled e)
        {
            if (e.Entity != EntityRef) return;

            if (e.AbilityType == AbilityType.AttackLight || 
                e.AbilityType == AbilityType.AttackHeavy)
            {
                _currentAttackStep = -1;
            }
        }

        private void OnAbilityEnded(EventAbilityEnded e)
        {
            if (e.Entity != EntityRef) return;

            if (e.AbilityType == AbilityType.AttackLight || 
                e.AbilityType == AbilityType.AttackHeavy)
            {
                _currentAttackStep = -1;
            }
        }

        private void OnComboAttackStarted(EventComboAttackStarted e)
        {
            if (e.Entity != EntityRef) return;

            int currentFrame = VerifiedFrame.Number;
            
            if (_lastComboEventFrame == currentFrame && _lastComboStep == e.Step)
            {
                return;
            }

            _lastComboEventFrame = currentFrame;
            _lastComboStep = e.Step;

            if (_currentAttackStep == e.Step)
            {
                return;
            }

            _currentAttackStep = e.Step;

            switch (e.Step)
            {
                case 1:
                    PlayAnimationSafe("Attack1", () => _manager.PlayAttack1());
                    break;
                case 2:
                    PlayAnimationSafe("Attack2", () => _manager.PlayAttack2());
                    break;
            }
        }

        private void OnChargeAttackStarted(EventChargingStarted e)
        {
            if (e.Entity != EntityRef) return;

            PlayAnimationSafe("ChargeStart", () => _manager.PlayChargeStart());
        }

        private void OnChargeAttackCancelled(EventChargingCancelled e)
        {
            if (e.Entity != EntityRef) return;
        }

        private void OnChargeAttackReleased(EventChargeAttackReleased e)
        {
            if (e.Entity != EntityRef) return;

            string animName = e.IsFullyCharged ? "HeavyAttackFull" : "HeavyAttack";
            PlayAnimationSafe(animName, () => _manager.PlayHeavyAttack());
        }
        
        private void OnCommandAttackExecuted(EventCommandAttackExecuted e)
        {
            if (e.PlayerEntityRef != EntityRef) return;
        }

        private void OnPlayerKnockedBack(EventOnPlayerKnockedBack e)
        {
            if (e.Entity != EntityRef) return;

            PlayAnimationSafe("Hurt", () => _manager.PlayHurt());
    
            if (_showDebugLog)
            {
                Debug.Log($"[PlayerViewController] Playing Hurt animation - Direction: {e.Direction}, Force: {e.Force}");
            }
        }
        
        private void OnPlayerRespawned(EventOnPlayerRespawned e)
        {
            if (e.Player != EntityRef) return;

            ResetViewState();

            if (_showDebugLog)
            {
                Debug.Log($"[PlayerViewController] {(_isLocalPlayer ? "LOCAL" : "REMOTE")} Player respawned, view state reset");
            }
        }

        private void ResetViewState()
        {
            _currentAttackStep = -1;
            _currentAnimationState = null;
            _lastAnimationTime = 0f;
            _lastComboEventFrame = -1;
            _lastComboStep = -1;

            if (_manager != null)
            {
                _manager.PlayIdle();
            }

            if (_useSmoothTransform && _smoothTransform != null)
            {
                Frame frame = VerifiedFrame ?? PredictedFrame;
                if (frame != null && frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out var transform2D))
                {
                    _smoothTransform.Initialize(transform2D->Position.ToUnityVector3());
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = _isLocalPlayer ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (_playerCenterTransform != null)
            {
                Gizmos.color = LookDirection > 0 ? Color.green : Color.red;
                Vector3 direction = LookDirection > 0 ? Vector3.right : Vector3.left;
                Gizmos.DrawRay(_playerCenterTransform.position, direction * 1f);
            }
        }
    }
}
