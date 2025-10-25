using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections.Generic;
using UnityCore.Extensions.UI;
using static UnityCore.Extensions.UI.LatButton;

namespace MyProject.Editor
{
	[CustomEditor(typeof(LatButton), true)]
	public class ButtonExEditor : SelectableEditor
	{
		/// <summary>
		/// 按钮交互事件绘制方法的字典
		/// </summary>
		Dictionary<LatButtonEventType, Action> callbackDrawersDic;


		private SerializedProperty spType;
		/// <summary>
		/// 按钮交互事件属性的字典
		/// </summary>
		Dictionary<LatButtonEventType, SerializedProperty> eventPropertiesDic;

		private SerializedProperty sponLongWaitTime;

		private SerializedProperty sponLongContinue;

		private SerializedProperty sponDoubleClickTime;


		protected override void OnEnable()
		{
			base.OnEnable();

			//初始化各属性
			InitProperties();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();

			GUILayout.Space(15);

			// 绘制按钮相关参数
			sponLongWaitTime.floatValue = EditorGUILayout.FloatField("长按判定时间", sponLongWaitTime.floatValue);
			sponLongContinue.boolValue = EditorGUILayout.Toggle("长按期间是否持续触发", sponLongContinue.boolValue);
			sponDoubleClickTime.floatValue = EditorGUILayout.FloatField("双击间隔判定时间", sponDoubleClickTime.floatValue);

			// 绘制按钮交互事件栏
			GUILayout.Space(10);
			int oldType = spType.intValue;
			spType.intValue = (int) ((LatButtonEventType) EditorGUILayout.EnumFlagsField(new GUIContent("按钮交互事件"), (LatButtonEventType) spType.intValue));
			foreach (LatButtonEventType e in Enum.GetValues(typeof(LatButtonEventType)))
			{
				if (0 < (spType.intValue & (int) e))
					callbackDrawersDic[e]();
				else if (0 < (oldType & (int) e))
					eventPropertiesDic[e].FindPropertyRelative("m_PersistentCalls").FindPropertyRelative("m_Calls").ClearArray();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void InitProperties()
		{
			eventPropertiesDic = new Dictionary<LatButtonEventType, SerializedProperty>()
			{
				{LatButtonEventType.Click, serializedObject.FindProperty("m_OnClick")},
				{LatButtonEventType.LongClick, serializedObject.FindProperty("m_OnLongClick")},
				{LatButtonEventType.Down, serializedObject.FindProperty("m_OnDown")},
				{LatButtonEventType.Up, serializedObject.FindProperty("m_OnUp")},
				{LatButtonEventType.Enter, serializedObject.FindProperty("m_OnEnter")},
				{LatButtonEventType.Exit, serializedObject.FindProperty("m_OnExit")},
				{LatButtonEventType.DoubleClick, serializedObject.FindProperty("m_onDoubleClick")},
			};

			callbackDrawersDic = new Dictionary<LatButtonEventType, Action>();
			foreach (LatButtonEventType eventType in Enum.GetValues(typeof(LatButtonEventType)))
			{
				callbackDrawersDic.Add(eventType, () =>
				{
					EditorGUILayout.LabelField(eventType.ToString(), EditorStyles.boldLabel);
					EditorGUILayout.PropertyField(eventPropertiesDic[eventType]);

					//可以根据EventType再特殊定制其他属性的显示
				});
			}

			spType = serializedObject.FindProperty("m_EventType");
			sponLongWaitTime = serializedObject.FindProperty("onLongWaitTime");
			sponLongContinue = serializedObject.FindProperty("onLongContinue");
			sponDoubleClickTime = serializedObject.FindProperty("onDoubleClickTime");
		}
	}
}