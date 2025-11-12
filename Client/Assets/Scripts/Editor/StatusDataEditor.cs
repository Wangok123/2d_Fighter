using UnityEditor;
using UnityEngine;
using Quantum;
using Photon.Deterministic;

namespace QuantumEditor
{
    [CustomEditor(typeof(StatusData))]
    public class StatusDataEditor : UnityEditor.Editor
    {
        private SerializedProperty maxHealthProp;
        private SerializedProperty respawnTimeProp;
        private SerializedProperty timeUntilRegenProp;
        private SerializedProperty regenRateProp;
        private SerializedProperty invincibleTimeProp;
        private SerializedProperty timeToDisconnectProp;
        private SerializedProperty minimumDamageProp;

        private void OnEnable()
        {
            maxHealthProp = serializedObject.FindProperty("MaxHealth");
            respawnTimeProp = serializedObject.FindProperty("RespawnTime");
            timeUntilRegenProp = serializedObject.FindProperty("TimeUntilRegen");
            regenRateProp = serializedObject.FindProperty("RegenRate");
            invincibleTimeProp = serializedObject.FindProperty("InvincibleTime");
            timeToDisconnectProp = serializedObject.FindProperty("TimeToDisconnect");
            minimumDamageProp = serializedObject.FindProperty("MinimumDamage");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
            EditorGUILayout.LabelField("角色状态配置 (Character Status Configuration)", headerStyle);
            
            EditorGUILayout.Space(10);
            
            // Health Section
            DrawSectionHeader("生命值设置 (Health Settings)", Color.red);
            EditorGUI.indentLevel++;
            DrawFPField(maxHealthProp, "最大生命值 (Max Health)", "角色满血时的生命值");
            DrawFPField(minimumDamageProp, "最小伤害值 (Minimum Damage)", "低于此值的伤害将被忽略");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Regeneration Section
            DrawSectionHeader("生命恢复设置 (Regeneration Settings)", Color.green);
            EditorGUI.indentLevel++;
            DrawFPField(timeUntilRegenProp, "恢复延迟 (Time Until Regen)", "受伤后多久开始恢复生命值（秒）");
            DrawFPField(regenRateProp, "恢复速度 (Regen Rate)", "每秒恢复的生命值");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Respawn Section
            DrawSectionHeader("重生设置 (Respawn Settings)", Color.cyan);
            EditorGUI.indentLevel++;
            DrawFPField(respawnTimeProp, "重生时间 (Respawn Time)", "死亡后重生所需时间（秒）");
            DrawFPField(invincibleTimeProp, "无敌时间 (Invincible Time)", "重生后的无敌保护时间（秒）");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Disconnect Section
            DrawSectionHeader("断线设置 (Disconnect Settings)", Color.yellow);
            EditorGUI.indentLevel++;
            DrawFPField(timeToDisconnectProp, "断线超时 (Time To Disconnect)", "玩家断线后多久销毁角色实体（秒）");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Show live values
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("运行时实时数值 (Runtime Values)", MessageType.Info);
                StatusData statusData = (StatusData)target;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("Max Health", statusData.MaxHealth.AsFloat);
                EditorGUILayout.FloatField("Respawn Time", statusData.RespawnTime.AsFloat);
                EditorGUILayout.FloatField("Time Until Regen", statusData.TimeUntilRegen.AsFloat);
                EditorGUILayout.FloatField("Regen Rate", statusData.RegenRate.AsFloat);
                EditorGUILayout.FloatField("Invincible Time", statusData.InvincibleTime.AsFloat);
                EditorGUILayout.FloatField("Time To Disconnect", statusData.TimeToDisconnect.AsFloat);
                EditorGUILayout.FloatField("Minimum Damage", statusData.MinimumDamage.AsFloat);
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSectionHeader(string title, Color color)
        {
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.normal.textColor = color;
            EditorGUILayout.LabelField(title, sectionStyle);
            EditorGUILayout.Space(2);
        }

        private void DrawFPField(SerializedProperty property, string label, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
            
            // Show the actual float value as a helper
            SerializedProperty rawValueProp = property.FindPropertyRelative("RawValue");
            if (rawValueProp != null)
            {
                long rawValue = rawValueProp.longValue;
                FP fpValue = FP.FromRaw(rawValue);
                EditorGUILayout.LabelField($"≈ {fpValue.AsFloat:F2}", GUILayout.Width(80));
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
