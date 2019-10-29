Shader "RDSystem/LiquidDrop"
{
    Properties
    {
        _Du("Diffusion (u)", Range(0, 1)) = 1
        _Dv("Diffusion (v)", Range(0, 1)) = 0.4
        _Feed("Feed", Range(0, 0.1)) = 0.05
		_Add("Add", Range(0, 1)) = 0.05
		_Position("Position", Vector) = (0.5,0.5,0,0)
		_Color1("Color1", Color) = (1.0, 0, 0, 1)
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"

    half _Du, _Dv;
	half _Feed;
	half _Seed;
	half _Add;
	float4 _Position;
	half3 _Color1;

    half4 frag(v2f_customrendertexture i) : SV_Target
    {
        float tw = 1 / _CustomRenderTextureWidth;
        float th = 1 / _CustomRenderTextureHeight;

        float2 uv = i.globalTexcoord;
        float4 duv = float4(tw, th, -tw, 0);

        half q = tex2D(_SelfTexture2D, uv).w;

        half dq = -q;
        dq += tex2D(_SelfTexture2D, uv - duv.xy).w * 0.05;
        dq += tex2D(_SelfTexture2D, uv - duv.wy).w * 0.20;
        dq += tex2D(_SelfTexture2D, uv - duv.zy).w * 0.05;
        dq += tex2D(_SelfTexture2D, uv + duv.zw).w * 0.20;
        dq += tex2D(_SelfTexture2D, uv + duv.xw).w * 0.20;
        dq += tex2D(_SelfTexture2D, uv + duv.zy).w * 0.05;
        dq += tex2D(_SelfTexture2D, uv + duv.wy).w * 0.20;
        dq += tex2D(_SelfTexture2D, uv + duv.xy).w * 0.05;

        half ABB = q * q * q;

        q += float(dq * _Dv + ABB - _Feed * q);
		q += _Add;
		q = saturate(q);

		half3 color = tex2D(_SelfTexture2D, i.globalTexcoord).xyz;
		color = lerp(color, _Color1, q);

        return half4(color, q);
    }

	half4 start(v2f_customrendertexture  i) : SV_Target
	{
		half4 color = tex2D(_SelfTexture2D, i.globalTexcoord);

		float pct = 0.0;
		// a. The DISTANCE from the pixel to the center
		pct = saturate(1.0f - distance(i.globalTexcoord.xy, _Position.xy) * 10.0f);

		return half4(color.xyz, pct);
	}

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "Update"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }

		Pass
		{
			Name "Start"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment start
			ENDCG
		}
    }
}