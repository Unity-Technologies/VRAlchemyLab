using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DepthCapturePass : ScriptableRenderPass
{
    Material depthMat;
    Camera bakeCam;
    bool enabled;

    static readonly ShaderTagId shaderTag = new ShaderTagId("DepthOnly");

    public DepthCapturePass(Material mat, Camera cam, bool enabled)
    {
        this.depthMat = mat;
        this.bakeCam = cam;
        this.enabled = enabled;

        renderPassEvent = RenderPassEvent.AfterRendering;
    }

    class PassData
    {
        public Material depthMat;
        public Camera camera;
        public CullingResults cull;
        public TextureHandle output;
        public RendererListHandle rendererList;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (!enabled)
            return;

        var camData = frameData.Get<UniversalCameraData>();
        var renderData = frameData.Get<UniversalRenderingData>();

        // Ignore bake camera
        if (camData.camera == bakeCam)
            return;

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("DepthCapturePass", out var passData))
        {
            passData.depthMat = depthMat;
            passData.camera = camData.camera;
            passData.cull = renderData.cullResults;

            // Output texture
            TextureDesc desc = new TextureDesc(camData.camera.pixelWidth, camData.camera.pixelHeight)
            {
                colorFormat = GraphicsFormat.R32_SFloat,
                depthBufferBits = DepthBits.None,
                clearBuffer = true,
                clearColor = Color.clear,
                name = "DepthCaptureTexture"
            };

            passData.output = builder.CreateTransientTexture(desc);
            builder.SetRenderAttachment(passData.output, 0);

            // Build RendererListParams (URP 17.5 compatible)
            var sorting = new SortingSettings(passData.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawing = new DrawingSettings(shaderTag, sorting)
            {
                overrideMaterial = passData.depthMat,
                overrideMaterialPassIndex = 0
            };

            var filtering = new FilteringSettings(RenderQueueRange.opaque);

            var rlParams = new RendererListParams(passData.cull, drawing, filtering);
            passData.rendererList = renderGraph.CreateRendererList(rlParams);

            builder.UseRendererList(passData.rendererList);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                ctx.cmd.ClearRenderTarget(true, true, Color.clear);
                ctx.cmd.DrawRendererList(data.rendererList);
            });
        }
    }
}
