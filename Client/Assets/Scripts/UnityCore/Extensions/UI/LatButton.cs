using System.Collections;
using UnityCore.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityCore.Extensions.UI
{
	[AddComponentMenu("UI/LatButton")]
	[RequireComponent(typeof(AudioButton))]
	public class LatButton : Button
	{
		/// <summary>
		/// 按钮交互事件枚举
		/// </summary>
		[System.Flags]
		public enum LatButtonEventType
		{
			Click = 1 << 0,
			LongClick = 1 << 1,
			Down = 1 << 2,
			Up = 1 << 3,
			Enter = 1 << 4,
			Exit = 1 << 5,
			DoubleClick = 1 << 6,
		}
		[SerializeField] private LatButtonEventType m_EventType = LatButtonEventType.Click;

		/// <summary>
		/// 长按判定时间
		/// </summary>
		[SerializeField] private float onLongWaitTime = 1.5f;

		/// <summary>
		/// 是否重复抛出长按事件（false：长按onLongWaitTime后只触发一次onLongClick  true：从onDown起，每onLongWaitTime触发一次onLongClick）
		/// </summary>
		[SerializeField] private bool onLongContinue = false;

		/// <summary>
		/// 双击判定时间（两次OnDown的间隔时间小于此值即判定为一次双击，但完全不影响onClick的触发）
		/// </summary>
		[SerializeField] private float onDoubleClickTime = 0.5f;

		[SerializeField] private ButtonClickedEvent m_OnLongClick = new ButtonClickedEvent(); //长按事件（触发一次）

		[SerializeField] private ButtonClickedEvent m_OnDown = new ButtonClickedEvent(); //按下事件

		[SerializeField] private ButtonClickedEvent m_OnUp = new ButtonClickedEvent(); //抬起事件

		[SerializeField] private ButtonClickedEvent m_OnEnter = new ButtonClickedEvent(); //进入事件

		[SerializeField] private ButtonClickedEvent m_OnExit = new ButtonClickedEvent(); //移出事件

		[SerializeField] private ButtonClickedEvent m_onDoubleClick = new ButtonClickedEvent(); //双击事件


		private Coroutine log;

		private bool isPointerDown = false;
		private bool isPointerInside = false;


		#region 对外属性

		/// <summary>
		/// 是否被按下
		/// </summary>
		public bool IsDown => isPointerDown;

		/// <summary>
		/// 是否进入
		/// </summary>
		public bool IsEnter => isPointerInside;

		/// <summary>
		/// 长按事件
		/// </summary>
		public ButtonClickedEvent OnLongClick
		{
			get => m_OnLongClick;
			set => m_OnLongClick = value;
		}

		/// <summary>
		/// 双击事件
		/// </summary>
		public ButtonClickedEvent OnDoubleClick
		{
			get => m_onDoubleClick;
			set => m_onDoubleClick = value;
		}

		/// <summary>
		/// 按下事件
		/// </summary>
		public ButtonClickedEvent OnDown
		{
			get => m_OnDown;
			set => m_OnDown = value;
		}

		/// <summary>
		/// 松开事件
		/// </summary>
		public ButtonClickedEvent OnUp
		{
			get => m_OnUp;
			set => m_OnUp = value;
		}

		/// <summary>
		/// 进入事件
		/// </summary>
		public ButtonClickedEvent OnEnter
		{
			get => m_OnEnter;
			set => m_OnEnter = value;
		}

		/// <summary>
		/// 离开事件
		/// </summary>
		public ButtonClickedEvent OnExit
		{
			get => m_OnExit;
			set => m_OnExit = value;
		}

		#endregion


		private float _lastClickTime;
		private void Down()
		{
			if (!IsActive() || !IsInteractable())
				return;
			m_OnDown.Invoke();
			if (_lastClickTime != 0 && Time.time - _lastClickTime <= onDoubleClickTime)
			{
				DoubleClick();
			}

			_lastClickTime = Time.time;
			log = StartCoroutine(Grow());
		}

		private void Up()
		{
			if (!IsActive() || !IsInteractable() || !IsDown)
				return;
			m_OnUp.Invoke();
			if (log != null)
			{
				StopCoroutine(log);
				log = null;
			}
		}

		private void Enter()
		{
			if (!IsActive())
				return;
			m_OnEnter.Invoke();
		}

		private void Exit()
		{
			if (!IsActive() || !IsEnter)
				return;
			m_OnExit.Invoke();
		}

		private void LongClick()
		{
			if (!IsActive() || !IsDown)
				return;
			m_OnLongClick.Invoke();
		}

		private void DoubleClick()
		{
			if (!IsActive() || !IsDown)
				return;
			m_onDoubleClick.Invoke();
		}

		private float _downTime = 0f;
		private IEnumerator Grow()
		{
			_downTime = Time.time;
			while (IsDown)
			{
				if (Time.time - _downTime > onLongWaitTime)
				{
					LongClick();
					if (onLongContinue)
						_downTime = Time.time;
					else
						break;
				}
				else
					yield return null;
			}
		}

		protected override void OnDisable()
		{
			isPointerDown = false;
			isPointerInside = false;
			base.OnDisable();
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			isPointerDown = true;
			Down();
			base.OnPointerDown(eventData);
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			Up();
			isPointerDown = false;
			base.OnPointerUp(eventData);
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			isPointerInside = true;
			Enter();
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			Exit();
			isPointerInside = false;
			base.OnPointerExit(eventData);
		}

		#region Button->ButtonEx转换相关

#if UNITY_EDITOR

		[MenuItem("CONTEXT/Button/Convert To ButtonEx", true)]
		static bool _ConvertToButtonEx(MenuCommand command)
		{
			return CanConvertTo<LatButton>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To ButtonEx", false)]
		static void ConvertToButtonEx(MenuCommand command)
		{
			ConvertTo<LatButton>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To Button", true)]
		static bool _ConvertToButton(MenuCommand command)
		{
			return CanConvertTo<Button>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To Button", false)]
		static void ConvertToButton(MenuCommand command)
		{
			ConvertTo<Button>(command.context);
		}

		protected static bool CanConvertTo<T>(Object context)
			where T : MonoBehaviour
		{
			return context && context.GetType() != typeof(T);
		}

		protected static void ConvertTo<T>(Object context) where T : MonoBehaviour
		{
			var target = context as MonoBehaviour;
			var so = new SerializedObject(target);
			so.Update();

			bool oldEnable = target.enabled;
			target.enabled = false;

			// Find MonoScript of the specified component.
			foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
			{
				if (script.GetClass() != typeof(T))
					continue;

				// Set 'm_Script' to convert.
				so.FindProperty("m_Script").objectReferenceValue = script;
				so.ApplyModifiedProperties();
				break;
			}

			(so.targetObject as MonoBehaviour).enabled = oldEnable;
		}
#endif

		#endregion

	}
}