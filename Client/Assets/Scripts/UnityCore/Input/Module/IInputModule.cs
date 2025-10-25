namespace UnityCore.Input.Module
{
    public interface IInputModule
    {
        void Initialize();
        void Cleanup();
        void Enable();
        void Disable();
    }
}