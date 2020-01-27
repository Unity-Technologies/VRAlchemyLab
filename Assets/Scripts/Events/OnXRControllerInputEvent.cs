using GameplayIngredients;
using GameplayIngredients.Events;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class OnXRControllerInputEvent : EventBase
{
    public enum InputXRNode
    {
        LeftHand = 4,
        RightHand = 5,
    }

    public enum XRButton
    {
        PrimaryButton,
        PrimaryTouch,
        SecondaryButton,
        SecondaryTouch,
        GripButton,
        TriggerButton,
        MenuButton,
        Primary2DAxisClick,
        Primary2DAxisTouch
    }
    [SerializeField]
    InputXRNode Hand = InputXRNode.RightHand;

    [SerializeField]
    XRButton Button;

    // Cached
    InputDevice m_Device;
    bool m_State;

    [ReorderableList]
    public Callable[] OnButtonDown;
    [ReorderableList]
    public Callable[] OnButtonUp;

    void Start()
    {
        m_Device = InputDevices.GetDeviceAtXRNode((XRNode)Hand);
        m_State = GetButtonInput(m_Device, Button);
    }

    private void Update()
    {
        bool previous = m_State;
        m_State = GetButtonInput(m_Device, Button);

        if (previous == false && m_State == true)
            Callable.Call(OnButtonDown);
        else if (previous == true && m_State == false)
            Callable.Call(OnButtonUp);
    }


    bool GetButtonInput(InputDevice device, XRButton button)
    {
        switch (button)
        {
            case XRButton.PrimaryButton:
                return GetValue(device, CommonUsages.primaryButton);
            case XRButton.PrimaryTouch:
                return GetValue(device, CommonUsages.primaryTouch);
            case XRButton.SecondaryButton:
                return GetValue(device, CommonUsages.secondaryButton);
            case XRButton.SecondaryTouch:
                return GetValue(device, CommonUsages.secondaryTouch);
            case XRButton.GripButton:
                return GetValue(device, CommonUsages.gripButton);
            case XRButton.TriggerButton:
                return GetValue(device, CommonUsages.triggerButton);
            case XRButton.MenuButton:
                return GetValue(device, CommonUsages.menuButton);
            case XRButton.Primary2DAxisClick:
                return GetValue(device, CommonUsages.primary2DAxisClick);
            case XRButton.Primary2DAxisTouch:
                return GetValue(device, CommonUsages.primary2DAxisTouch);
            default:
                return false;
        }
    }

    bool GetValue(InputDevice device, InputFeatureUsage<bool> usage)
    {
        bool value = false;
        if (device.TryGetFeatureValue(usage, out value))
            return value;
        else
            return false;
            
    }
}
