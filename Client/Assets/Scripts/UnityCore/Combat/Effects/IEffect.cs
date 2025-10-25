namespace Combat.Effects
{
    public interface IEffect<ITarget>
    {
        void Apply(ITarget target);
        void Cancel();
    }
}