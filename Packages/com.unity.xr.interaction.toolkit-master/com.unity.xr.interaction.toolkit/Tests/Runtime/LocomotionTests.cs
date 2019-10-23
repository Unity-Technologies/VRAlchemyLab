using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SpatialTracking;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    public class LocomotionTests
    {
        const float k_Epsilon = 0.001f;

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllInteractionObjects();
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndOrientCamera()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrRig = TestUtilities.CreateXRRig();

            // config teleportation on XR rig
            LocomotionSystem locoSys = xrRig.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrRig.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;
                        
            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrRig.cameraFloorOffsetObject.transform);
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.Camera;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-90, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.1f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    false, false, false);
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            Vector3 cameraPosAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
            Assert.That(xrRig.cameraGameObject.transform.position == teleAnchor.transform.position + cameraPosAdjustment);
            Assert.That(xrRig.rig.transform.up == teleAnchor.transform.up);
            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(xrRig.cameraGameObject.transform.forward, teleAnchor.transform.up);
            Assert.That(projectedCameraForward.normalized == teleAnchor.transform.forward);

        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithProjectile()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrRig = TestUtilities.CreateXRRig();

            // config teleportation on XR rig
            LocomotionSystem locoSys = xrRig.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrRig.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrRig.cameraFloorOffsetObject.transform);
            interactor.lineType = XRRayInteractor.LineType.ProjectileCurve; // projectile curve
         
            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.None;

            // set teleportation anchor plane
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-90, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.1f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    false, false, false);
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);

            Vector3 cameraPosAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
            Assert.That(xrRig.cameraGameObject.transform.position == teleAnchor.transform.position + cameraPosAdjustment);
            Assert.That(xrRig.rig.transform.up == teleAnchor.transform.up);
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithBezierCurve()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrRig = TestUtilities.CreateXRRig();

            // config teleportation on XR rig
            LocomotionSystem locoSys = xrRig.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrRig.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrRig.cameraFloorOffsetObject.transform);
            interactor.lineType = XRRayInteractor.LineType.BezierCurve; // projectile curve

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.None;

            // set teleportation anchor plane
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-90, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.1f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    false, false, false);
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);

            Vector3 cameraPosAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
            Assert.That(xrRig.cameraGameObject.transform.position == teleAnchor.transform.position + cameraPosAdjustment);
            Assert.That(xrRig.rig.transform.up == teleAnchor.transform.up);
        }


        [UnityTest]
        public IEnumerator SnapTurn()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrRig = TestUtilities.CreateXRRig();

            // config snap turn on XR rig
            LocomotionSystem locoSys = xrRig.gameObject.AddComponent<LocomotionSystem>();
            locoSys.xrRig = xrRig;
            SnapTurnProvider snapProvider = xrRig.gameObject.AddComponent<SnapTurnProvider>();
            snapProvider.system = locoSys;
            snapProvider.enablePrimaryDevice = true;
            snapProvider.PrimaryDeviceNode = XRNode.LeftHand;
            float turnAmount = snapProvider.turnAmount;                     

            snapProvider.FakeStartTurn(false);

            yield return TestUtilities.WaitForInteraction();

            Assert.That(xrRig.transform.rotation.eulerAngles == new Vector3(0, turnAmount, 0));

        }

    }
}


