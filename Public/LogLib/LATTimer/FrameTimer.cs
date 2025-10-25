namespace LATTimer
{
    public class FrameTimer : Timer
    {
        private ulong _currentFrame;
        private const string FrameLock = "FrameLock";

        private readonly Dictionary<int, FrameTask> _tasks;
        private List<int> _tidList;

        public FrameTimer(ulong frameId = 0)
        {
            _currentFrame = frameId;
            _tasks = new Dictionary<int, FrameTask>();
            _tidList = new List<int>();
        }

        public void UpdateTasks()
        {
            _currentFrame++;
            _tidList.Clear();

            foreach (var pair in _tasks)
            {
                FrameTask task = pair.Value;
                if (task.DestFrame <= _currentFrame)
                {
                    task.EndCallback?.Invoke(task.Tid);
                    task.DestFrame += task.Delay;
                    task.Count--;
                    if (task.Count == 0)
                    {
                        _tidList.Add(task.Tid);
                    }
                }
            }

            for (int i = 0; i < _tidList.Count; i++)
            {
                if (_tasks.Remove(_tidList[i]))
                {
                    LogCallback?.Invoke($"tid: {_tidList[i]} has completed");
                }
                else
                {
                    ErrorCallback?.Invoke($"");
                }
            }
        }

        public override int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1)
        {
            int tid = GenerateTid();
            ulong destFrame = _currentFrame + delay;
            FrameTask task = new FrameTask(tid, delay, count, destFrame, endCallback, cancelCallback);
            if (_tasks.ContainsKey(tid))
            {
                ErrorCallback?.Invoke($"key: {tid} has already exist");
                return -1;
            }
            else
            {
                _tasks.Add(tid, task);
                return tid;
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out FrameTask task))
            {
                if (_tasks.Remove(taskId))
                {
                    task.CancelCallback?.Invoke(taskId);
                    return true;
                }
                else
                {
                    ErrorCallback?.Invoke($"Remove taskID {taskId} Error!!!!!");
                    return false;
                }
            }
            else
            {
                ErrorCallback?.Invoke($"TaskID {taskId} is not exist");
                return false;
            }
        }

        public override void Reset()
        {
            _tasks.Clear();
            _tidList.Clear();
            _currentFrame = 0;
        }

        public override int GenerateTid()
        {
            lock (FrameLock)
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

        class FrameTask
        {
            public int Tid;
            public uint Delay;
            public int Count;
            public ulong DestFrame;
            public Action<int> EndCallback;
            public Action<int> CancelCallback;

            public FrameTask(int tid, uint delay, int count, ulong destFrame, Action<int> endCallback,
                Action<int> cancelCallback)
            {
                Tid = tid;
                Delay = delay;
                Count = count;
                DestFrame = destFrame;
                EndCallback = endCallback;
                CancelCallback = cancelCallback;
            }
        }
    }
}