Shader "bard/role/invisible"
{
    Properties
    {
        _MainTex("MainTex",2D) = "White"{}
        _Alpha("Alpha",Range(0,1)) = 0.5
    }
    
    SubShader
    {
    
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
        
    	Stencil
	    {
	        Ref 2
	        Comp Always
	        Pass Replace 
	    } 
        
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha  OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex VS
            #pragma fragment PS

            struct VertexIn
            {
                float4 PosL                                     : POSITION;
                float2 TexC                                     : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 PosH                                     : SV_POSITION;
                float2 TexC                                     : TEXCOORD0;
                float4 PosS                                     : TEXCOORD1;
            };

            uniform TEXTURE2D(_MainTex);                        uniform SAMPLER(sampler_MainTex);
            uniform TEXTURE2D(_CameraOpaqueTexture);             uniform SAMPLER(sampler_CameraOpaqueTexture);
            
            CBUFFER_START(UnityPerMaterial)
            uniform half                                        _Alpha;
            CBUFFER_END
            
            VertexOut VS (VertexIn vin)
            {
                VertexOut                                       vout;
                vout.PosH                                       = TransformObjectToHClip(vin.PosL);
                vout.TexC                                       = vin.TexC;
                vout.PosS                                       = ComputeScreenPos(vout.PosH);
                return vout;
            }

            half4 PS (VertexOut pin) : SV_Target
            {
                half2 screenUV                                  = pin.PosS.xy / pin.PosS.w;
	            half4 opaqueColor				                = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV);
                half4 color                                     = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.TexC);
                color                                           = lerp(opaqueColor,color,_Alpha);
                return color;
            }
            ENDHLSL
        }
    }
}
