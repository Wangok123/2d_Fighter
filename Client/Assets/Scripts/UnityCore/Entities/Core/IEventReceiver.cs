namespace UnityCore.Entities.Core
{
    public interface IEventReceiver<T> where T : struct
    {
        void HandleEvent(T eventData);
    }
}