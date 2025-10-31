namespace Quantum
{
    using System.Collections.Generic;
    using System.Linq;
    using Photon.Deterministic;

    /// <summary>
    /// Manager that coordinates different attack handlers using the Strategy Pattern.
    /// Provides explicit priority management and better extensibility.
    /// 
    /// Note: Sorting is performed in constructor and when adding handlers.
    /// For typical usage (3-5 handlers, rarely modified at runtime), this is efficient.
    /// If handlers are frequently added/removed at runtime, consider using a priority queue.
    /// </summary>
    public unsafe class AttackHandlerManager
    {
        private readonly List<IAttackHandler> _handlers;

        public AttackHandlerManager()
        {
            _handlers = new List<IAttackHandler>
            {
                new SpecialMoveAttackHandler(),
                new HeavyAttackHandler(),
                new LightAttackHandler()
            };

            // Sort handlers by priority (highest first)
            _handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Process attack input by iterating through handlers in priority order.
        /// The first handler that can execute will handle the attack.
        /// </summary>
        /// <returns>True if any handler executed an attack</returns>
        public bool ProcessAttack(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            foreach (var handler in _handlers)
            {
                if (handler.CanExecute(frame, ref filter, input, config))
                {
                    return handler.Execute(frame, ref filter, input, config);
                }
            }

            return false;
        }

        /// <summary>
        /// Get all registered handlers sorted by priority.
        /// Useful for debugging and testing.
        /// </summary>
        public IReadOnlyList<IAttackHandler> GetHandlers()
        {
            return _handlers.AsReadOnly();
        }

        /// <summary>
        /// Add a custom attack handler dynamically.
        /// The list will be re-sorted by priority after adding.
        /// </summary>
        public void AddHandler(IAttackHandler handler)
        {
            _handlers.Add(handler);
            _handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Remove a handler by type.
        /// </summary>
        public bool RemoveHandler<T>() where T : IAttackHandler
        {
            var handler = _handlers.FirstOrDefault(h => h is T);
            if (handler != null)
            {
                _handlers.Remove(handler);
                return true;
            }
            return false;
        }
    }
}
