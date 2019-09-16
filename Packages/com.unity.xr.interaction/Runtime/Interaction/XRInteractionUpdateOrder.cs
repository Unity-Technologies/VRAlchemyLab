namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// The update order for <c>MonoBehaviour</c>s in XRInteraction.
    /// </summary>
    public static class XRInteractionUpdateOrder
    {
        /// <summary>
        /// The <see cref="XRControllerRecorder"/>'s update order. Should come first.
        /// </summary>
        public const int k_ControllerRecorder = -100;
        
        /// <summary>
        /// The <see cref="XRInteractionManager"/>'s update order. Should come after
        /// the <see cref="XRControllerRecorder"/>.
        /// </summary>
        public const int k_InteractionManager = k_ControllerRecorder + 1;

        /// <summary>
        /// The <see cref="XRPointer"/>'s update order. Should come after
        /// the <see cref="XRInteractionManager"/>.
        /// </summary>
        public const int k_XRPointer = k_InteractionManager + 1;
    }
}
