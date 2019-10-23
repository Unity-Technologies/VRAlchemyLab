using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    public class RayInteractorTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllInteractionObjects();
        }

        [UnityTest]
        public IEnumerator RayInteractorCanHoverInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateRayInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            yield return TestUtilities.WaitForInteraction();

            List<XRBaseInteractable> validTargets = new List<XRBaseInteractable>();
            manager.GetValidTargets(interactor, validTargets);
            Assert.That(validTargets, Has.Exactly(1).EqualTo(interactable));

            List<XRBaseInteractable> hoverTargetList = new List<XRBaseInteractable>();
            interactor.GetHoverTargets(hoverTargetList);
            Assert.That(hoverTargetList, Has.Exactly(1).EqualTo(interactable));
        }

        [UnityTest]
        public IEnumerator RayInteractorCanSelectInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateRayInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            var controller = interactor.GetComponent<XRController>();
            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });
            controllerRecorder.isPlaying = true;

            yield return TestUtilities.WaitForInteraction();

            Assert.That(interactor.selectTarget, Is.EqualTo(interactable));
        }
    }
}
