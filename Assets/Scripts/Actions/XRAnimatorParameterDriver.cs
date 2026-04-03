using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRAnimatorParameterDriver : MonoBehaviour
{
    [Header("Animator Target")]
    public Animator animator;

    [Header("Animator Parameter")]
    public string parameterName;

    [Header("XR Input Settings")]
    public string actionMapName;
    public string actionName;

    [Header("Grab Settings")]
    [Tooltip("If enabled, the animation only updates while the object is held.")]
    public bool isGrabbable = false;

    private XRIAlchemyLabInputActions actions;
    private XRBaseInteractable interactable;
    private bool isHeld = false;

    private AnimatorControllerParameterType parameterType;

    private void Awake()
    {
        actions = new XRIAlchemyLabInputActions();

        if (isGrabbable)
            interactable = GetComponent<XRBaseInteractable>();

        ResolveParameterType();
    }

    private void ResolveParameterType()
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
            return;

        foreach (var p in animator.parameters)
        {
            if (p.name == parameterName)
            {
                parameterType = p.type;
                return;
            }
        }

        Debug.LogWarning($"Parameter '{parameterName}' not found in the Animator.");
    }

    private void OnEnable()
    {
        actions.asset.Enable();

        if (isGrabbable && interactable != null)
        {
            interactable.selectEntered.AddListener(OnGrab);
            interactable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        actions.asset.Disable();

        if (isGrabbable && interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGrab);
            interactable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args) => isHeld = true;
    private void OnRelease(SelectExitEventArgs args) => isHeld = false;

    private void Update()
    {
        if (animator == null)
            return;

        // If the object is grabbable but not currently held → do nothing
        if (isGrabbable && !isHeld)
            return;

        InputAction action = GetAction();
        if (action == null)
            return;

        float value = ReadValue(action);

        switch (parameterType)
        {
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(parameterName, value);
                break;

            case AnimatorControllerParameterType.Bool:
                animator.SetBool(parameterName, value > 0.5f);
                break;

            case AnimatorControllerParameterType.Trigger:
                if (value > 0.5f)
                    animator.SetTrigger(parameterName);
                break;

            case AnimatorControllerParameterType.Int:
                animator.SetInteger(parameterName, Mathf.RoundToInt(value));
                break;
        }
    }

    private InputAction GetAction()
    {
        if (string.IsNullOrEmpty(actionMapName) || string.IsNullOrEmpty(actionName))
            return null;

        return actions.asset.FindAction($"{actionMapName}/{actionName}", false);
    }

    private float ReadValue(InputAction action)
    {
        if (action.expectedControlType == "Vector2")
            return action.ReadValue<Vector2>().magnitude;

        return action.ReadValue<float>();
    }
}
