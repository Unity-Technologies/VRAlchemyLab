using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// MonoBehaviour that drives interaction recording and playback (via XRControllerRecording assets).
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("XR/XR Controller Recorder")]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_ControllerRecorder)]
    public class XRControllerRecorder : MonoBehaviour
    {
        [Header("Input Recording/Playback")]

        [SerializeField, Tooltip("Play back recording upon start.")]
        bool m_PlayOnStart;
        /// <summary>Gets or sets whether this recording will start playing when the component is started.</summary>
        public bool playOnStart { get { return m_PlayOnStart; } set { m_PlayOnStart = value; } }

        [SerializeField, Tooltip("Controller Recording asset for recording and playback of controller events.")]
        XRControllerRecording m_Recording;

        [SerializeField, Tooltip("Controller Recording asset for recording and playback of controller events.")]
        XRController m_Controller;
        /// <summary>Gets or sets whether the controller that this recording uses for recording and playback.</summary>
        public XRController controller { get { return m_Controller; } set { m_Controller = value; } }

        double m_CurrentTime;
        bool m_IsRecording;
        bool m_IsPlaying;
        double m_LastPlaybackTime;
        int m_LastFrameIdx;

        /// <summary>Gets or sets recording asset for this recorder.</summary>
        internal XRControllerRecording recording
        {
            get { return m_Recording; }
            set { m_Recording = value; }
        }

        /// <summary>Gets or sets whether the XRControllerRecorder is currently recording interaction state.</summary>
        public bool isRecording
        {
            get { return m_IsRecording; }
            set
            {
                if (m_IsRecording != value)
                {
                    isPlaying = false;
                    m_CurrentTime = 0.0;
                    if (m_Recording)
                    {
                        if (value)
                            m_Recording.InitRecording();
                        else
                            m_Recording.SaveRecording();
                    }
                    m_IsRecording = value;
                }
            }
        }

        /// <summary>Gets or sets whether the XRControllerRecorder is currently playing back interaction state.</summary>
        public bool isPlaying
        {
            get { return m_IsPlaying; }
            set
            {
                if (m_IsPlaying != value)
                {
                    if (m_Controller)
                        m_Controller.enableInputTracking = !value;
                    isRecording = false;
                    if (m_Recording) ResetPlayback();
                    m_CurrentTime = 0.0;
                    m_IsPlaying = value;
                }
            }
        }

        /// <summary>Gets current recording/playback time.</summary>
        public double currentTime { get { return m_CurrentTime; } }

        /// <summary>Gets total playback time (or 0.0f if no recording).</summary>
        public double duration { get { return (m_Recording) ? m_Recording.duration : 0.0f; } }

        void Awake()
        {
            m_CurrentTime = 0.0;

            if (m_PlayOnStart)
                isPlaying = true;
        }

        void OnDestroy()
        {
            isPlaying = isRecording = false;
        }

        void ToggleRecording()
        {
            isRecording = !isRecording;
        }

        void Update()
        {
            if (isRecording && m_Controller)
            {
                m_Recording.AddRecordingFrame(m_CurrentTime,
                    m_Controller.transform.position, m_Controller.transform.rotation,
                    m_Controller.selectInteractionState.active, m_Controller.activateInteractionState.active, m_Controller.uiPressInteractionState.active);
            }
            else if (isPlaying)
                UpdatePlaybackTime(m_CurrentTime);

            if (isRecording || isPlaying)
                m_CurrentTime += Time.deltaTime;
            if (isPlaying && m_CurrentTime > m_Recording.duration)
                isPlaying = false;
        }

        void UpdateControllerRecordingUpdate(XRControllerRecording.Frame recordingFrame)
        {
            if (m_Controller)
            {
                m_Controller.UpdateControllerPose(recordingFrame.position, recordingFrame.rotation);
                m_Controller.UpdateInteractionType(XRController.InteractionTypes.select, recordingFrame.selectActive);
                m_Controller.UpdateInteractionType(XRController.InteractionTypes.activate, recordingFrame.activateActive);
                m_Controller.UpdateInteractionType(XRController.InteractionTypes.uiPress, recordingFrame.pressActive);
            }
        }

        /// <summary>
        /// Resets the recorder to the start of the clip.
        /// </summary>
        public void ResetPlayback()
        {
            m_LastPlaybackTime = 0.0f;
            m_LastFrameIdx = 0;
        }

        void UpdatePlaybackTime(double playbackTime)
        {
            if (!m_Recording)
                return;



            // look for next frame in order (binary search would be faster but we are only searching from last cached frame index)
            var prevFrame = m_Recording.frames[m_LastFrameIdx];
            var frameIdx = m_LastFrameIdx;
            if(prevFrame.time < playbackTime)
            {
                for (; frameIdx < m_Recording.frames.Count &&
                    m_Recording.frames[frameIdx].time >= m_LastPlaybackTime &&
                    m_Recording.frames[frameIdx].time <= playbackTime;
                ++frameIdx) { }
            }


            // past last frame or on the same frame, don't do anything
            if (frameIdx >= m_Recording.frames.Count)
                return;

            // we passed a valid frame, update our controller
            if (m_Controller)
            {
                var recordingFrame = m_Recording.frames[frameIdx];
                UpdateControllerRecordingUpdate(recordingFrame);
            }

            m_LastFrameIdx = frameIdx;
            m_LastPlaybackTime = playbackTime;
        }
    }
}