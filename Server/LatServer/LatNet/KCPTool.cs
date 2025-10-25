using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Google.Protobuf;

namespace LATNet
{
    public class KCPTool
    {
        public static Action<string> LogFunc;
        public static Action<KCPLogColor, string> ColorLogFunc;
        public static Action<string> WarnFunc;
        public static Action<string> ErrorFunc;

        public static void Log(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (LogFunc != null)
            {
                LogFunc(msg);
            }
            else
            {
                ConsoleLog(msg, KCPLogColor.None);
            }
        }

        public static void ColorLog(KCPLogColor color, string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ColorLogFunc != null)
            {
                ColorLogFunc(color, msg);
            }
            else
            {
                ConsoleLog(msg, color);
            }
        }

        public static void Warn(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (WarnFunc != null)
            {
                WarnFunc(msg);
            }
            else
            {
                ConsoleLog(msg, KCPLogColor.Yellow);
            }
        }

        public static void Error(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ErrorFunc != null)
            {
                ErrorFunc(msg);
            }
            else
            {
                ConsoleLog(msg, KCPLogColor.Red);
            }
        }

        private static void ConsoleLog(string msg, KCPLogColor color)
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            msg = string.Format("Thread:{0} {1}", threadID, msg);

            switch (color)
            {
                case KCPLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Magentna:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.None:
                default:
                    break;
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(input, 0, input.Length);
                    zip.Close();
                }

                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream(input))
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream outBuffer = new MemoryStream())
                    {
                        byte[] block = new byte[1024];
                        int bytesRead = 0;
                        while ((bytesRead = zip.Read(block, 0, block.Length)) > 0)
                        {
                            outBuffer.Write(block, 0, bytesRead);
                        }

                        return outBuffer.ToArray();
                    }
                }
            }
        }

        private static readonly DateTime UtcStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static ulong GetUTCStartMilliseconds()
        {
            return (ulong)(DateTime.UtcNow - UtcStart).TotalMilliseconds;
        }
    }

    public enum KCPLogColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magentna,
        Yellow
    }
}