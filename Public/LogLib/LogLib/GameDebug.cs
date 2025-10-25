using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace LATLog
{
    public static class ExtensionMethods
    {
        public static void Log(this object obj)
        {
            GameDebug.Log(obj);
        }
    
        public static void Log(this string msg, params object[] args)
        {
            GameDebug.Log(msg, args);
        }
    
        public static void LogColor(this object obj, LogColorType color)
        {
            GameDebug.LogColor(color, obj);
        }
    
        public static void LogColor(this string msg, LogColorType color, params object[] args)
        {
            GameDebug.LogColor(color, msg, args);
        }
    
        public static void LogWarning(this string msg, params object[] args)
        {
            GameDebug.LogWarning(msg, args);
        }
    
        public static void LogError(this object obj)
        {
            GameDebug.LogError(obj);
        }
    
        public static void LogError(this string msg, params object[] args)
        {
            GameDebug.LogError(msg, args);
        }
    
        public static void LogWarning(this object obj)
        {
            GameDebug.LogWarning(obj);
        }
    
        public static void Trace(this string msg, params object[] args)
        {
            GameDebug.Trace(msg, args);
        }
    
        public static void Trace(this object obj)
        {
            GameDebug.Trace(obj);
        }
    }

    public static class GameDebug
    {
        private static LogConfig Cfg { get; set; }
        private static ILogger _logger;
    
        private static StreamWriter? _logWriter = null;

        public static void InitSettings(LogConfig? cfg = null)
        {
            Cfg = cfg ?? new LogConfig();

            if (Cfg.LoggerType == LoggerType.Unity)
            {
                _logger = new UnityLogger();
            }
            else
            {
                _logger = new ConsoleLogger();
            }

            if (!Cfg.EnableSaveToFile)
            {
                return;
            }

            if (Cfg.EnableCover)
            {
                string logPath = Cfg.SavePath + Cfg.SaveName;
                try
                {
                    if (Directory.Exists(Cfg.SavePath))
                    {
                        if (File.Exists(logPath))
                        {
                            File.Delete(logPath);
                        }
                    }else
                    {
                        Directory.CreateDirectory(Cfg.SavePath);
                    }
                
                    _logWriter = File.AppendText(logPath);
                    _logWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logWriter = null;
                    throw;
                }
            }
            else
            {
                string prefix = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string logPath = Cfg.SavePath + prefix + "_" + Cfg.SaveName;
                try
                {
                    if (!Directory.Exists(Cfg.SavePath))
                    {
                        Directory.CreateDirectory(Cfg.SavePath);
                    }
                
                    _logWriter = File.AppendText(logPath);
                    _logWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logWriter = null;
                    throw;
                }
            }
        }
    
        public static void Log(string msg, params object[] args)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            msg = DecorateLog(string.Format(msg, args));
            _logger.Log(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[L]{msg}");
            }
        }
    
        public static void Log(object obj)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            string msg = DecorateLog(obj.ToString());
            _logger.Log(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[L]{msg}");
            }
        }
    
        public static void LogColor(LogColorType color, string msg, params object[] args)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            msg = DecorateLog(string.Format(msg, args));
            _logger.Log(msg, color);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[L]{msg}");
            }
        }
    
        public static void LogColor(LogColorType color, object obj)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            string msg = DecorateLog(obj.ToString());
            _logger.Log(msg, color);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[L]{msg}");
            }
        }
    

        public static void LogWarning(string msg, params object[] args)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            msg = DecorateLog(string.Format(msg, args));
            _logger.LogWarning(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[W]{msg}");
            }
        }
    
        public static void LogWarning(object obj)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            string msg = DecorateLog(obj.ToString());
            _logger.LogWarning(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[W]{msg}");
            }
        }
    
        public static void LogError(string msg, params object[] args)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            msg = DecorateLog(string.Format(msg, args));
            _logger.LogError(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[E]{msg}");
            }
        }
    
        public static void LogError(object obj)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            string msg = DecorateLog(obj.ToString());
            _logger.LogError(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[E]{msg}");
            }
        }
    
        public static void Trace(string msg, params object[] args)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            msg = DecorateLog(string.Format(msg, args), true);
            _logger.Log(msg);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[T]{msg}");
            }
        }

        public static void Trace(object obj)
        {
            if (!Cfg.EnableLog)
            {
                return;
            }

            string msg = DecorateLog(obj.ToString(), true);
            _logger.Log(msg, LogColorType.Magenta);
        
            if (Cfg.EnableSaveToFile)
            {
                WriteToFile($"[T]{msg}");
            }
        }

        private static string DecorateLog(string msg, bool isTrace = false)
        {
            StringBuilder sb = new StringBuilder(Cfg.LogPrefix, 100);
            if (Cfg.EnableTime)
            {
                sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");
            }
        
            if (Cfg.EnableThreadId)
            {
                sb.Append($"[Thread:{Thread.CurrentThread.ManagedThreadId}]");
            }
        
            sb.Append(Cfg.LogSeparator);
            sb.Append(msg);

            if (isTrace)
            {
                sb.Append(Environment.NewLine);
                sb.Append("Trace Info:");
                var traceInfo = GetLogTrace();
                sb.Append(traceInfo);
            }
        
            return sb.ToString();
        }

        private static string GetLogTrace()
        {
            StackTrace st = new StackTrace(3, true);
            string traceInfo = string.Empty;
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame? sf = st.GetFrame(i);
                if (sf == null)
                {
                    continue;
                }
            
                traceInfo += $"\n {sf.GetFileName()}::{sf.GetMethod()} line:{sf.GetFileLineNumber()}";
            }
        
            return traceInfo;
        }
    
        private static void WriteToFile(string msg)
        {
            if (_logWriter != null)
            {
                try
                {
                    _logWriter.WriteLine(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logWriter = null;
                    throw;
                }
            }
        }

        private class UnityLogger : ILogger
        {
            readonly Type? _type = Type.GetType("UnityEngine.Debug, UnityEngine.CoreModule");
            private Type LogType => _type ?? throw new Exception("Unity Engine is not found.");

            public void Log(string msg, LogColorType color = LogColorType.Default)
            {
                if (color != LogColorType.Default)
                {
                    msg = ColorUnityLog(msg, color);
                }

                var methodInfo = LogType.GetMethod("Log", new Type[] { typeof(object) });
                if (methodInfo == null)
                {
                    throw new Exception("Unity Debug.Log method is not found.");
                }
            
                methodInfo.Invoke(null, new object[] { msg });
            }

            public void LogWarning(string msg)
            {
                msg = ColorUnityLog(msg, LogColorType.Yellow);
            
                var methodInfo = LogType.GetMethod("LogWarning", new Type[] { typeof(object) });
                if (methodInfo == null)
                {
                    throw new Exception("Unity Debug.LogWarning method is not found.");
                }
            
                methodInfo.Invoke(null, new object[] { msg });
            }

            public void LogError(string msg)
            {
                msg = ColorUnityLog(msg, LogColorType.Red);
            
                var methodInfo = LogType.GetMethod("LogError", new Type[] { typeof(object) });
                if (methodInfo == null)
                {
                    throw new Exception("Unity Debug.LogError method is not found.");
                }
            
                methodInfo.Invoke(null, new object[] { msg });
            }
        
            private string ColorUnityLog(string msg, LogColorType color)
            {
                return color switch
                {
                    LogColorType.Blue => $"<color=blue>{msg}</color>",
                    LogColorType.Cyan => $"<color=cyan>{msg}</color>",
                    LogColorType.Gray => $"<color=gray>{msg}</color>",
                    LogColorType.Green => $"<color=green>{msg}</color>",
                    LogColorType.Magenta => $"<color=magenta>{msg}</color>",
                    LogColorType.Red => $"<color=red>{msg}</color>",
                    LogColorType.White => $"<color=white>{msg}</color>",
                    LogColorType.Yellow => $"<color=yellow>{msg}</color>",
                    _ => msg,
                };
            }
        }

        private class ConsoleLogger : ILogger
        {
            public void Log(string msg, LogColorType color = LogColorType.Default)
            {
                WriteConsoleLog(msg, color);
            }

            public void LogWarning(string msg)
            {
                WriteConsoleLog(msg, LogColorType.Yellow);
            }

            public void LogError(string msg)
            {
                WriteConsoleLog(msg, LogColorType.Red);
            }

            private void WriteConsoleLog(string msg, LogColorType color)
            {
                Console.ForegroundColor = color switch
                {
                    LogColorType.Blue => ConsoleColor.Blue,
                    LogColorType.Cyan => ConsoleColor.Cyan,
                    LogColorType.Gray => ConsoleColor.Gray,
                    LogColorType.Green => ConsoleColor.Green,
                    LogColorType.Magenta => ConsoleColor.Magenta,
                    LogColorType.Red => ConsoleColor.DarkRed,
                    LogColorType.White => ConsoleColor.White,
                    LogColorType.Yellow => ConsoleColor.Yellow,
                    _ => ConsoleColor.White,
                };

                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }
    }
}