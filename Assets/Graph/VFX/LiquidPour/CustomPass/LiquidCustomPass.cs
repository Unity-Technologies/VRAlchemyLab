using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering.RendererUtils;

class LiquidCustomPass : CustomPass
{
    ShaderTagId[] shaderTags;
    [Range(0, 20)]
    public float radius = 4;
    public LayerMask maskLayer = 0;

    Material blurMaterial;// To destroy in the end
    RTHandle downSampleBuffer;
    RTHandle blurBuffer;

    static class ShaderID
    {
        public static readonly int _BlitTexture = Shader.PropertyToID("_BlitTexture");
        public static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        public static readonly int _BlitMipLevel = Shader.PropertyToID("_BlitMipLevel");
        public static readonly int _Radius = Shader.PropertyToID("_Radius");
        public static readonly int _Source = Shader.PropertyToID("_Source");
        public static readonly int _ViewPortSize = Shader.PropertyToID("_ViewPortSize");
    }


    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/FullScreen/BlurPasses"));

        shaderTags = new ShaderTagId[4]
        {
            new ShaderTagId("Forward"),
            new ShaderTagId("ForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("FirstPass"),
        };

        // Allocate the buffers used for the blur in half resolution to save some memory
        downSampleBuffer = RTHandles.Alloc(
            Vector2.one * 0.5f, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
            useDynamicScale: true, name: "DownSampleBuffer"
        );

        blurBuffer = RTHandles.Alloc(
            Vector2.one * 0.5f, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
            useDynamicScale: true, name: "BlurBuffer"
        );


    }

    protected override void Execute(CustomPassContext ctx)
    {
        ctx.hdCamera.camera.TryGetCullingParameters(out var cullingParams);
        cullingParams.cullingMask = (uint)1 << LayerMask.NameToLayer("Liquid") ;
        ctx.cullingResults = ctx.renderContext.Cull(ref cullingParams);

        var result = new RendererListDesc(shaderTags, ctx.cullingResults, ctx.hdCamera.camera)
        {
            rendererConfiguration = PerObjectData.None,
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.BackToFront,
            excludeObjectMotionVectors = false,
            layerMask = 1 << LayerMask.NameToLayer("Liquid"),
        };

        CustomPassUtils.DrawRenderers(ctx, LayerMask.GetMask("Liquid"));
        //CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(result));
        GenerateGaussianMips(ctx);


    }

    // We need the viewport size in our shader because we're using half resolution render targets (and so the _ScreenSize
    // variable in the shader does not match the viewport).
    void SetViewPortSize(CommandBuffer cmd, MaterialPropertyBlock block, RTHandle target)
    {
        Vector2Int scaledViewportSize = target.GetScaledSize(target.rtHandleProperties.currentViewportSize);
        block.SetVector(ShaderID._ViewPortSize, new Vector4(scaledViewportSize.x, scaledViewportSize.y, 1.0f / scaledViewportSize.x, 1.0f / scaledViewportSize.y));
    }
    
    
    void GenerateGaussianMips(CustomPassContext ctx)
    {
        //GetCustomBuffers(out var customColorBuffer, out var _);

        // Downsample
        using (new ProfilingSample(ctx.cmd, "Downsample", CustomSampler.Create("Downsample")))
        {
            // This Blit will automatically downsample the color because our target buffer have been allocated in half resolution
            HDUtils.BlitCameraTexture(ctx.cmd, ctx.customColorBuffer.Value, downSampleBuffer,0, true);
        }

        // Horizontal Blur
        using (new ProfilingSample(ctx.cmd, "H Blur", CustomSampler.Create("H Blur")))
        {
            var hBlurProperties = new MaterialPropertyBlock();
            hBlurProperties.SetFloat(ShaderID._Radius, radius / 4.0f); // The blur is 4 pixel wide in the shader
            hBlurProperties.SetTexture(ShaderID._Source, downSampleBuffer); // The blur is 4 pixel wide in the shader
            SetViewPortSize(ctx.cmd, hBlurProperties, blurBuffer);
            HDUtils.DrawFullScreen(ctx.cmd, blurMaterial, blurBuffer, hBlurProperties, shaderPassId: 0); // Do not forget the shaderPassId: ! or it won't work
        }

        // Copy back the result in the color buffer while doing a vertical blur
        using (new ProfilingSample(ctx.cmd, "V Blur + Copy back", CustomSampler.Create("V Blur + Copy back")))
        {
            var vBlurProperties = new MaterialPropertyBlock();
            // When we use a mask, we do the vertical blur into the downsampling buffer instead of the camera buffer
            // We need that because we're going to write to the color buffer and read from this blured buffer which we can't do
            // if they are in the same buffer
            vBlurProperties.SetFloat(ShaderID._Radius, radius / 4.0f); // The blur is 4 pixel wide in the shader
            vBlurProperties.SetTexture(ShaderID._Source, blurBuffer);
            SetViewPortSize(ctx.cmd, vBlurProperties, ctx.customColorBuffer.Value);
            HDUtils.DrawFullScreen(ctx.cmd, blurMaterial, ctx.customColorBuffer.Value, vBlurProperties, shaderPassId: 1); // Do not forget the shaderPassId: ! or it won't work
        }
       
    }
    


    protected override void Cleanup()
    {
        CoreUtils.Destroy(blurMaterial);
        downSampleBuffer.Release();
        blurBuffer.Release();
    }
}