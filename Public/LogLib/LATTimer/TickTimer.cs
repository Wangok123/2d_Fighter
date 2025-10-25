using System.Collections.Concurrent;

namespace LATTimer
{
    /// <summary>
    /// 高频高精度毫秒定时器
    /// </summary>
    public class TickTimer : Timer
    {
        private readonly DateTime _startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private readonly ConcurrentDictionary<int, TickTask> _tasks;
        private readonly ConcurrentQueue<TickTaskPack> _taskPackQueue;
        private readonly bool _setHandler;
        private const string TIDLOCK = "TIDLOCK";

        private readonly Thread _timerThread;

        /// <param name="interval">为0用默认线程</param>
        /// <param name="setHandler">表示是否使用线程池</param>
        public TickTimer(int interval = 0, bool setHandler = true)
        {
            TickTimer tickTimer = this;
            _tasks = new ConcurrentDictionary<int, TickTask>();
            _setHandler = setHandler;
            if (setHandler)
            {
                _taskPackQueue = new ConcurrentQueue<TickTaskPack>();
            }

            if (interval != 0)
            {
                _timerThread = new Thread(new ThreadStart(StartTick));
                _timerThread.Start();
            }

            void StartTick()
            {
                try
                {
                    while (true)
                    {
                        tickTimer.UpdateTasks();
                        Thread.Sleep(interval);
                    }
                }
                catch (ThreadAbortException e)
                {
                    Action<string> warnFunc = tickTimer.ErrorCallback;
                    warnFunc(string.Format($"Tick Thread Abort:{(object)e}."));
                }
            }
        }

        public void UpdateTasks()
        {
            double currentTime = GetUtcMilliseconds();
            foreach (var pair in _tasks)
            {
                TickTask tickTask = pair.Value;
                if (currentTime >= tickTask.DestTime)
                {
                    tickTask.LoopIndex++;
                    if (tickTask.Count > 0)
                    {
                        tickTask.Count--;
                        if (tickTask.Count == 0)
                        {
                            FinishTask(tickTask.Tid);
                        }
                        else
                        {
                            tickTask.DestTime = tickTask.StartTime + tickTask.Delay * (tickTask.LoopIndex + 1);
                            CallTaskCallback(tickTask.Tid, tickTask.EndCallback);
                        }
                    }
                    else
                    {
                        tickTask.DestTime = tickTask.StartTime + tickTask.Delay * (tickTask.LoopIndex + 1);
                        CallTaskCallback(tickTask.Tid, tickTask.EndCallback);
                    }
                }
            }
        }

        public void HandleTask()
        {
            while (_taskPackQueue.Count > 0)
            {
                if (_taskPackQueue.TryDequeue(out TickTaskPack taskPack))
                {
                    taskPack.Callback?.Invoke(taskPack.Tid);
                }
                else
                {
                    ErrorCallback?.Invoke("HandleTask failed");
                }
            }
        }

        void FinishTask(int tid)
        {
            if (_tasks.TryRemove(tid, out TickTask task))
            {
                CallTaskCallback(tid, task.EndCallback);
                task.EndCallback?.Invoke(tid);
            }
            else
            {
                ErrorCallback?.Invoke($"FinishTask failed, tid:{tid}");
            }
        }

        void CallTaskCallback(int tid, Action<int> callback)
        {
            if (_setHandler)
            {
                _taskPackQueue.Enqueue(new TickTaskPack(tid, callback));
            }
            else
            {
                callback?.Invoke(tid);
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="delay">持续时间</param>
        /// <param name="count">重复次数</param>
        /// <returns></returns>
        public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
        {
            int tid = GenerateTid();
            double startTime = GetUtcMilliseconds();
            double destTime = startTime + delay;
            TickTask task = new TickTask(tid, delay, count, startTime, destTime,
                endCallback, cancelCallback);
            if (_tasks.TryAdd(tid, task))
            {
                return tid;
            }
            else
            {
                ErrorCallback?.Invoke($"AddTask failed, tid:{tid}");
                return -1;
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_tasks.TryRemove(taskId, out TickTask task))
            {
                if (_setHandler)
                {
                    _taskPackQueue.Enqueue(new TickTaskPack(taskId, task.CancelCallback));
                }
                else
                {
                    task.CancelCallback?.Invoke(taskId);
                }

                return true;
            }
            else
            {
                ErrorCallback?.Invoke($"DeleteTask failed, tid:{taskId}");
                return false;
            }
        }

        public override void Reset()
        {
            if (_taskPackQueue.IsEmpty)
            {
                WarningCallback?.Invoke("Reset failed, taskPackQueue is not empty");
            }

            _tasks.Clear();
            _timerThread.Abort();
        }

        private double GetUtcMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - _startTime;
            return ts.TotalMilliseconds;
        }

        public override int GenerateTid()
        {
            lock (TIDLOCK)
            {
                while (true)
                {
                    Tid++;
                    if (Tid == int.MaxValue)
                    {
                        Tid = 0;
                    }

                    if (!_tasks.ContainsKey(Tid))
                    {
                        return Tid;
                    }
                }
            }
        }

        class TickTask
        {
            public int Tid;
            public uint Delay;
            public int Count;
            public double StartTime;
            public double DestTime;
            public ulong LoopIndex;
            public Action<int> EndCallback;
            public Action<int> CancelCallback;

            public TickTask(int tid, uint delay, int count, double startTime, double destTime, Action<int> endCallback,
                Action<int> cancelCallback)
            {
                Tid = tid;
                Delay = delay;
                Count = count;
                StartTime = startTime;
                DestTime = destTime;
                EndCallback = endCallback;
                CancelCallback = cancelCallback;
                LoopIndex = 0UL;
            }
        }

        class TickTaskPack
        {
            public int Tid;
            public Action<int> Callback;

            public TickTaskPack(int tid, Action<int> callback)
            {
                Tid = tid;
                Callback = callback;
            }
        }
    }
}