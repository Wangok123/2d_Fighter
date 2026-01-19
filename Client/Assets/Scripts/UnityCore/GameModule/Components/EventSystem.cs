using System;
using System.Collections.Generic;
using Wjybxx.Commons.Fx;

namespace UnityCore.GameModule.Components
{
    [ComponentDefine(Kind = ComponentKind.Behavior)]
    public class EventSystem : GComponent
    {
        private readonly Dictionary<int, List<EventHandler>> _handlerDic = new();
    
        public void Register(int type, EventHandler handler)
        {
            if (!_handlerDic.TryGetValue(type, out var handlers))
            {
                handlers = new List<EventHandler>();
                _handlerDic[type] = handlers;
            }
        
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }
    
        public bool Unregister(int type, EventHandler handler)
        {
            if (_handlerDic.TryGetValue(type, out var handlers))
            {
                return handlers.Remove(handler);
            }
            return false;
        }
    
        public void Trigger(int type, object sender, EventArgs e)
        {
            if (_handlerDic.TryGetValue(type, out var handlers))
            {
                // 复制列表以防在遍历时修改
                var handlersCopy = new List<EventHandler>(handlers);
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler(sender, e);
                    }
                    catch (Exception ex)
                    {
                        // 记录异常但继续执行其他处理器
                        Console.WriteLine($"事件处理器异常: {ex}");
                    }
                }
            }
        }
    
        public void Clear(int type)
        {
            if (_handlerDic.TryGetValue(type, out var handlers))
            {
                handlers.Clear();
            }
        }
    
        public override void Reset()
        {
            base.Reset();
            _handlerDic.Clear();
        }
    }
}