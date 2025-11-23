using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class OverrideProtodll 
    {
        [MenuItem("Tools/OverrideDllFile")]
        public static void Main()
        {
            string dllFileName = "LatProtocol.dll";
            
            string rootDir = Environment.CurrentDirectory;
            // 去掉最后一个层级
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                rootDir = rootDir.Substring(0, rootDir.LastIndexOf("\\", StringComparison.Ordinal));
            }
            else
            {
                rootDir = rootDir.Substring(0, rootDir.LastIndexOf("/", StringComparison.Ordinal));
            }
            
            string source = Path.Combine(rootDir, "Server", "LatServer", "LatProtocol", "bin", "Release", "netstandard2.1", dllFileName);
            if (!File.Exists(source))
            {
                Debug.LogError($"源DLL文件不存在: {source}");
                return;
            }

            string assetPath = Application.dataPath;
            string targetPath = Path.Combine(assetPath, "Plugins", "Tools", "KCPNet",dllFileName);

            try
            {
                // 确保目标文件已被覆盖
                File.Copy(source, targetPath, overwrite: true);
                EditorUtility.DisplayDialog("覆盖成功", $"已成功覆盖 {dllFileName} 文件, 等待编译", "确定");
            }
            catch (Exception ex)
            {
                Debug.LogError($"覆盖DLL文件失败: {ex.Message}");
            }
            
            AssetDatabase.Refresh();
        }
    }
}