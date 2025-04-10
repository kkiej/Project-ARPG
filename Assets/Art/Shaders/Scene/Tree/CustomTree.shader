Shader "Custom/Tree"
{
    Properties
    {
        _TopColor("Top Color", Color) = (0, 0.6, 0, 1)
        _BottomColor("Bottom Color", Color) = (0, 0.3, 0, 1)
        _ColorThreshold("Color Threshold", Range(0, 1)) = 0
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularScale("Specular Scale", Range(0, 10)) = 3
        _MainTex("Main Tex", 2D) = "white"{}
        _Intensity("Intensity", Range(0, 1)) = 0.5
        _Distortion("Distortion", Range(0, 5)) = 1
    	_Power("Power", Range(0, 20)) = 10
    	_Scale("Scale", Range(0, 1)) = 0.2
    	_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimOffset("Rim Offset", Range(0, 1)) = 0
		//_RimThreshold("Rim Threshold", Range(0, 1)) = 0
    	//_MinRange("Min Range", Range(0, 1)) = 0.01
    	//_MaxRange("Max Range", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        
        Cull Off
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
        half4 _TopColor;
        half4 _BottomColor;
        half _ColorThreshold;
        half _Intensity;
        half _SpecularScale;
        half4 _SpecularColor;
        half _Distortion;
        half _Power;
        half _Scale;
        half4 _RimColor;
		half _RimOffset;
		//half _RimThreshold;
        //half _MinRange;
        //half _MaxRange;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        struct a2v
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
        	half4 vertexColor : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            half fogCoord : TEXCOORD2;
            float3 positionWS : TEXCOORD3;
        	float4 screenPos : TEXCOORD4;
        	float3 originNormal : TEXCOORD5;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        ENDHLSL
        
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM

            #pragma multi_compile_ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            
            #pragma vertex vert
            #pragma fragment frag
            
            v2f vert (a2v v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
                o.normalWS = normalInputs.normalWS;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionWS = positionInputs.positionWS;
                o.positionCS = positionInputs.positionCS;
                
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
                o.uv = v.uv;

        		o.screenPos = ComputeScreenPos(o.positionCS);
        		o.originNormal = (v.vertexColor.xyz * 2 - 1);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
				float3 viewDirWS = GetCameraPositionWS() - i.positionWS;
            	
                //BaseMap
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            	half viewMask = 1 - saturate(dot(i.originNormal, viewDirWS));
                clip(mainTex.a - 0.5);

                half texColorReverse = 1 - mainTex.r;
                half texcolor = saturate(texColorReverse - texColorReverse * _Intensity + mainTex.r);
                texcolor = lerp(texcolor, 1, texcolor);
                
                half colorMask = (i.normalWS.y * 0.5 + 0.5) * _ColorThreshold;
                colorMask = smoothstep(0, 1, colorMask);
                half3 basemap = lerp(_BottomColor, _TopColor, colorMask).rgb * texcolor;
				
                //Diffuse
                Light mainLight = GetMainLight(shadowCoord);
                half Attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half halfLambert = dot(normalize(i.normalWS), normalize(mainLight.direction)) * 0.5 + 0.5;
				half3 diffuse = halfLambert * Attenuation * mainLight.color;
            	
            	//Fast SSS
            	float3 halfDir = normalize(mainLight.direction + i.normalWS * _Distortion);
            	half sss = pow(saturate(dot(viewDirWS, -halfDir)), _Power) * _Scale;
            	
                //Ambient
                half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

            	//RimColor
            	float2 RimScreenUV = i.screenPos.xy / i.screenPos.w;
				float3 N_VS = normalize(mul((float3x3)UNITY_MATRIX_V, i.normalWS));
				float2 RimOffsetUV = RimScreenUV + N_VS.xy * _RimOffset * 0.01;
				
				float ScreenDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, RimScreenUV);
				float OffsetDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, RimOffsetUV);
								
				float linear01EyeOffsetDepth = Linear01Depth(OffsetDepth, _ZBufferParams);
				float linear01EyeTrueDepth = Linear01Depth(ScreenDepth, _ZBufferParams);
				
				float diff = linear01EyeOffsetDepth - linear01EyeTrueDepth;
            	
				half3 RimColor = saturate(diff * _RimColor.rgb) * _RimColor.a;
            	
            	half3 color = diffuse * basemap + sss * mainLight.color + ambient * 0.2 + RimColor;
                color = MixFog(color, i.fogCoord);
                
                return half4(color, 1);
            }
            ENDHLSL
        }
        
        Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0

			HLSLPROGRAM

			#pragma multi_compile_instancing

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			v2f vert ( a2v v )
			{
				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
                o.normalWS = normalInputs.normalWS;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionWS = positionInputs.positionWS;
                o.positionCS = positionInputs.positionCS;
                
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
                o.uv = v.uv;

        		o.screenPos = ComputeScreenPos(o.positionCS);
        		o.originNormal = (v.vertexColor.xyz * 2 - 1);
                
                return o;
			}

			half4 frag(v2f i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
								
				float4 BaseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				clip(BaseColor.a - 0.5);
				
				return 0;
			}
			ENDHLSL
		}
    	
    	Pass
		{
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM

			#pragma multi_compile_instancing
			
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			
			v2f vert ( a2v v )
			{
				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
                o.normalWS = normalInputs.normalWS;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionWS = positionInputs.positionWS;
                o.positionCS = positionInputs.positionCS;
                
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
                o.uv = v.uv;

        		o.screenPos = ComputeScreenPos(o.positionCS);
        		o.originNormal = (v.vertexColor.xyz * 2 - 1);
                
                return o;
			}

			half4 frag(v2f i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
								
				float4 BaseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				clip(BaseColor.a - 0.5);
				
				return 0;
			}
			ENDHLSL
		}
    }
}
