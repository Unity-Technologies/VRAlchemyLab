using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[DefaultExecutionOrder(kControllerManagerUpdateOrder)]
public class ControllerManager : MonoBehaviour
{
    // Slightly after the default, so that any actions such as release or grab 
    // can be processed *before* we switch controllers.
    public const int kControllerManagerUpdateOrder = 10;

    InputDevice m_RightController;
    InputDevice m_LeftController;

    [SerializeField]
    [Tooltip("Button readers that will trigger a transition to the Teleport Controller.")]
    List<XRInputDeviceButtonReader> m_ActivationButtons = new List<XRInputDeviceButtonReader>();
    public List<XRInputDeviceButtonReader> activationButtons { get => m_ActivationButtons; set => m_ActivationButtons = value; }

    [SerializeField]
    [Tooltip("Button readers that will force a deactivation of the teleport option.")]
    List<XRInputDeviceButtonReader> m_DeactivationButtons = new List<XRInputDeviceButtonReader>();
    public List<XRInputDeviceButtonReader> deactivationButtons { get => m_DeactivationButtons; set => m_DeactivationButtons = value; }

    [SerializeField]
    [Tooltip("The GameObject representing the left hand for normal interaction.")]
    GameObject m_LeftBaseController;
    public GameObject leftBaseController { get => m_LeftBaseController; set => m_LeftBaseController = value; }

    [SerializeField]
    [Tooltip("The GameObject representing the left hand when teleporting.")]
    GameObject m_LeftTeleportController;
    public GameObject leftTeleportController { get => m_LeftTeleportController; set => m_LeftTeleportController = value; }

    [SerializeField]
    [Tooltip("The GameObject representing the right hand for normal interaction.")]
    GameObject m_RightBaseController;
    public GameObject rightBaseController { get => m_RightBaseController; set => m_RightBaseController = value; }

    [SerializeField]
    [Tooltip("The GameObject representing the right hand when teleporting.")]
    GameObject m_RightTeleportController;
    public GameObject rightTeleportController { get => m_RightTeleportController; set => m_RightTeleportController = value; }

    bool m_LeftTeleportDeactivated = false;
    bool m_RightTeleportDeactivated = false;

    struct InteractorController
    {
        public GameObject m_GO;
        public XRController m_XRController;
        public UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual m_LineRenderer;
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor m_Interactor;

        public void Attach(GameObject go)
        {
            m_GO = go;
            if (m_GO != null)
            {
                m_XRController = m_GO.GetComponent<XRController>();
                m_LineRenderer = m_GO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
                m_Interactor = m_GO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
                Leave();
            }
        }

        public void Enter()
        {
            if (m_LineRenderer) m_LineRenderer.enabled = true;
            if (m_XRController) m_XRController.enableInputActions = true;
            if (m_Interactor) m_Interactor.enabled = true;
        }

        public void Leave()
        {
            if (m_LineRenderer) m_LineRenderer.enabled = false;
            if (m_XRController) m_XRController.enableInputActions = false;
            if (m_Interactor) m_Interactor.enabled = false;
        }
    }

    public enum ControllerStates
    {
        Select = 0,
        Teleport = 1,
        MAX = 2,
    }

    struct ControllerState
    {
        ControllerStates m_State;
        InteractorController[] m_Interactors;

        public void Initialize()
        {
            m_State = ControllerStates.MAX;
            m_Interactors = new InteractorController[(int)ControllerStates.MAX];
        }

        public void ClearAll()
        {
            if (m_Interactors == null) return;
            for (int i = 0; i < (int)ControllerStates.MAX; ++i)
                m_Interactors[i].Leave();
        }

        public void SetGameObject(ControllerStates state, GameObject go)
        {
            if (state == ControllerStates.MAX || m_Interactors == null) return;
            m_Interactors[(int)state].Attach(go);
        }

        public void SetState(ControllerStates nextState)
        {
            if (nextState == m_State || nextState == ControllerStates.MAX)
                return;

            if (m_State != ControllerStates.MAX)
                m_Interactors[(int)m_State].Leave();

            m_State = nextState;
            m_Interactors[(int)m_State].Enter();
        }
    }

    ControllerState m_RightControllerState;
    ControllerState m_LeftControllerState;

    void OnEnable()
    {
        m_LeftTeleportDeactivated = false;
        m_RightTeleportDeactivated = false;

        m_RightControllerState.Initialize();
        m_LeftControllerState.Initialize();

        m_RightControllerState.SetGameObject(ControllerStates.Select, m_RightBaseController);
        m_RightControllerState.SetGameObject(ControllerStates.Teleport, m_RightTeleportController);

        m_LeftControllerState.SetGameObject(ControllerStates.Select, m_LeftBaseController);
        m_LeftControllerState.SetGameObject(ControllerStates.Teleport, m_LeftTeleportController);

        m_LeftControllerState.ClearAll();
        m_RightControllerState.ClearAll();

        InputDevices.deviceConnected += RegisterDevices;

        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var d in devices)
            RegisterDevices(d);
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= RegisterDevices;
    }

    void RegisterDevices(InputDevice connectedDevice)
    {
        if (!connectedDevice.isValid)
            return;

        if ((connectedDevice.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            m_LeftController = connectedDevice;
            m_LeftControllerState.ClearAll();
            m_LeftControllerState.SetState(ControllerStates.Select);
        }
        else if ((connectedDevice.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            m_RightController = connectedDevice;
            m_RightControllerState.ClearAll();
            m_RightControllerState.SetState(ControllerStates.Select);
        }
    }

    void Update()
    {
        // LEFT HAND
        if (m_LeftController.isValid)
        {
            bool activated = false;
            foreach (var reader in m_ActivationButtons)
                activated |= reader.ReadValue() > 0.5f;

            bool deactivated = false;
            foreach (var reader in m_DeactivationButtons)
                deactivated |= reader.ReadValue() > 0.5f;

            if (deactivated)
                m_LeftTeleportDeactivated = true;

            if (activated && !m_LeftTeleportDeactivated)
            {
                m_LeftControllerState.SetState(ControllerStates.Teleport);
            }
            else
            {
                m_LeftControllerState.SetState(ControllerStates.Select);
                if (!activated)
                    m_LeftTeleportDeactivated = false;
            }
        }

        // RIGHT HAND
        if (m_RightController.isValid)
        {
            bool activated = false;
            foreach (var reader in m_ActivationButtons)
                activated |= reader.ReadValue() > 0.5f;

            bool deactivated = false;
            foreach (var reader in m_DeactivationButtons)
                deactivated |= reader.ReadValue() > 0.5f;

            if (deactivated)
                m_RightTeleportDeactivated = true;

            if (activated && !m_RightTeleportDeactivated)
            {
                m_RightControllerState.SetState(ControllerStates.Teleport);
            }
            else
            {
                m_RightControllerState.SetState(ControllerStates.Select);
                if (!activated)
                    m_RightTeleportDeactivated = false;
            }
        }
    }
}
