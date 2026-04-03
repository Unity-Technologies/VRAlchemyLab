Shader "Hidden/URP/BlurPasses"
{
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma fragment Frag
    #pragma target 4.5

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D(_Source);
    SAMPLER(sampler_Source);

    float _Radius;
    float4 _Source_TexelSize; // x = 1/width, y = 1/height

    float4 BlurPixels(float4 taps[9])
    {
        return 0.27343750 * taps[4]
            + 0.21875000 * (taps[3] + taps[5])
            + 0.10937500 * (taps[2] + taps[6])
            + 0.03125000 * (taps[1] + taps[7])
            + 0.00390625 * (taps[0] + taps[8]);
    }

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    Varyings Vert(Attributes IN)
    {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
    }

    // -------------------------
    // HORIZONTAL BLUR
    // -------------------------
    float4 HorizontalBlur(float2 uv)
    {
        float offset = _Radius * _Source_TexelSize.x;

        float4 taps[9];
        for (int i = -4; i <= 4; i++)
        {
            float2 uvOffset = uv + float2(i * offset, 0);
            taps[i + 4] = SAMPLE_TEXTURE2D(_Source, sampler_Source, uvOffset);
        }

        return BlurPixels(taps);
    }

    // -------------------------
    // VERTICAL BLUR
    // -------------------------
    float4 VerticalBlur(float2 uv)
    {
        float offset = _Radius * _Source_TexelSize.y;

        float4 taps[9];
        for (int i = -4; i <= 4; i++)
        {
            float2 uvOffset = uv + float2(0, i * offset);
            taps[i + 4] = SAMPLE_TEXTURE2D(_Source, sampler_Source, uvOffset);
        }

        return BlurPixels(taps);
    }

    // -------------------------
    // FRAGMENT (select pass via keyword)
    // -------------------------
    #pragma multi_compile _ BLUR_HORIZONTAL BLUR_VERTICAL

    float4 Frag(Varyings IN) : SV_Target
    {
        #if defined(BLUR_HORIZONTAL)
            return HorizontalBlur(IN.uv);
        #else
            return VerticalBlur(IN.uv);
        #endif
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Horizontal Blur"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM
                #define BLUR_HORIZONTAL
            ENDHLSL
        }

        Pass
        {
            Name "Vertical Blur"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM
                #define BLUR_VERTICAL
            ENDHLSL
        }
    }

    Fallback Off
}
