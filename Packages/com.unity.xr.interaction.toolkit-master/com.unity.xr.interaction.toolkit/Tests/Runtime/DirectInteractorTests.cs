using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    public class DirectInteractorTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllInteractionObjects();
        }

        [UnityTest]
        public IEnumerator DirectInteractorCanHoverInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();

            yield return TestUtilities.WaitForInteraction();

            List<XRBaseInteractable> validTargets = new List<XRBaseInteractable>();
            manager.GetValidTargets(directInteractor, validTargets);
            Assert.That(validTargets, Has.Exactly(1).EqualTo(interactable));

            List<XRBaseInteractable> hoverTargetList = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargetList);
            Assert.That(hoverTargetList, Has.Exactly(1).EqualTo(interactable));
        }

        [UnityTest]
        public IEnumerator DirectInteractorCanSelectInteractable()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();
            var controller = directInteractor.GetComponent<XRController>();
            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });
            controllerRecorder.isPlaying = true;

            yield return TestUtilities.WaitForInteraction();

            Assert.That(directInteractor.selectTarget, Is.EqualTo(interactable));
        }
    }
}
