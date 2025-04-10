Shader "Custom/Weapon"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_MainColor("MainColor", Color) = (1, 1, 1, 0)
		_ShadowColor("ShadowColor", Color) = (0.5882353,0.7019608,0.8588236,0)
		_ShadowRange("ShadowRange", Range( 0 , 1)) = 0
		_ShadowSmooth("ShadowSmooth", Range( 0 , 1)) = 1
		_BaseColor("Base Color", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Metal("Metal", 2D) = "white" {}
		_SpecularColor("Specular Color", Color) = (1,1,1,0)
		_SpecularScale("Specular Scale", Float) = 10
		_SpecularIntensity("Specular Intensity", Range( 1 , 10)) = 5
		_OutlineColor("Outline Color", Color) = (0.254717,0.1778213,0.1778213,1)
		_OutlineWidth("Outline Width", Range( 0 , 2)) = 0.6
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimOffset("Rim Offset", Range(0, 1)) = 0
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

		Cull Back
		
		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
		struct VertexInput
		{
			float4 positionOS : POSITION;
			float3 normalOS : NORMAL;
			float4 uv : TEXCOORD0;
			float4 tangentOS : TANGENT;
			float4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct VertexOutput
		{
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 TtoW0 : TEXCOORD1;
			float4 TtoW1 : TEXCOORD2;
			float4 TtoW2 : TEXCOORD3;
			
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};
		
		CBUFFER_START(UnityPerMaterial)
		float4 _OutlineColor;
		float4 _Mask_ST;
		float4 _Normal_ST;
		float4 _ShadowColor;
		float4 _MainColor;
		float4 _BaseColor_ST;
		float4 _SpecularColor;
		float _OutlineWidth;
		float _ShadowSmooth;
		float _ShadowRange;
		float _SpecularScale;
		float _SpecularIntensity;
		float4 _RimColor;
		float _RimOffset;
		float _RimThreshold;
		CBUFFER_END

		half4 _DepthTextureSourceSize;
		half _CameraAspect;
		half _CameraFOV;
		
		TEXTURE2D(_Mask);
		SAMPLER(sampler_Mask);
		TEXTURE2D(_Metal);
		SAMPLER(sampler_Metal);
		TEXTURE2D(_Normal);
		SAMPLER(sampler_Normal);
		TEXTURE2D(_BaseColor);
		SAMPLER(sampler_BaseColor);
		ENDHLSL
		
		Pass 
		{
            Name "OutLine"
            Tags { "LightMode"="SRPDefaultUnlit" }
			
	    	Blend One Zero
			Cull Front
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			
	    	HLSLPROGRAM
	    	#pragma vertex vert  
	    	#pragma fragment frag
	    	VertexOutput vert(VertexInput input)
	    	{
				VertexOutput output;
				
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
				float3 positionWS = vertexInput.positionWS;

				float3 binormal = cross(input.normalOS, input.tangentOS.xyz) * input.tangentOS.w;
				float3x3 rotation = float3x3(input.tangentOS.xyz, binormal, input.normalOS);
				float3 anormal = input.color.rgb * 2 - 1;
				anormal = normalize(mul(transpose(rotation), anormal));
				
				output.positionCS = vertexInput.positionCS;
				float3 normalVS = mul((float3x3)UNITY_MATRIX_IT_MV, anormal);
				float3 normalCS = normalize(TransformWViewToHClip(normalVS)).xyz;
				float aspect = _ScreenParams.x / _ScreenParams.y;
				normalCS.x /= aspect;
                
				output.positionCS.xy += 0.01 * _OutlineWidth * normalCS.xy * output.positionCS.w;
                
				float3 tangentWS = normalInput.tangentWS;
				float3 normalWS = normalInput.normalWS;
				float3 bitangentWS = normalInput.bitangentWS;
				output.TtoW0 = float4(tangentWS.x, bitangentWS.x, normalWS.x, positionWS.x);
				output.TtoW1 = float4(tangentWS.y, bitangentWS.y, normalWS.y, positionWS.y);
                output.TtoW2 = float4(tangentWS.z, bitangentWS.z, normalWS.z, positionWS.z);
				
				output.uv = TRANSFORM_TEX(input.uv.xy, _BaseColor);
				return output;
	    	}
	    	
	     	float4 frag(VertexOutput input) : SV_Target
	    	{
	    		return float4(_OutlineColor.rgb, 1);
	     	}
			ENDHLSL
		}
		
		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			//Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM

			#pragma multi_compile_instancing

			#pragma instancing_options renderinglayer

			#pragma multi_compile _ LIGHTMAP_ON
        	#pragma multi_compile _ DIRLIGHTMAP_COMBINED
        	#pragma shader_feature _ _SAMPLE_GI
        	#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        	#pragma multi_compile_fragment _ DEBUG_DISPLAY
        	#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        	#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			#pragma vertex vert
			#pragma fragment frag

			inline int2 GetDepthUVOffset(half offset, half2 positionCSXY, half3 mainLightDir, half2 depthTexWH, float linearEyeDepth)
			{
			    // 1 / depth when depth < 1 is wrong, this is like point light attenuation
			    // 0.5625 is aspect, hard code for now
			    // 0.333 is fov, hard code for now
			    float2 UVOffset = _CameraAspect * (offset * _CameraFOV / (1 + linearEyeDepth)); 
			    half2 mainLightDirVS = TransformWorldToView(mainLightDir).xy;
			    //mainLightDirVS.x *= -1;
			    UVOffset = mainLightDirVS * UVOffset;
			    half2 downSampleFix = _DepthTextureSourceSize.zw / depthTexWH.xy;
			    int2 loadTexPos = positionCSXY / downSampleFix + UVOffset * depthTexWH.xy;
			    loadTexPos = min(loadTexPos, depthTexWH.xy-1);
			    return loadTexPos;
			}

			inline half DepthRim(half depthRimOffset, half rimDepthDiffThresholdOffset, half2 positionCSXY, half3 mainLightDir, float linearEyeDepth)
			{
			    int2 loadPos = GetDepthUVOffset(depthRimOffset, positionCSXY, mainLightDir,  _DepthTextureSourceSize.zw, linearEyeDepth);
			    float depthTextureValue = LoadSceneDepth(loadPos);
			    float depthTextureLinearDepth = LinearEyeDepth(depthTextureValue, _ZBufferParams);
			    
			    float threshold = saturate(0.1 + rimDepthDiffThresholdOffset);
			    half depthRim = saturate((depthTextureLinearDepth - (linearEyeDepth + threshold)) * 5);
			    depthRim = lerp(0, depthRim, linearEyeDepth);
			    return depthRim;
			}
						
			VertexOutput vert (VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS.xyz, v.tangentOS);
				float3 positionWS = positionInputs.positionWS;
				o.positionCS = positionInputs.positionCS;
				
				float3 tangentWS = normalInput.tangentWS;
				float3 normalWS = normalInput.normalWS;
				float3 bitangentWS = normalInput.bitangentWS;
				o.TtoW0 = float4(tangentWS.x, bitangentWS.x, normalWS.x, positionWS.x);
				o.TtoW1 = float4(tangentWS.y, bitangentWS.y, normalWS.y, positionWS.y);
                o.TtoW2 = float4(tangentWS.z, bitangentWS.z, normalWS.z, positionWS.z);
				
				o.uv = TRANSFORM_TEX(v.uv.xy, _BaseColor);
				
				return o;
			}

			half4 frag (VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				
				float3 positionWS = float3(IN.TtoW0.w, IN.TtoW1.w, IN.TtoW2.w);
				
				float2 uv_Mask = IN.uv * _Mask_ST.xy + _Mask_ST.zw;
				float4 Mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv_Mask);
								
				float2 uv_Normal = IN.uv * _Normal_ST.xy + _Normal_ST.zw;
				float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, uv_Normal), 1.0f);
				
				float3 normalWS = normalize(float3(dot(IN.TtoW0.xyz, normalTS), dot(IN.TtoW1.xyz, normalTS), dot(IN.TtoW2.xyz, normalTS)));
				float3 NormalVS = mul(UNITY_MATRIX_V, float4(normalWS, 0)).xyz;
				
				float3 ViewDirWS = normalize(_WorldSpaceCameraPos.xyz - positionWS);
				float3 ViewDirVS = mul(UNITY_MATRIX_V, float4(ViewDirWS, 0)).xyz;
				
				float3 break83 = cross(NormalVS, ViewDirVS);
				float2 uv_MatCap = float2(break83.y * -1.0, break83.x);
				float4 MetalSpecular = Mask.g * SAMPLE_TEXTURE2D(_Metal, sampler_Metal, uv_MatCap * 0.5 + 0.5);

				Light mainLight = GetMainLight();
				float NdotL = dot(normalize(mainLight.direction), normalWS );
				
				float halfLambert = NdotL * 0.5 + 0.5;
				float shadowmask = smoothstep(0.0, _ShadowSmooth, halfLambert - _ShadowRange);
				float4 diffuse = lerp(_ShadowColor, _MainColor, shadowmask);
								
				float2 uv_BaseColor = IN.uv * _BaseColor_ST.xy + _BaseColor_ST.zw;
				float4 BaseColor = SAMPLE_TEXTURE2D(_BaseColor, sampler_BaseColor, uv_BaseColor);
				
				float3 halfDir = normalize(normalize(mainLight.direction) + ViewDirWS);
				float NdotH = dot(halfDir, normalWS);
				float4 Specular = saturate(pow(NdotH, _SpecularScale) * Mask.b * _SpecularIntensity * _SpecularColor);

				float2 RimScreenUV = float2(IN.positionCS.x / _ScreenParams.x, IN.positionCS.y / _ScreenParams.y);
				
				float3 N_VS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));
				//偏移UV
				float2 RimOffsetUV = RimScreenUV + N_VS.xy * _RimOffset * 0.01;//float2(_RimOffset / i.clipW, 0); 
				
				//采样深度图
				float ScreenDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, RimScreenUV);
				float OffsetDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, RimOffsetUV);
								
				float linear01EyeOffsetDepth = LinearEyeDepth(OffsetDepth, _ZBufferParams);
				float linear01EyeTrueDepth = LinearEyeDepth(ScreenDepth, _ZBufferParams);
				
				float diff = linear01EyeOffsetDepth - linear01EyeTrueDepth;    //深度差
				float rimMask = step(_RimThreshold * 0.1, diff) * step(0, NdotL);
				
				// half4 RimColor = float4(rimMask * _RimColor.rgb * _RimColor.a, 1) * _EnableRim;
				half3 RimColor = saturate(rimMask * _RimColor).rgb * _RimColor.a;
				float linearEyeDepth = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);
				//float4 RimColor = DepthRim(_RimOffset, _RimThreshold, IN.positionCS.xy, mainLight.direction, linearEyeDepth) * _RimColor;
				float3 Color = saturate((MetalSpecular + diffuse * BaseColor + Specular + RimColor) * mainLight.color * mainLight.distanceAttenuation).rgb;
				float Alpha = BaseColor.a;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif
				
				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4(Color, Alpha);
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

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS.xyz, v.tangentOS);
				float3 positionWS = positionInputs.positionWS;
				o.positionCS = positionInputs.positionCS;
				
				float3 tangentWS = normalInput.tangentWS;
				float3 normalWS = normalInput.normalWS;
				float3 bitangentWS = normalInput.bitangentWS;
				o.TtoW0 = float4(tangentWS.x, bitangentWS.x, normalWS.x, positionWS.x);
				o.TtoW1 = float4(tangentWS.y, bitangentWS.y, normalWS.y, positionWS.y);
                o.TtoW2 = float4(tangentWS.z, bitangentWS.z, normalWS.z, positionWS.z);
				
				o.uv = TRANSFORM_TEX(v.uv.xy, _BaseColor);
				
				return o;
			}

			half4 frag(VertexOutput IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				
				float2 uv_BaseColor = IN.uv * _BaseColor_ST.xy + _BaseColor_ST.zw;
				float4 BaseColor = SAMPLE_TEXTURE2D(_BaseColor, sampler_BaseColor, uv_BaseColor);
				
				float Alpha = BaseColor.a;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif
				
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
			
			VertexOutput vert (VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS.xyz, v.tangentOS);
				float3 positionWS = positionInputs.positionWS;
				o.positionCS = positionInputs.positionCS;
				
				float3 tangentWS = normalInput.tangentWS;
				float3 normalWS = normalInput.normalWS;
				float3 bitangentWS = normalInput.bitangentWS;
				o.TtoW0 = float4(tangentWS.x, bitangentWS.x, normalWS.x, positionWS.x);
				o.TtoW1 = float4(tangentWS.y, bitangentWS.y, normalWS.y, positionWS.y);
                o.TtoW2 = float4(tangentWS.z, bitangentWS.z, normalWS.z, positionWS.z);
				
				o.uv = TRANSFORM_TEX(v.uv.xy, _BaseColor);
				
				return o;
			}

			half4 frag(VertexOutput IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
				
				float2 uv_BaseColor = IN.uv * _BaseColor_ST.xy + _BaseColor_ST.zw;
				float4 BaseColor = SAMPLE_TEXTURE2D(_BaseColor, sampler_BaseColor, uv_BaseColor);

				float Alpha = BaseColor.a;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif
				
				return 0;
			}
			ENDHLSL
		}
	}	
}