using UnityEngine;
using UnityCore.AnimationSystem;

namespace Examples
{
    /// <summary>
    /// 取消机制验证测试
    /// Cancel Mechanism Validation Test
    /// 
    /// 此脚本用于验证取消机制是否正常工作
    /// This script validates that the cancel mechanism works correctly
    /// </summary>
    public class CancelMechanismValidation : MonoBehaviour
    {
        private AnimationStateManager _stateManager;
        private bool _testsPassed = true;

        void Start()
        {
            // 创建一个模拟的Animator用于测试
            // Create a mock Animator for testing
            var animator = gameObject.AddComponent<Animator>();
            _stateManager = new AnimationStateManager(animator);

            Debug.Log("========== Cancel Mechanism Validation Tests ==========");
            
            RunTests();

            if (_testsPassed)
            {
                Debug.Log("✅ All tests passed!");
            }
            else
            {
                Debug.LogError("❌ Some tests failed!");
            }
            
            Debug.Log("======================================================");
        }

        void RunTests()
        {
            TestPriorityComparison();
            TestCancelPolicies();
            TestCancelWindows();
            TestBackwardCompatibility();
            TestQueryMethods();
        }

        void TestPriorityComparison()
        {
            Debug.Log("\n--- Test: Priority Comparison ---");

            // 注册不同优先级的状态
            _stateManager.RegisterState("Idle", priority: AnimationPriority.Idle);
            _stateManager.RegisterState("Attack", priority: AnimationPriority.Attack);
            _stateManager.RegisterState("Hit", priority: AnimationPriority.Hit);
            _stateManager.RegisterState("Death", priority: AnimationPriority.Death);

            // Test 1: Higher priority can interrupt lower
            Assert(
                _stateManager.GetPriority("Hit") > _stateManager.GetPriority("Attack"),
                "Hit should have higher priority than Attack"
            );

            // Test 2: Death has highest priority
            Assert(
                _stateManager.GetPriority("Death") == AnimationPriority.Death,
                "Death should have Death priority"
            );

            Debug.Log("✓ Priority comparison tests passed");
        }

        void TestCancelPolicies()
        {
            Debug.Log("\n--- Test: Cancel Policies ---");

            // 清除之前的状态
            _stateManager.Clear();

            // 注册带不同取消策略的状态
            _stateManager.RegisterState("AlwaysCancellable", 
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            _stateManager.RegisterState("NonCancellable",
                priority: AnimationPriority.Attack,
                cancelPolicy: AnimationCancelPolicy.NonCancellable);

            _stateManager.RegisterState("TestTarget",
                priority: AnimationPriority.Movement);

            // Note: 由于没有实际的Animator，我们只能测试逻辑
            // 实际的动画播放测试需要在Unity场景中进行
            
            Debug.Log("✓ Cancel policy tests setup completed");
        }

        void TestCancelWindows()
        {
            Debug.Log("\n--- Test: Cancel Windows ---");

            // 测试取消窗口创建
            var window1 = new CancelWindow(0.4f, 0.7f, new[] { "Attack" });
            
            Assert(window1.IsInWindow(0.5f), "0.5 should be in window [0.4, 0.7]");
            Assert(!window1.IsInWindow(0.3f), "0.3 should not be in window [0.4, 0.7]");
            Assert(!window1.IsInWindow(0.8f), "0.8 should not be in window [0.4, 0.7]");

            // 测试目标检查
            Assert(window1.CanCancelTo("Attack"), "Should be able to cancel to Attack");
            Assert(!window1.CanCancelTo("Jump"), "Should not be able to cancel to Jump");

            // 测试无限制窗口
            var window2 = new CancelWindow(0.5f, 1.0f, null);
            Assert(window2.CanCancelTo("AnyAnimation"), "Null targets should allow any animation");

            Debug.Log("✓ Cancel window tests passed");
        }

        void TestBackwardCompatibility()
        {
            Debug.Log("\n--- Test: Backward Compatibility ---");

            _stateManager.Clear();

            // 测试旧API仍然工作
            _stateManager.RegisterState("OldStyle1");
            _stateManager.RegisterState("OldStyle2", layer: 1);
            _stateManager.RegisterState("OldStyle3", layer: 0, crossfadeDuration: 0.2f);
            _stateManager.RegisterState("OldStyle4", isDefault: true);

            // 验证默认值
            Assert(
                _stateManager.GetPriority("OldStyle1") == AnimationPriority.Idle,
                "Default priority should be Idle"
            );

            Debug.Log("✓ Backward compatibility tests passed");
        }

        void TestQueryMethods()
        {
            Debug.Log("\n--- Test: Query Methods ---");

            _stateManager.Clear();

            _stateManager.RegisterState("TestAnim", 
                priority: AnimationPriority.Attack,
                isDefault: true);

            // 测试优先级查询
            var priority = _stateManager.GetPriority("TestAnim");
            Assert(
                priority == AnimationPriority.Attack,
                "GetPriority should return correct priority"
            );

            // 测试当前优先级（在没有播放动画时应该是Idle）
            var currentPriority = _stateManager.GetCurrentPriority();
            Assert(
                currentPriority == AnimationPriority.Idle,
                "Current priority should be Idle when no animation is playing"
            );

            Debug.Log("✓ Query method tests passed");
        }

        void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($"❌ Test failed: {message}");
                _testsPassed = false;
            }
            else
            {
                Debug.Log($"  ✓ {message}");
            }
        }
    }
}
