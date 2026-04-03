using GameplayIngredients;
using GameplayIngredients.Events;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OnXRControllerInputAxisEvent : EventBase
{
    public enum Direction
    {
        Center = 0,
        Up = 10,
        Down = -10,
        Left = -1,
        Right = 1,
        UpRight = 11,
        UpLeft = 9,
        DownRight = -9,
        DownLeft = -11,
    }

    [Header("XR")]
    [Tooltip("XR Direct Interactor that will drive the axis events (left or right hand).")]
    public XRDirectInteractor handInteractor;

    [Header("Input System")]
    [Tooltip("Input Actions asset containing XR bindings (ex: XRI Default Input Actions).")]
    public InputActionAsset inputActions;

    [Tooltip("Action Map name (ex: 'XRI Right Locomotion', 'XRI Left Interaction').")]
    public string actionMapName;

    [Tooltip("Action name providing a Vector2 stick value (ex: 'Move', 'Primary2DAxis').")]
    public string actionName;

    [Header("Direction Detection")]
    [Tooltip("Direction to detect from the Vector2 input.")]
    public Direction direction;

    [ReorderableList] public Callable[] OnDirectionEnter;
    [ReorderableList] public Callable[] OnDirectionLeave;

    [Header("Runtime Control")]
    [Tooltip("Enables or disables the internal logic without touching the Input Action Map.")]
    public bool isActive = true;

    private InputActionMap _map;
    private InputAction _action;

    private bool _state = false;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("[OnXRControllerInputAxisEvent] No InputActionAsset assigned.");
            return;
        }

        _map = inputActions.FindActionMap(actionMapName, false);
        if (_map == null)
        {
            Debug.LogError($"[OnXRControllerInputAxisEvent] Action Map '{actionMapName}' not found.");
            return;
        }

        _action = _map.FindAction(actionName, false);
        if (_action == null)
        {
            Debug.LogError($"[OnXRControllerInputAxisEvent] Action '{actionName}' not found in map '{actionMapName}'.");
            return;
        }

        // IMPORTANT: We never enable/disable the Action Map here.
        // We simply read the action every frame.
    }

    private void Update()
    {
        if (!isActive) return;
        if (_action == null) return;

        // Only trigger events if the hand is holding something
        if (handInteractor == null || !handInteractor.hasSelection)
            return;

        var selected = handInteractor.firstInteractableSelected;
        if (selected == null)
            return;

        Vector2 value = _action.ReadValue<Vector2>();
        Direction current = ComputeDirection(value);

        bool newState = (current == direction);

        if (!_state && newState)
            Callable.Call(OnDirectionEnter, selected.transform.gameObject);
        else if (_state && !newState)
            Callable.Call(OnDirectionLeave, selected.transform.gameObject);

        _state = newState;
    }

    // ---------------------------------------------------------
    // Methods callable from Select Entered / Select Exited
    // ---------------------------------------------------------
    public void EnableInput()
    {
        isActive = true;
    }

    public void DisableInput()
    {
        isActive = false;
        _state = false; // Reset state to avoid ghost transitions
    }

    // ---------------------------------------------------------
    // Direction computation
    // ---------------------------------------------------------
    private Direction ComputeDirection(Vector2 v)
    {
        // Deadzone + rounding
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);

        int idx = 0;
        if (v.x > 0) idx += 1;
        if (v.x < 0) idx -= 1;
        if (v.y > 0) idx += 10;
        if (v.y < 0) idx -= 10;

        return (Direction)idx;
    }
}
