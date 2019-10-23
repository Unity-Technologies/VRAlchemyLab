using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The snap turn provider is a locomotion provider that allows the user to rotate their rig using a specified 2d axis input.
    /// the provider can take input from two different devices (eg: L & R hands). 
    /// </summary>
    public sealed class SnapTurnProvider : LocomotionProvider
    {
        /// <summary>
        /// This is the list of possible valid "InputAxes" that we allow users to read from.
        /// </summary>
        public enum InputAxes
        {
            Primary2DAxis = 0,
            Secondary2DAxis = 1,
            DPad = 2,           
        };

        // Mapping of the above InputAxes to actual common usage values
        static readonly InputFeatureUsage<Vector2>[] m_Vec2UsageList = new InputFeatureUsage<Vector2>[] {
            CommonUsages.primary2DAxis,
            CommonUsages.secondary2DAxis,            
        };

        [SerializeField]
        [Tooltip("If this is enabled, we will attempt to read data from the selected primary device.")]
        bool m_EnablePrimaryDevice;
        /// <summary>
        /// If this is enabled, we will attempt to read data from the selected primary device.
        /// </summary>
        public bool enablePrimaryDevice {  get { return m_EnablePrimaryDevice; } set { m_EnablePrimaryDevice = value; } }

        [SerializeField]
        [Tooltip("The device role that will be used to locate the 'Primary' controller device to use for snap turning.")]
        XRNode m_PrimaryDeviceNode;
        /// <summary>
        /// The device node that will be used to locate the "Primary" controller device to use for snap turning.
        /// </summary>
        public XRNode PrimaryDeviceNode { get { return m_PrimaryDeviceNode; } set { m_PrimaryDeviceNode = value; TryResolveDevice(m_PrimaryDeviceNode, out m_CurrentPrimaryDevice); } }

        [SerializeField]
        [Tooltip("The 2D Input Axis on the primary device that will be used to trigger a snap turn.")]
        InputAxes m_PrimaryDeviceTurnAxis = InputAxes.Primary2DAxis;
        /// <summary>
        /// The 2D Input Axis on the primary device that will be used to trigger a snap turn.
        /// </summary>
        public InputAxes PrimaryDeviceTurnAxis { get { return m_PrimaryDeviceTurnAxis; } set { m_PrimaryDeviceTurnAxis = value; } }

        [SerializeField]
        [Tooltip("If this is enabled, we will attempt to read data from the selected secondary device.")]
        bool m_EnableSecondaryDevice;
        /// <summary>
        /// If this is enabled, we will attempt to read data from the selected secondary device.
        /// </summary>
        public bool enableSecondaryDevice { get { return m_EnableSecondaryDevice; } set { m_EnableSecondaryDevice = value; } }

        [SerializeField]
        [Tooltip("The device role that will be used to locate the 'Secondary' controller device to use for snap turning.")]
        XRNode m_SecondaryDeviceNode;
        /// <summary>
        /// The device node that will be used to locate the "Secondary" controller device to use for snap turning.
        /// </summary>
        public XRNode SecondaryDeviceNode { get { return m_SecondaryDeviceNode; } set { m_SecondaryDeviceNode = value; TryResolveDevice(m_SecondaryDeviceNode, out m_CurrentSecondaryDevice); } }
        
        [SerializeField]
        [Tooltip("The 2D Input Axis on the secondary device that will be used to trigger a snap turn.")]
        InputAxes m_SecondaryDeviceTurnAxis = InputAxes.Primary2DAxis;
        /// <summary>
        /// The 2D Input Axis on the secondary device that will be used to trigger a snap turn.
        /// </summary>
        public InputAxes SecondaryDeviceTurnAxis { get { return m_SecondaryDeviceTurnAxis; } set { m_SecondaryDeviceTurnAxis = value; } }

        [SerializeField]
        [Tooltip("The number of degrees clockwise to rotate when snap turning clockwise.")]
        float m_TurnAmount = 45.0f;
        /// <summary>
        /// The number of degrees clockwise to rotate when snap turning clockwise.
        /// </summary>
        public float turnAmount {  get { return m_TurnAmount; } set { m_TurnAmount = value; } }

        [SerializeField]
        [Tooltip("The amount of time that the system will wait before starting another snap turn.")]
        float m_DebounceTime = 0.5f;
        /// <summary>
        /// The amount of time that the system will wait before starting another snap turn.
        /// </summary>
        public float debounceTime { get { return m_DebounceTime; } set { m_DebounceTime = value; } }

        [SerializeField]
        [Tooltip("The deadzone that the controller movement will have to be above to trigger a snap turn.")]
        float m_DeadZone = 0.15f;
        /// <summary>
        /// The deadzone that the controller movement will have to be above to trigger a snap turn.
        /// </summary>
        public float deadZone {  get { return m_DeadZone;  } set { m_DeadZone = value; } }

        // state data
        float m_CurrentTurnAmount = 0.0f;
        float m_TimeStarted = 0.0f;

        InputDevice m_CurrentPrimaryDevice;
        InputDevice m_CurrentSecondaryDevice;

        protected override void Awake()
        {
            base.Awake();
            TryResolveDevice(m_PrimaryDeviceNode, out m_CurrentPrimaryDevice);
            TryResolveDevice(m_SecondaryDeviceNode, out m_CurrentSecondaryDevice);
        }

        private void Update()
        {           
            if(!m_CurrentPrimaryDevice.isValid)
            {
                TryResolveDevice(m_PrimaryDeviceNode, out m_CurrentPrimaryDevice);
            }
            if(!m_CurrentSecondaryDevice.isValid)
            {
                TryResolveDevice(m_SecondaryDeviceNode, out m_CurrentSecondaryDevice);
            }

            // wait for a certain amount of time before allowing another turn.
            if (m_TimeStarted > 0.0f && (m_TimeStarted + m_DebounceTime < Time.time))
            {
                m_TimeStarted = 0.0f;
                return;
            }
           
            // if idle, check other input
            if (m_EnablePrimaryDevice)
                InspectInput(m_Vec2UsageList[(int)m_PrimaryDeviceTurnAxis], m_CurrentPrimaryDevice);

            if (m_EnableSecondaryDevice)
                InspectInput(m_Vec2UsageList[(int)m_SecondaryDeviceTurnAxis], m_CurrentSecondaryDevice);
        

            if (Math.Abs(m_CurrentTurnAmount) > 0.0f && BeginLocomotion())
            {
                var xrRig = system.xrRig;
                if (xrRig != null)
                {
                    xrRig.RotateAroundCameraUsingRigUp(m_CurrentTurnAmount);
                }
                m_CurrentTurnAmount = 0.0f;
                EndLocomotion();
            }
        }
        


        private bool TryResolveDevice(XRNode role, out InputDevice device)
        {
            
            var inputDevice = InputDevices.GetDeviceAtXRNode(role);
            if(inputDevice.isValid)
            {
                device = inputDevice;
                return true;
            }
            device = new InputDevice();
            return false;
        }

        private void InspectInput(InputFeatureUsage<Vector2> feature, InputDevice device)
        {
            if (device != null)
            {
                Vector2 currentState;
                if (device.TryGetFeatureValue(feature, out currentState))
                {
                    if (currentState.x > deadZone)
                    {
                        StartTurn(m_TurnAmount);
                    }
                    else if (currentState.x < -deadZone)
                    {
                        StartTurn(-m_TurnAmount);
                    }
                }
            }        
        }

        internal void FakeStartTurn(bool isLeft)
        {
            StartTurn(isLeft ? -m_TurnAmount : m_TurnAmount);
        }

        private void StartTurn(float amount)
        {
            if (m_TimeStarted != 0.0f) 
                return;

            if (!CanBeginLocomotion())
                return;
           
            m_TimeStarted = Time.time;
            m_CurrentTurnAmount = amount;            
        }
    }
}
