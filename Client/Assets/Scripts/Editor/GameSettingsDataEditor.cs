using UnityEditor;
using UnityEngine;
using Quantum;
using LayerMask = UnityEngine.LayerMask;

namespace QuantumEditor
{
    [CustomEditor(typeof(GameSettingsData))]
    public class GameSettingsDataEditor : UnityEditor.Editor
    {
        private SerializedProperty playerLayerMaskProp;

        private void OnEnable()
        {
            playerLayerMaskProp = serializedObject.FindProperty("PlayerLayerMask");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
            EditorGUILayout.LabelField("游戏设置配置 (Game Settings Configuration)", headerStyle);
            
            EditorGUILayout.Space(10);
            
            // Layer Settings Section
            DrawSectionHeader("物理层设置 (Physics Layer Settings)", new Color(0.8f, 0.4f, 1f));
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(playerLayerMaskProp, 
                new GUIContent("玩家层遮罩 (Player Layer Mask)", 
                "定义哪些物理层会与玩家发生交互"));
            
            // Show which layers are selected
            if (playerLayerMaskProp != null)
            {
                SerializedProperty bitMaskProp = playerLayerMaskProp.FindPropertyRelative("BitMask");
                if (bitMaskProp != null)
                {
                    int layerMask = bitMaskProp.intValue;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("已选择的层 (Selected Layers):", EditorStyles.miniLabel);
                    EditorGUI.indentLevel++;
                    
                    bool hasLayers = false;
                    for (int i = 0; i < 32; i++)
                    {
                        if ((layerMask & (1 << i)) != 0)
                        {
                            string layerName = LayerMask.LayerToName(i);
                            if (!string.IsNullOrEmpty(layerName))
                            {
                                EditorGUILayout.LabelField($"• Layer {i}: {layerName}", EditorStyles.miniLabel);
                                hasLayers = true;
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"• Layer {i}: (未命名)", EditorStyles.miniLabel);
                                hasLayers = true;
                            }
                        }
                    }
                    
                    if (!hasLayers)
                    {
                        EditorGUILayout.LabelField("• (未选择任何层)", EditorStyles.miniLabel);
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Help box
            EditorGUILayout.HelpBox(
                "物理层遮罩用于控制玩家与哪些物理层的对象进行碰撞检测。\n" +
                "Layer Mask controls which physics layers the player can interact with.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSectionHeader(string title, Color color)
        {
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.normal.textColor = color;
            EditorGUILayout.LabelField(title, sectionStyle);
            EditorGUILayout.Space(2);
        }
    }
}
