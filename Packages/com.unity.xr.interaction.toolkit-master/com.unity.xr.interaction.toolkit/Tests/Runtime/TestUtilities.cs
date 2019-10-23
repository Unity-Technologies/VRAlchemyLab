using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    public static class TestUtilities
    {
        internal static void DestroyAllInteractionObjects()
        {
            foreach (var gameObject in Object.FindObjectsOfType<XRInteractionManager>())
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject.transform.root.gameObject);
            }
            foreach (var gameObject in Object.FindObjectsOfType<XRBaseInteractable>())
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject.transform.root.gameObject);
            }
            foreach (var gameObject in Object.FindObjectsOfType<XRBaseInteractor>())
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject.transform.root.gameObject);
            }
            foreach (var gameObject in Object.FindObjectsOfType<XRController>())
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject.transform.root.gameObject);
            }
        }
        internal static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }

        internal static XRInteractionManager CreateInteractionManager()
        {
            GameObject managerGO = new GameObject();
            XRInteractionManager manager = managerGO.AddComponent<XRInteractionManager>();
            return manager;
        }

        internal static XRDirectInteractor CreateDirectInteractor()
        {
            GameObject interactorGO = new GameObject();
            CreateGOSphereCollider(interactorGO);
            XRDirectInteractor interactor = interactorGO.AddComponent<XRDirectInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            controller.enableInputTracking = false;
            return interactor;
        }

        internal static XRRig CreateXRRig()
        {
            GameObject xrRigGO = new GameObject();
            xrRigGO.name = "XR Rig";
            XRRig xrRig = xrRigGO.AddComponent<XRRig>();

            // add camera offset
            GameObject cameraOffsetGO = new GameObject();
            cameraOffsetGO.name = "CameraOffset";
            cameraOffsetGO.transform.SetParent(xrRig.transform,false);
            xrRig.cameraFloorOffsetObject = cameraOffsetGO;

            xrRig.transform.position = Vector3.zero;
            xrRig.transform.rotation = Quaternion.identity;

            // camera and track pose driver
            GameObject cameraGO = new GameObject();
            cameraGO.name = "Camera";
            var camera = cameraGO.AddComponent<Camera>();

            cameraGO.transform.SetParent(cameraOffsetGO.transform, false);
            xrRig.cameraGameObject = cameraGO;

            XR.XRDevice.DisableAutoXRCameraTracking(camera, true);

            return xrRig;
        }
        
        internal static TeleportationAnchor CreateTeleportAnchorPlane()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "plane";
            TeleportationAnchor teleAnchor = plane.AddComponent<TeleportationAnchor>();
            return teleAnchor;
        } 

        internal static XRRayInteractor CreateRayInteractor()
        {
            GameObject interactorGO = new GameObject();
            interactorGO.name = "Ray Interactor";
            XRRayInteractor interactor = interactorGO.AddComponent<XRRayInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            XRInteractorLineVisual ilv = interactorGO.AddComponent<XRInteractorLineVisual>();
            controller.enableInputTracking = false;
            return interactor;
        }
        internal static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject();
            CreateGOSphereCollider(interactorGO);
            XRSocketInteractor interactor = interactorGO.AddComponent<XRSocketInteractor>();
            return interactor;
        }
        internal static XRGrabInteractable CreateGrabInteractable()
        {
            GameObject interactableGO = new GameObject();
            CreateGOSphereCollider(interactableGO, false);
            XRGrabInteractable interactable = interactableGO.AddComponent<XRGrabInteractable>();
            var rididBody = interactableGO.GetComponent<Rigidbody>();
            rididBody.useGravity = false;
            rididBody.isKinematic = true;
            return interactable;
        }

        internal static XRControllerRecorder CreateControllerRecorder(XRController controller, Action<XRControllerRecording> addRecordingFrames)
        {
            var controllerRecorder = controller.gameObject.AddComponent<XRControllerRecorder>();
            controllerRecorder.controller = controller;
            controllerRecorder.recording = ScriptableObject.CreateInstance<XRControllerRecording>();

            addRecordingFrames(controllerRecorder.recording);
            return controllerRecorder;
        }

        static readonly int k_WaitCount = 5;
        internal static IEnumerator WaitForInteraction()
        {
            for (int i=0; i < k_WaitCount; ++i)
                yield return null;
        }
    }
}
