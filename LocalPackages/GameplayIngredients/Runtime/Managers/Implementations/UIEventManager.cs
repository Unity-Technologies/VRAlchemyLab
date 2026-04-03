using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // <-- Required for InputSystemUIInputModule

namespace GameplayIngredients
{
	// Require the new Input System module instead of StandaloneInputModule
	[RequireComponent(typeof(EventSystem))]
	[RequireComponent(typeof(InputSystemUIInputModule))]
	[ManagerDefaultPrefab("UIEventManager")]
	public class UIEventManager : Manager
	{
		public EventSystem eventSystem { get { return m_EventSystem; } }

		[SerializeField]
		private EventSystem m_EventSystem;

		private void OnEnable()
		{
			// Cache the EventSystem component
			m_EventSystem = GetComponent<EventSystem>();
		}
	}
}



