using UnityEngine;
using Core.AnimationSystem;

namespace Examples
{
    /// <summary>
    /// 动画系统使用示例
    /// Example usage of the animation system
    /// 
    /// 这是一个简单的示例，展示如何使用动画状态管理系统
    /// This is a simple example showing how to use the animation state management system
    /// </summary>
    public class AnimationSystemExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        private AnimationStateManager _stateManager;

        void Start()
        {
            // 初始化动画管理器
            // Initialize animation manager
            _stateManager = new AnimationStateManager(animator);

            // 注册动画状态
            // Register animation states
            _stateManager.RegisterState("Idle", isDefault: true);
            _stateManager.RegisterState("Walk", crossfadeDuration: 0.15f);
            _stateManager.RegisterState("Run", crossfadeDuration: 0.15f);
            _stateManager.RegisterState("Jump", crossfadeDuration: 0.1f);
            _stateManager.RegisterState("Attack", crossfadeDuration: 0.05f);

            // 播放默认状态
            // Play default state
            _stateManager.PlayDefaultState();

            Debug.Log("Animation System Example initialized");
        }

        void Update()
        {
            // 示例：使用键盘控制动画
            // Example: Control animations with keyboard
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Playing Idle animation");
                _stateManager.PlayAnimation("Idle");
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Playing Walk animation");
                _stateManager.PlayAnimation("Walk");
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Playing Run animation");
                _stateManager.PlayAnimation("Run");
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Playing Jump animation");
                _stateManager.PlayAnimation("Jump");
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Playing Attack animation");
                _stateManager.PlayAnimation("Attack");
            }

            // 示例：检查动画状态
            // Example: Check animation state
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log($"Current Animation: {_stateManager.CurrentState}");
                Debug.Log($"Is Idle Playing: {_stateManager.IsPlaying("Idle")}");
                Debug.Log($"Animation Progress: {_stateManager.GetNormalizedTime():F2}");
            }

            // 示例：等待动画完成后返回Idle
            // Example: Return to Idle after animation finishes
            if (_stateManager.IsPlaying("Attack") && _stateManager.IsAnimationFinished())
            {
                _stateManager.PlayAnimation("Idle");
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Animation System Example");
            GUILayout.Label("Press 1-3 for Idle/Walk/Run");
            GUILayout.Label("Press Space for Jump");
            GUILayout.Label("Press F for Attack");
            GUILayout.Label("Press Q for Status Info");
            GUILayout.Label($"Current: {_stateManager?.CurrentState ?? "None"}");
            GUILayout.EndArea();
        }
    }
}
