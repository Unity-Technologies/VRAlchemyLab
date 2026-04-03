using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthCaptureFeature : ScriptableRendererFeature
{
    public Material depthMaterial;
    public Camera bakeCamera;
    public bool render = true;

    DepthCapturePass pass;

    public override void Create()
    {
        pass = new DepthCapturePass(depthMaterial, bakeCamera, render);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (render)
            renderer.EnqueuePass(pass);
    }
}
