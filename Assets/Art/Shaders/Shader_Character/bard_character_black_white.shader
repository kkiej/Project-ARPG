Shader "Unlit/bard_character_black_white"
{
    Properties
    {
        _MainTex("MainTex",2D) = "white"{}
		[HDR]_OutColor ("外描边颜色", Color) = (0, 0, 0, 1)
        _Outline ("外描边范围", Range(0,0.5)) = 0.0005
    	
    	_DarkStrength("Dark Strength",Range(0,1)) = 0
    	_ColorLight("Light Area Color",color) = (1,1,1,1)
    	_ColorShadow("Shadow Area Color",color) = (1,1,1,1)
    	_FlickerShadowRange("Shadoow Step",Range(0,1)) = 0
        _FlickerFresnelRange("Fresnel Step",Range(0,1)) = 0
    	[Toggle]_UseClip("Use Clip",float) = 1
    }
    SubShader
    {
        HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D(_MainTex);					SAMPLER(sampler_MainTex);
		uniform half3							_DirectionLightPos;
		CBUFFER_START(UnityPerMaterial)
		uniform half4							_ColorLight;
		uniform half4							_ColorShadow;
		uniform half                            _FlickerShadowRange;
		uniform half                            _FlickerFresnelRange;
		uniform half							_UseClip;
		uniform half							_DarkStrength;

		uniform half							_Outline;
		uniform half4							_OutColor;
        CBUFFER_END
        ENDHLSL
    	
        pass
        {
	        Cull Front
	        HLSLPROGRAM
			#pragma vertex VS
			#pragma fragment PS
	            
			struct VertexIn 
	        {
				float4 PosL							: POSITION; 
				float3 NormalL						: NORMAL;
				float2 TexC							: TEXCOORD0;
			};
			struct VertexOut
	        {
			    float4 pos							: SV_POSITION;
				float2 TexC							: TEXCOORD2;
			};
	        
	        
			VertexOut VS (VertexIn vin)
	        {
				VertexOut vout						= (VertexOut)0.0f;
				vin.PosL.xyz						+= vin.NormalL * _Outline * 0.1;
				vout.pos							= TransformObjectToHClip(vin.PosL);
				vout.TexC							= vin.TexC;
				return vout;
			}
			
			float4 PS(VertexOut pin) : SV_Target
	        {
	        	half4 texColor						= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.TexC);
	        	if(_UseClip > 0.5)
	        		clip(texColor.a - 0.5);
				float4 c = _OutColor;
				return c;               
			}
        	
        	ENDHLSL
        }

        pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull Back
            HLSLPROGRAM
			#pragma vertex VS
			#pragma fragment PS
	           
			struct VertexIn 
	        {
				float4 PosL							: POSITION; 
				float3 NormalL						: NORMAL;
				float2 TexC							: TEXCOORD0;
			};
			struct VertexOut
	        {
			    float4 PosH							: SV_POSITION;
				float3 NormalW						: TEXCOORD0;
				float3 PosW							: TEXCOORD1;
				float2 TexC							: TEXCOORD2;
			};
            
			VertexOut VS (VertexIn vin)
	        {
				VertexOut vout						= (VertexOut)0.0f;
				vout.PosH							= TransformObjectToHClip(vin.PosL);
				vout.NormalW						= TransformObjectToWorldNormal(vin.NormalL);
				vout.PosW							= TransformObjectToWorld(vin.PosL);
				vout.TexC							= vin.TexC;
				return vout;
			}
			
			float4 PS(VertexOut pin) : SV_Target
	        {
	        	half4 texColor						= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.TexC);
	        	
	        	if(_UseClip > 0.5)
	        		clip(texColor.a - 0.5);
	        	
	        	half darkColor						= (texColor.r + texColor.g + texColor.b) / 3;
	        	darkColor							= pow(darkColor,5);
	        	darkColor							= saturate(10 * darkColor);
	        	
	        	half NoL							= saturate(dot(normalize(_DirectionLightPos.xyz),normalize(pin.NormalW)));
	        	half3 V								= normalize(_WorldSpaceCameraPos.xyz - pin.PosW);
		        half F								= saturate(dot(normalize(pin.NormalW),normalize(V)));
		        F									= step(pow(1.0 - max(0, F), 1),_FlickerFresnelRange);
	        	half set_Shadow						= 1 - saturate(((NoL - (_FlickerShadowRange-0)) * - 1.0 ) / (_FlickerShadowRange - (_FlickerShadowRange-0)) + 1 );
				set_Shadow							= set_Shadow * F;

	        	texColor							= lerp(_ColorShadow,texColor,set_Shadow);
	        	half4 darkRGBA						= lerp(_ColorShadow, _ColorLight, set_Shadow * darkColor);
	        	half4 outColor						= lerp(texColor, darkRGBA, _DarkStrength);
	        	
				return outColor;
			}
            
            ENDHLSL
        }
    }
}
