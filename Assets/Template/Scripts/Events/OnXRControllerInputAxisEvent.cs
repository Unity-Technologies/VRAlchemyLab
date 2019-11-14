using GameplayIngredients;
using GameplayIngredients.Events;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class OnXRControllerInputAxisEvent : EventBase
{
    public enum InputXRNode
    {
        LeftHand = 4,
        RightHand = 5,
    }

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

    public enum Stick
    {
        Primary,
        Secondary,
    }

    [SerializeField]
    InputXRNode hand = InputXRNode.RightHand;

    [SerializeField]
    Stick stick = Stick.Primary;

    [SerializeField]
    Direction direction;

    // Cached
    InputDevice m_Device;
    bool m_State;

    [ReorderableList]
    public Callable[] OnDirectionEnter;
    [ReorderableList]
    public Callable[] OnDirectionLeave;

    void Start()
    {
        m_Device = InputDevices.GetDeviceAtXRNode((XRNode)hand);
        m_State = GetDirectionInput(m_Device, direction);
    }

    private void Update()
    {
        bool previous = m_State;
        m_State = GetDirectionInput(m_Device, direction);

        if (previous == false && m_State == true)
            Callable.Call(OnDirectionEnter);
        else if (previous == true && m_State == false)
            Callable.Call(OnDirectionLeave);
    }

    bool GetDirectionInput(InputDevice device, Direction direction)
    {
        InputFeatureUsage<Vector2> feature;
        switch (stick)
        {
            default:
            case Stick.Primary:
                feature = CommonUsages.primary2DAxis;
                break;
            case Stick.Secondary:
                feature = CommonUsages.secondary2DAxis;
                break;
        }
        Vector2 value = GetValue(device, feature);

        int idx = 0;
        if (value.x > 0) idx += 1;
        if (value.x < 0) idx -= 1;
        if (value.y > 0) idx += 10;
        if (value.y < 0) idx -= 10;
        Direction d = (Direction)idx;

        return d == direction;
    }

    Vector2 GetValue(InputDevice device, InputFeatureUsage<Vector2> usage)
    {
        Vector2 value = Vector2.zero;
        if (device.TryGetFeatureValue(usage, out value))
        {
            value.x = Mathf.Round(value.x);
            value.y = Mathf.Round(value.y);
            return value;
        }
        else
            return Vector2.zero;

    }
}
