namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// 连招系统 - 处理连招逻辑
    /// Combo System - Handles combo logic
    /// </summary>
    public unsafe class ComboSystem : SystemMainThreadFilter<ComboSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public ComboData* ComboData;
            public AnimationState* AnimationState;
        }

        // 连招窗口时间（秒）
        private static readonly FP COMBO_WINDOW_TIME = FP._0_50; // 0.5秒

        public override void Update(Frame frame, ref Filter filter)
        {
            UpdateComboWindow(frame, ref filter);
        }

        /// <summary>
        /// 尝试执行攻击（会检查连招）
        /// Try to attack (will check combo)
        /// </summary>
        public static bool TryAttack(Frame frame, EntityRef attacker, int attackHash)
        {
            if (!frame.Unsafe.TryGetPointer(attacker, out ComboData* comboData))
                return false;

            // 尝试播放攻击动画
            bool canPlay = AnimationSystem.CanPlayAnimation(frame, attacker, attackHash);
            if (!canPlay)
                return false;

            bool success = AnimationSystem.TryPlayAnimation(frame, attacker, attackHash);
            if (!success)
                return false;

            // 更新连招数据
            if (comboData->ComboWindowTimer.IsRunning && comboData->LastAttackHash == attackHash)
            {
                // 在连招窗口内，增加连招计数
                comboData->ComboCount++;
            }
            else
            {
                // 新的连招开始
                comboData->ComboCount = 1;
            }

            comboData->LastAttackHash = attackHash;
            comboData->ComboWindowTimer = FrameTimer.CreateFromSeconds(frame, COMBO_WINDOW_TIME);

            return true;
        }

        /// <summary>
        /// 重置连招
        /// Reset combo
        /// </summary>
        public static void ResetCombo(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer(entity, out ComboData* comboData))
            {
                comboData->ComboCount = 0;
                comboData->LastAttackHash = 0;
                comboData->ComboWindowTimer = default;
            }
        }

        /// <summary>
        /// 获取当前连招数
        /// Get current combo count
        /// </summary>
        public static int GetComboCount(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer(entity, out ComboData* comboData))
            {
                return comboData->ComboCount;
            }
            return 0;
        }

        private void UpdateComboWindow(Frame frame, ref Filter filter)
        {
            // 如果连招窗口过期，重置连招
            if (filter.ComboData->ComboWindowTimer.IsRunning && 
                filter.ComboData->ComboWindowTimer.Expired(frame))
            {
                filter.ComboData->ComboCount = 0;
                filter.ComboData->LastAttackHash = 0;
            }
        }
    }
}
