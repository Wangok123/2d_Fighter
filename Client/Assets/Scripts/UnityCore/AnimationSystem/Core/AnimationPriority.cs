namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// 动画优先级枚举 - 用于控制动画切换的优先级
    /// Animation Priority Enum - Used to control animation transition priority
    /// 
    /// 在格斗游戏中，某些动画不应该被其他动画打断
    /// In fighting games, certain animations should not be interrupted by others
    /// 
    /// 优先级规则 / Priority Rules:
    /// - 高优先级动画可以打断低优先级动画
    /// - Higher priority animations can interrupt lower priority animations
    /// - 相同优先级需要检查取消策略
    /// - Same priority requires checking cancel policy
    /// </summary>
    public enum AnimationPriority
    {
        /// <summary>默认/闲置状态 - 最低优先级，可被任何动画打断</summary>
        Idle = 0,

        /// <summary>移动动画 - 低优先级，如行走、跑步</summary>
        Movement = 10,

        /// <summary>跳跃/空中动作 - 中低优先级</summary>
        Jump = 20,

        /// <summary>普通攻击 - 中等优先级</summary>
        Attack = 30,

        /// <summary>特殊技能 - 中高优先级</summary>
        Skill = 40,

        /// <summary>投技/抓取攻击 - 高优先级</summary>
        Throw = 50,

        /// <summary>受击反应 - 很高优先级，几乎总是可以打断其他动画</summary>
        Hit = 60,

        /// <summary>倒地/击飞 - 极高优先级</summary>
        Knockdown = 70,

        /// <summary>死亡动画 - 最高优先级，不可被打断</summary>
        Death = 100
    }

    /// <summary>
    /// 动画取消策略 - 定义动画何时可以被取消
    /// Animation Cancel Policy - Defines when an animation can be cancelled
    /// 
    /// 格斗游戏中的"取消"机制允许玩家在特定时机用其他动作打断当前动作
    /// The "cancel" mechanism in fighting games allows players to interrupt current actions at specific timings
    /// </summary>
    public enum AnimationCancelPolicy
    {
        /// <summary>不可取消 - 动画必须播放完成</summary>
        NonCancellable = 0,

        /// <summary>任意时刻可取消 - 可随时被打断</summary>
        AlwaysCancellable = 1,

        /// <summary>仅在取消窗口内可取消 - 需要精确时机</summary>
        CancellableInWindow = 2,

        /// <summary>仅在动画结束时可取消 - 需要等待动画接近完成</summary>
        CancellableOnEnd = 3,

        /// <summary>仅可被更高优先级打断</summary>
        OnlyByHigherPriority = 4
    }

    /// <summary>
    /// 取消窗口 - 定义动画可以被取消的时间范围
    /// Cancel Window - Defines the time range when an animation can be cancelled
    /// 
    /// 用于实现格斗游戏中的"连招"系统
    /// Used to implement combo systems in fighting games
    /// </summary>
    public struct CancelWindow
    {
        /// <summary>开始时间（归一化时间 0-1）</summary>
        public float StartTime;

        /// <summary>结束时间（归一化时间 0-1）</summary>
        public float EndTime;

        /// <summary>允许取消到的目标动画列表（null表示任意动画）</summary>
        public string[] AllowedTargets;

        public CancelWindow(float startTime, float endTime, string[] allowedTargets = null)
        {
            StartTime = startTime;
            EndTime = endTime;
            AllowedTargets = allowedTargets;
        }

        /// <summary>
        /// 检查当前时间是否在取消窗口内
        /// Check if current time is within cancel window
        /// </summary>
        public bool IsInWindow(float normalizedTime)
        {
            return normalizedTime >= StartTime && normalizedTime <= EndTime;
        }

        /// <summary>
        /// 检查目标动画是否允许被取消到
        /// Check if target animation is allowed to cancel to
        /// </summary>
        public bool CanCancelTo(string targetAnimation)
        {
            // 如果没有指定目标，允许取消到任意动画
            if (AllowedTargets == null || AllowedTargets.Length == 0)
                return true;

            // 检查目标是否在允许列表中
            foreach (var allowed in AllowedTargets)
            {
                if (allowed == targetAnimation)
                    return true;
            }

            return false;
        }
    }
}
