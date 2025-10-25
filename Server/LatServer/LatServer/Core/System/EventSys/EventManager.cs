namespace LatServer.Core.System.EventSys;

public static class EventManager
{
    private static class EventPool<T> where T : IEvent, new()
    {
        private static readonly Queue<T> _pool = new Queue<T>(32);

        public static T Get()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : new T();
        }

        public static void Release(T evt)
        {
            _pool.Enqueue(evt);
        }
    }

    // 事件类型与处理器的映射
    private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

    // 注册监听
    public static void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (Handlers.TryGetValue(type, out var existing))
        {
            Handlers[type] = Delegate.Combine(existing, handler);
        }
        else
        {
            Handlers[type] = handler;
        }
    }

    // 取消监听
    public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (Handlers.TryGetValue(type, out var existing))
        {
            var newDelegate = Delegate.Remove(existing, handler);
            if (newDelegate == null)
            {
                Handlers.Remove(type);
            }
            else
            {
                Handlers[type] = newDelegate;
            }
        }
    }

    // 触发事件（带对象池）
    public static void Publish<T>(T evt) where T : IEvent, new()
    {
        var type = typeof(T);
        if (Handlers.TryGetValue(type, out var handler))
        {
            ((Action<T>)handler)?.Invoke(evt);
        }
        EventPool<T>.Release(evt);
    }

    // 快速触发（自动创建事件对象）
    public static void Publish<T>() where T : IEvent, new()
    {
        var evt = EventPool<T>.Get();
        Publish(evt);
    }
}