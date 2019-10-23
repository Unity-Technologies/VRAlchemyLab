namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The update order for <c>MonoBehaviour</c>s in XRInteraction.
    /// This is primarily used to control initialization order as the update of interactors / interaction manager / interactables is handled by the
    /// Interaction managers themselves.
    /// 
    /// In this case, we want the OnAwake of the interaction manager to occur first. then 
    /// </summary>
    public static class XRInteractionUpdateOrder
    {

        public const int k_ControllerRecorder = -30000;        
        public const int k_Controllers = k_ControllerRecorder + 1;
        public const int k_ControllerInteractionManager = -1000;
        public const int k_InteractionManager = -100;
        public const int k_XRUIPointer = k_InteractionManager + 1;
        public const int k_Interactors = k_XRUIPointer + 1;
        public const int k_Interactables = k_Interactors + 1;

        public const int k_LineVisual = 100;

        public const int k_BeforeRenderOrder = 100;

        public enum UpdatePhase
        {
            Fixed,
            Dynamic,
            Late,
            OnBeforeRender,
        }
    }

  
}
