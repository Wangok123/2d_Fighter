namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Refactored attack system using Strategy Pattern for better extensibility and maintainability.
    /// 
    /// Previous Implementation Issues:
    /// 1. Tight coupling - priority was implicit in code structure
    /// 2. Hard to extend - adding new attack types required modifying core logic
    /// 3. Poor testability - couldn't test attack types in isolation
    /// 4. Maintainability - priority changes required code restructuring
    /// 
    /// Current Implementation Benefits:
    /// 1. Separation of concerns - each attack type is a separate handler
    /// 2. Explicit priority system - priorities are configurable and visible
    /// 3. Easy to extend - new attack types are just new handler classes
    /// 4. Better testability - handlers can be tested independently
    /// 5. Flexible - can add/remove handlers at runtime if needed
    /// </summary>
    public unsafe class NormalAttackSystem : SystemMainThreadFilter<NormalAttackSystem.Filter>
    {
        private AttackHandlerManager _attackHandlerManager;

        public override void OnInit(Frame frame)
        {
            base.OnInit(frame);
            _attackHandlerManager = new AttackHandlerManager();
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if dead
            if (filter.Status->IsDead)
            {
                return;
            }

            // Get input
            SimpleInput2D input = default;
            if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }

            // Get attack config
            var attackConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (attackConfig == null)
            {
                return;
            }

            // Update combo reset timer
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }

            // Update attack cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Use strategy pattern to handle attacks
            // Handlers are automatically sorted by priority (Special > Heavy > Light)
            _attackHandlerManager.ProcessAttack(frame, ref filter, input, attackConfig);
        }

        public struct Filter
        {
            public EntityRef Entity;
            public CharacterStatus* Status;
            public AttackData* AttackData;
        }
    }
}
