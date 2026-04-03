using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class LiquidBlurComputePass : ScriptableRenderPass
{
    ComputeShader compute;
    int kernelH;
    int kernelV;
    bool enabled;

    class PassData
    {
        public ComputeShader compute;
        public int kernelH;
        public int kernelV;
        public TextureHandle source;
        public TextureHandle temp;
        public int width;
        public int height;
        public Vector2 texelSize;
    }

    public LiquidBlurComputePass(ComputeShader compute, bool enabled)
    {
        this.compute = compute;
        this.enabled = enabled;

        kernelH = compute.FindKernel("BlurHorizontal");
        kernelV = compute.FindKernel("BlurVertical");

        renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (!enabled || compute == null)
            return;

        var camData = frameData.Get<UniversalCameraData>();

        using (var builder = renderGraph.AddComputePass<PassData>("LiquidBlurComputePass", out var passData))
        {
            passData.compute = compute;
            passData.kernelH = kernelH;
            passData.kernelV = kernelV;

            // Source = camera color buffer
            passData.source = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                camData.cameraTargetDescriptor,
                "_LiquidBlurSource",
                true
            );

            // Temp = downsample
            var desc = camData.cameraTargetDescriptor;
            desc.width /= 2;
            desc.height /= 2;

            passData.temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                desc,
                "_LiquidBlurTemp",
                true
            );

            passData.width = desc.width;
            passData.height = desc.height;
            passData.texelSize = new Vector2(1f / desc.width, 1f / desc.height);

            // 🔥 CRUCIAL : déclarer les textures utilisées
            builder.UseTexture(passData.source, AccessFlags.ReadWrite);
            builder.UseTexture(passData.temp, AccessFlags.ReadWrite);

            builder.SetRenderFunc((PassData data, ComputeGraphContext ctx) =>
            {
                ctx.cmd.SetComputeVectorParam(data.compute, "_TexelSize", data.texelSize);

                int groupsX = Mathf.CeilToInt(data.width / 8f);
                int groupsY = Mathf.CeilToInt(data.height / 8f);

                // Horizontal
                ctx.cmd.SetComputeTextureParam(data.compute, data.kernelH, "_Source", data.source);
                ctx.cmd.SetComputeTextureParam(data.compute, data.kernelH, "_Result", data.temp);
                ctx.cmd.DispatchCompute(data.compute, data.kernelH, groupsX, groupsY, 1);

                // Vertical
                ctx.cmd.SetComputeTextureParam(data.compute, data.kernelV, "_Source", data.temp);
                ctx.cmd.SetComputeTextureParam(data.compute, data.kernelV, "_Result", data.source);
                ctx.cmd.DispatchCompute(data.compute, data.kernelV, groupsX, groupsY, 1);
            });
        }
    }
}
