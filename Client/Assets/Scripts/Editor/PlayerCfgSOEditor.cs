#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Configs;
using System.Collections.Generic;

/// <summary>
/// PlayerCfgSO 的自定义编辑器
/// Custom Editor for PlayerCfgSO - Visualizes player configuration with organized sections
/// </summary>
[CustomEditor(typeof(PlayerCfgSO), true)]
public class PlayerCfgSOEditor : Editor
{
    // Player Info
    private SerializedProperty moveSpeedProp;
    private SerializedProperty jumpForceProp;
    private SerializedProperty airSpeedMultiplierProp;

    // Dash Info
    private SerializedProperty dashTimeProp;
    private SerializedProperty dashSpeedProp;
    private SerializedProperty dashCooldownProp;

    // Wall Slide Info
    private SerializedProperty wallSlideSpeedMultiplierProp;
    private SerializedProperty wallJumpForceProp;
    private SerializedProperty wallJumpCooldownProp;
    private SerializedProperty attackSlideTimeProp;

    // Attack Info
    private SerializedProperty attackMovementProp;
    private SerializedProperty comboResetTimeProp;

    private bool showPlayerInfo = true;
    private bool showDashInfo = true;
    private bool showWallSlideInfo = true;
    private bool showAttackInfo = true;

    private void OnEnable()
    {
        // Player Info
        moveSpeedProp = serializedObject.FindProperty("moveSpeed");
        jumpForceProp = serializedObject.FindProperty("jumpForce");
        airSpeedMultiplierProp = serializedObject.FindProperty("airSpeedMultiplier");

        // Dash Info
        dashTimeProp = serializedObject.FindProperty("dashTime");
        dashSpeedProp = serializedObject.FindProperty("dashSpeed");
        dashCooldownProp = serializedObject.FindProperty("dashCooldown");

        // Wall Slide Info
        wallSlideSpeedMultiplierProp = serializedObject.FindProperty("wallSlideSpeedMultiplier");
        wallJumpForceProp = serializedObject.FindProperty("wallJumpForce");
        wallJumpCooldownProp = serializedObject.FindProperty("wallJumpCooldown");
        attackSlideTimeProp = serializedObject.FindProperty("attackSlideTime");

        // Attack Info
        attackMovementProp = serializedObject.FindProperty("attackMovement");
        comboResetTimeProp = serializedObject.FindProperty("comboResetTime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 标题
        CustomEditorStyles.DrawHeader("玩家配置 Player Configuration", CustomEditorStyles.Icons.Config);

        EditorGUILayout.Space(10);

        // ======= 玩家移动设置 =======
        showPlayerInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayerInfo, 
            $"{CustomEditorStyles.Icons.Movement} 移动设置 Player Movement");
        
        if (showPlayerInfo)
        {
            CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);

            // 移动速度
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(moveSpeedProp, new GUIContent("移动速度 Move Speed"));
            float moveSpeed = moveSpeedProp.floatValue;
            Color speedColor = moveSpeed > 8f ? CustomEditorStyles.Colors.StatusValid :
                              moveSpeed > 4f ? CustomEditorStyles.Colors.StatusWarning :
                              CustomEditorStyles.Colors.StatusError;
            CustomEditorStyles.DrawColoredValue($"{moveSpeed:F1} m/s", speedColor);
            EditorGUILayout.EndHorizontal();

            // 移动速度评估
            string speedRating = moveSpeed > 8f ? "🏃 快速 Fast" :
                                moveSpeed > 4f ? "🚶 中等 Medium" :
                                "🐌 缓慢 Slow";
            EditorGUILayout.LabelField($"   速度评估: {speedRating}");

            // 速度进度条
            Rect speedRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            CustomEditorStyles.DrawProgressBar(speedRect, Mathf.Clamp01(moveSpeed / 15f), speedColor, $"{moveSpeed:F1} m/s");

            EditorGUILayout.Space(5);

            // 跳跃力度
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(jumpForceProp, new GUIContent("跳跃力度 Jump Force"));
            float jumpForce = jumpForceProp.floatValue;
            Color jumpColor = jumpForce > 15f ? CustomEditorStyles.Colors.StatusValid :
                             jumpForce > 8f ? CustomEditorStyles.Colors.StatusWarning :
                             CustomEditorStyles.Colors.StatusError;
            CustomEditorStyles.DrawColoredValue($"{jumpForce:F1}", jumpColor);
            EditorGUILayout.EndHorizontal();

            // 跳跃高度估算（简化物理计算）
            float estimatedHeight = (jumpForce * jumpForce) / (2f * 9.81f);
            EditorGUILayout.LabelField($"   预计跳跃高度 Est. Height: {estimatedHeight:F2}m");

            EditorGUILayout.Space(5);

            // 空中速度倍率
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(airSpeedMultiplierProp, new GUIContent("空中速度倍率 Air Speed Mult"));
            float airMult = airSpeedMultiplierProp.floatValue;
            Color airColor = airMult >= 0.8f ? CustomEditorStyles.Colors.StatusValid :
                            airMult >= 0.5f ? CustomEditorStyles.Colors.StatusWarning :
                            CustomEditorStyles.Colors.StatusError;
            CustomEditorStyles.DrawColoredValue($"×{airMult:F2}", airColor);
            EditorGUILayout.EndHorizontal();

            // 空中实际速度
            float airSpeed = moveSpeed * airMult;
            EditorGUILayout.LabelField($"   空中速度 Air Speed: {airSpeed:F2} m/s");

            CustomEditorStyles.EndColoredBox();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // ======= 冲刺设置 =======
        showDashInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showDashInfo, 
            $"{CustomEditorStyles.Icons.Charge} 冲刺设置 Dash Settings");
        
        if (showDashInfo)
        {
            CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);

            // 冲刺时间
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dashTimeProp, new GUIContent("冲刺持续时间 Dash Time"));
            float dashTime = dashTimeProp.floatValue;
            Color dashTimeColor = CustomEditorStyles.GetTimingColor(dashTime);
            CustomEditorStyles.DrawColoredValue($"{dashTime:F2}s", dashTimeColor);
            EditorGUILayout.EndHorizontal();

            // 冲刺速度
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dashSpeedProp, new GUIContent("冲刺速度 Dash Speed"));
            float dashSpeed = dashSpeedProp.floatValue;
            Color dashSpeedColor = dashSpeed > 30f ? CustomEditorStyles.Colors.StatusValid :
                                   dashSpeed > 20f ? CustomEditorStyles.Colors.StatusWarning :
                                   CustomEditorStyles.Colors.StatusError;
            CustomEditorStyles.DrawColoredValue($"{dashSpeed:F1} m/s", dashSpeedColor);
            EditorGUILayout.EndHorizontal();

            // 冲刺距离计算
            float dashDistance = dashSpeed * dashTime;
            EditorGUILayout.LabelField($"   冲刺距离 Dash Distance: {dashDistance:F2}m");

            // 冲刺距离可视化
            Rect dashDistRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            DrawDashVisualization(dashDistRect, dashDistance);

            EditorGUILayout.Space(5);

            // 冲刺冷却
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dashCooldownProp, new GUIContent("冲刺冷却 Dash Cooldown"));
            float dashCooldown = dashCooldownProp.floatValue;
            Color cooldownColor = CustomEditorStyles.GetTimingColor(dashCooldown);
            CustomEditorStyles.DrawColoredValue($"{dashCooldown:F2}s", cooldownColor);
            EditorGUILayout.EndHorizontal();

            // 冷却进度条
            Rect cdRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            CustomEditorStyles.DrawProgressBar(cdRect, Mathf.Clamp01(dashCooldown / 5f), cooldownColor, $"{dashCooldown:F2}s");

            CustomEditorStyles.EndColoredBox();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // ======= 蹬墙/滑墙设置 =======
        showWallSlideInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showWallSlideInfo, 
            $"🧗 蹬墙滑墙设置 Wall Slide Settings");
        
        if (showWallSlideInfo)
        {
            CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);

            // 滑墙速度倍率
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(wallSlideSpeedMultiplierProp, new GUIContent("滑墙速度倍率 Wall Slide Mult"));
            float wallSlideMult = wallSlideSpeedMultiplierProp.floatValue;
            Color wallSlideColor = wallSlideMult <= 0.8f ? CustomEditorStyles.Colors.StatusValid :
                                   CustomEditorStyles.Colors.StatusWarning;
            CustomEditorStyles.DrawColoredValue($"×{wallSlideMult:F2}", wallSlideColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("滑墙速度倍率越小，下滑越慢（推荐 0.5-0.8）", MessageType.Info);

            EditorGUILayout.Space(5);

            // 蹬墙跳力度
            EditorGUILayout.PropertyField(wallJumpForceProp, new GUIContent("蹬墙跳力度 Wall Jump Force"));
            
            Vector2 wallJumpForce = wallJumpForceProp.vector2Value;
            EditorGUILayout.LabelField($"   水平力度: {wallJumpForce.x:F1}");
            EditorGUILayout.LabelField($"   垂直力度: {wallJumpForce.y:F1}");

            // 蹬墙跳方向可视化
            Rect wallJumpRect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
            DrawWallJumpVisualization(wallJumpRect, wallJumpForce);

            EditorGUILayout.Space(5);

            // 蹬墙跳冷却
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(wallJumpCooldownProp, new GUIContent("蹬墙跳冷却 Wall Jump Cooldown"));
            float wallJumpCooldown = wallJumpCooldownProp.floatValue;
            Color wallJumpCdColor = CustomEditorStyles.GetTimingColor(wallJumpCooldown);
            CustomEditorStyles.DrawColoredValue($"{wallJumpCooldown:F2}s", wallJumpCdColor);
            EditorGUILayout.EndHorizontal();

            // 攻击时滑墙时间
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(attackSlideTimeProp, new GUIContent("攻击滑墙时间 Attack Slide Time"));
            float attackSlideTime = attackSlideTimeProp.floatValue;
            Color attackSlideColor = CustomEditorStyles.GetTimingColor(attackSlideTime);
            CustomEditorStyles.DrawColoredValue($"{attackSlideTime:F2}s", attackSlideColor);
            EditorGUILayout.EndHorizontal();

            CustomEditorStyles.EndColoredBox();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // ======= 攻击设置 =======
        showAttackInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showAttackInfo, 
            $"{CustomEditorStyles.Icons.Attack} 攻击设置 Attack Settings");
        
        if (showAttackInfo)
        {
            CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);

            // 连招重置时间
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(comboResetTimeProp, new GUIContent("连招重置时间 Combo Reset Time"));
            double comboResetTime = comboResetTimeProp.doubleValue;
            Color comboResetColor = CustomEditorStyles.GetTimingColor((float)comboResetTime);
            CustomEditorStyles.DrawColoredValue($"{comboResetTime:F2}s", comboResetColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("连招重置时间：两次攻击间隔超过此时间则连招重置", MessageType.Info);

            EditorGUILayout.Space(5);

            // 攻击移动列表
            EditorGUILayout.PropertyField(attackMovementProp, new GUIContent("攻击位移 Attack Movement"), true);

            // 如果有攻击移动数据，显示可视化
            if (attackMovementProp != null && attackMovementProp.arraySize > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"攻击段数: {attackMovementProp.arraySize}", EditorStyles.boldLabel);
                
                // 绘制每段攻击的位移
                for (int i = 0; i < attackMovementProp.arraySize; i++)
                {
                    SerializedProperty movementProp = attackMovementProp.GetArrayElementAtIndex(i);
                    Vector2 movement = movementProp.vector2Value;
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  第 {i + 1} 段:", GUILayout.Width(60));
                    
                    // 位移箭头可视化（简化版）
                    string arrow = "";
                    if (Mathf.Abs(movement.x) > 0.1f)
                    {
                        arrow += movement.x > 0 ? "→" : "←";
                    }
                    if (Mathf.Abs(movement.y) > 0.1f)
                    {
                        arrow += movement.y > 0 ? "↑" : "↓";
                    }
                    if (string.IsNullOrEmpty(arrow))
                    {
                        arrow = "•";
                    }

                    EditorGUILayout.LabelField($"{arrow} ({movement.x:F1}, {movement.y:F1})", GUILayout.Width(120));
                    
                    float magnitude = movement.magnitude;
                    Color magnitudeColor = magnitude > 2f ? CustomEditorStyles.Colors.StatusValid :
                                          magnitude > 0.5f ? CustomEditorStyles.Colors.StatusWarning :
                                          CustomEditorStyles.Colors.StatusDisabled;
                    CustomEditorStyles.DrawColoredValue($"{magnitude:F2}m", magnitudeColor);
                    
                    EditorGUILayout.EndHorizontal();
                }
            }

            CustomEditorStyles.EndColoredBox();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // ======= 配置总结 =======
        CustomEditorStyles.BeginColoredBox(new Color(0.2f, 0.3f, 0.4f, 0.3f));
        CustomEditorStyles.DrawSubHeader("配置总结 Configuration Summary", CustomEditorStyles.Icons.Info);
        
        float moveSpeed = moveSpeedProp.floatValue;
        float dashSpeed = dashSpeedProp.floatValue;
        float dashDistance = dashSpeed * dashTimeProp.floatValue;
        
        EditorGUILayout.LabelField($"• 基础移动速度: {moveSpeed:F1} m/s");
        EditorGUILayout.LabelField($"• 冲刺速度: {dashSpeed:F1} m/s ({(dashSpeed/moveSpeed):F1}x 移动速度)");
        EditorGUILayout.LabelField($"• 单次冲刺距离: {dashDistance:F2}m");
        
        int comboSteps = attackMovementProp != null ? attackMovementProp.arraySize : 0;
        EditorGUILayout.LabelField($"• 连招段数: {comboSteps}");

        // 机动性评估
        float mobility = (moveSpeed + dashSpeed) / 2f;
        string mobilityRating = mobility > 20f ? "🚀 极高 Excellent" :
                                mobility > 15f ? "⚡ 高 High" :
                                mobility > 10f ? "✓ 中等 Medium" :
                                "⚠ 低 Low";
        EditorGUILayout.LabelField($"• 机动性评估: {mobilityRating}");

        CustomEditorStyles.EndColoredBox();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制冲刺距离可视化
    /// </summary>
    private void DrawDashVisualization(Rect rect, float distance)
    {
        // 背景
        EditorGUI.DrawRect(rect, CustomEditorStyles.Colors.BackgroundDark);

        // 起点标记
        Vector2 startPos = new Vector2(rect.x + 10, rect.y + rect.height / 2);
        Handles.color = Color.green;
        Handles.DrawSolidDisc(new Vector3(startPos.x, startPos.y, 0), Vector3.forward, 5);

        // 冲刺箭头
        float arrowLength = Mathf.Min(distance * 10, rect.width - 30);
        Vector2 endPos = new Vector2(startPos.x + arrowLength, startPos.y);
        
        Handles.color = new Color(0.3f, 0.7f, 1f);
        Handles.DrawLine(startPos, endPos);
        
        // 箭头头部
        Vector2 arrowTip = endPos;
        Vector2 arrowLeft = new Vector2(endPos.x - 8, endPos.y - 5);
        Vector2 arrowRight = new Vector2(endPos.x - 8, endPos.y + 5);
        Handles.DrawAAConvexPolygon(
            new Vector3(arrowTip.x, arrowTip.y, 0),
            new Vector3(arrowLeft.x, arrowLeft.y, 0),
            new Vector3(arrowRight.x, arrowRight.y, 0)
        );

        // 距离标签
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        
        Rect labelRect = new Rect(startPos.x, startPos.y + 10, arrowLength, 15);
        GUI.Label(labelRect, $"{distance:F2}m", labelStyle);
    }

    /// <summary>
    /// 绘制蹬墙跳方向可视化
    /// </summary>
    private void DrawWallJumpVisualization(Rect rect, Vector2 force)
    {
        // 背景
        EditorGUI.DrawRect(rect, CustomEditorStyles.Colors.BackgroundDark);

        // 墙壁
        Rect wallRect = new Rect(rect.x + 10, rect.y, 5, rect.height);
        EditorGUI.DrawRect(wallRect, new Color(0.5f, 0.5f, 0.5f));

        // 玩家位置
        Vector2 playerPos = new Vector2(rect.x + 20, rect.y + rect.height - 15);
        Handles.color = Color.cyan;
        Handles.DrawSolidDisc(new Vector3(playerPos.x, playerPos.y, 0), Vector3.forward, 5);

        // 跳跃力度箭头
        float scale = 3f; // 缩放因子
        Vector2 forceVec = new Vector2(force.x * scale, -force.y * scale);
        Vector2 endPos = playerPos + forceVec;

        // 水平分量（红色）
        Handles.color = new Color(1f, 0.3f, 0.3f, 0.7f);
        Handles.DrawLine(playerPos, new Vector2(endPos.x, playerPos.y));

        // 垂直分量（绿色）
        Handles.color = new Color(0.3f, 1f, 0.3f, 0.7f);
        Handles.DrawLine(new Vector2(endPos.x, playerPos.y), endPos);

        // 合力箭头（黄色）
        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(3f, playerPos, endPos);
        
        // 箭头头部
        Vector2 direction = (endPos - playerPos).normalized;
        Vector2 arrowTip = endPos;
        Vector2 arrowLeft = endPos - new Vector2(direction.x - direction.y, direction.y + direction.x) * 8;
        Vector2 arrowRight = endPos - new Vector2(direction.x + direction.y, direction.y - direction.x) * 8;
        Handles.DrawAAConvexPolygon(
            new Vector3(arrowTip.x, arrowTip.y, 0),
            new Vector3(arrowLeft.x, arrowLeft.y, 0),
            new Vector3(arrowRight.x, arrowRight.y, 0)
        );

        // 标签
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(rect.x + 30, rect.y + 5, 100, 15), $"X: {force.x:F1}", labelStyle);
        GUI.Label(new Rect(rect.x + 30, rect.y + 20, 100, 15), $"Y: {force.y:F1}", labelStyle);
    }
}
#endif
