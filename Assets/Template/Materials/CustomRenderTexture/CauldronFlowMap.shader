Shader "Custom RenderTexture/Cauldron Flow Map"
{
	Properties
	{
		_Flowmap ("_Flowmap", 2D) = "white" {}
		_FlowmapTilingAndScrollSpeed("_FlowmapTilingAndScrollSpeed", Vector) = (2.0,2.0,0.1,0.1)
		_FlowmapStrength("_FlowmapStrength", Float) = 0.1
	}
	SubShader
	{
		Lighting Off
		Blend One Zero

		Pass
		{
		CGPROGRAM
		#include "UnityCustomRenderTexture.cginc"
		#pragma vertex CustomRenderTextureVertexShader
		#pragma fragment frag
		#pragma target 3.0

		float4		_FlowmapTilingAndScrollSpeed;
		float		_FlowmapStrength;
		sampler2D	_Flowmap;

		float4 frag(v2f_customrendertexture IN) : COLOR
		{
			float t = _CosTime.w * .5 + .5;
			float2 uv = IN.globalTexcoord.xy * _FlowmapTilingAndScrollSpeed.xy;
			uv += _Time.y * _FlowmapTilingAndScrollSpeed.zw;
			float4 flow = tex2D(_Flowmap, uv);
			float2 blend = lerp(flow.xy, flow.zw, t) * 2.0 - 1.0;
			blend *= _FlowmapStrength;
			return float4(blend, length(blend), 1);
		}
		ENDCG
		}
	}
}
