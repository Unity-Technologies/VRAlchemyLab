using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DebugInteractorState : MonoBehaviour
{
    XRDirectInteractor interactor;

    void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    void Update()
    {
        if (interactor.hasSelection)
            Debug.Log(">>> RIGHT HAND: STILL HAS SELECTION");
    }
}
