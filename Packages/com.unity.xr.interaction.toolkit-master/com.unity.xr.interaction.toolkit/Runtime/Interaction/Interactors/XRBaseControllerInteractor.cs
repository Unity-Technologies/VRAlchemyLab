using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Abstract base class from which all interactors that are controller driven derive.
    /// This class hooks into the interaction system (via XRInteractionManager) and provides base virtual methods for handling
    /// hover and selection.  Additionally, this class provides functionality for checking the controller's selection status
    /// and hiding the controller on selection.
    /// </summary>
    [RequireComponent(typeof(XRController))]
    public abstract class XRBaseControllerInteractor : XRBaseInteractor
    {
        [SerializeField]
        bool m_ToggleSelect;
        /// <summary>Gets or sets whether this interactor toggles selection on button press (rather than selection on press).</summary>
        public bool toggleSelect { get { return m_ToggleSelect; } set { m_ToggleSelect = value; } }

        [SerializeField]
        bool m_HideControllerOnSelect;
        /// <summary>Gets or sets whether this interactor should hide the controller on selection.</summary>
        public bool hideControllerOnSelect
        {
            get { return m_HideControllerOnSelect; }
            set
            {
                m_HideControllerOnSelect = value;
                if (!m_HideControllerOnSelect)
                    m_Controller.hideControllerModel = false;
            }
        }
       
        [SerializeField]
        bool m_PlayAudioClipOnSelectEnter;
        /// <summary>Gets or sets whether to play AudioClip on Select Enter.</summary>
        public bool playAudioClipOnSelectEnter { get { return m_PlayAudioClipOnSelectEnter; } set { m_PlayAudioClipOnSelectEnter = value; } }

        [SerializeField]
        AudioClip m_AudioClipForOnSelectEnter;
        /// <summary>Gets or sets the AudioClip to play on Select Enter.</summary>
        public AudioClip AudioClipForOnSelectEnter { get { return m_AudioClipForOnSelectEnter; } set { m_AudioClipForOnSelectEnter = value; } }

        [SerializeField]
        bool m_PlayAudioClipOnSelectExit;
        /// <summary>Gets or sets whether to play AudioClip on Select Exit.</summary>
        public bool playAudioClipOnSelectExit { get { return m_PlayAudioClipOnSelectExit; } set { m_PlayAudioClipOnSelectExit = value; } }

        [SerializeField]
        AudioClip m_AudioClipForOnSelectExit;
        /// <summary>Gets or sets the AudioClip to play on Select Exit.</summary>
        public AudioClip AudioClipForOnSelectExit { get { return m_AudioClipForOnSelectExit; } set { m_AudioClipForOnSelectExit = value; } }

        [SerializeField]
        bool m_PlayAudioClipOnHoverEnter;
        /// <summary>Gets or sets whether to play AudioClip on Hover Enter.</summary>
        public bool playAudioClipOnHoverEnter { get { return m_PlayAudioClipOnHoverEnter; } set { m_PlayAudioClipOnHoverEnter = value; } }

        [SerializeField]
        AudioClip m_AudioClipForOnHoverEnter;
        /// <summary>Gets or sets the AudioClip to play on Hover Enter.</summary>
        public AudioClip AudioClipForOnHoverEnter { get { return m_AudioClipForOnHoverEnter; } set { m_AudioClipForOnHoverEnter = value; } }

        [SerializeField]
        bool m_PlayAudioClipOnHoverExit;
        /// <summary>Gets or sets whether to play AudioClip on Hover Exit.</summary>
        public bool playAudioClipOnHoverExit { get { return m_PlayAudioClipOnHoverExit; } set { m_PlayAudioClipOnHoverExit = value; } }

        [SerializeField]
        AudioClip m_AudioClipForOnHoverExit;
        /// <summary>Gets or sets the AudioClip to play on Hover Exit.</summary>
        public AudioClip AudioClipForOnHoverExit { get { return m_AudioClipForOnHoverExit; } set { m_AudioClipForOnHoverExit = value; } }

        [SerializeField]
        bool m_PlayHapticsOnSelectEnter;
        /// <summary>Gets or sets whether to play haptics on SelectEnter.</summary>
        public bool playHapticsOnSelectEnter { get { return m_PlayHapticsOnSelectEnter; } set { m_PlayHapticsOnSelectEnter = value; } }

        [SerializeField]
        float m_HapticSelectEnterIntensity;
        /// <summary>Gets or sets the Haptics intensity to play on SelectEnter.</summary>
        public float hapticSelectEnterIntensity{ get { return m_HapticSelectEnterIntensity; } set { m_HapticSelectEnterIntensity= value; } }

        [SerializeField]
        float m_HapticSelectEnterDuration;
        /// <summary>Gets or sets the Haptics duration to play on SelectEnter.</summary>
        public float hapticSelectEnterDuration { get { return m_HapticSelectEnterDuration; } set { m_HapticSelectEnterDuration = value; } }

        [SerializeField]
        bool m_PlayHapticsOnSelectExit;
        /// <summary>Gets or sets whether to play haptics on SelectExit.</summary>
        public bool playHapticsOnSelectExit { get { return m_PlayHapticsOnSelectExit; } set { m_PlayHapticsOnSelectExit = value; } }

        [SerializeField]
        float m_HapticSelectExitIntensity;
        /// <summary>Gets or sets the Haptics intensity to play on SelectExit.</summary>
        public float hapticSelectExitIntensity{ get { return m_HapticSelectExitIntensity; } set { m_HapticSelectExitIntensity= value; } }

        [SerializeField]
        float m_HapticSelectExitDuration;
        /// <summary>Gets or sets the Haptics duration to play on SelectExit.</summary>
        public float hapticSelectExitDuration { get { return m_HapticSelectExitDuration; } set { m_HapticSelectExitDuration = value; } }

        [SerializeField]
        bool m_PlayHapticsOnHoverEnter;
        /// <summary>Gets or sets whether to play haptics on HoverEnter.</summary>
        public bool playHapticsOnHoverEnter { get { return m_PlayHapticsOnHoverEnter; } set { m_PlayHapticsOnHoverEnter = value; } }

        [SerializeField]
        float m_HapticHoverEnterIntensity;
        /// <summary>Gets or sets the Haptics intensity to play on HoverEnter.</summary>
        public float hapticHoverEnterIntensity { get { return m_HapticHoverEnterIntensity; } set { m_HapticHoverEnterIntensity = value; } }

        [SerializeField]
        float m_HapticHoverEnterDuration;
        /// <summary>Gets or sets the Haptics duration to play on HoverEnter.</summary>
        public float hapticHoverEnterDuration { get { return m_HapticHoverEnterDuration; } set { m_HapticHoverEnterDuration = value; } }

        [SerializeField]
        bool m_PlayHapticsOnHoverExit;
        /// <summary>Gets or sets whether to play haptics on HoverExit.</summary>
        public bool playHapticsOnHoverExit { get { return m_PlayHapticsOnHoverExit; } set { m_PlayHapticsOnHoverExit = value; } }

        [SerializeField]
        float m_HapticHoverExitIntensity;
        /// <summary>Gets or sets the Haptics intensity to play on HoverExit.</summary>
        public float hapticHoverExitIntensity { get { return m_HapticHoverExitIntensity; } set { m_HapticHoverExitIntensity = value; } }

        [SerializeField]
        float m_HapticHoverExitDuration;
        /// <summary>Gets or sets the Haptics duration to play on HoverExit.</summary>
        public float hapticHoverExitDuration { get { return m_HapticHoverExitDuration; } set { m_HapticHoverExitDuration = value; } }

        bool m_ToggleSelectActive;
        XRController m_Controller;
        AudioSource m_EffectsAudioSource;

        protected override void Awake()
        {
            base.Awake();

            // Setup interaction controller (for sending down selection state and input)
            m_Controller = GetComponent<XRController>();
            if (!m_Controller)
                Debug.LogWarning("Could not find XR Controller component.", this);

            // if we are toggling selection and have a starting object, start out holding it
            if (m_ToggleSelect && startingSelectedInteractable != null)
                m_ToggleSelectActive = true;

            if (m_PlayAudioClipOnSelectEnter || m_PlayAudioClipOnSelectExit 
                || m_PlayAudioClipOnHoverEnter || m_PlayAudioClipOnHoverExit )
            {
                CreateEffectsAudioSource();
            }
        }

        void CreateEffectsAudioSource()
        {
            m_EffectsAudioSource = gameObject.AddComponent<AudioSource>();
            m_EffectsAudioSource.loop = false;
            m_EffectsAudioSource.playOnAwake = false;
        }

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            // perform toggling of selection state
            // and activation of selected object on activate
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (m_Controller)
                {
                    if (m_Controller.selectInteractionState.activatedThisFrame)
                        m_ToggleSelectActive = !m_ToggleSelectActive;
                    if (selectTarget && m_Controller.activateInteractionState.activatedThisFrame)
                        selectTarget.OnActivate(this);
                    if (selectTarget && m_Controller.activateInteractionState.deActivatedThisFrame)
                        selectTarget.OnDeactivate(this);
                }
            }
        }

        /// <summary>
        /// Gets whether the selection state is active for this interactor.  This will check if the controller has a valid selection
        /// state or whether toggle selection is currently on and active.
        /// </summary>
        public override bool isSelectActive
        {
            get
            {
                return base.isSelectActive && ((m_Controller && m_Controller.selectInteractionState.active) 
                    || m_ToggleSelect && m_ToggleSelectActive);
            }
        }

        /// <summary>This method is called when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        protected internal override void OnSelectEnter(XRBaseInteractable interactable)
        {
            base.OnSelectEnter(interactable);

            if (m_HideControllerOnSelect && m_Controller)
                m_Controller.hideControllerModel = true;

            if (m_PlayHapticsOnSelectEnter && m_Controller)
                SendHapticImpulse(m_HapticSelectEnterIntensity, m_HapticSelectEnterDuration);

            if (playAudioClipOnSelectEnter && AudioClipForOnSelectEnter != null)
            {
                if (m_EffectsAudioSource == null)
                    CreateEffectsAudioSource();

                m_EffectsAudioSource.PlayOneShot(AudioClipForOnSelectEnter);
            }
        }

        /// <summary>This method is called when the interactor ends selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer selected.</param>
        protected internal override void OnSelectExit(XRBaseInteractable interactable)
        {
            base.OnSelectExit(interactable);

            if (m_Controller)
                m_Controller.hideControllerModel = false;
            
            if (m_PlayHapticsOnSelectExit && m_Controller)
                SendHapticImpulse(m_HapticSelectExitIntensity, m_HapticSelectExitDuration);

            if (playAudioClipOnSelectExit && AudioClipForOnSelectExit != null)
            {
                if (m_EffectsAudioSource == null)
                    CreateEffectsAudioSource();
                m_EffectsAudioSource.PlayOneShot(AudioClipForOnSelectExit);
            }
        }

        /// <summary>This method is called when the interactor first initiates hover of an interactable.</summary>
        /// <param name="interactable">Interactable that is being hovered.</param>
        protected internal override void OnHoverEnter(XRBaseInteractable interactable)
        {
            base.OnHoverEnter(interactable);
            if (m_PlayHapticsOnHoverEnter && m_Controller)
                SendHapticImpulse(m_HapticHoverEnterIntensity, m_HapticHoverEnterDuration);

            if (playAudioClipOnHoverEnter && AudioClipForOnHoverEnter != null)
            {
                if (m_EffectsAudioSource == null)
                    CreateEffectsAudioSource();

                m_EffectsAudioSource.PlayOneShot(AudioClipForOnHoverEnter);
            }
        }

        /// /// <summary>This method is called by the interaction manager 
        /// when the interactor ends hovering over an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer hovered over.</param>
        protected internal override void OnHoverExit(XRBaseInteractable interactable)
        {
            base.OnHoverExit(interactable);
            
            if (m_PlayHapticsOnHoverExit && m_Controller)
                SendHapticImpulse(m_HapticHoverExitIntensity, m_HapticHoverExitDuration);

            if (playAudioClipOnHoverExit && AudioClipForOnHoverExit != null)
            {
                if (m_EffectsAudioSource == null)
                    CreateEffectsAudioSource();

                m_EffectsAudioSource.PlayOneShot(AudioClipForOnHoverExit);
            }
        }

        /// <summary>Play a haptic impulse on the controller if one is available</summary>
        /// <param name="amplitude">Amplitude (from 0.0 to 1.0) to play impulse at.</param>
        /// <param name="duration">Duration (in seconds) to play haptic impulse.</param>
        public bool SendHapticImpulse(float amplitude, float duration)
        {
            if (m_Controller)
                return m_Controller.SendHapticImpulse(amplitude, duration);
            return false;
        }
    }
}

