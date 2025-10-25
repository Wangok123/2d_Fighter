
namespace LATLog
{
    public interface ILogger
    {
        void Log(string msg, LogColorType color = LogColorType.Default);
        void LogWarning(string msg);
        void LogError(string msg);
    }
}
