using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Editor
{
    public class Proto2CSEditor
    {
        [MenuItem("Tools/Proto2CS_Server")]
        public static void AllProto2CS_Server()
        {
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

            string protoDir = Path.Combine(rootDir, "Protocal");

            string protoc;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                protoc = Path.Combine(protoDir, "protoc.exe");
            }
            else
            {
                protoc = Path.Combine(protoDir, "protoc");
            }

            string hotfixMessageCodePath = Path.Combine(rootDir, "Server", "LatServer", "LatProtocol", "Protocol");

            // 如果不存在这个目录，就创建，存在就删除其中内容
            if (!Directory.Exists(hotfixMessageCodePath))
            {
                Directory.CreateDirectory(hotfixMessageCodePath);
            }
            else
            {
                string[] files = Directory.GetFiles(hotfixMessageCodePath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }

            string protocolDir;
            // 筛选出所有的proto文件
            protocolDir = Path.Combine(protoDir, "Proto");
            string[] protoFiles = Directory.GetFiles(protocolDir, "*.proto", SearchOption.AllDirectories);
            foreach (string protoFile in protoFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(protoFile);
                string argument =
                    $"--csharp_out=\"{hotfixMessageCodePath}\" --proto_path=\"{protocolDir}\" {fileName}.proto";
                Run(protoc, argument, waitExit: true);
            }

            StringBuilder protoContent = new StringBuilder();

            foreach (var protoFile in protoFiles)
            {
                string protoPath = Path.Combine(protocolDir, protoFile);
                if (!File.Exists(protoFile))
                {
                    Console.WriteLine("Proto file not found!");
                    return;
                }

                protoContent.AppendLine(File.ReadAllText(protoPath));
            }

            var messages = ExtractTargetMessages(protoContent.ToString());

            string enumCode = GenerateEnum(messages);
            string finalOutput = enumCode;
            string enumOutputPath = Path.Combine(rootDir, "Server", "LatServer", "LatProtocol", "ProtocolID.cs");
            // 如果finalOutput有内容，先删除旧的文件
            if (File.Exists(enumOutputPath) && finalOutput.Length > 0)
            {
                File.Delete(enumOutputPath);
            }

            //先写入命名空间
            finalOutput = "namespace LatProtocol;\n\n" + finalOutput;
            File.WriteAllText(enumOutputPath, finalOutput); // 输出到一个C#文件

            string mappingCode = GenerateMapping(messages);
            string finalMappingOutput = mappingCode;
            string mappingOutputPath = Path.Combine(rootDir, "Server", "LatServer", "LatProtocol", "ProtocolMapping.cs");
            // 如果finalMappingOutput有内容，先删除旧的文件
            if (File.Exists(mappingOutputPath) && finalMappingOutput.Length > 0)
            {
                File.Delete(mappingOutputPath);
            }
            
            //先写入命名空间
            finalMappingOutput = "using GameProtocol;\n\nnamespace LatProtocol;\n\n" + finalMappingOutput;
            File.WriteAllText(mappingOutputPath, finalMappingOutput); // 输出到一个C#文件
            
            UnityEngine.Debug.Log("proto2cs succeed!");

            AssetDatabase.Refresh();
        }

        public static Process Run(string exe, string arguments, string workingDirectory = ".", bool waitExit = false)
        {
            try
            {
                bool redirectStandardOutput = true;
                bool redirectStandardError = true;
                bool useShellExecute = false;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    redirectStandardOutput = false;
                    redirectStandardError = false;
                    useShellExecute = true;
                }

                if (waitExit)
                {
                    redirectStandardOutput = true;
                    redirectStandardError = true;
                    useShellExecute = false;
                }

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = redirectStandardOutput,
                    RedirectStandardError = redirectStandardError,
                };

                Process process = Process.Start(info);

                if (waitExit)
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception(
                            $"{process.StandardOutput.ReadToEnd()} {process.StandardError.ReadToEnd()}");
                    }
                }

                return process;
            }
            catch (Exception e)
            {
                throw new Exception($"dir: {Path.GetFullPath(workingDirectory)}, command: {exe} {arguments}", e);
            }
        }

        // 提取符合条件的 message 名称
        private static List<string> ExtractTargetMessages(string protoContent)
        {
            var result = new List<string>();
            var regex = new Regex(@"message\s+(\w+)\s*{", RegexOptions.Compiled);

            foreach (Match match in regex.Matches(protoContent))
            {
                string name = match.Groups[1].Value;
                if (name.EndsWith("Request") || name.EndsWith("Response") || name.EndsWith("Notification"))
                {
                    result.Add(name);
                }
            }

            return result;
        }

        // 生成 enum
        private static string GenerateEnum(List<string> messages)
        {
            var sb = new StringBuilder();
            sb.AppendLine("public enum ProtocolID");
            sb.AppendLine("{");

            for (int i = 0; i < messages.Count; i++)
            {
                sb.AppendLine($"    {messages[i]} = {i + 1},");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
        
        private static string GenerateMapping(List<string> messages)
        {
            var sb = new StringBuilder();
            sb.AppendLine("public static class ProtocolMapping");
            sb.AppendLine("{");
            sb.AppendLine("    public static Dictionary<ushort, Type> ProtocolMap = new Dictionary<ushort, Type>");
            sb.AppendLine("    {");
            for (int i = 0; i < messages.Count; i++)
            {
                
                // 以{ (ushort)ProtocolID.LoginRequest, typeof(LoginRequest) },格式
                sb.AppendLine($"        {{ (ushort)ProtocolID.{messages[i]}, typeof({messages[i]}) }},");
            }

            sb.AppendLine("    };");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}