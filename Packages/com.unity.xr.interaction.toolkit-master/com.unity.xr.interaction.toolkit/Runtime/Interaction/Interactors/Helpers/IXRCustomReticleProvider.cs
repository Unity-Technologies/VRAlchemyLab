using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{

    /// <summary>An interface that allows interactables to request that an interactor use a custom reticle</summary>
    public interface IXRCustomReticleProvider
    {
        bool AttachCustomReticle(GameObject reticleInstance);
        bool RemoveCustomReticle();
    }
}
