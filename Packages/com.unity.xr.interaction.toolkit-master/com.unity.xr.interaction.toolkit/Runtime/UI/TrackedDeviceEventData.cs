using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    public class TrackedDeviceEventData : PointerEventData
    {
        public TrackedDeviceEventData(EventSystem eventSystem)
            : base(eventSystem)
        { }

        public Ray ray { get; set; }
        public float maxDistance { get; set; }
    }
}