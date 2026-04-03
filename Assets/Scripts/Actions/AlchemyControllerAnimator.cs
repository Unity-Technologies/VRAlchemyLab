using UnityEngine;
using UnityEngine.XR;

public class AlchemyControllerAnimator : MonoBehaviour
{
    [Header("Controller Node (Left or Right)")]
    public XRNode controllerNode = XRNode.RightHand;

    private InputDevice device;

    [Header("Thumbstick")]
    public Transform thumbstickTransform;
    public Vector3 thumbstickRotationRange = new Vector3(30f, 0f, 30f);
    public Vector3 thumbstickPositionRange = Vector3.zero;

    Vector3 thumbstickBasePos;
    Quaternion thumbstickBaseRot;

    [Header("Trigger")]
    public Transform triggerTransform;
    public Vector3 triggerRotationRange = new Vector3(-15f, 0f, 0f);
    public Vector3 triggerPositionRange = Vector3.zero;

    Vector3 triggerBasePos;
    Quaternion triggerBaseRot;

    [Header("Grip")]
    public Transform gripTransform;
    public Vector3 gripRotationRange = Vector3.zero;
    public Vector3 gripPositionRange = new Vector3(-0.0015f, 0f, 0f);

    Vector3 gripBasePos;
    Quaternion gripBaseRot;

    [Header("Primary Button")]
    public Transform primaryButtonTransform;
    public Vector3 primaryButtonRotationRange = Vector3.zero;
    public Vector3 primaryButtonPositionRange = new Vector3(-0.0015f, 0f, 0f);

    Vector3 primaryBasePos;
    Quaternion primaryBaseRot;

    [Header("Secondary Button")]
    public Transform secondaryButtonTransform;
    public Vector3 secondaryButtonRotationRange = Vector3.zero;
    public Vector3 secondaryButtonPositionRange = new Vector3(-0.0015f, 0f, 0f);

    Vector3 secondaryBasePos;
    Quaternion secondaryBaseRot;

    void Awake()
    {
        CacheBase(thumbstickTransform, out thumbstickBasePos, out thumbstickBaseRot);
        CacheBase(triggerTransform, out triggerBasePos, out triggerBaseRot);
        CacheBase(gripTransform, out gripBasePos, out gripBaseRot);
        CacheBase(primaryButtonTransform, out primaryBasePos, out primaryBaseRot);
        CacheBase(secondaryButtonTransform, out secondaryBasePos, out secondaryBaseRot);
    }

    void OnEnable()
    {
        TryInitializeDevice();
    }

    void TryInitializeDevice()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    void CacheBase(Transform t, out Vector3 pos, out Quaternion rot)
    {
        pos = t ? t.localPosition : Vector3.zero;
        rot = t ? t.localRotation : Quaternion.identity;
    }

    void Update()
    {
        if (!device.isValid)
            TryInitializeDevice();

        AnimateThumbstick();
        AnimateTrigger();
        AnimateGrip();
        AnimatePrimaryButton();
        AnimateSecondaryButton();
    }

    Vector3 RotatedLocalOffset(Quaternion localRot, Vector3 range, float val)
    {
        return localRot * (range * val);
    }

    void AnimateThumbstick()
    {
        if (!thumbstickTransform) return;

        Vector2 stick;
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out stick);

        thumbstickTransform.localRotation =
            thumbstickBaseRot *
            Quaternion.Euler(
                stick.y * thumbstickRotationRange.x,
                stick.x * thumbstickRotationRange.y,
                stick.x * thumbstickRotationRange.z
            );

        Vector3 offset = RotatedLocalOffset(thumbstickBaseRot, thumbstickPositionRange, stick.magnitude);
        thumbstickTransform.localPosition = thumbstickBasePos + offset;
    }

    void AnimateTrigger()
    {
        if (!triggerTransform) return;

        float val;
        device.TryGetFeatureValue(CommonUsages.trigger, out val);

        triggerTransform.localRotation =
            triggerBaseRot *
            Quaternion.Euler(
                triggerRotationRange.x * val,
                triggerRotationRange.y * val,
                triggerRotationRange.z * val
            );

        Vector3 offset = RotatedLocalOffset(triggerBaseRot, triggerPositionRange, val);
        triggerTransform.localPosition = triggerBasePos + offset;
    }

    void AnimateGrip()
    {
        if (!gripTransform) return;

        float val;
        device.TryGetFeatureValue(CommonUsages.grip, out val);

        gripTransform.localRotation =
            gripBaseRot *
            Quaternion.Euler(
                gripRotationRange.x * val,
                gripRotationRange.y * val,
                gripRotationRange.z * val
            );

        Vector3 offset = RotatedLocalOffset(gripBaseRot, gripPositionRange, val);
        gripTransform.localPosition = gripBasePos + offset;
    }

    void AnimatePrimaryButton()
    {
        if (!primaryButtonTransform) return;

        bool pressed;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out pressed);
        float val = pressed ? 1f : 0f;

        primaryButtonTransform.localRotation =
            primaryBaseRot *
            Quaternion.Euler(
                primaryButtonRotationRange.x * val,
                primaryButtonRotationRange.y * val,
                primaryButtonRotationRange.z * val
            );

        Vector3 offset = RotatedLocalOffset(primaryBaseRot, primaryButtonPositionRange, val);
        primaryButtonTransform.localPosition = primaryBasePos + offset;
    }

    void AnimateSecondaryButton()
    {
        if (!secondaryButtonTransform) return;

        bool pressed;
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed);
        float val = pressed ? 1f : 0f;

        secondaryButtonTransform.localRotation =
            secondaryBaseRot *
            Quaternion.Euler(
                secondaryButtonRotationRange.x * val,
                secondaryButtonRotationRange.y * val,
                secondaryButtonRotationRange.z * val
            );

        Vector3 offset = RotatedLocalOffset(secondaryBaseRot, secondaryButtonPositionRange, val);
        secondaryButtonTransform.localPosition = secondaryBasePos + offset;
    }
}
