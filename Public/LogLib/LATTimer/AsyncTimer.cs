using System.Collections.Concurrent;

namespace LATTimer
{
    public class AsyncTimer : Timer
    {
        private const string AsyncLock = "AsyncLock";
        private readonly ConcurrentDictionary<int, AsyncTask> _tasks;
        private readonly ConcurrentQueue<AsyncTaskPack> _taskPackQueue;
        private readonly bool _setHandler;

        public AsyncTimer(bool setHandle)
        {
            _tasks = new ConcurrentDictionary<int, AsyncTask>();
            _setHandler = setHandle;
            if (setHandle)
            {
                _taskPackQueue = new ConcurrentQueue<AsyncTaskPack>();
            }
        }

        public void HandleTask()
        {
            while (_taskPackQueue.Count > 0)
            {
                if (_taskPackQueue.TryDequeue(out AsyncTaskPack taskPack))
                {
                    taskPack.Callback?.Invoke(taskPack.Tid);
                }
                else
                {
                    ErrorCallback?.Invoke("HandleTask failed");
                }
            }
        }


        public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
        {
            int tid = GenerateTid();
            AsyncTask task = new AsyncTask(tid, delay, count, endCallback, cancelCallback);
            RunTaskInPool(task);

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

        private void RunTaskInPool(AsyncTask task)
        {
            Task.Run(async () =>
            {
                if (task.Count > 0)
                {
                    do
                    {
                        task.Count--;
                        task.LoopIndex++;
                        int delay = (int)(task.Delay + task.FixDelta);
                        if (delay > 0)
                        {
                            await Task.Delay(delay, task.Ct);
                        }

                        TimeSpan ts = DateTime.UtcNow - task.StartTime;
                        task.FixDelta = (int)(task.Delay * task.LoopIndex - ts.TotalMilliseconds);
                        CallbackTaskCB(task);
                    } while (task.Count > 0);
                }
                else
                {
                    //
                    while (true)
                    {
                        task.LoopIndex++;
                        int delay = (int)(task.Delay + task.FixDelta);
                        if (delay > 0)
                        {
                            await Task.Delay(delay, task.Ct);
                        }

                        TimeSpan ts = DateTime.UtcNow - task.StartTime;
                        task.FixDelta = (int)(task.Delay * task.LoopIndex - ts.TotalMilliseconds);
                        CallbackTaskCB(task);
                    }
                }
            });
        }

        private void CallbackTaskCB(AsyncTask task)
        {
            if (_setHandler)
            {
                _taskPackQueue.Enqueue(new AsyncTaskPack(task.Tid, task.EndCallback));
            }
            else
            {
                task.EndCallback?.Invoke(task.Tid);
            }

            if (task.Count == 0)
            {
                if (_tasks.TryRemove(task.Tid, out AsyncTask tmp))
                {
                    LogCallback?.Invoke($"{tmp.Tid} is end");
                }
                else
                {
                    ErrorCallback?.Invoke($"FinishTask failed, tid:{task.Tid}");
                }
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_tasks.TryRemove(taskId, out AsyncTask task))
            {
                task.Cts.Cancel();

                if (_setHandler)
                {
                    _taskPackQueue.Enqueue(new AsyncTaskPack(taskId, task.CancelCallback));
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
            Tid = 0;
        }

        public override int GenerateTid()
        {
            lock (AsyncLock)
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

        class AsyncTaskPack
        {
            public int Tid;
            public Action<int> Callback;

            public AsyncTaskPack(int tid, Action<int> callback)
            {
                Tid = tid;
                Callback = callback;
            }
        }

        class AsyncTask
        {
            public int Tid;
            public uint Delay;
            public int Count;
            public DateTime StartTime;
            public ulong LoopIndex;
            public Action<int> EndCallback;
            public Action<int> CancelCallback;
            public int FixDelta;

            public CancellationTokenSource Cts;
            public CancellationToken Ct;


            public AsyncTask(int tid, uint delay, int count, Action<int> endCallback,
                Action<int> cancelCallback)
            {
                Tid = tid;
                Delay = delay;
                Count = count;
                StartTime = DateTime.UtcNow;
                EndCallback = endCallback;
                CancelCallback = cancelCallback;
                LoopIndex = 0UL;
                FixDelta = 0;
            }
        }
    }
}