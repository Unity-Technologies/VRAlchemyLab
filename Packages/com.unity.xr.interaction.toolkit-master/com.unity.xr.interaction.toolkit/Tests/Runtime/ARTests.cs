
#if AR_FOUNDATION_PRESENT
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    public class ARTests
    {
        static GameObject CreateTestPlane()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Plane";
            plane.layer = 9;
            return plane;
        }
        
        [SetUp]
        public void SetUp()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var camera = ObjectFactory.CreateGameObject("Camera", typeof(Camera), typeof(ARPlaneManager), typeof(ARRaycastManager), typeof(ARGestureInteractor));
            camera.tag = "MainCamera";

            camera.transform.position = new Vector3(0.0f, 5.0f, -5.0f);
            camera.transform.LookAt(Vector3.zero, Vector3.up);
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllInteractionObjects();

            var plane = GameObject.Find("Plane");
            if (plane != null)
                Object.DestroyImmediate(plane);
            var camera = GameObject.Find("Camera");
            if (camera != null)
                Object.DestroyImmediate(camera);
        }

        IEnumerator SimulateTouches(Vector2[] touchesBegan, Vector2[] touchesMoved, Vector2[] touchesEnded)
        {
            Debug.Assert(touchesBegan.Length >= touchesMoved.Length);
            Debug.Assert(touchesBegan.Length >= touchesEnded.Length);

            // Give interaction system and gesture recognizer a chance to process gesture input and resulting interactions
            IEnumerator DoYield()
            {
                yield return null;
                yield return null;
            }

            if (touchesBegan.Length > 0)
            {
                GestureTouchesUtility.s_MockTouches.Clear();
                for (int i = 0; i < touchesBegan.Length; ++i)
                {
                    GestureTouchesUtility.s_MockTouches.Add(
                        new MockTouch()
                        {
                            phase = TouchPhase.Began,
                            deltaPosition = new Vector2(0, 0),
                            position = touchesBegan[i],
                            fingerId = i
                        });
                }
                
                yield return DoYield();
            }

            if (touchesMoved.Length > 0)
            {
                // Break move into steps to track
                const int k_MoveSegments = 10;
                for (int iSegments = 0; iSegments < k_MoveSegments; ++iSegments)
                {
                    // Track initial move from start location
                    GestureTouchesUtility.s_MockTouches.Clear();
                    for (int i = 0; i < touchesMoved.Length; ++i)
                    {
                        var deltaMove = (touchesMoved[i] - touchesBegan[i]) / k_MoveSegments;
                        GestureTouchesUtility.s_MockTouches.Add(
                            new MockTouch()
                            {
                                phase = TouchPhase.Moved,
                                deltaPosition = deltaMove,
                                position = touchesBegan[i] + deltaMove * (iSegments + 1),
                                fingerId = i
                            });
                    }

                    yield return DoYield();
                }
            }

            if (touchesEnded.Length > 0)
            {
                GestureTouchesUtility.s_MockTouches.Clear();
                for (int i = 0; i < touchesEnded.Length; ++i)
                {
                    GestureTouchesUtility.s_MockTouches.Add(
                        new MockTouch()
                        {
                            phase = TouchPhase.Ended,
                            deltaPosition = touchesEnded[i] - touchesBegan[i],
                            position = touchesEnded[i],
                            fingerId = i
                        });
                }

                yield return DoYield();
            }
        }

        [UnityTest]
        public IEnumerator GestureInteractor_SelectPlacementInteractable_CreatesGO()
        {
            var interactable = ObjectFactory.CreateGameObject("ARPlacementInteractable", typeof(ARPlacementInteractable));
            interactable.GetComponent<ARPlacementInteractable>().PlacementPrefab = new GameObject();
            CreateTestPlane();

            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { },
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) });

            Assert.NotNull(GameObject.Find("PlacementAnchor"));
        }
        
        [UnityTest]
        public IEnumerator GestureInteractor_SelectSelectionInteractable_Selects()
        {
            var interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.AddComponent<ARSelectionInteractable>();

            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { },
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) });

            Assert.That(interactable.GetComponent<ARSelectionInteractable>().isSelected);
        }
        
        [UnityTest]
        public IEnumerator GestureInteractor_DragTranslateInteractable_TranslatesAlongPlane()
        {
            var parent = new GameObject();
            var interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.AddComponent<ARSelectionInteractable>();
            interactable.AddComponent<ARTranslationInteractable>();
            interactable.transform.parent = parent.transform;
            CreateTestPlane();
            var originalPosition = interactable.transform.position;

            // Select
            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { },
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) });

            // Drag
            const float translateDelta = 200.0f;
            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { new Vector2(Screen.width / 2 + translateDelta, Screen.height / 2 + translateDelta) },
                new Vector2[] { new Vector2(Screen.width / 2 + translateDelta, Screen.height / 2 + translateDelta) });
            
            Assert.That(interactable.transform.position.x > originalPosition.x);
            Assert.That(interactable.transform.position.y > originalPosition.y);
        }
        
        [UnityTest]
        public IEnumerator GestureInteractor_PinchScaleInteractable_Scales()
        {
            var interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.AddComponent<ARSelectionInteractable>();
            interactable.AddComponent<ARScaleInteractable>();
            var originalScale = interactable.transform.localScale;

            // Select
            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { },
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) });

            // Pinch
            const float pinchStartOffset = 10.0f;
            const float pinchDelta = 200.0f;
            yield return SimulateTouches(
                new Vector2[] { 
                    new Vector2(Screen.width / 2 - pinchStartOffset, Screen.height / 2), 
                    new Vector2(Screen.width / 2 + pinchStartOffset, Screen.height / 2)
                },
                new Vector2[] { 
                    new Vector2(Screen.width / 2 - pinchDelta, Screen.height / 2), 
                    new Vector2(Screen.width / 2 + pinchDelta, Screen.height / 2)
                },
                new Vector2[] { 
                    new Vector2(Screen.width / 2 - pinchDelta, Screen.height / 2), 
                    new Vector2(Screen.width / 2 + pinchDelta, Screen.height / 2)
                });
            
            Assert.That(interactable.transform.localScale.x > originalScale.x);
            Assert.That(interactable.transform.localScale.y > originalScale.y);
            Assert.That(interactable.transform.localScale.z > originalScale.z);
        }
        
        [UnityTest]
        public IEnumerator GestureInteractor_TwistRotationInteractable_Rotates()
        {
            var interactable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactable.AddComponent<ARSelectionInteractable>();
            interactable.AddComponent<ARRotationInteractable>();
            var originalRotation = interactable.transform.rotation;

            // Select
            yield return SimulateTouches(
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) },
                new Vector2[] { },
                new Vector2[] { new Vector2(Screen.width / 2, Screen.height / 2) });

            // Twist
            const float startOffsetDelta = 100.0f;
            const float rotateOffsetDelta = 200.0f;
            yield return SimulateTouches(
                new Vector2[] { 
                    new Vector2(Screen.width / 2 - startOffsetDelta, Screen.height / 2), 
                    new Vector2(Screen.width / 2 + startOffsetDelta, Screen.height / 2) },
                new Vector2[]
                {
                    new Vector2(Screen.width / 2 - startOffsetDelta + rotateOffsetDelta, Screen.height / 2 - rotateOffsetDelta), 
                    new Vector2(Screen.width / 2 + startOffsetDelta - rotateOffsetDelta, Screen.height / 2 + rotateOffsetDelta)
                },
                new Vector2[]
                {
                    new Vector2(Screen.width / 2 - startOffsetDelta + rotateOffsetDelta, Screen.height / 2 - rotateOffsetDelta), 
                    new Vector2(Screen.width / 2 + startOffsetDelta - rotateOffsetDelta, Screen.height / 2 + rotateOffsetDelta)
                });
            
            Assert.That(interactable.transform.rotation != originalRotation);
        }
    }
}

#endif
