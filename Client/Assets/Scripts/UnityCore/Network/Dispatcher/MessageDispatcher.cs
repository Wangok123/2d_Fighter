using System;
using System.Collections.Generic;
using LATLog;

namespace UnityCore.Network.Dispatcher
{
    public class MessageDispatcher: IMessageDispatcher
    {
        private readonly Dictionary<uint, IMessageHandler> _handlers = new Dictionary<uint, IMessageHandler>();

        public void RegisterHandler(IMessageHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
        
            if (_handlers.ContainsKey(handler.CommandId))
            {
                GameDebug.LogWarning($"重复注册消息处理器: {handler.CommandId}");
                return;
            }

            _handlers[handler.CommandId] = handler;
            GameDebug.Log($"注册消息处理器: {handler.CommandId}");
        }

        public void Dispatch(MsgPack message)
        {
            if (message == null)
            {
                GameDebug.LogWarning("收到空消息");
                return;
            }

            try
            {
                if (_handlers.TryGetValue(message.CMDID, out var handler))
                {
                    handler.Handle(message);
                }
                else
                {
                    GameDebug.LogWarning($"未找到消息处理器: {message.CMDID}");
                    HandleUnknownMessage(message);
                }
            }
            catch (Exception ex)
            {
                GameDebug.LogError($"处理消息失败: {message.CMDID} {ex.Message} {ex.StackTrace}");
                HandleError(message, ex);
            }
        }

        protected virtual void HandleUnknownMessage(MsgPack message)
        {
            // 可重写默认行为
        }

        protected virtual void HandleError(MsgPack message, Exception ex)
        {
            // 可重写错误处理
        }
    }
}