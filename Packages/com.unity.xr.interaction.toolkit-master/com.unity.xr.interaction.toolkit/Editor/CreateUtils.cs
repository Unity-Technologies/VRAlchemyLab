using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR;

#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.AR;
#endif

namespace UnityEditor.XR.Interaction.Toolkit
{
    internal static class CreateUtils
    {
        static readonly string k_LineMaterial = "Default-Line.mat";

        static void CreateInteractionManager()
        {
            if (!Object.FindObjectOfType<XRInteractionManager>())
            {
                ObjectFactory.CreateGameObject("XR Interaction Manager", typeof(XRInteractionManager));
            }
        }

        static GameObject CreateRayInteractorInternal(string gameObjectName)
        {
            var rayInteractableGo = ObjectFactory.CreateGameObject(gameObjectName,
                typeof(XRController),                
                typeof(XRUIPointer),
                typeof(XRRayInteractor),
                typeof(LineRenderer),
                typeof(XRInteractorLineVisual));

            SetupLineRenderer(rayInteractableGo.GetComponent<LineRenderer>());

            return rayInteractableGo;
        }
        


        [MenuItem("GameObject/XR/Ray Interactor", false, 10)]
        static void CreateRayInteractor()
        {
            CreateInteractionManager();
            CreateRayInteractorInternal("Ray Interactor");
        }

        [MenuItem("GameObject/XR/Direct Interactor", false, 10)]
        static void CreateDirectInteractor()
        {
            CreateInteractionManager();

            var directInteractableGo = ObjectFactory.CreateGameObject("Direct Interactor",
                typeof(XRController),
                typeof(SphereCollider),
                typeof(XRDirectInteractor),
                typeof(XRUIPointer));
            var sphereCollider = directInteractableGo.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.1f;
        }
        
        [MenuItem("GameObject/XR/Socket Interactor", false, 10)]
        static void CreateSocketInteractor()
        {
            CreateInteractionManager();

            var socketInteractableGo = ObjectFactory.CreateGameObject("Socket Interactor",
                typeof(XRController),
                typeof(SphereCollider),
                typeof(XRSocketInteractor));
            var sphereCollider = socketInteractableGo.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.1f;
        }

        [MenuItem("GameObject/XR/Grab Interactable", false, 10)]
        static void CreateGrabInteractable()
        {
            CreateInteractionManager();

            var grabInteractableGo = ObjectFactory.CreateGameObject("Grab Interactable",
                typeof(XRGrabInteractable),
                typeof(SphereCollider));
            var sphereCollider = grabInteractableGo.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = false;
            sphereCollider.radius = 0.1f;
        }

        static XRRig CreateXRRig()
        {
            CreateInteractionManager();

            var xrRigGO = ObjectFactory.CreateGameObject("XR Rig", typeof(XRRig));
            var cameraOffsetGO = ObjectFactory.CreateGameObject("Camera Offset");
            Undo.SetTransformParent(cameraOffsetGO.transform, xrRigGO.transform, "Parent Camera Offset to XR Rig");

            var xrCamera = Object.FindObjectOfType<Camera>();
            if (xrCamera == null)
            {
                var xrCameraGO = ObjectFactory.CreateGameObject("Main Camera", typeof(Camera));
                xrCamera = xrCameraGO.GetComponent<Camera>();
            }
            Undo.SetTransformParent(xrCamera.transform, cameraOffsetGO.transform, "Parent Camera to Camera Offset");
            
            xrCamera.transform.localPosition = Vector3.zero;
            xrCamera.transform.localRotation = Quaternion.identity;
            xrCamera.tag = "MainCamera";
            
            TrackedPoseDriver trackedPoseDriver = xrCamera.gameObject.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                trackedPoseDriver = Undo.AddComponent<TrackedPoseDriver>(xrCamera.gameObject);
            }
            trackedPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
            trackedPoseDriver.UseRelativeTransform = false;

            var xrRig = xrRigGO.GetComponent<XRRig>();
            xrRig.cameraFloorOffsetObject = cameraOffsetGO;
            xrRig.cameraGameObject = xrCamera.gameObject;
            return xrRig;
        }

        
        static XRRig CreateXRRigWithHandControllers()
        {
            CreateInteractionManager();

            var xrRig = CreateXRRig();
            var cameraOffsetGO = xrRig.transform.GetChild(0).gameObject;
            
            var leftHandRayInteractorGO = CreateRayInteractorInternal("LeftHand Controller");                            
            var leftHandController = leftHandRayInteractorGO.GetComponent<XRController>();
            leftHandController.controllerNode = XRNode.LeftHand;


            var rightHandRayInteractorGO = CreateRayInteractorInternal("RightHand Controller");
            var rightHandController = rightHandRayInteractorGO.GetComponent<XRController>();
            rightHandController.controllerNode = XRNode.RightHand;
            
            Undo.SetTransformParent(leftHandRayInteractorGO.transform, cameraOffsetGO.transform, "Parent Left Hand to Camera Offset");
            Undo.SetTransformParent(rightHandRayInteractorGO.transform, cameraOffsetGO.transform, "Parent Right Hand to Camera Offset");

            return xrRig;
        }
        

        [MenuItem("GameObject/XR/Room-Scale XR Rig", false, 10)]
        static void CreateRoomScaleVRCameraRig()
        {
            var vrCameraRig = CreateXRRigWithHandControllers();
            vrCameraRig.trackingSpace = UnityEngine.XR.TrackingSpaceType.RoomScale;
        }

        [MenuItem("GameObject/XR/Stationary XR Rig", false, 10)]
        static void CreateStationaryVRCameraRig()
        {
            var vrCameraRig = CreateXRRigWithHandControllers();
            vrCameraRig.trackingSpace = UnityEngine.XR.TrackingSpaceType.Stationary;
        }

        [MenuItem("GameObject/XR/Locomotion System", false, 10)]
        static void CreateLocomotionSystem()
        {
            var locomotionSystemGO = ObjectFactory.CreateGameObject("Locomotion System",
                typeof(LocomotionSystem),
                typeof(TeleportationProvider),
                typeof(SnapTurnProvider));

            LocomotionSystem locomotionSystem = locomotionSystemGO.GetComponent<LocomotionSystem>();

            TeleportationProvider teleportationProvider = locomotionSystemGO.GetComponent<TeleportationProvider>();
            teleportationProvider.system = locomotionSystem;

            SnapTurnProvider snapTurnProvider = locomotionSystemGO.GetComponent<SnapTurnProvider>();
            snapTurnProvider.system = locomotionSystem;
            snapTurnProvider.enablePrimaryDevice = true;
            snapTurnProvider.PrimaryDeviceNode = XRNode.LeftHand;
            snapTurnProvider.enableSecondaryDevice = true;
            snapTurnProvider.SecondaryDeviceNode = XRNode.RightHand;
        }

        [MenuItem("GameObject/XR/Teleportation Area", false, 10)]
        static void CreateTeleportationArea()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.AddComponent<TeleportationArea>();
            plane.name = "Teleportation Area";
        }

        [MenuItem("GameObject/XR/Teleportation Anchor", false, 10)]
        static void CreateTeleportationAnchor()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            GameObject anchor = ObjectFactory.CreateGameObject("Anchor");
            anchor.transform.parent = plane.transform;

            TeleportationAnchor teleportationAnchor = plane.AddComponent<TeleportationAnchor>();
            plane.name = "Teleportation Anchor";
            teleportationAnchor.teleportAnchorTransform = anchor.transform;
        }

        [MenuItem("GameObject/XR/UI Canvas", false, 10)]
        static void CreateXRUICanvas()
        {
            var vrCamera = Object.FindObjectOfType<Camera>();

            // Ensure there is at least one EventSystem setup properly
            XRUIInputModule inputModule = Object.FindObjectOfType<XRUIInputModule>();
            if (inputModule == null)
            {
                EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
                GameObject eventSystemGo;
                if (eventSystem == null)
                    eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
                else
                    eventSystemGo = eventSystem.gameObject;

                //Remove the Standalone Input Module if already implemented, since it will block the XRUIInputModule
                StandaloneInputModule standaloneInputModule = eventSystemGo.GetComponent<StandaloneInputModule>();
                if (standaloneInputModule != null)
                    Object.Destroy(standaloneInputModule);

                inputModule = eventSystemGo.AddComponent<XRUIInputModule>();
            }
            inputModule.uiCamera = vrCamera;

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TrackedDeviceGraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = vrCamera;
        }

        [MenuItem("GameObject/XR/UI Pointer", false, 10)]
        static void CreateXRUIPointer()
        {
            GameObject selectedGo = Selection.activeGameObject;
            if(selectedGo == null)
            {
                selectedGo = new GameObject("Controller", typeof(XRController), typeof(XRUIPointer));
            }
           
            XRController controller = selectedGo.GetComponent<XRController>();
            if(controller == null)
            {
                GameObject controllerGo = new GameObject("Controller", typeof(XRController), typeof(XRUIPointer));
                controller = selectedGo.GetComponent<XRController>();
                Undo.SetTransformParent(controllerGo.transform, selectedGo.transform, "Parent Controller to Selected GameObject");
                selectedGo = controllerGo;
            }

            XRUIPointer pointer = selectedGo.GetComponent<XRUIPointer>();
            if (pointer == null)
                selectedGo.AddComponent<XRUIPointer>();

            Selection.activeGameObject = selectedGo;
        }

        static void SetupLineRenderer(LineRenderer lineRenderer)
        {
            var materials = new Material[1];
            materials[0] = AssetDatabase.GetBuiltinExtraResource<Material>(k_LineMaterial);
            lineRenderer.materials = materials;
            lineRenderer.loop = false;
            lineRenderer.widthMultiplier = 0.005f;
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 5;
        }

#if AR_FOUNDATION_PRESENT
        [MenuItem("GameObject/XR/AR Gesture Interactor", false, 10)]
        static void CreateARGestureInteractor()
        {
            CreateInteractionManager();
            
            var originGo = ObjectFactory.CreateGameObject("AR Session Origin", typeof(ARSessionOrigin));
            var cameraGo = ObjectFactory.CreateGameObject("AR Camera",
                typeof(Camera),
                typeof(TrackedPoseDriver),
                typeof(ARCameraManager),
                typeof(ARCameraBackground),
                typeof(ARGestureInteractor));

            Undo.SetTransformParent(cameraGo.transform, originGo.transform, "Parent camera to session origin");

            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;

            var origin = originGo.GetComponent<ARSessionOrigin>();
            origin.camera = camera;

            var tpd = cameraGo.GetComponent<TrackedPoseDriver>();
            tpd.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.ColorCamera);
            
            Selection.activeGameObject = cameraGo;
        }
        
        [MenuItem("GameObject/XR/AR Placement Interactable", false, 10)]
        static void CreateARPlacementInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Placement Interactable", typeof(ARPlacementInteractable));

            Selection.activeGameObject = placementGo;
        }
        
        [MenuItem("GameObject/XR/AR Selection Interactable", false, 10)]
        static void CreateARSelectionInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Selection Interactable", typeof(ARSelectionInteractable));

            Selection.activeGameObject = placementGo;
        }
        
        [MenuItem("GameObject/XR/AR Translation Interactable", false, 10)]
        static void CreateARTranslationInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Translation Interactable", typeof(ARTranslationInteractable));

            Selection.activeGameObject = placementGo;
        }
        
        [MenuItem("GameObject/XR/AR Scale Interactable", false, 10)]
        static void CreateARScaleInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Scale Interactable", typeof(ARScaleInteractable));

            Selection.activeGameObject = placementGo;
        }
        
        [MenuItem("GameObject/XR/AR Rotation Interactable", false, 10)]
        static void CreateARRotationInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Rotation Interactable", typeof(ARRotationInteractable));

            Selection.activeGameObject = placementGo;
        }
        
        [MenuItem("GameObject/XR/AR Annotation Interactable", false, 10)]
        static void CreateARAnnotationInteractable()
        {
            CreateInteractionManager();
            
            var placementGo = ObjectFactory.CreateGameObject("AR Annotation Interactable", typeof(ARRotationInteractable));

            Selection.activeGameObject = placementGo;
        }
#endif
    }
}
