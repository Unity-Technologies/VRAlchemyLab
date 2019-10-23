using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    public class SocketInteractorTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllInteractionObjects();
        }

        [UnityTest]
        public IEnumerator SocketInteractorCanHoverInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var socketInteractor = TestUtilities.CreateSocketInteractor();
            var interactable = TestUtilities.CreateGrabInteractable();
            var interactable2 = TestUtilities.CreateGrabInteractable();

            // interactable 2 will be hovered as interactable will be SELECTED.
            yield return TestUtilities.WaitForInteraction();

            List<XRBaseInteractable> hoverTargetList = new List<XRBaseInteractable>();
            socketInteractor.GetHoverTargets(hoverTargetList);
            Assert.That(hoverTargetList, Has.Exactly(1).EqualTo(interactable2));
        }

        [UnityTest]
        public IEnumerator SocketInteractorCanSelectInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var socketInteractor = TestUtilities.CreateSocketInteractor();
            var interactable = TestUtilities.CreateGrabInteractable();

            yield return TestUtilities.WaitForInteraction();

            Assert.That(socketInteractor.selectTarget, Is.EqualTo(interactable));
        }

        [UnityTest]
        public IEnumerator SocketInteractorCanDirectInteractorStealFrom()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var socketInteractor = TestUtilities.CreateSocketInteractor();
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

            Assert.That(socketInteractor.selectTarget, Is.EqualTo(null));
            Assert.That(directInteractor.selectTarget, Is.EqualTo(interactable));
        }
    }
}
