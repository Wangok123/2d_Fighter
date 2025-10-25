namespace LATTimer
{
    public abstract class Timer
    {
        public Action<string> LogCallback;
        public Action<string> WarningCallback;
        public Action<string> ErrorCallback; 
        
        
        /// <summary>
        /// 增加一个任务
        /// </summary>
        /// <param name="delay">毫秒</param>
        /// <param name="count">重复计数次数</param>
        /// <returns></returns>
        public abstract int AddTask(uint delay, Action<int> endCallback, Action<int> cancelCallback, int count = 1);
        public abstract bool DeleteTask(int taskId);
        public abstract void Reset();
        protected int Tid;
        public abstract int GenerateTid();
    }
}