using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

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

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        if (injectionPoint != CustomPassInjectionPoint.AfterPostProcess)
        {
            Debug.LogError("CustomPassInjectionPoint shouild be set on AfterPostProcess");
            return;
        }
            

        if (render && hdCamera.camera != bakeCamera)
        {
            bakeCamera.TryGetCullingParameters(out var cullingParams);
            cullingParams.cullingOptions = CullingOptions.ShadowCasters;
            cullingResult = renderContext.Cull(ref cullingParams);

            var result = new RendererListDesc(shaderTags, cullingResult, bakeCamera)
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


            var p = GL.GetGPUProjectionMatrix(bakeCamera.projectionMatrix, true);
            Matrix4x4 scaleMatrix = Matrix4x4.identity;
            scaleMatrix.m22 = -1.0f;
            var v = scaleMatrix * bakeCamera.transform.localToWorldMatrix.inverse;
            var vp = p * v;
            cmd.SetGlobalMatrix("_ViewMatrix", v);
            cmd.SetGlobalMatrix("_InvViewMatrix", v.inverse);
            cmd.SetGlobalMatrix("_ProjMatrix", p);
            cmd.SetGlobalMatrix("_InvProjMatrix", p.inverse);
            cmd.SetGlobalMatrix("_ViewProjMatrix", vp);
            cmd.SetGlobalMatrix("_InvViewProjMatrix", vp.inverse);
            cmd.SetGlobalMatrix("_CameraViewProjMatrix", vp);
            cmd.SetGlobalVector("_WorldSpaceCameraPos", Vector3.zero);
            cmd.SetGlobalVector("_ShadowClipPlanes", Vector3.zero);

            CoreUtils.SetRenderTarget(cmd, depthFromCam, ClearFlag.All);

            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(result));
        }
    }

    protected override void Cleanup()
    {
        // Cleanup code
    }
}