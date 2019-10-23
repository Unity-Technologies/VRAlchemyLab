using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// XRControllerRecording ScriptableObject stores position, rotation and Interaction state changes from the XRController for playback
    /// </summary>
	[CreateAssetMenu(menuName = "XR/XR Controller Recording")]
	[Serializable, PreferBinarySerialization]
	internal class XRControllerRecording : ScriptableObject 
	{
        [Serializable]
		internal struct Frame
		{
			public double time;
			public Vector3 position;
			public Quaternion rotation;
			public bool selectActive;
            public bool activateActive;
            public bool pressActive;

			public Frame(double time, Vector3 position, Quaternion rotation, bool selectActive, bool activateActive, bool pressActive)
			{
				this.time = time;
				this.position = position;
				this.rotation = rotation;
				this.selectActive = selectActive;
                this.activateActive = activateActive;
                this.pressActive = pressActive;
			}

            public override string ToString ()
            {
                return $"time: {time}, position: {position}, rotation: {rotation}, selectActive: {selectActive}, activateActive: {activateActive}, pressActive: {pressActive}";
            }
		}

		[SerializeField, Tooltip("Frames stored in this recording.")]
        List<Frame> m_Frames = new List<Frame>();
		internal List<Frame> frames { get { return m_Frames; } }

        /// <summary>Gets total playback time for this recording.</summary>
		public double duration { get { return (m_Frames.Count == 0) ? 0.0 : m_Frames[m_Frames.Count-1].time; } }

        internal void AddRecordingFrame(double time, Vector3 position, Quaternion rotation,
            bool selectActive, bool activateActive, bool pressActive)
        {
            frames.Add(new Frame(time, position, rotation, selectActive, activateActive, pressActive));
        }

        internal void InitRecording()
        {
            m_Frames.Clear();
#if UNITY_EDITOR
            Undo.RecordObject(this, "Recording XR Controller");
#endif
        }

        internal void SaveRecording()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}