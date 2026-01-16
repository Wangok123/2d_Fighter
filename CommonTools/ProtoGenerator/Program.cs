using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProtoGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ProtoGenerator <proto_directory> <output_directory>");
                return 1;
            }

            string protoDir = args[0];
            string outputDir = args[1];

            if (!Directory.Exists(protoDir))
            {
                Console.WriteLine($"Error: Proto directory not found: {protoDir}");
                return 1;
            }

            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine($"Error: Output directory not found: {outputDir}");
                return 1;
            }

            try
            {
                var protoFiles = Directory.GetFiles(protoDir, "*.proto", SearchOption.AllDirectories);
                var protoContent = new StringBuilder();

                foreach (var protoFile in protoFiles)
                {
                    if (!File.Exists(protoFile))
                    {
                        Console.WriteLine($"Warning: Proto file not found: {protoFile}");
                        continue;
                    }

                    protoContent.AppendLine(File.ReadAllText(protoFile));
                }

                var messages = ExtractTargetMessages(protoContent.ToString());

                Console.WriteLine($"Found {messages.Count} protocol messages");

                string enumCode = GenerateEnum(messages);
                string enumOutput = "namespace LatProtocol;\n\n" + enumCode;
                string enumOutputPath = Path.Combine(outputDir, "ProtocolID.cs");

                if (File.Exists(enumOutputPath))
                {
                    File.Delete(enumOutputPath);
                }
                File.WriteAllText(enumOutputPath, enumOutput);
                Console.WriteLine($"Generated: {enumOutputPath}");

                string mappingCode = GenerateMapping(messages);
                string mappingOutput = "using GameProtocol;\n\nnamespace LatProtocol;\n\n" + mappingCode;
                string mappingOutputPath = Path.Combine(outputDir, "ProtocolMapping.cs");

                if (File.Exists(mappingOutputPath))
                {
                    File.Delete(mappingOutputPath);
                }
                File.WriteAllText(mappingOutputPath, mappingOutput);
                Console.WriteLine($"Generated: {mappingOutputPath}");

                Console.WriteLine("Protocol generation completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

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
                sb.AppendLine($"        {{ (ushort)ProtocolID.{messages[i]}, typeof({messages[i]}) }},");
            }

            sb.AppendLine("    };");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
