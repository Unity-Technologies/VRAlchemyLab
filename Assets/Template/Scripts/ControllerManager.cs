using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerManager : MonoBehaviour
{
   
   List<InputDevice> inputDevice = new List<InputDevice>();
 
   InputDevice m_RightController;
   InputDevice m_LeftController;

   bool m_LeftTouchPadClicked;
   bool m_LeftPrimaryButtonClicked;
   bool m_RightTouchPadClicked;
   bool m_RightPrimaryButtonClicked;

   [SerializeField]
   GameObject m_LeftBaseController;
   
   [SerializeField]
   GameObject m_LeftTeleportController;

   [SerializeField]
   GameObject m_LeftUIController;

   [SerializeField]
   GameObject m_RightBaseController;
   
   [SerializeField]
   GameObject m_RightTeleportController;

   [SerializeField]
   GameObject m_RightUIController;


   void Start()
   {
      InputDevices.GetDevices(inputDevice);
      
      foreach (var device in inputDevice)
      {
         if (device.role == InputDeviceRole.LeftHanded)
         {
            m_LeftController = device;
         }

         if (device.role == InputDeviceRole.RightHanded)
         {
            m_RightController = device;
         }
      }
   }

   void Update()
   {
      if (m_LeftController.isValid)
      {
         
         Debug.Log("left valid");
         
         m_LeftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_LeftTouchPadClicked);
         m_LeftController.TryGetFeatureValue(CommonUsages.primaryButton, out m_LeftPrimaryButtonClicked);

         if (m_LeftTouchPadClicked || m_LeftPrimaryButtonClicked)
         {
            m_LeftBaseController.SetActive(false);
         }
         else
         {
            m_LeftBaseController.SetActive(true);
         }
         
         m_LeftTeleportController.SetActive(m_LeftTouchPadClicked);
         m_LeftUIController.SetActive(m_LeftPrimaryButtonClicked);
      }

      if (m_RightController.isValid)
      {
         m_RightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_RightTouchPadClicked);
         m_RightController.TryGetFeatureValue(CommonUsages.primaryButton, out m_RightPrimaryButtonClicked);

         if (m_RightTouchPadClicked || m_RightPrimaryButtonClicked)
         {
            m_RightBaseController.SetActive(false);
         }
         else
         {
            m_RightBaseController.SetActive(true);
         }
         
         m_RightTeleportController.SetActive(m_RightTouchPadClicked);
         m_RightUIController.SetActive(m_RightPrimaryButtonClicked);
      }
      
     

   }
   
   

}
