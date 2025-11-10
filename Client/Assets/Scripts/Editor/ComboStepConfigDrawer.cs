#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义属性绘制器，用于优化 ComboStepConfig 的显示
/// 使连招配置更加直观和易于编辑
/// </summary>
[CustomPropertyDrawer(typeof(ComboStepConfig))]
public class ComboStepConfigDrawer : PropertyDrawer
{
    private bool isExpanded = false;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // 绘制折叠标题
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        isExpanded = property.isExpanded;
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            // 时序设置组
            DrawGroupLabel(position, ref yOffset, "⏱ 时序设置");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "Duration", "持续时间", "s");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "HitboxActiveTime", "判定激活时间", "s");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "HitboxActiveDuration", "判定持续时间", "s");
            
            yOffset += EditorGUIUtility.standardVerticalSpacing * 2;
            
            // 击退设置组
            DrawGroupLabel(position, ref yOffset, "💥 击退设置");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "KnockbackForce", "击退力度", "");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "KnockbackDirectionX", "击退方向X", "");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "KnockbackDirectionY", "击退方向Y", "");
            
            yOffset += EditorGUIUtility.standardVerticalSpacing * 2;
            
            // 硬直设置组
            DrawGroupLabel(position, ref yOffset, "🛑 硬直设置");
            DrawFPPropertyWithInfo(position, property, ref yOffset, "HitstunDuration", "硬直时间", "s");
            DrawEnumProperty(position, property, ref yOffset, "HitType", "受击类型");
            
            yOffset += EditorGUIUtility.standardVerticalSpacing * 2;
            
            // 攻击形状
            DrawGroupLabel(position, ref yOffset, "🎯 攻击形状");
            DrawComplexProperty(position, property, ref yOffset, "AttackShape", "攻击判定形状");
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        float height = EditorGUIUtility.singleLineHeight; // 标题行
        
        // 时序设置
        height += EditorGUIUtility.singleLineHeight; // 组标签
        height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3; // 3个属性
        height += EditorGUIUtility.standardVerticalSpacing * 2; // 组间距
        
        // 击退设置
        height += EditorGUIUtility.singleLineHeight; // 组标签
        height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3; // 3个属性
        height += EditorGUIUtility.standardVerticalSpacing * 2; // 组间距
        
        // 硬直设置
        height += EditorGUIUtility.singleLineHeight; // 组标签
        height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2; // 2个属性
        height += EditorGUIUtility.standardVerticalSpacing * 2; // 组间距
        
        // 攻击形状
        height += EditorGUIUtility.singleLineHeight; // 组标签
        var attackShapeProp = property.FindPropertyRelative("AttackShape");
        if (attackShapeProp != null)
        {
            height += EditorGUI.GetPropertyHeight(attackShapeProp, true);
        }
        
        return height;
    }
    
    private void DrawGroupLabel(Rect position, ref float yOffset, string label)
    {
        Rect labelRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
        yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
    
    private void DrawFPPropertyWithInfo(Rect position, SerializedProperty property, ref float yOffset, string propertyName, string label, string unit)
    {
        var prop = property.FindPropertyRelative(propertyName);
        if (prop == null) return;
        
        Rect propertyRect = new Rect(position.x, position.y + yOffset, position.width - 80, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(propertyRect, prop, new GUIContent(label));
        
        // 显示实际数值
        var rawValueProp = prop.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            
            Rect valueRect = new Rect(position.x + position.width - 75, position.y + yOffset, 75, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(valueRect, $"≈ {value:F2}{unit}", EditorStyles.miniLabel);
        }
        
        yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
    
    private void DrawEnumProperty(Rect position, SerializedProperty property, ref float yOffset, string propertyName, string label)
    {
        var prop = property.FindPropertyRelative(propertyName);
        if (prop == null) return;
        
        Rect propertyRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(propertyRect, prop, new GUIContent(label));
        
        yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
    
    private void DrawComplexProperty(Rect position, SerializedProperty property, ref float yOffset, string propertyName, string label)
    {
        var prop = property.FindPropertyRelative(propertyName);
        if (prop == null) return;
        
        float propHeight = EditorGUI.GetPropertyHeight(prop, true);
        Rect propertyRect = new Rect(position.x, position.y + yOffset, position.width, propHeight);
        EditorGUI.PropertyField(propertyRect, prop, new GUIContent(label), true);
        
        yOffset += propHeight + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
