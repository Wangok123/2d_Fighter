namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// 战斗输入系统 - 处理玩家战斗输入
    /// Combat Input System - Handles player combat input
    /// </summary>
    public unsafe class CombatInputSystem : SystemMainThreadFilter<CombatInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public AnimationState* AnimationState;
            public AnimationConfig* AnimationConfig;
            public ThrowState* ThrowState;
            public ComboData* ComboData;
            public HitReaction* HitReaction;
            public Status* Status;
            public Transform2D* Transform;
            public MovementData* MovementData;
        }

        // 动画状态哈希值（应该在初始化时从字符串生成）
        private const int IDLE_HASH = -1000;
        private const int ATTACK_HASH = -1001;
        private const int THROW_START_HASH = -1002;
        private const int JUMP_HASH = -1003;

        // 投技配置
        private static readonly FP GRAB_RANGE = FP._1_50; // 1.5米
        private static readonly FP GRAB_ANGLE = 45; // 45度
        private static readonly FP THROW_DAMAGE = 50;
        private static readonly FP THROW_BREAK_WINDOW = FP._0 + FP._0_30; // 0.3秒

        public override void Update(Frame frame, ref Filter filter)
        {
            // 如果死亡或在硬直中，不处理输入
            if (filter.Status->IsDead || filter.HitReaction->HitStunTimer.IsRunning)
                return;

            // 获取玩家输入
            var input = frame.GetPlayerInput(filter.PlayerLink->Player);

            // 处理攻击输入
            HandleAttackInput(frame, ref filter, input);

            // 处理投技输入
            HandleThrowInput(frame, ref filter, input);

            // 处理投技破解
            HandleThrowBreakInput(frame, ref filter, input);
        }

        private void HandleAttackInput(Frame frame, ref Filter filter, SimpleInput2D* input)
        {
            // 检查是否按下攻击键（这里假设Jump按钮用于攻击，实际应该扩展Input定义）
            if (input->Jump.WasPressed)
            {
                // 使用连招系统尝试攻击
                bool success = ComboSystem.TryAttack(frame, filter.Entity, ATTACK_HASH);
                
                if (success)
                {
                    int comboCount = ComboSystem.GetComboCount(frame, filter.Entity);
                    Log.Info($"Attack! Combo: {comboCount}");
                }
            }
        }

        private void HandleThrowInput(Frame frame, ref Filter filter, SimpleInput2D* input)
        {
            // 投技输入（这里假设Down+Jump，实际应该定义专门的按键）
            if (input->Down.WasPressed && input->Jump.WasPressed)
            {
                bool success = ThrowSystem.TryThrow(frame, filter.Entity, GRAB_RANGE, GRAB_ANGLE);
                
                if (success)
                {
                    Log.Info("Throw initiated!");
                }
            }
        }

        private void HandleThrowBreakInput(Frame frame, ref Filter filter, SimpleInput2D* input)
        {
            // 如果正在被投技，尝试破解
            // 这里需要检测是否有其他实体正在投技自己
            // 简化实现：检查自己是否是某个ThrowState的目标
            
            // 破解输入（快速连按攻击键）
            if (input->Jump.WasPressed)
            {
                // 检查是否有人在投技我
                // 这需要遍历所有实体的ThrowState，简化起见这里省略
            }
        }
    }
}
