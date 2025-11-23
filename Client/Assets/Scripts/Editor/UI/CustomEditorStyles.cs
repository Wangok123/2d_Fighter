#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 自定义编辑器样式库
/// 提供统一的颜色、样式和绘制方法供所有编辑器使用
/// </summary>
public static class CustomEditorStyles
{
    // 颜色主题
    public static class Colors
    {
        // 时序相关颜色
        public static readonly Color TimingVeryShort = new Color(1f, 0.3f, 0.3f);      // 红色 - 极短
        public static readonly Color TimingShort = new Color(0.3f, 1f, 0.3f);          // 绿色 - 短
        public static readonly Color TimingModerate = new Color(1f, 1f, 0.3f);         // 黄色 - 中等
        public static readonly Color TimingLong = new Color(1f, 0.6f, 0.3f);           // 橙色 - 长
        
        // 伤害/效果相关颜色
        public static readonly Color DamageLow = new Color(0.3f, 1f, 0.3f);            // 绿色 - 低伤害
        public static readonly Color DamageMedium = new Color(1f, 1f, 0.3f);           // 黄色 - 中伤害
        public static readonly Color DamageHigh = new Color(1f, 0.5f, 0.3f);           // 橙色 - 高伤害
        public static readonly Color DamageVeryHigh = new Color(1f, 0.3f, 0.3f);       // 红色 - 极高伤害
        
        // 治疗相关颜色
        public static readonly Color HealLow = new Color(0.3f, 1f, 0.5f);
        public static readonly Color HealMedium = new Color(0.3f, 1f, 0.7f);
        public static readonly Color HealHigh = new Color(0.3f, 1f, 0.9f);
        
        // 状态相关颜色
        public static readonly Color StatusValid = new Color(0.3f, 1f, 0.3f);          // 绿色 - 有效
        public static readonly Color StatusWarning = new Color(1f, 1f, 0.3f);          // 黄色 - 警告
        public static readonly Color StatusError = new Color(1f, 0.3f, 0.3f);          // 红色 - 错误
        public static readonly Color StatusDisabled = new Color(0.5f, 0.5f, 0.5f);     // 灰色 - 禁用
        
        // 类型相关颜色
        public static readonly Color TypeHitbox = new Color(0.5f, 0.7f, 1f);           // 蓝色 - 碰撞盒
        public static readonly Color TypeProjectile = new Color(1f, 0.7f, 0.3f);       // 橙色 - 飞行道具
        public static readonly Color TypeSkillField = new Color(0.5f, 1f, 0.7f);       // 青色 - 技能场
        
        // 输入相关颜色
        public static readonly Color InputAttack = new Color(1f, 0.3f, 0.3f);          // 红色 - 攻击键
        public static readonly Color InputMovement = new Color(0.3f, 1f, 0.3f);        // 绿色 - 移动键
        public static readonly Color InputDirection = new Color(0.3f, 0.7f, 1f);       // 蓝色 - 方向键
        
        // 背景相关颜色
        public static readonly Color BackgroundDark = new Color(0.15f, 0.15f, 0.2f);
        public static readonly Color BackgroundMedium = new Color(0.2f, 0.2f, 0.25f);
        public static readonly Color BackgroundLight = new Color(0.25f, 0.25f, 0.3f);
        
        // 网格相关颜色
        public static readonly Color GridLine = new Color(0.4f, 0.4f, 0.4f, 0.3f);
        public static readonly Color GridBackground = new Color(0.2f, 0.2f, 0.2f);
    }

    // 图标和表情符号
    public static class Icons
    {
        public const string Timing = "⏱";
        public const string Movement = "🎯";
        public const string Damage = "⚔️";
        public const string Defense = "🛡";
        public const string Effect = "💫";
        public const string Buff = "⬆️";
        public const string Debuff = "⬇️";
        public const string Heal = "💚";
        public const string Attack = "💥";
        public const string Projectile = "🚀";
        public const string SkillField = "✨";
        public const string Hitbox = "📦";
        public const string Cancel = "🔄";
        public const string Priority = "🔢";
        public const string UI = "🎨";
        public const string Target = "🎲";
        public const string Area = "⭕";
        public const string Config = "⚙️";
        public const string Info = "📖";
        public const string Warning = "⚠️";
        public const string Success = "✓";
        public const string Error = "✗";
        public const string Direction = "↗";
        public const string Input = "🎮";
        public const string Charge = "⚡";
    }

    // GUIStyle 缓存
    private static GUIStyle _headerStyle;
    private static GUIStyle _subHeaderStyle;
    private static GUIStyle _sectionStyle;
    private static GUIStyle _valueStyle;
    private static GUIStyle _labelCenteredStyle;
    private static GUIStyle _boldCenteredStyle;

    public static GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft
                };
                _headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
            }
            return _headerStyle;
        }
    }

    public static GUIStyle SubHeaderStyle
    {
        get
        {
            if (_subHeaderStyle == null)
            {
                _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
                _subHeaderStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            }
            return _subHeaderStyle;
        }
    }

    public static GUIStyle SectionStyle
    {
        get
        {
            if (_sectionStyle == null)
            {
                _sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 11
                };
                _sectionStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            }
            return _sectionStyle;
        }
    }

    public static GUIStyle ValueStyle
    {
        get
        {
            if (_valueStyle == null)
            {
                _valueStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
            }
            return _valueStyle;
        }
    }

    public static GUIStyle LabelCenteredStyle
    {
        get
        {
            if (_labelCenteredStyle == null)
            {
                _labelCenteredStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
            return _labelCenteredStyle;
        }
    }

    public static GUIStyle BoldCenteredStyle
    {
        get
        {
            if (_boldCenteredStyle == null)
            {
                _boldCenteredStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
            return _boldCenteredStyle;
        }
    }

    // 实用绘制方法
    public static void DrawHeader(string text, string icon = "")
    {
        EditorGUILayout.Space(5);
        string displayText = string.IsNullOrEmpty(icon) ? text : $"{icon} {text}";
        EditorGUILayout.LabelField(displayText, HeaderStyle);
        EditorGUILayout.Space(5);
    }

    public static void DrawSubHeader(string text, string icon = "")
    {
        string displayText = string.IsNullOrEmpty(icon) ? text : $"{icon} {text}";
        EditorGUILayout.LabelField(displayText, SubHeaderStyle);
    }

    public static void DrawSectionLabel(string text)
    {
        EditorGUILayout.LabelField(text, SectionStyle);
    }

    public static void DrawColoredValue(string value, Color color, float width = 100f)
    {
        GUIStyle style = new GUIStyle(ValueStyle);
        style.normal.textColor = color;
        EditorGUILayout.LabelField(value, style, GUILayout.Width(width));
    }

    public static Color GetTimingColor(float seconds)
    {
        if (seconds < 0.1f) return Colors.TimingVeryShort;
        if (seconds < 1f) return Colors.TimingShort;
        if (seconds < 5f) return Colors.TimingModerate;
        return Colors.TimingLong;
    }

    public static Color GetDamageColor(int damage)
    {
        int absDamage = Mathf.Abs(damage);
        if (absDamage < 20) return Colors.DamageLow;
        if (absDamage < 50) return Colors.DamageMedium;
        if (absDamage < 100) return Colors.DamageHigh;
        return Colors.DamageVeryHigh;
    }

    public static Color GetHealColor(int heal)
    {
        if (heal < 20) return Colors.HealLow;
        if (heal < 50) return Colors.HealMedium;
        return Colors.HealHigh;
    }

    public static void DrawGradientBackground(Rect rect, Color topColor, Color bottomColor)
    {
        Texture2D gradientTex = new Texture2D(1, 2);
        gradientTex.SetPixel(0, 0, bottomColor);
        gradientTex.SetPixel(0, 1, topColor);
        gradientTex.Apply();
        GUI.DrawTexture(rect, gradientTex);
        Object.DestroyImmediate(gradientTex);
    }

    public static void DrawGrid(Rect rect, int horizontalLines = 4, int verticalLines = 4)
    {
        Handles.color = Colors.GridLine;
        
        // 横向网格线
        for (int i = 1; i < horizontalLines; i++)
        {
            float y = rect.y + rect.height * i / (float)horizontalLines;
            Handles.DrawLine(new Vector2(rect.x, y), new Vector2(rect.x + rect.width, y));
        }
        
        // 纵向网格线
        for (int i = 1; i < verticalLines; i++)
        {
            float x = rect.x + rect.width * i / (float)verticalLines;
            Handles.DrawLine(new Vector2(x, rect.y), new Vector2(x, rect.y + rect.height));
        }
    }

    public static void DrawProgressBar(Rect rect, float progress, Color fillColor, string label = "")
    {
        // 背景
        EditorGUI.DrawRect(rect, Colors.BackgroundDark);
        
        // 进度条
        progress = Mathf.Clamp01(progress);
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * progress, rect.height);
        EditorGUI.DrawRect(fillRect, fillColor);
        
        // 边框
        Handles.color = new Color(0.5f, 0.5f, 0.5f);
        Handles.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin));
        Handles.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax));
        Handles.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax));
        Handles.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin));
        
        // 文字
        if (!string.IsNullOrEmpty(label))
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = Color.white;
            GUI.Label(rect, label, textStyle);
        }
    }

    public static void DrawBadge(Rect rect, string text, Color backgroundColor, Color? textColor = null)
    {
        EditorGUI.DrawRect(rect, backgroundColor);
        
        GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniLabel);
        badgeStyle.alignment = TextAnchor.MiddleCenter;
        badgeStyle.fontStyle = FontStyle.Bold;
        badgeStyle.normal.textColor = textColor ?? Color.white;
        
        GUI.Label(rect, text, badgeStyle);
    }

    public static void DrawInfoBox(string message, MessageType messageType = MessageType.Info, bool richText = false)
    {
        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        if (richText)
        {
            style.richText = true;
            style.wordWrap = true;
        }
        EditorGUILayout.LabelField(message, style);
    }

    public static void DrawSeparator(float height = 1f, float spacing = 10f)
    {
        EditorGUILayout.Space(spacing);
        Rect rect = EditorGUILayout.GetControlRect(false, height);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(spacing);
    }

    public static Rect BeginColoredBox(Color color, float alpha = 0.3f)
    {
        Rect rect = EditorGUILayout.BeginVertical(GUI.skin.box);
        Color bgColor = new Color(color.r, color.g, color.b, alpha);
        EditorGUI.DrawRect(rect, bgColor);
        return rect;
    }

    public static void EndColoredBox()
    {
        EditorGUILayout.EndVertical();
    }
}
#endif
