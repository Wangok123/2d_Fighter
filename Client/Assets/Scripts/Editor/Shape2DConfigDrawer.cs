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

        float yPos = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // 绘制形状类型选择
        Rect shapeTypeRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.PropertyField(shapeTypeRect, shapeTypeProp, new GUIContent("形状类型 (Shape Type)"));
        yPos += lineHeight + spacing;

        // 根据形状类型显示对应的参数
        EditorGUI.indentLevel++;
        
        switch (shapeType)
        {
            case 0: // None
                Rect helpBoxRect = new Rect(position.x, yPos, position.width, lineHeight * 2);
                EditorGUI.HelpBox(helpBoxRect, "未设置形状", MessageType.Warning);
                yPos += lineHeight * 2 + spacing;
                break;
                
            case 1: // Polygon
                yPos = DrawPolygonProperties(position, property, yPos);
                break;
                
            case 2: // Circle
                yPos = DrawCircleProperties(position, property, yPos);
                break;
                
            case 3: // Capsule
                yPos = DrawCapsuleProperties(position, property, yPos);
                break;
                
            case 4: // Box
                yPos = DrawBoxProperties(position, property, yPos);
                break;
                
            case 5: // Edge
                yPos = DrawEdgeProperties(position, property, yPos);
                break;
                
            case 6: // Compound
                yPos = DrawCompoundProperties(position, property, yPos);
                break;
        }
        
        EditorGUI.indentLevel--;
        
        yPos += spacing;
        
        // 通用属性
        Rect commonLabelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(commonLabelRect, "位置与旋转", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        yPos = DrawFPVector2Property(position, property, yPos, "PositionOffset", "位置偏移");
        yPos = DrawFPProperty(position, property, yPos, "RotationOffset", "旋转偏移", "°");
        EditorGUI.indentLevel--;
        
        yPos += spacing;
        
        // 其他通用设置
        Rect otherLabelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(otherLabelRect, "其他设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        Rect userTagRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.PropertyField(userTagRect, property.FindPropertyRelative("UserTag"), new GUIContent("用户标签"));
        yPos += lineHeight + spacing;
        
        Rect persistentRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.PropertyField(persistentRect, property.FindPropertyRelative("IsPersistent"), new GUIContent("持久化"));
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var shapeTypeProp = property.FindPropertyRelative("ShapeType");
        var shapeType = shapeTypeProp.enumValueIndex;
        
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = lineHeight + spacing; // Shape type field
        
        // Calculate height based on shape type
        switch (shapeType)
        {
            case 0: // None
                height += lineHeight * 2 + spacing; // Help box
                break;
            case 1: // Polygon
                height += GetPolygonPropertiesHeight(property);
                break;
            case 2: // Circle
                height += GetCirclePropertiesHeight(property);
                break;
            case 3: // Capsule
                height += GetCapsulePropertiesHeight(property);
                break;
            case 4: // Box
                height += GetBoxPropertiesHeight(property);
                break;
            case 5: // Edge
                height += GetEdgePropertiesHeight(property);
                break;
            case 6: // Compound
                height += GetCompoundPropertiesHeight(property);
                break;
        }
        
        height += spacing; // Extra spacing
        
        // Common properties
        height += lineHeight + spacing; // "位置与旋转" label
        height += GetFPVector2PropertyHeight(); // PositionOffset
        height += GetFPPropertyHeight(); // RotationOffset
        
        height += spacing; // Extra spacing
        
        // Other settings
        height += lineHeight + spacing; // "其他设置" label
        height += lineHeight + spacing; // UserTag
        height += lineHeight + spacing; // IsPersistent
        
        return height;
    }

    private float DrawPolygonProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "多边形设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        Rect polyRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.PropertyField(polyRect, property.FindPropertyRelative("PolygonCollider"), new GUIContent("多边形碰撞器"));
        yPos += lineHeight + spacing;
        
        Rect helpRect = new Rect(position.x, yPos, position.width, lineHeight * 2);
        EditorGUI.HelpBox(helpRect, "需要引用一个预定义的多边形碰撞器资源", MessageType.Info);
        yPos += lineHeight * 2 + spacing;
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetPolygonPropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return lineHeight + spacing + // Label
               lineHeight + spacing + // PolygonCollider
               lineHeight * 2 + spacing; // HelpBox
    }

    private float DrawCircleProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "圆形设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        yPos = DrawFPProperty(position, property, yPos, "CircleRadius", "半径", "");
        
        // 可视化
        var radiusProp = property.FindPropertyRelative("CircleRadius");
        var rawValue = radiusProp?.FindPropertyRelative("RawValue");
        if (rawValue != null)
        {
            float radius = rawValue.longValue / 65536f;
            yPos = DrawCirclePreview(position, yPos, radius);
        }
        
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetCirclePropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = lineHeight + spacing + // Label
                      GetFPPropertyHeight(); // CircleRadius
        
        var radiusProp = property.FindPropertyRelative("CircleRadius");
        var rawValue = radiusProp?.FindPropertyRelative("RawValue");
        if (rawValue != null)
        {
            height += 100 + lineHeight + spacing * 2; // Preview
        }
        
        return height;
    }

    private float DrawCapsuleProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "胶囊设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        yPos = DrawFPVector2Property(position, property, yPos, "CapsuleSize", "尺寸 (宽, 高)");
        
        // 可视化
        var sizeProp = property.FindPropertyRelative("CapsuleSize");
        if (sizeProp != null)
        {
            float width = GetFPValue(sizeProp, "X");
            float height = GetFPValue(sizeProp, "Y");
            yPos = DrawCapsulePreview(position, yPos, width, height);
        }
        
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetCapsulePropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = lineHeight + spacing + // Label
                      GetFPVector2PropertyHeight(); // CapsuleSize
        
        var sizeProp = property.FindPropertyRelative("CapsuleSize");
        if (sizeProp != null)
        {
            height += lineHeight + spacing; // Preview text
        }
        
        return height;
    }

    private float DrawBoxProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "矩形设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        yPos = DrawFPVector2Property(position, property, yPos, "BoxExtents", "半尺寸 (宽/2, 高/2)");
        
        // 可视化
        var extentsProp = property.FindPropertyRelative("BoxExtents");
        if (extentsProp != null)
        {
            float halfWidth = GetFPValue(extentsProp, "X");
            float halfHeight = GetFPValue(extentsProp, "Y");
            
            Rect sizeRect = new Rect(position.x, yPos, position.width, lineHeight);
            EditorGUI.LabelField(sizeRect, $"实际尺寸: {halfWidth * 2:F2} × {halfHeight * 2:F2}", EditorStyles.miniLabel);
            yPos += lineHeight + spacing;
            
            yPos = DrawBoxPreview(position, yPos, halfWidth, halfHeight);
        }
        
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetBoxPropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = lineHeight + spacing + // Label
                      GetFPVector2PropertyHeight(); // BoxExtents
        
        var extentsProp = property.FindPropertyRelative("BoxExtents");
        if (extentsProp != null)
        {
            height += lineHeight + spacing; // Size label
            height += 100 + lineHeight + spacing * 2; // Preview
        }
        
        return height;
    }

    private float DrawEdgeProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "边缘设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        yPos = DrawFPProperty(position, property, yPos, "EdgeExtent", "延伸长度", "");
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetEdgePropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return lineHeight + spacing + // Label
               GetFPPropertyHeight(); // EdgeExtent
    }

    private float DrawCompoundProperties(Rect position, SerializedProperty property, float yPos)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, "复合形状设置", EditorStyles.boldLabel);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        var compoundProp = property.FindPropertyRelative("CompoundShapes");
        float propHeight = EditorGUI.GetPropertyHeight(compoundProp, true);
        Rect propRect = new Rect(position.x, yPos, position.width, propHeight);
        EditorGUI.PropertyField(propRect, compoundProp, new GUIContent("子形状列表"), true);
        yPos += propHeight + spacing;
        
        Rect helpRect = new Rect(position.x, yPos, position.width, lineHeight * 2);
        EditorGUI.HelpBox(helpRect, "复合形状由多个子形状组成", MessageType.Info);
        yPos += lineHeight * 2 + spacing;
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetCompoundPropertiesHeight(SerializedProperty property)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        var compoundProp = property.FindPropertyRelative("CompoundShapes");
        float propHeight = EditorGUI.GetPropertyHeight(compoundProp, true);
        
        return lineHeight + spacing + // Label
               propHeight + spacing + // CompoundShapes
               lineHeight * 2 + spacing; // HelpBox
    }

    private float DrawFPProperty(Rect position, SerializedProperty property, float yPos, string fieldName, string label, string unit)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if (prop == null) return yPos;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect propRect = new Rect(position.x, yPos, position.width - 80, lineHeight);
        EditorGUI.PropertyField(propRect, prop, new GUIContent(label));
        
        var rawValueProp = prop.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            Rect valueRect = new Rect(position.x + position.width - 80, yPos, 80, lineHeight);
            EditorGUI.LabelField(valueRect, $"≈ {value:F2}{unit}");
        }
        
        return yPos + lineHeight + spacing;
    }
    
    private float GetFPPropertyHeight()
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return lineHeight + spacing;
    }

    private float DrawFPVector2Property(Rect position, SerializedProperty property, float yPos, string fieldName, string label)
    {
        var prop = property.FindPropertyRelative(fieldName);
        if (prop == null) return yPos;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, label);
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel++;
        
        float x = GetFPValue(prop, "X");
        float y = GetFPValue(prop, "Y");
        
        // X field
        Rect xLabelRect = new Rect(position.x, yPos, 20, lineHeight);
        EditorGUI.LabelField(xLabelRect, "X:");
        
        var xProp = prop.FindPropertyRelative("X");
        Rect xFieldRect = new Rect(position.x + 20, yPos, position.width - 80, lineHeight);
        EditorGUI.PropertyField(xFieldRect, xProp, GUIContent.none);
        
        Rect xValueRect = new Rect(position.x + position.width - 60, yPos, 60, lineHeight);
        EditorGUI.LabelField(xValueRect, $"≈ {x:F2}");
        yPos += lineHeight + spacing;
        
        // Y field
        Rect yLabelRect = new Rect(position.x, yPos, 20, lineHeight);
        EditorGUI.LabelField(yLabelRect, "Y:");
        
        var yProp = prop.FindPropertyRelative("Y");
        Rect yFieldRect = new Rect(position.x + 20, yPos, position.width - 80, lineHeight);
        EditorGUI.PropertyField(yFieldRect, yProp, GUIContent.none);
        
        Rect yValueRect = new Rect(position.x + position.width - 60, yPos, 60, lineHeight);
        EditorGUI.LabelField(yValueRect, $"≈ {y:F2}");
        yPos += lineHeight + spacing;
        
        EditorGUI.indentLevel--;
        
        return yPos;
    }
    
    private float GetFPVector2PropertyHeight()
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return lineHeight + spacing + // Label
               lineHeight + spacing + // X field
               lineHeight + spacing;  // Y field
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

    private float DrawCirclePreview(Rect position, float yPos, float radius)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        yPos += spacing;
        
        Rect rect = new Rect(position.x, yPos, 100, 100);
        
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        Vector2 center = new Vector2(rect.x + 50, rect.y + 50);
        float displayRadius = Mathf.Min(radius * 20f, 40f);
        
        Handles.color = new Color(0f, 1f, 0f, 0.5f);
        Handles.DrawSolidDisc(center, Vector3.forward, displayRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(center, Vector3.forward, displayRadius);
        
        yPos += 100 + spacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, $"预览 (比例缩放)", EditorStyles.miniLabel);
        yPos += lineHeight + spacing;
        
        return yPos;
    }

    private float DrawCapsulePreview(Rect position, float yPos, float width, float height)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        yPos += spacing;
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, $"预览: 宽 {width:F2} × 高 {height:F2}", EditorStyles.miniLabel);
        yPos += lineHeight + spacing;
        
        return yPos;
    }

    private float DrawBoxPreview(Rect position, float yPos, float halfWidth, float halfHeight)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        yPos += spacing;
        
        Rect rect = new Rect(position.x, yPos, 100, 100);
        
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
        
        yPos += 100 + spacing;
        
        Rect labelRect = new Rect(position.x, yPos, position.width, lineHeight);
        EditorGUI.LabelField(labelRect, $"预览 (比例缩放)", EditorStyles.miniLabel);
        yPos += lineHeight + spacing;
        
        return yPos;
    }
}
#endif
