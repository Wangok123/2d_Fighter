#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义属性绘制器，用于优化 Shape2DConfig 的显示
/// 根据形状类型动态显示相关参数
/// </summary>
[CustomPropertyDrawer(typeof(Shape2DConfig))]
public class Shape2DConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var shapeTypeProp = property.FindPropertyRelative("ShapeType");
        var shapeType = shapeTypeProp.enumValueIndex;

        // 绘制形状类型选择
        EditorGUILayout.PropertyField(shapeTypeProp, new GUIContent("形状类型 (Shape Type)"));
        EditorGUILayout.Space(5);

        // 根据形状类型显示对应的参数
        EditorGUI.indentLevel++;
        
        switch (shapeType)
        {
            case 0: // None
                EditorGUILayout.HelpBox("未设置形状", MessageType.Warning);
                break;
                
            case 1: // Polygon
                DrawPolygonProperties(property);
                break;
                
            case 2: // Circle
                DrawCircleProperties(property);
                break;
                
            case 3: // Capsule
                DrawCapsuleProperties(property);
                break;
                
            case 4: // Box
                DrawBoxProperties(property);
                break;
                
            case 5: // Edge
                DrawEdgeProperties(property);
                break;
                
            case 6: // Compound
                DrawCompoundProperties(property);
                break;
        }
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // 通用属性
        EditorGUILayout.LabelField("位置与旋转", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawFPVector2Property(property, "PositionOffset", "位置偏移");
        DrawFPProperty(property, "RotationOffset", "旋转偏移", "°");
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // 其他通用设置
        EditorGUILayout.LabelField("其他设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(property.FindPropertyRelative("UserTag"), new GUIContent("用户标签"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("IsPersistent"), new GUIContent("持久化"));
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -2f; // 使用 EditorGUILayout，返回 -2 表示不使用固定高度
    }

    private void DrawPolygonProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("多边形设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(property.FindPropertyRelative("PolygonCollider"), new GUIContent("多边形碰撞器"));
        EditorGUILayout.HelpBox("需要引用一个预定义的多边形碰撞器资源", MessageType.Info);
        EditorGUI.indentLevel--;
    }

    private void DrawCircleProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("圆形设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawFPProperty(property, "CircleRadius", "半径", "");
        
        // 可视化
        var radiusProp = property.FindPropertyRelative("CircleRadius");
        var rawValue = radiusProp?.FindPropertyRelative("RawValue");
        if (rawValue != null)
        {
            float radius = rawValue.longValue / 65536f;
            DrawCirclePreview(radius);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawCapsuleProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("胶囊设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawFPVector2Property(property, "CapsuleSize", "尺寸 (宽, 高)");
        
        // 可视化
        var sizeProp = property.FindPropertyRelative("CapsuleSize");
        if (sizeProp != null)
        {
            float width = GetFPValue(sizeProp, "X");
            float height = GetFPValue(sizeProp, "Y");
            DrawCapsulePreview(width, height);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawBoxProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("矩形设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawFPVector2Property(property, "BoxExtents", "半尺寸 (宽/2, 高/2)");
        
        // 可视化
        var extentsProp = property.FindPropertyRelative("BoxExtents");
        if (extentsProp != null)
        {
            float halfWidth = GetFPValue(extentsProp, "X");
            float halfHeight = GetFPValue(extentsProp, "Y");
            EditorGUILayout.LabelField($"实际尺寸: {halfWidth * 2:F2} × {halfHeight * 2:F2}", EditorStyles.miniLabel);
            DrawBoxPreview(halfWidth, halfHeight);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawEdgeProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("边缘设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawFPProperty(property, "EdgeExtent", "延伸长度", "");
        EditorGUI.indentLevel--;
    }

    private void DrawCompoundProperties(SerializedProperty property)
    {
        EditorGUILayout.LabelField("复合形状设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(property.FindPropertyRelative("CompoundShapes"), new GUIContent("子形状列表"), true);
        EditorGUILayout.HelpBox("复合形状由多个子形状组成", MessageType.Info);
        EditorGUI.indentLevel--;
    }

    private void DrawFPProperty(SerializedProperty property, string fieldName, string label, string unit)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(prop, new GUIContent(label));
        
        var rawValueProp = prop.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            EditorGUILayout.LabelField($"≈ {value:F2}{unit}", GUILayout.Width(80));
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFPVector2Property(SerializedProperty property, string fieldName, string label)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if (prop == null) return;

        EditorGUILayout.LabelField(label);
        EditorGUI.indentLevel++;
        
        float x = GetFPValue(prop, "X");
        float y = GetFPValue(prop, "Y");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("X:", GUILayout.Width(20));
        DrawFPSubProperty(prop, "X");
        EditorGUILayout.LabelField($"≈ {x:F2}", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Y:", GUILayout.Width(20));
        DrawFPSubProperty(prop, "Y");
        EditorGUILayout.LabelField($"≈ {y:F2}", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
    }

    private void DrawFPSubProperty(SerializedProperty vectorProp, string axis)
    {
        var axisProp = vectorProp.FindPropertyRelative(axis);
        if (axisProp != null)
        {
            EditorGUILayout.PropertyField(axisProp, GUIContent.none);
        }
    }

    private float GetFPValue(SerializedProperty vectorProp, string axis)
    {
        var axisProp = vectorProp.FindPropertyRelative(axis);
        var rawValue = axisProp?.FindPropertyRelative("RawValue");
        if (rawValue != null)
        {
            return rawValue.longValue / 65536f;
        }
        return 0f;
    }

    private void DrawCirclePreview(float radius)
    {
        EditorGUILayout.Space(5);
        Rect rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
        
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        Vector2 center = new Vector2(rect.x + 50, rect.y + 50);
        float displayRadius = Mathf.Min(radius * 20f, 40f);
        
        Handles.color = new Color(0f, 1f, 0f, 0.5f);
        Handles.DrawSolidDisc(center, Vector3.forward, displayRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(center, Vector3.forward, displayRadius);
        
        EditorGUILayout.LabelField($"预览 (比例缩放)", EditorStyles.miniLabel);
    }

    private void DrawCapsulePreview(float width, float height)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"预览: 宽 {width:F2} × 高 {height:F2}", EditorStyles.miniLabel);
    }

    private void DrawBoxPreview(float halfWidth, float halfHeight)
    {
        EditorGUILayout.Space(5);
        Rect rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
        
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        Vector2 center = new Vector2(rect.x + 50, rect.y + 50);
        float displayWidth = Mathf.Min(halfWidth * 20f, 40f);
        float displayHeight = Mathf.Min(halfHeight * 20f, 40f);
        
        Rect boxRect = new Rect(center.x - displayWidth, center.y - displayHeight, displayWidth * 2, displayHeight * 2);
        EditorGUI.DrawRect(boxRect, new Color(0f, 1f, 0f, 0.3f));
        
        Handles.color = Color.green;
        Handles.DrawLine(new Vector2(boxRect.xMin, boxRect.yMin), new Vector2(boxRect.xMax, boxRect.yMin));
        Handles.DrawLine(new Vector2(boxRect.xMax, boxRect.yMin), new Vector2(boxRect.xMax, boxRect.yMax));
        Handles.DrawLine(new Vector2(boxRect.xMax, boxRect.yMax), new Vector2(boxRect.xMin, boxRect.yMax));
        Handles.DrawLine(new Vector2(boxRect.xMin, boxRect.yMax), new Vector2(boxRect.xMin, boxRect.yMin));
        
        EditorGUILayout.LabelField($"预览 (比例缩放)", EditorStyles.miniLabel);
    }
}
#endif
