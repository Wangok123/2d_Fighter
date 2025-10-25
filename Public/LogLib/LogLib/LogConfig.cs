using System;

namespace LATLog
{
    public enum LoggerType
    {
        Unity,
        Console,
    }

    public enum LogColorType
    {
        Default,
        Blue,
        Cyan,
        Gray,
        Green,
        Magenta,
        Red,
        White,
        Yellow,
    }

    public class LogConfig
    {
        public bool EnableLog = true;
        public string LogPrefix = "##";
        public bool EnableTime = true;
        public string LogSeparator = ">>>>>>>>";
        public bool EnableThreadId = true;
        public bool EnableSaveToFile = true;
        public bool EnableCover = true;
        public string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}Logs\\";
        public string SaveName = "Log.log";
        public LoggerType LoggerType = LoggerType.Console;
    }
}