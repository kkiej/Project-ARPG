shader "bard/role/circle"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        
        pass
        {
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex                      VS
            #pragma fragment                    PS

            struct VertexIn
            {
                float4 PosL                     : POSITION;
                float2 TexC                     : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 PosH                     : SV_POSITION;
                float2 TexC                     : TEXCOORD0;
            };

            uniform TEXTURE2D(_MainTex);                    uniform SAMPLER(sampler_MainTex);
            uniform half4                                   _PlaneShadowColor;
            
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
            
            VertexOut VS (VertexIn vin)
            {
                VertexOut                       vout;
                vout.PosH                       = TransformObjectToHClip(vin.PosL);
                vout.TexC                       = vin.TexC;
                return vout;
            }

            half4 PS (VertexOut pin) : SV_Target
            {
                half4 color                     = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.TexC);
                color.rgb                       *= _PlaneShadowColor.rgb;
                return color;
            }
            ENDHLSL
        }
    }
}
