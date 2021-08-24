using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RendererUtils;

class DepthCapturePass : CustomPass
{
    public RenderTexture depthFromCam;
    public Material depthMaterial;
    public Camera bakeCamera;
    public bool render;

    ShaderTagId[] shaderTags;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        shaderTags = new ShaderTagId[2]
        {
            new ShaderTagId("DepthForwardOnly"),
            new ShaderTagId("DepthOnly")
        };
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (injectionPoint != CustomPassInjectionPoint.AfterPostProcess)
        {
            Debug.LogError("CustomPassInjectionPoint shouild be set on AfterPostProcess");
            return;
        }
            

        if (render && ctx.hdCamera.camera != bakeCamera && ctx.hdCamera.camera.cameraType != CameraType.SceneView)
        {
            bakeCamera.TryGetCullingParameters(out var cullingParams);
            cullingParams.cullingOptions = CullingOptions.ShadowCasters;
            ctx.cullingResults = ctx.renderContext.Cull(ref cullingParams);

            var result = new RendererListDesc(shaderTags, ctx.cullingResults, bakeCamera)
            {
                rendererConfiguration = PerObjectData.None,
                //renderQueueRange = RenderQueueRange.all,
                renderQueueRange = GetRenderQueueRange(RenderQueueType.AllOpaque),
                sortingCriteria = SortingCriteria.BackToFront,
                excludeObjectMotionVectors = false,
                layerMask = -1,
                overrideMaterial = depthMaterial,
                overrideMaterialPassIndex = depthMaterial.FindPass("ForwardOnly"),
            };

            //renderContext.StereoEndRender(hdCamera.camera);
            ctx.renderContext.ExecuteCommandBuffer(ctx.cmd);
            ctx.cmd.Clear();
            ctx.renderContext.StopMultiEye(ctx.hdCamera.camera);

            var p = GL.GetGPUProjectionMatrix(bakeCamera.projectionMatrix, true);
            Matrix4x4 scaleMatrix = Matrix4x4.identity;
            scaleMatrix.m22 = -1.0f;
            var v = scaleMatrix * bakeCamera.transform.localToWorldMatrix.inverse;
            var vp = p * v;
            ctx.cmd.SetGlobalMatrix("_ViewMatrix", v);
            ctx.cmd.SetGlobalMatrix("_InvViewMatrix", v.inverse);
            ctx.cmd.SetGlobalMatrix("_ProjMatrix", p);
            ctx.cmd.SetGlobalMatrix("_InvProjMatrix", p.inverse);
            ctx.cmd.SetGlobalMatrix("_ViewProjMatrix", vp);
            ctx.cmd.SetGlobalMatrix("_InvViewProjMatrix", vp.inverse);
            ctx.cmd.SetGlobalMatrix("_CameraViewProjMatrix", vp);
            ctx.cmd.SetGlobalVector("_WorldSpaceCameraPos", Vector3.zero);
            ctx.cmd.SetGlobalVector("_ShadowClipPlanes", Vector3.zero);

            CoreUtils.SetRenderTarget(ctx.cmd, depthFromCam, ClearFlag.All);

            //HDUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(result));

            ctx.renderContext.StartMultiEye(ctx.hdCamera.camera);
            ctx.renderContext.ExecuteCommandBuffer(ctx.cmd);
            ctx.cmd.Clear();
        }
    }

    protected override void Cleanup()
    {
        // Cleanup code
    }
}