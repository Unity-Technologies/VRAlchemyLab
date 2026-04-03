using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{
    [Header("Interactors")]
    public XRRayInteractor teleportInteractor;   // Teleport ray
    public XRRayInteractor grabInteractor;       // Normal grab ray

    [Header("Actions")]
    public InputActionProperty teleportModeAction;     // Primary Button
    public InputActionProperty grabModeAction;         // Secondary Button
    public InputActionProperty teleportActivateAction; // Trigger to aim/confirm teleport

    [Header("Scene Objects Visibility")]
    public GameObject[] teleportModeObjects; // Objects shown in teleport mode
    public GameObject[] grabModeObjects;     // Objects shown in grab mode

    private bool isTeleportMode = false;

    void Start()
    {
        SetTeleportMode();

        teleportModeAction.action.performed += OnTeleportModeSelected;
        grabModeAction.action.performed += OnGrabModeSelected;

        teleportActivateAction.action.performed += OnTeleportActivated;
    }

    private void OnTeleportModeSelected(InputAction.CallbackContext ctx)
    {
        SetTeleportMode();
    }

    private void OnGrabModeSelected(InputAction.CallbackContext ctx)
    {
        SetGrabMode();
    }

    private void OnTeleportActivated(InputAction.CallbackContext ctx)
    {
        // Teleport should only activate when in teleport mode
        if (!isTeleportMode)
            return;

        // Teleport ray is already visible; here you only trigger the teleport logic
        teleportInteractor.gameObject.SetActive(true);
    }

    void Update()
    {
        // When the teleport activation button is released, you can hide the ray if desired
        if (isTeleportMode && teleportActivateAction.action.WasReleasedThisFrame())
        {
            // Keep the ray visible in teleport mode
            teleportInteractor.gameObject.SetActive(true);
        }
    }

    private void SetTeleportMode()
    {
        isTeleportMode = true;

        // Interactors
        grabInteractor.gameObject.SetActive(false);

        // Teleport ray should be visible as soon as teleport mode is selected
        teleportInteractor.gameObject.SetActive(true);

        // Scene objects
        SetObjectsActive(teleportModeObjects, true);
        SetObjectsActive(grabModeObjects, false);
    }

    private void SetGrabMode()
    {
        isTeleportMode = false;

        // Interactors
        grabInteractor.gameObject.SetActive(true);
        teleportInteractor.gameObject.SetActive(false);

        // Scene objects
        SetObjectsActive(teleportModeObjects, false);
        SetObjectsActive(grabModeObjects, true);
    }

    private void SetObjectsActive(GameObject[] list, bool state)
    {
        if (list == null) return;

        foreach (var go in list)
        {
            if (go != null)
                go.SetActive(state);
        }
    }
}
