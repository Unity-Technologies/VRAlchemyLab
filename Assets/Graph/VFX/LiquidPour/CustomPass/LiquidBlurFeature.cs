using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LiquidBlurFeature : ScriptableRendererFeature
{
    public ComputeShader blurCompute;
    public bool enabled = true;

    LiquidBlurComputePass pass;

    public override void Create()
    {
        if (blurCompute != null)
            pass = new LiquidBlurComputePass(blurCompute, enabled);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!enabled || blurCompute == null)
            return;

        if (renderingData.cameraData.isSceneViewCamera)
            return;

        if (pass == null)
            pass = new LiquidBlurComputePass(blurCompute, enabled);

        renderer.EnqueuePass(pass);
    }
}
