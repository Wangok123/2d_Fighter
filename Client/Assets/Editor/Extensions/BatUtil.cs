using System.Diagnostics;
using Core;
using LATLog;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    public static class BatUtil
    {
        
        
        public static Process CreateShellExProcess(string cmd, string args, string workingDir = "")
        {
            var realWorkingDir = FormatPath(workingDir);
            
            var pStartInfo = new ProcessStartInfo(cmd);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = false;
            pStartInfo.UseShellExecute = true;
            pStartInfo.RedirectStandardError = false;
            pStartInfo.RedirectStandardInput = false;
            pStartInfo.RedirectStandardOutput = false;
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = realWorkingDir;
            var process = Process.Start(pStartInfo);
            if (process == null)
            {
                GameDebug.LogError($"Failed to start process: {cmd} {args}");
            }
            
            return process;
        }
        
        public static void RunBat(string batFile, string args, string workingDir = "")
        {
            var p = CreateShellExProcess(batFile, args, workingDir);
            p.WaitForExit();
            p.Close();
            //刷新Unity资源
            AssetDatabase.Refresh();
        }
        
        public static string FormatPath(string path)
        {
            path = path.Replace("/", "\\");
            if (Application.platform == RuntimePlatform.OSXEditor)
                path = path.Replace("\\", "/");
            
            return path;
        }
    }
}