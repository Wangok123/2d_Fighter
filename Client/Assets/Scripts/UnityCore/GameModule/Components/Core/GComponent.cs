using System;
using Wjybxx.Commons.Fx;

namespace UnityCore.GameModule.Components
{
    public abstract class GComponent
    {
        /// <summary>
        /// 组件id池
        /// </summary>
        public static readonly ComponentIdPool ID_POOL = ComponentIdPool.NewPool();

        [NonSerialized] private GameUnit _gameUnit;

        [NonSerialized] private ComponentId _cid;

        [NonSerialized] private ComponentStatus _status = ComponentStatus.New;

        private bool _enabled = true;

        [NonSerialized] private GComponent _next; // 同组件ID的下一个组件

        public ComponentId Cid
        {
            get => _cid ?? ID_POOL.ValueOf(GetType());
            set
            {
                if (_status != ComponentStatus.New)
                    throw new InvalidOperationException("只能在New状态下修改ComponentId");
                _cid = value;
            }
        }

        public GameUnit GameUnit => _gameUnit;

        public ComponentStatus Status => _status;

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public GComponent Next => _next;

        internal void SetNext(GComponent next) => _next = next;

        /// <summary>
        /// 绑定到GameUnit时调用
        /// </summary>
        internal void SetEntity(GameUnit gameUnit)
        {
            if (_status != ComponentStatus.New)
                throw new InvalidOperationException($"组件状态错误: {_status}");

            _gameUnit = gameUnit;
            _status = ComponentStatus.Ready;

            try
            {
                OnAwake();
            }
            catch (Exception e)
            {
                // 这里应该记录日志
                Console.WriteLine($"组件OnAwake异常: {e}");
            }
        }

        /// <summary>
        /// 销毁组件
        /// </summary>
        internal void InvokeDestroy()
        {
            if (_status == ComponentStatus.Destroyed)
                return;

            _status = ComponentStatus.Stopping;

            try
            {
                OnDestroy();
            }
            catch (Exception e)
            {
                // 这里应该记录日志
                Console.WriteLine($"组件OnDestroy异常: {e}");
            }

            _status = ComponentStatus.Destroyed;
            _gameUnit = null;
            _next = null;
        }

        /// <summary>
        /// 对象重用时调用
        /// </summary>
        public virtual void Reset()
        {
            _enabled = true;
            _next = null;
            // 注意：这里不重置_gameUnit，因为重用时不一定会绑定到新的GameUnit
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnDestroy()
        {
        }
    }
}