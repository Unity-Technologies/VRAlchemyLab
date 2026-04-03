Shader "FullScreen/LiquidCompositing_URP"
{
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma fragment FullScreenPass
    #pragma target 4.5

    // Includes URP
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

    TEXTURE2D_X(_CameraOpaqueTexture);
    SAMPLER(sampler_CameraOpaqueTexture);

    TEXTURE2D_X(_LiquidTex);
    SAMPLER(sampler_LiquidTex);

    float4 _CameraOpaqueTexture_TexelSize;

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    Varyings Vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
    }

    float4 FullScreenPass(Varyings IN) : SV_Target
    {
        // Load depth (URP version)
        float depth = SampleSceneDepth(IN.uv);

        // Sample camera color
        float3 cameraColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, IN.uv);

        // Sample your liquid buffer (replacement for HDRP SampleCustomColor)
        float4 liquidColor = SAMPLE_TEXTURE2D_X(_LiquidTex, sampler_LiquidTex, IN.uv);

        // Same alpha logic as HDRP version
        liquidColor.a = saturate(sign(liquidColor.a - 0.5));

        return liquidColor;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "LiquidCompositingURP"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            ENDHLSL
        }
    }

    Fallback Off
}
