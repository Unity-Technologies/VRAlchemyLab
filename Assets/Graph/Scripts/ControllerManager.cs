using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerManager : MonoBehaviour
{
    InputDevice m_RightController;
    InputDevice m_LeftController;

    bool m_LeftPrimaryButtonPressed;
    bool m_LeftSecondaryButtonPressed;
    //bool m_LeftThumbstickPressed;

    bool m_RightTouchPadClicked;
    bool m_RightPrimaryButtonClicked;

    [SerializeField]
    GameObject m_LeftBaseController;

    [SerializeField]
    GameObject m_LeftTeleportController;

    [SerializeField]
    GameObject m_RightBaseController;

    [SerializeField]
    GameObject m_RightTeleportController;


    struct InteractorController
    {
        public GameObject m_GO;
        public XRController m_XRController;
        public XRInteractorLineVisual m_LineRenderer;
        public XRBaseInteractor m_Interactor;

        public void Attach(GameObject gameObject)
        {
            m_GO = gameObject;
            if (m_GO != null)
            {
                m_XRController = m_GO.GetComponent<XRController>();
                m_LineRenderer = m_GO.GetComponent<XRInteractorLineVisual>();
                m_Interactor = m_GO.GetComponent<XRBaseInteractor>();

                Leave();               
            }
        }

        public void Enter()
        {
            if (m_LineRenderer)
            {
                m_LineRenderer.enabled = true;
            }
            if (m_XRController)
            {
                m_XRController.enableInputActions = true;
            }
            if (m_Interactor)
            {
                m_Interactor.enabled = true;
            }
        }

        public void Leave()
        {
            if (m_LineRenderer)
            {
                m_LineRenderer.enabled = false;
            }
            if (m_XRController)
            {
                m_XRController.enableInputActions = false;
            }
            if(m_Interactor)
            {
                m_Interactor.enabled = false;
            }
        }
    }

    enum ControllerStates
    {
        Select = 0,
        Teleport = 1,        
        MAX = 2,
    }

    struct ControllerState
    {
        ControllerStates m_State;
        InteractorController[] m_Interactors;

        public void Initalize()
        {
            m_State = ControllerStates.MAX;
            m_Interactors = new InteractorController[(int)ControllerStates.MAX];
        }

        public void ClearAll()
        {
            for(int i = 0; i < (int)ControllerStates.MAX; ++i)
            {
                m_Interactors[i].Leave();
            }
        }

        public void SetGameObject(ControllerStates state, GameObject parentGamObject)
        {
            if (state == ControllerStates.MAX)
                return;

            m_Interactors[(int)state].Attach(parentGamObject);
        }

        public void SetState(ControllerStates nextState)
        {
            if (nextState == m_State || nextState == ControllerStates.MAX)
            {
                return;
            }
            else
            {
                if (m_State != ControllerStates.MAX)
                {
                    m_Interactors[(int)m_State].Leave();                    
                }

                m_State = nextState;           
                m_Interactors[(int)m_State].Enter();                
            }
        }
    }

    ControllerState m_RightControllerState;
    ControllerState m_LeftControllerState;


    void OnEnable()
    {
        InputDevices.deviceConnected += RegisterDevices;

        m_RightControllerState.Initalize();
        m_LeftControllerState.Initalize();
      
        m_RightControllerState.SetGameObject(ControllerStates.Select, m_RightBaseController);
        m_RightControllerState.SetGameObject(ControllerStates.Teleport, m_RightTeleportController);

        m_LeftControllerState.SetGameObject(ControllerStates.Select, m_LeftBaseController);
        m_LeftControllerState.SetGameObject(ControllerStates.Teleport, m_LeftTeleportController);

        m_LeftControllerState.ClearAll();
        m_RightControllerState.ClearAll();
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= RegisterDevices;
    }

    void RegisterDevices(InputDevice connectedDevice)
    {
        if (connectedDevice.isValid)
        {
            if (connectedDevice.role == InputDeviceRole.LeftHanded)
            {
                m_LeftController = connectedDevice;
                m_LeftControllerState.ClearAll();
                m_LeftControllerState.SetState(ControllerStates.Teleport);
            }
            else if (connectedDevice.role == InputDeviceRole.RightHanded)
            {
                m_RightController = connectedDevice;          
                m_RightControllerState.ClearAll();
                m_RightControllerState.SetState(ControllerStates.Select);                                
            }
        }
    }

    void Update()
    {
        if (m_LeftController.isValid)
        {           
            
            m_LeftController.TryGetFeatureValue(CommonUsages.primaryButton, out m_LeftPrimaryButtonPressed);
            m_LeftController.TryGetFeatureValue(CommonUsages.secondaryButton, out m_LeftSecondaryButtonPressed);

            //m_LeftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_LeftThumbstickPressed);

            if (m_LeftPrimaryButtonPressed)
            {
                m_LeftControllerState.SetState(ControllerStates.Teleport);
            }
            if (m_LeftSecondaryButtonPressed)
            {
                m_LeftControllerState.SetState(ControllerStates.Select);
            }
            /*if (m_LeftThumbstickPressed)
            {
                m_LeftControllerState.ClearAll();
            }*/
        }

        if (m_RightController.isValid)
        {        
            m_RightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_RightTouchPadClicked);
            m_RightController.TryGetFeatureValue(CommonUsages.primaryButton, out m_RightPrimaryButtonClicked);


            if (m_RightTouchPadClicked || m_RightPrimaryButtonClicked)
            {
                m_RightControllerState.SetState(ControllerStates.Teleport);
            }
            else
            {
                m_RightControllerState.SetState(ControllerStates.Select);
            }
        }
    }
}
