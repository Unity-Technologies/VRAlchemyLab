using UnityEngine;
using System;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactable component that allows basic "grab" functionality.
    /// Can attach to selecting interactor and follow it around while obeying physics (and inherit velocity when released).
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("XR/XR Grab Interactable")]
    public class XRGrabInteractable : XRBaseInteractable
    {

        const float k_DefaultTighteningAmount = 0.5f;
        const float k_DefaultSmoothingAmount = 5f;
        const float k_VelocityPredictionFactor = 0.6f;
        const float k_AngularVelocityDamping = 0.95f;
        const int k_ThrowSmoothingFrameCount = 20;
        const float k_DefaultAttachEaseInTime = 0.15f;
        const float k_DefaultThrowSmoothingDuration = 0.25f;
        const float k_DefaultThrowVelocityScale = 1.5f;
        const float k_DefaultThrowAngularVelocityScale = 0.25f;

        [SerializeField]
        Transform m_AttachTransform;
        /// <summary>Gets or sets attach point to use on this Interactable (will use RigidBody center if none set).</summary>
        public Transform attachTransform { get { return m_AttachTransform; } set { m_AttachTransform = value; } }

        [SerializeField]
        float m_AttachEaseInTime = k_DefaultAttachEaseInTime;
        /// <summary>Gets or sets Time it takes to ease in the attach (time of 0.0 indicates no easing).</summary>
        public float attachEaseInTime { get { return m_AttachEaseInTime; } set { m_AttachEaseInTime = value; } }

        [SerializeField]
        XRBaseInteractable.MovementType m_MovementType = MovementType.Kinematic;
        /// <summary>Gets or sets movement type for RigidBody.</summary>
        public XRBaseInteractable.MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        bool m_TrackPosition = true;
        /// <summary>Gets or sets whether this interactable should track the position of the interactor.</summary>
        public bool trackPosition { get { return m_TrackPosition; } set { m_TrackPosition = value; } }

        [SerializeField]
        bool m_SmoothPosition;
        /// <summary>Gets or sets whether smoothing is applied to the position of the object.</summary>
        public bool smoothPosition { get { return m_SmoothPosition; } set { m_SmoothPosition = value; } }

        [SerializeField, Range(0.0f, 20.0f)]
        float m_SmoothPositionAmount = k_DefaultSmoothingAmount;
        /// <summary>Gets or sets the smoothing applied to the object's position when following.</summary>
        public float smoothPositionAmount { get { return m_SmoothPositionAmount; } set { m_SmoothPositionAmount = value; } }

        [SerializeField, Range(0.0f, 1.0f)]
        float m_TightenPosition = k_DefaultTighteningAmount;
        /// <summary>Gets or sets the maximum follow position difference when using smoothing.</summary>
        public float tightenPosition { get { return m_TightenPosition; } set { m_TightenPosition = value; } }

        [SerializeField]
        bool m_TrackRotation = true;
        /// <summary>Gets or sets whether this interactable should track the rotation of the interactor.</summary>
        public bool trackRotation { get { return m_TrackRotation; } set { m_TrackRotation = value; } }

        [SerializeField]
        bool m_SmoothRotation;
        /// <summary>Gets or sets Apply smoothing to the follow rotation of the object.</summary>
        public bool smoothRotation { get { return m_SmoothRotation; } set { m_SmoothRotation = value; } }

        [SerializeField, Range(0.0f, 20.0f)]
        float m_SmoothRotationAmount = k_DefaultSmoothingAmount;
        /// <summary>Gets or sets the smoothing applied to the object's rotation when following.</summary>
        public float smoothRotationAmount { get { return m_SmoothRotationAmount; } set { m_SmoothRotationAmount = value; } }

        [SerializeField, Range(0.0f, 1.0f)]
        float m_TightenRotation = k_DefaultTighteningAmount;
        /// <summary>Gets or sets the maximum follow rotation difference when using smoothing.</summary>
        public float tightenRotation { get { return m_TightenRotation; } set { m_TightenRotation = value; } }

        [SerializeField]
        bool m_ThrowOnDetach = true;
        /// <summary>Gets or sets whether the object inherits the interactor's velocity when released.</summary>
        public bool throwOnDetach { get { return m_ThrowOnDetach; } set { m_ThrowOnDetach = value; } }

        [SerializeField]
        float m_ThrowSmoothingDuration = k_DefaultThrowSmoothingDuration;
        /// <summary>Gets or sets the time period to average thrown velocity over</summary>
        public float throwSmoothingDuration { get { return m_ThrowSmoothingDuration; } set { m_ThrowSmoothingDuration = value; } }

        [SerializeField]
        [Tooltip("The curve to use to weight velocity smoothing (most recent frames to the right.")]
        AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

        [SerializeField]
        float m_ThrowVelocityScale = k_DefaultThrowVelocityScale;
        /// <summary>Gets or set the the velocity scale used when throwing.</summary>
        public float throwVelocityScale { get { return m_ThrowVelocityScale; } set { m_ThrowVelocityScale = value; } }

        [SerializeField]
        float m_ThrowAngularVelocityScale = k_DefaultThrowAngularVelocityScale;
        /// <summary>Gets or set the the angular velocity scale used when throwing.</summary>
        public float throwAngularVelocityScale { get { return m_ThrowAngularVelocityScale; } set { m_ThrowAngularVelocityScale = value; } }

        [SerializeField]
        bool m_GravityOnDetach;
        /// <summary>Gets or sets whether object has gravity when released.</summary>
        public bool gravityOnDetach { get { return m_GravityOnDetach; } set { m_GravityOnDetach = value; } }

        // RigidBody's previous settings
        bool m_WasKinematic;
        bool m_UsedGravity;

        // Interactor
        XRBaseInteractor m_SelectingInteractor;

        // Point we are attaching to on this Interactable (in interactor's attach's coordinate space)
        Vector3 m_InteractorLocalPosition;
        Quaternion m_InteractorLocalRotation;

        // Point we are moving towards each frame (eventually will be at Interactor's attach point)
        Vector3 m_TargetWorldPosition;
        Quaternion m_TargetWorldRotation;

        float m_CurrentAttachEaseTime;
        XRBaseInteractable.MovementType m_CurrentMovementType;

        bool m_DetachInLateUpdate;
        Vector3 m_DetachVelocity;
        Vector3 m_DetachAngularVelocity;

        int m_ThrowSmoothingCurrentFrame;
        float[] m_ThrowSmoothingFrameTimes = new float[k_ThrowSmoothingFrameCount];
        Vector3[] m_ThrowSmoothingVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
        Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];

        Rigidbody m_RigidBody;
        Vector3 m_LastPosition;
        Quaternion m_LastRotation;

        protected override void Awake()
        {
            base.Awake();

            m_CurrentMovementType = m_MovementType;
            if (m_RigidBody == null)
                m_RigidBody = GetComponent<Rigidbody>();
            if (m_RigidBody == null)
                Debug.LogWarning("Grab Interactable does not have a required RigidBody.", this);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            switch (updatePhase)
            {
                //
                // during Fixed update we want to perform any physics based updates (eg: Kinematic or VelocityTracking)
                // the call to MoveToTarget will perform the correct the type of update depending on the update phase
                //
                case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                    {
                        if (isSelected)
                        {
                            if (m_CurrentMovementType == MovementType.Kinematic)
                            {
                                PerformKinematicUpdate(Time.unscaledDeltaTime, updatePhase);
                            }
                            else if (m_CurrentMovementType == MovementType.VelocityTracking)
                            {
                                PerformVelocityTrackingUpdate(Time.unscaledDeltaTime, updatePhase);
                            }
                        }
                    }
                    break;
                //
                // during Dynamic update we want to perform any GameObject based manipulation (eg: Instantaneous) 
                // the call to MoveToTarget will perform the correct the type of update depending on the update phase
                //
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                    {
                        if (isSelected)
                        {
                            UpdateTarget(Time.unscaledDeltaTime);
                            SmoothVelocityUpdate();
                            if(m_CurrentMovementType == MovementType.Instantaneous)
                            {
                                PerformInstantaneousUpdate(Time.unscaledDeltaTime, updatePhase);
                            }
                        }
                    }
                    break;
                //
                // during OnBeforeUpdate we want to perform any last minute GameObject position changes before rendering. (eg: Instantaneous) 
                // the call to MoveToTarget will perform the correct the type of update depending on the update phase
                //
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    {
                        if (isSelected)
                        {
                            UpdateTarget(Time.unscaledDeltaTime);
                            if (m_CurrentMovementType == MovementType.Instantaneous)
                            {
                                PerformInstantaneousUpdate(Time.unscaledDeltaTime, updatePhase);
                            }
                        }
                    }
                    break;
                //
                // Late update is only used to handle detach as late as possible.
                //
                case XRInteractionUpdateOrder.UpdatePhase.Late:
                    {
                        if (m_DetachInLateUpdate)
                        {
                            if (!m_SelectingInteractor)
                                Detach();
                            m_DetachInLateUpdate = false;
                        }
                    }
                    break;
            }
        }


        // Calculate the world position/rotation to place this object at when selected
        Vector3 GetWorldAttachPosition(XRBaseInteractor interactor)
        {
            return interactor.attachTransform.position + interactor.attachTransform.rotation * m_InteractorLocalPosition;
        }
        Quaternion GetWorldAttachRotation(XRBaseInteractor interactor)
        {
            return interactor.attachTransform.rotation * m_InteractorLocalRotation;
        }

        void UpdateTarget(float timeDelta)
        {
            if (m_AttachEaseInTime > 0.0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
            {
                var currentAttachDelta = m_CurrentAttachEaseTime / m_AttachEaseInTime;
                m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, GetWorldAttachPosition(m_SelectingInteractor), currentAttachDelta);
                m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, GetWorldAttachRotation(m_SelectingInteractor), currentAttachDelta);
                m_CurrentAttachEaseTime += Time.unscaledDeltaTime;
            }
            else
            {
                if (m_SmoothPosition)
                {
                    m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, GetWorldAttachPosition(m_SelectingInteractor), m_SmoothPositionAmount * timeDelta);
                    m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, GetWorldAttachPosition(m_SelectingInteractor), m_TightenPosition);
                }
                else
                    m_TargetWorldPosition = GetWorldAttachPosition(m_SelectingInteractor);

                if (m_SmoothRotation)
                {
                    m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, GetWorldAttachRotation(m_SelectingInteractor), m_SmoothRotationAmount * timeDelta);
                    m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, GetWorldAttachRotation(m_SelectingInteractor), m_TightenRotation);
                }
                else
                    m_TargetWorldRotation = GetWorldAttachRotation(m_SelectingInteractor);
            }
        }

        void PerformInstantaneousUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                if (trackPosition)
                {
                    transform.position = m_TargetWorldPosition;
                }
                if (trackRotation)
                {
                    transform.rotation = m_TargetWorldRotation;
                }
            }
        }

        void PerformKinematicUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                if (trackPosition)
                {
                    var positionDelta = m_TargetWorldPosition - m_RigidBody.worldCenterOfMass;
                    m_RigidBody.velocity = Vector3.zero;
                    m_RigidBody.MovePosition(m_RigidBody.position + positionDelta);
                }
                if (trackRotation)
                {
                    m_RigidBody.angularVelocity = Vector3.zero;
                    m_RigidBody.MoveRotation(m_TargetWorldRotation);
                }
            }
        }

        void PerformVelocityTrackingUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                // Do velocity tracking
                if (trackPosition)
                {
                    // scale initialized velocity by prediction factor
                    m_RigidBody.velocity *= k_VelocityPredictionFactor;
                    var posDelta = m_TargetWorldPosition - m_RigidBody.worldCenterOfMass;
                    var velocity = posDelta / timeDelta;

                    if (!float.IsNaN(velocity.x))
                        m_RigidBody.velocity += velocity;
                }

                // Do angular velocity tracking
                if (trackRotation)
                {
                    // scale initialized velocity by prediction factor
                    m_RigidBody.angularVelocity *= k_VelocityPredictionFactor;
                    var rotationDelta = m_TargetWorldRotation * Quaternion.Inverse(m_RigidBody.rotation);
                    float angleInDegrees; Vector3 rotationAxis;
                    rotationDelta.ToAngleAxis(out angleInDegrees, out rotationAxis);
                    if (angleInDegrees > 180)
                        angleInDegrees -= 360;

                    if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon)
                    {
                        var angularVelocity = (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / timeDelta;
                        if (!float.IsNaN(angularVelocity.x))
                            m_RigidBody.angularVelocity += angularVelocity * k_AngularVelocityDamping;
                    }
                }
            }
        }

        void Teleport(Transform teleportTransform)
        {
            if (trackPosition)
            {
                var posDelta = teleportTransform.position - m_RigidBody.worldCenterOfMass;
                m_RigidBody.velocity = Vector3.zero;
                transform.Translate(posDelta, Space.World);
            }
            if (trackRotation)
            {
                m_RigidBody.angularVelocity = Vector3.zero;
                m_RigidBody.transform.rotation = teleportTransform.rotation;
            }
        }

        void Detach()
        {
            if (m_ThrowOnDetach)
            {
                m_RigidBody.velocity = m_DetachVelocity;
                m_RigidBody.angularVelocity = m_DetachAngularVelocity;
            }
        }

        // In order to move the Interactable to the Interactor we need to
        // calculate the Interactable attach point in the coordinate system of the
        // Interactor's attach point.
        void UpdateInteractorLocalPose(XRBaseInteractor interactor)
        {
            var attachTransform = m_AttachTransform ? m_AttachTransform : transform;
            var attachPosition = m_AttachTransform ? m_AttachTransform.position : m_RigidBody.worldCenterOfMass;
            var attachOffset = m_RigidBody.worldCenterOfMass - attachPosition;
            var localAttachOffset = attachTransform.InverseTransformDirection(attachOffset);

            var inverseLocalScale = interactor.attachTransform.lossyScale;            
            inverseLocalScale = new Vector3(1f / inverseLocalScale.x, 1f / inverseLocalScale.y, 1f / inverseLocalScale.z);
            localAttachOffset.Scale(inverseLocalScale);

            m_InteractorLocalPosition = localAttachOffset;
            m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(m_RigidBody.rotation) * attachTransform.rotation);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
		protected internal override void OnSelectEnter(XRBaseInteractor interactor)
		{
            if (!interactor)
                return;
            base.OnSelectEnter(interactor);

            m_SelectingInteractor = interactor;
            transform.parent = null;

            // special case where the interactor will override this objects movement type (used for Sockets and other absolute interactors)
            m_CurrentMovementType = (interactor.selectedInteractableMovementTypeOverride != null) ? interactor.selectedInteractableMovementTypeOverride.Value : m_MovementType;

            // remember RigidBody settings and setup to move
            m_WasKinematic = m_RigidBody.isKinematic;               
            m_UsedGravity = m_RigidBody.useGravity;
            m_RigidBody.isKinematic = (m_CurrentMovementType == XRBaseInteractable.MovementType.Kinematic);
            m_RigidBody.useGravity = false;
            m_RigidBody.drag = 0f;
            m_RigidBody.angularDrag = 0f;
 
            // forget if we have previous detach velocities
            m_DetachVelocity = m_DetachAngularVelocity = Vector3.zero;

            UpdateInteractorLocalPose(interactor);
            
            var teleportOnSelect = false;
            if (teleportOnSelect)
                Teleport(m_SelectingInteractor.attachTransform);
            else if (m_AttachEaseInTime > 0.0f)
            {
                m_TargetWorldPosition = m_RigidBody.worldCenterOfMass;
                m_TargetWorldRotation = transform.rotation;
                m_CurrentAttachEaseTime = 0f;
            }

            SmoothVelocityStart();
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the selection.</param>
		protected internal override void OnSelectExit(XRBaseInteractor interactor)
		{
            base.OnSelectExit(interactor);

            // reset RididBody settings
            m_RigidBody.isKinematic = m_WasKinematic;
            m_RigidBody.useGravity = m_UsedGravity | m_GravityOnDetach;

            m_CurrentMovementType = m_MovementType;
            m_SelectingInteractor = null;
            m_DetachInLateUpdate = true;
            SmoothVelocityEnd();
        }

        /// <summary>
        /// Determines if this interactable can be hovered by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>True if hovering is valid this frame, False if not.</returns>
        public override bool IsHoverableBy(XRBaseInteractor interactor) 
        { 
            return true;
        }

        /// <summary>
        /// Determines if this interactable can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            return !m_SelectingInteractor || m_SelectingInteractor == interactor || !m_SelectingInteractor.isSelectExclusive;
        }

        //
        // velocity smoothing
        //

        void SmoothVelocityStart()
        {
            if (!m_SelectingInteractor)
                return;
            m_LastPosition = m_SelectingInteractor.attachTransform.position;
            m_LastRotation = m_SelectingInteractor.attachTransform.rotation;
            Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
            Array.Clear(m_ThrowSmoothingVelocityFrames, 0, m_ThrowSmoothingVelocityFrames.Length);
            Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
            m_ThrowSmoothingCurrentFrame = 0;
        }

        void SmoothVelocityEnd()
        {
            if (m_ThrowOnDetach)
            {
                Vector3 smoothedVelocity = getSmoothedVelocityValue(m_ThrowSmoothingVelocityFrames);
                Vector3 smoothedAngularVelocity = getSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
                m_DetachVelocity = smoothedVelocity * m_ThrowVelocityScale;
                m_DetachAngularVelocity = smoothedAngularVelocity * m_ThrowAngularVelocityScale;
            }
        }

        void SmoothVelocityUpdate()
        {
            if (!m_SelectingInteractor)
                return;
            m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
            m_ThrowSmoothingVelocityFrames[m_ThrowSmoothingCurrentFrame] = (m_SelectingInteractor.attachTransform.position - m_LastPosition) / Time.deltaTime;
            
            Quaternion VelocityDiff = (m_SelectingInteractor.attachTransform.rotation * Quaternion.Inverse(m_LastRotation));
            m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = (new Vector3(Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.x), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.y), Mathf.DeltaAngle(0, VelocityDiff.eulerAngles.z))
                / Time.deltaTime) * Mathf.Deg2Rad;            

            m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % k_ThrowSmoothingFrameCount;
            m_LastPosition = m_SelectingInteractor.attachTransform.position;
            m_LastRotation = m_SelectingInteractor.attachTransform.rotation;
        }

        Vector3 getSmoothedVelocityValue(Vector3[] velocityFrames)
        {
            Vector3 calcVelocity = new Vector3();
            int frameCounter = 0;
            float totalWeights = 0.0f;
            for (; frameCounter < k_ThrowSmoothingFrameCount; frameCounter++)
            {
                int frameIdx = (((m_ThrowSmoothingCurrentFrame - frameCounter - 1) % k_ThrowSmoothingFrameCount) + k_ThrowSmoothingFrameCount) % k_ThrowSmoothingFrameCount;
                if (m_ThrowSmoothingFrameTimes[frameIdx] == 0.0f)
                    break;

                float timeAlpha = (Time.time - m_ThrowSmoothingFrameTimes[frameIdx]) / m_ThrowSmoothingDuration;
                float velocityWeight = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1.0f - timeAlpha, 0.0f, 1.0f));
                calcVelocity += velocityFrames[frameIdx] * velocityWeight;
                totalWeights += velocityWeight;
                if (Time.time - m_ThrowSmoothingFrameTimes[frameIdx] > m_ThrowSmoothingDuration)
                    break;
            }
            if (totalWeights > 0.0f)
                return calcVelocity / totalWeights;
            else
                return Vector3.zero;
        }
    }
}