namespace UnityCore.Entities.Core
{
    public interface ISystem
    {
        void Update();
    }

    public interface IFixedUpdateSystem
    {
        void FixedUpdate();
    }

    public interface IInitializableSystem
    {
        void Initialize();
    }

    public interface ICleanupSystem
    {
        void Cleanup();
    }
}