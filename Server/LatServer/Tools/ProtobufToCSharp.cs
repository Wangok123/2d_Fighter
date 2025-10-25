using System.Text;
using System.Text.RegularExpressions;

namespace Tools;

public class ProtobufToCSharp
{
    // 提取符合条件的 message 名称
    public static List<string> ExtractTargetMessages(string protoContent)
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
    public static string GenerateEnum(List<string> messages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("public enum ProtocolType");
        sb.AppendLine("{");

        for (int i = 0; i < messages.Count; i++)
        {
            sb.AppendLine($"    {messages[i]} = {i + 1},");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // 生成类模板
    public static string GenerateClassStubs(List<string> messages)
    {
        var sb = new StringBuilder();
        foreach (var name in messages)
        {
            sb.AppendLine($"public class {name}");
            sb.AppendLine("{");
            sb.AppendLine("    // TODO: define fields based on proto");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}