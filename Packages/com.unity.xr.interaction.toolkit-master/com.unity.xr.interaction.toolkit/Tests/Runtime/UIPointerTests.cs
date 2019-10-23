using NUnit.Framework;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{

    internal static class XRControllerRecorderExtensions
    {
        internal static void SetNextPose(this XRControllerRecorder recorder, Vector3 position, Quaternion rotation, bool selectActive, bool activateActive, bool pressActive)
        {
            XRControllerRecording currentRecording = recorder.recording;
            currentRecording.InitRecording();
            currentRecording.AddRecordingFrame(0.0f, position, rotation, selectActive, activateActive, pressActive);
            currentRecording.AddRecordingFrame(1000f, position, rotation, selectActive, activateActive, pressActive);
            recorder.recording = currentRecording;
            recorder.ResetPlayback();
            recorder.isPlaying = true;
        }
    }

    [TestFixture]
    internal class UIPointerTests
    {
        internal enum EventType
        {
            Click,
            Down,
            Up,
            Enter,
            Exit,
            Select,
            Deselect,
            PotentialDrag,
            BeginDrag,
            Dragging,
            Drop,
            EndDrag,
            Move,
            Submit,
            Cancel,
            Scroll
        }

        internal struct TestObjects
        {
            public TestEventSystem eventSystem;
            public XRControllerRecorder controllerRecorder;
            public UICallbackReceiver leftUIReceiver;
            public UICallbackReceiver rightUIReceiver;
        }

        internal class UICallbackReceiver : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler,
            IPointerExitHandler, IPointerUpHandler, IMoveHandler, ISelectHandler, IDeselectHandler, IInitializePotentialDragHandler,
            IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, ISubmitHandler, ICancelHandler, IScrollHandler
        {
            public struct Event
            {
                public EventType type;
                public BaseEventData data;

                public Event(EventType type, BaseEventData data)
                {
                    this.type = type;
                    this.data = data;
                }

                public override string ToString()
                {
                    var dataString = data.ToString();
                    dataString = dataString.Replace("\n", "\n\t");
                    return $"{type}[\n\t{dataString}]";
                }
            }

            public List<Event> events = new List<Event>();

            public void Reset()
            {
                events.Clear();
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Click, ClonePointerEventData(eventData)));
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Down, ClonePointerEventData(eventData)));
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Enter, ClonePointerEventData(eventData)));
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Exit, ClonePointerEventData(eventData)));
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Up, ClonePointerEventData(eventData)));
            }

            public void OnMove(AxisEventData eventData)
            {
                events.Add(new Event(EventType.Move, CloneAxisEventData(eventData)));
            }

            public void OnSubmit(BaseEventData eventData)
            {
                events.Add(new Event(EventType.Submit, null));
            }

            public void OnCancel(BaseEventData eventData)
            {
                events.Add(new Event(EventType.Cancel, null));
            }

            public void OnSelect(BaseEventData eventData)
            {
                events.Add(new Event(EventType.Select, null));
            }

            public void OnDeselect(BaseEventData eventData)
            {
                events.Add(new Event(EventType.Deselect, null));
            }

            public void OnInitializePotentialDrag(PointerEventData eventData)
            {
                events.Add(new Event(EventType.PotentialDrag, ClonePointerEventData(eventData)));
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                events.Add(new Event(EventType.BeginDrag, ClonePointerEventData(eventData)));
            }

            public void OnDrag(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Dragging, ClonePointerEventData(eventData)));
            }

            public void OnDrop(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Drop, ClonePointerEventData(eventData)));
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                events.Add(new Event(EventType.EndDrag, ClonePointerEventData(eventData)));
            }

            public void OnScroll(PointerEventData eventData)
            {
                events.Add(new Event(EventType.Scroll, ClonePointerEventData(eventData)));
            }

            private AxisEventData CloneAxisEventData(AxisEventData eventData)
            {
                return new AxisEventData(EventSystem.current)
                {
                    moveVector = eventData.moveVector,
                    moveDir = eventData.moveDir
                };
            }

            private PointerEventData ClonePointerEventData(PointerEventData eventData)
            {
                return new PointerEventData(EventSystem.current)
                {
                    pointerId = eventData.pointerId,
                    position = eventData.position,
                    button = eventData.button,
                    clickCount = eventData.clickCount,
                    clickTime = eventData.clickTime,
                    eligibleForClick = eventData.eligibleForClick,
                    delta = eventData.delta,
                    scrollDelta = eventData.scrollDelta,
                    dragging = eventData.dragging,
                    hovered = new List<GameObject>(eventData.hovered),
                    pointerDrag = eventData.pointerDrag,
                    pointerEnter = eventData.pointerEnter,
                    pointerPress = eventData.pointerPress,
                    pressPosition = eventData.pressPosition,
                    pointerCurrentRaycast = eventData.pointerCurrentRaycast,
                    pointerPressRaycast = eventData.pointerPressRaycast,
                    rawPointerPress = eventData.rawPointerPress,
                    useDragThreshold = eventData.useDragThreshold,
                };
            }
        }

        internal class TestEventSystem : EventSystem
        {
            public void InvokeUpdate()
            {
                current = this; // Needs to be current to be allowed to update.
                Update();
            }
        }

        internal static TestObjects SetupUIScene()
        {
            TestObjects testObjects = new TestObjects();

            // Set up camera and canvas on which we can perform raycasts.
            GameObject cameraGo = new GameObject("Camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.pixelRect = new Rect(0, 0, 640, 480);

            GameObject eventSystemGo = new GameObject("EventSystem", typeof(TestEventSystem), typeof(XRUIInputModule));
            testObjects.eventSystem = eventSystemGo.GetComponent<TestEventSystem>();
            testObjects.eventSystem.UpdateModules();
            testObjects.eventSystem.InvokeUpdate(); // Initial update only sets current module.
            XRUIInputModule inputModule = eventSystemGo.GetComponent<XRUIInputModule>();
            inputModule.uiCamera = camera;

            GameObject pointerGo = new GameObject("XR UI Pointer", typeof(XRUIPointer), typeof(XRController), typeof(XRControllerRecorder));
            testObjects.controllerRecorder = pointerGo.GetComponent<XRControllerRecorder>();
            testObjects.controllerRecorder.controller = pointerGo.GetComponent<XRController>();
            testObjects.controllerRecorder.recording = ScriptableObject.CreateInstance<XRControllerRecording>();
            XRUIPointer pointer = pointerGo.GetComponent<XRUIPointer>();
            pointer.controller = pointerGo.GetComponent<XRController>();

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TrackedDeviceGraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;

            // Set up a GameObject hierarchy that we send events to. In a real setup,
            // this would be a hierarchy involving UI components.
            var parentGameObject = new GameObject("Parent");
            var parentTransform = parentGameObject.AddComponent<RectTransform>();
            parentGameObject.AddComponent<UICallbackReceiver>();

            var leftChildGameObject = new GameObject("Left Child");
            var leftChildTransform = leftChildGameObject.AddComponent<RectTransform>();
            leftChildGameObject.AddComponent<Image>();
            testObjects.leftUIReceiver = leftChildGameObject.AddComponent<UICallbackReceiver>();

            var rightChildGameObject = new GameObject("Right Child");
            var rightChildTransform = rightChildGameObject.AddComponent<RectTransform>();
            rightChildGameObject.AddComponent<Image>();
            testObjects.rightUIReceiver = rightChildGameObject.AddComponent<UICallbackReceiver>(); ;

            parentTransform.SetParent(canvas.transform, worldPositionStays: false);
            leftChildTransform.SetParent(parentTransform, worldPositionStays: false);
            rightChildTransform.SetParent(parentTransform, worldPositionStays: false);

            // Parent occupies full space of canvas.
            parentTransform.sizeDelta = new Vector2(640, 480);

            // Left child occupies left half of parent.
            leftChildTransform.anchoredPosition = new Vector2(-(640 / 4), 0);
            leftChildTransform.sizeDelta = new Vector2(320, 480);

            // Right child occupies right half of parent.
            rightChildTransform.anchoredPosition = new Vector2(640 / 4, 0);
            rightChildTransform.sizeDelta = new Vector2(320, 480);

            return testObjects;
        }

        [UnityTest]
        public IEnumerator TrackedDevicesCanDriveUI()
        {
            TestObjects testObjects = SetupUIScene();

            UICallbackReceiver leftUIReceiver = testObjects.leftUIReceiver;
            UICallbackReceiver rightUIReceiver = testObjects.rightUIReceiver;

            XRControllerRecorder recorder = testObjects.controllerRecorder;

            // Reset to Defaults
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -90.0f, 0.0f), false, false, false);
            yield return TestUtilities.WaitForInteraction();

            leftUIReceiver.Reset();
            rightUIReceiver.Reset();

            // Move over left child.
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -30.0f, 0.0f), false, false, false);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(1));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Enter));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(0));

            // Check basic down/up
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -30.0f, 0.0f), false, false, true);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Down));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.PotentialDrag));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(0));

            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -30.0f, 0.0f), false, false, false);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Up));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.Click));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(0));

            // Check down and drag
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -30.0f, 0.0f), false, false, true);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Down));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.PotentialDrag));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(0));

            // Move to new location on left child
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, -10.0f, 0.0f), false, false, true);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.BeginDrag));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.Dragging));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(0));

            // Move children
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, 30.0f, 0.0f), false, false, true);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Exit));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.Dragging));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(1));
            Assert.That(rightUIReceiver.events[0].type, Is.EqualTo(EventType.Enter));
            rightUIReceiver.Reset();

            //Deselect
            recorder.SetNextPose(Vector3.zero, Quaternion.Euler(0.0f, 30.0f, 0.0f), false, false, false);
            yield return TestUtilities.WaitForInteraction();

            Assert.That(leftUIReceiver.events, Has.Count.EqualTo(2));
            Assert.That(leftUIReceiver.events[0].type, Is.EqualTo(EventType.Up));
            Assert.That(leftUIReceiver.events[1].type, Is.EqualTo(EventType.EndDrag));
            leftUIReceiver.Reset();
            Assert.That(rightUIReceiver.events, Has.Count.EqualTo(1));
            Assert.That(rightUIReceiver.events[0].type, Is.EqualTo(EventType.Drop));
            rightUIReceiver.Reset();
        }
    }
}