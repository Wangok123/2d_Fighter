using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EditorTools
    {
        [MenuItem("Tools/生成配置")]
        public static void GenerateLuban()
        {
            string path = Application.dataPath;
            string parentPath = System.IO.Directory.GetParent(path)?.FullName;
            if (parentPath == null)
            {
                Debug.LogError("路径错误");
                return;
            }
            string pparentPath = System.IO.Directory.GetParent(parentPath)?.FullName;
            if (pparentPath == null)
            {
                Debug.LogError("路径错误");
                return;
            }
            
            BatUtil.RunBat("gen.bat", "", pparentPath + "/Public/");
        }
        
        [MenuItem("Tools/生成服务器配置")]
        public static void GenerateServerLuban()
        {
            string path = Application.dataPath;
            string parentPath = System.IO.Directory.GetParent(path)?.FullName;
            if (parentPath == null)
            {
                Debug.LogError("路径错误");
                return;
            }
            string pparentPath = System.IO.Directory.GetParent(parentPath)?.FullName;
            if (pparentPath == null)
            {
                Debug.LogError("路径错误");
                return;
            }
            
            BatUtil.RunBat("gen_server.bat", "", pparentPath + "/Public/");
        }
    }
}