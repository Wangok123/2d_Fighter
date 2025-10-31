namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Interface for attack handlers using the Strategy Pattern.
    /// Each attack type (special, heavy, light) implements this interface.
    /// </summary>
    public unsafe interface IAttackHandler
    {
        /// <summary>
        /// Priority of this attack handler. Higher values = higher priority.
        /// Special moves should have highest priority, light attacks lowest.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this attack can be executed based on current input and state.
        /// </summary>
        /// <returns>True if the attack can be executed</returns>
        bool CanExecute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config);

        /// <summary>
        /// Execute the attack logic.
        /// </summary>
        /// <returns>True if attack was executed, false otherwise</returns>
        bool Execute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config);
    }
}
