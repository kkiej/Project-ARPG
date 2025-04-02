Shader "Water"
{
	Properties
	{
		_NormalTexture("Normal Texture", 2D) = "white" {}
		[Toggle]_WorldSpaceUV("World Space UV", Float) = 0
		_NormalMovement("Normal Movement", Vector) = (0.04,0.36,0,0)
		_Refraction("Refraction", Float) = 0.022
		_GradientTexture("Gradient Texture", 2D) = "white" {}
		[Toggle]_Gradient("Gradient", Float) = 0
		_ShallowColor("Shallow Color", Color) = (0,0.772549,0.6235294,0.5019608)
		_DeepColor("Deep Color", Color) = (0.01176471,0.2862745,0.509804,1)
		_ColorDepth("Color Depth", Float) = 0.84
		_HorizonColor ("Horizon Color", Color) = (0, 0.04231141, 0.6038274, 1)
		_HorizonDistance("Horizon Distance", Float) = 20
		_ShoreDepth("Shore Depth", Float) = 0.74
		_ShoreBlend("Shore Blend", Float) = 1
		_ShoreFade("Shore Fade", Float) = 1
		_ShoreColor("Shore Color", Color) = (0,0.9803922,0.3019608,0.3411765)
		_ReflectionStrength("Reflection Strength", Float) = 0.82
		_WaveVisuals("Wave Visuals", Vector) = (0.132,7.9,1.38,0)
		_ReflectionFresnel("Reflection Fresnel", Float) = 2.5
		_WaveDirections("Wave Directions", Vector) = (0,0.64,1,0.33)
		_SurfaceFoamMovement("Surface Foam Movement", Vector) = (0.7,0,0.47,0.01)
		_SurfaceFoamTillingandOffset("Surface Foam Tilling and Offset", Vector) = (-1,-1,0.57,0.22)
		_SurfaceFoamSampling("Surface Foam Sampling", Vector) = (0.62,0,0,0)
		_SurfaceFoamColor1("Surface Foam Color 1", Color) = (1,1,1,0.3215686)
		_SurfaceFoamColor2("Surface Foam Color 2", Color) = (1,1,1,0.7294118)
		_SurfaceFoamShadowProjection("Surface Foam Shadow Projection", Float) = 15
		_SurfaceFoamShadowDepth("Surface Foam Shadow Depth", Float) = 10
		_HeightMask("Height Mask", Range( 0 , 1)) = 1
		_HeightMaskSmoothness("Height Mask Smoothness", Range( 0 , 1)) = 1
		_ShadowStrength("Shadow Strength", Float) = 0.45
		[Toggle(_SURFACEFOAM_ON)] _SurfaceFoam("Surface Foam", Float) = 0
		_IntersectionDepth("Intersection Depth", Float) = 0.72
		[Toggle(_FOAMSHADOWS_ON)] _FoamShadows("Foam Shadows", Float) = 1
		[Toggle(_SHOREMOVEMENT_ON)] _ShoreMovement("Shore Movement", Float) = 0
		[Toggle(_INTERSECTIONEFFECTS_ON)] _IntersectionEffects("Intersection Effects", Float) = 1
		_SurfaceFoamTexture("Surface Foam Texture", 2D) = "white" {}
		_IntersectionFoamSampling("Intersection Foam Sampling", Vector) = (1,0,0,0)
		_IntersectionFoamColor("Intersection Foam Color", Color) = (1,1,1,1)
		_IntersectionFoamShadowProjection("Intersection Foam Shadow Projection", Float) = 4.6
		_FoamShadowDepth("Foam Shadow Depth", Float) = 10
		_IntersectionFoamMovement("Intersection Foam Movement", Vector) = (0,0,0,0)
		_IntersectionFoamScale("Intersection Foam Scale", Float) = 0.19
		_IntersectionWaterBlend("Intersection Water Blend", Range( 0 , 1)) = 0
		_IntersectionFoamTexture("Intersection Foam Texture", 2D) = "white" {}
		_SurfaceFoamBlend("Surface Foam Blend", Range( 0 , 1)) = 1
		_IntersectionFoamBlend("Intersection Foam Blend", Range( 0 , 1)) = 1
		_ShoreFoamWidth("Shore Foam Width", Float) = 0
		_ShoreFoamFrequency("Shore Foam Frequency", Float) = 0
		_ShoreFoamSpeed("Shore Foam Speed", Float) = 0
		_ShoreFoamBreakupStrength("Shore Foam Breakup Strength", Float) = 0
		_ShoreFoamBreakupScale("Shore Foam Breakup Scale", Float) = 0
		_SpecularColor("Specular Color", Color) = (1,1,1,0)
		_DiffuseColor("Diffuse Color", Color) = (0,0,0,0)
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Hardness("Hardness", Float) = 0
		_NormalStrength("Normal Strength", Float) = 0.29
		[Toggle(_LIGHTING_ON)] _Lighting("Lighting", Float) = 1
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Back

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		ENDHLSL
		
		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1
			#define REQUIRE_OPAQUE_TEXTURE 1

			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma shader_feature _ _SAMPLE_GI
			#pragma multi_compile _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

			#include "./Includes/StylizedWaterForURP.hlsl"
			
			#pragma shader_feature_local _LIGHTING_ON
			#pragma shader_feature_local _INTERSECTIONEFFECTS_ON
			#pragma shader_feature_local _SURFACEFOAM_ON
			#pragma shader_feature_local _FOAMSHADOWS_ON
			#pragma shader_feature_local _SHOREMOVEMENT_ON

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 vertexColor : COLOR;
				float4 uv : TEXCOORD0;
				float4 tangentOS : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
				half fogFactor : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 uv : TEXCOORD3;
				float4 tangentWS : TEXCOORD4;
				float4 normalWS : TEXCOORD5;
				float4 bitangentWS : TEXCOORD6;
				float4 vertexColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _SurfaceFoamMovement, _SpecularColor, _IntersectionFoamColor, _SurfaceFoamTillingandOffset, _HorizonColor;
			half4 _SurfaceFoamColor2, _SurfaceFoamColor1, _ShoreColor, _ShallowColor, _DiffuseColor, _DeepColor;
			float4 _WaveDirections;
			float3 _WaveVisuals;
			float2 _NormalMovement, _IntersectionFoamSampling, _SurfaceFoamSampling, _IntersectionFoamMovement;
			half _ShadowStrength, _Smoothness, _NormalStrength, _ColorDepth, _IntersectionFoamBlend, _ShoreFoamBreakupStrength;
			half _ShoreFoamBreakupScale, _ShoreFoamSpeed, _ShoreFoamFrequency, _ShoreFoamWidth, _SurfaceFoamBlend;
			half _IntersectionWaterBlend, _HorizonDistance, _IntersectionFoamShadowProjection, _IntersectionDepth;
			half _ReflectionFresnel, _IntersectionFoamScale, _ShoreFade, _FoamShadowDepth, _SurfaceFoamShadowProjection;
			half _ShoreDepth, _WorldSpaceUV, _Hardness, _HeightMaskSmoothness, _HeightMask, _Refraction;
			half _ReflectionStrength, _SurfaceFoamShadowDepth, _ShoreBlend, _Gradient;
			CBUFFER_END

			sampler2D _PlanarReflectionTexture;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _NormalTexture;
			sampler2D _GradientTexture;
			sampler2D _SurfaceFoamTexture;
			sampler2D _IntersectionFoamTexture;

			float2 UnityGradientNoiseDir( float2 p )
			{
				p = fmod(p , 289);
				float x = fmod((34 * p.x + 1) * p.x , 289) + p.y;
				x = fmod( (34 * x + 1) * x , 289);
				x = frac( x / 41 ) * 2 - 1;
				return normalize( float2(x - floor(x + 0.5 ), abs( x ) - 0.5 ) );
			}
			
			float UnityGradientNoise( float2 UV, float Scale )
			{
				float2 p = UV * Scale;
				float2 ip = floor( p );
				float2 fp = frac( p );
				float d00 = dot( UnityGradientNoiseDir( ip ), fp );
				float d01 = dot( UnityGradientNoiseDir( ip + float2( 0, 1 ) ), fp - float2( 0, 1 ) );
				float d10 = dot( UnityGradientNoiseDir( ip + float2( 1, 0 ) ), fp - float2( 1, 0 ) );
				float d11 = dot( UnityGradientNoiseDir( ip + float2( 1, 1 ) ), fp - float2( 1, 1 ) );
				fp = fp * fp * fp * ( fp * ( fp * 6 - 15 ) + 10 );
				return lerp( lerp( d00, d01, fp.y ), lerp( d10, d11, fp.y ), fp.x ) + 0.5;
			}
			
			float3 NormalStrength358( float3 In, float Strength )
			{
				float3 Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
				return Out;
			}

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
				// VISUALS
				// x: steepness
				// y: wavelength
				// z: speed
				float3 Visuals = float3(v.vertexColor.g * _WaveVisuals.x, _WaveVisuals.y, _WaveVisuals.z);
				float3 Offset = float3(0,0,0);
				float3 Normal = float3(0,0,0);
				GerstnerWaves_float(positionWS, Visuals, _WaveDirections, Offset, Normal);

				o.positionWS = positionWS + Offset;
				
				float3 positionOS = TransformWorldToObject(o.positionWS);

				o.positionCS = TransformWorldToHClip(o.positionWS);
				
				o.screenPos = ComputeScreenPos(o.positionCS);

				VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
				o.tangentWS.xyz = normalInput.tangentWS;
				o.normalWS.xyz = normalInput.normalWS;
				o.bitangentWS.xyz = normalInput.bitangentWS;
				
				o.uv.xy = v.uv.xy;
				
				o.vertexColor = v.vertexColor;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.uv.zw = 0;
				o.tangentWS.w = 0;
				o.normalWS.w = 0;
				o.bitangentWS.w = 0;

				o.fogFactor = ComputeFogFactor(o.positionCS.z);

				return o;
			}

			half4 frag (VertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				
				float4 screenPos = i.screenPos;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = UNITY_NEAR_CLIP_VALUE >= 0 ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				
				float2 uv = _WorldSpaceUV ? -(float2(i.positionWS.x, i.positionWS.z) * 0.1) : i.uv.xy;
				float2 UV1 = float2(-1, 0) * _Time.y * (_NormalMovement.x * -0.5) + uv / (_NormalMovement.y * 0.5);
				float2 UV2 = float2(-1, 0) * _Time.y * _NormalMovement.x + uv / _NormalMovement.y;
				half3 normalTS1 = UnpackNormal(tex2D(_NormalTexture, UV1));
				half3 normalTS2 = UnpackNormal(tex2D(_NormalTexture, UV2));
				half3 SurfaceNormal = BlendNormal(normalTS1, normalTS2);

				half3x3 tangentToWorld = half3x3(i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
				half3 normalVS = mul(UNITY_MATRIX_V, half4(TransformTangentToWorld(SurfaceNormal, tangentToWorld), 0)).xyz;
				float4 screenUV_Offset = ase_screenPosNorm + half4(normalVS * (_Refraction * 0.2), 0.0);
				half sceneDepth_Offset = LinearEyeDepth(SampleSceneDepth(screenUV_Offset.xy), _ZBufferParams);
				
				float3 positionVS = TransformWorldToView(i.positionWS);
				half sceneDepth_Raw = SampleSceneDepth(ase_screenPosNorm.xy);
				// Convert to world units
				float sceneDepth_world = lerp(_ProjectionParams.y, _ProjectionParams.z, _ProjectionParams.x > 0.0 ? sceneDepth_Raw : (1.0 - sceneDepth_Raw));
				positionVS = float3(positionVS.x, positionVS.y, -sceneDepth_world);
				float3 scenePositionWS = mul(UNITY_MATRIX_I_V, float4(positionVS, 1)).xyz;

				float3 viewVector = (i.positionWS - _WorldSpaceCameraPos) / screenPos.w;
				float3 scenePositionWS1 = lerp(viewVector * sceneDepth_Offset + _WorldSpaceCameraPos, scenePositionWS, unity_OrthoParams.w);
				float2 RefractUV = ((i.positionWS - scenePositionWS1).y >= 0.0 ? screenUV_Offset : ase_screenPosNorm).xy;
				
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS);
				half NdotV = dot(i.normalWS.xyz, viewDir);
				half OneMinusNdotV = max(1 - NdotV, 0);
				half ReflectionMask = _ReflectionStrength * pow(OneMinusNdotV, _ReflectionFresnel);
				half4 ReflectionColor = lerp(float4(0,0,0,0), tex2D(_PlanarReflectionTexture, RefractUV), ReflectionMask);
				
				float sceneDepth_Refract = LinearEyeDepth(SampleSceneDepth(RefractUV), _ZBufferParams);
				float3 scenePositionWS2 = lerp(viewVector * sceneDepth_Refract + _WorldSpaceCameraPos, scenePositionWS, unity_OrthoParams.w);
				half heightSubtract = (i.positionWS - scenePositionWS2).y;

				half DepthFade_Exp = saturate(exp(-heightSubtract / _ColorDepth));
				half4 DepthColor = lerp(_DeepColor, _ShallowColor, DepthFade_Exp);
				half DepthFade_Linear = saturate(heightSubtract / _ColorDepth);
				DepthColor = _Gradient ? tex2D(_GradientTexture, float2(DepthFade_Linear.xx)) : DepthColor;
				
				half HorizonMask = pow(OneMinusNdotV, _HorizonDistance);
				half4 HorizonColor = lerp(DepthColor, _HorizonColor, HorizonMask);
				
				half sceneDepth_Eye = LinearEyeDepth(sceneDepth_Raw, _ZBufferParams);
				float3 scenePositionWS3 = lerp(viewVector * sceneDepth_Eye + _WorldSpaceCameraPos, scenePositionWS, unity_OrthoParams.w);
				half heightSubtract3 = (i.positionWS - scenePositionWS3).y;
				half shoreDepth = saturate(heightSubtract3 / _ShoreDepth);
				half smoothstepResult101 = smoothstep(1.0 - _ShoreFade, 1.0, shoreDepth + 0.1);
				half smoothstepResult107 = smoothstep(_ShoreBlend, 0.0, shoreDepth + 0.3);
				half ShoreMask = lerp((1.0 - smoothstepResult101) * _ShoreColor.a, 0.0, smoothstepResult107);
				half4 ShoreColor = lerp(HorizonColor, _ShoreColor, saturate(ShoreMask));
				
				half4 UnderWaterColor = float4(SampleSceneColor(RefractUV), 1.0);
				UnderWaterColor = UnderWaterColor * (1.0 - HorizonColor.a);
				half4 col = ReflectionColor + (ShoreColor + UnderWaterColor);
				
				float3 Visuals = float3(i.vertexColor.g * _WaveVisuals.x, _WaveVisuals.y, _WaveVisuals.z);
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( i.positionWS , Visuals, _WaveDirections, Offset145 , Normal145 );
				half height = saturate(Offset145.y);
				half SurfaceFoamMask = smoothstep(_HeightMask, _HeightMask + _HeightMaskSmoothness, height);
				SurfaceFoamMask = lerp(1.0, SurfaceFoamMask, step(0.0, _HeightMask));
				
				float4 SurfaceFoamUV = float4(0,0,0,0);
				SurfaceFoamUV_half(uv, _SurfaceFoamMovement, _SurfaceFoamTillingandOffset, SurfaceFoamUV);
				
				float3 LightDirTS = mul(tangentToWorld, SafeNormalize(_MainLightPosition.xyz));
				float3 TtoW0 = float3(i.tangentWS.x, i.bitangentWS.x, i.normalWS.x);
				float3 TtoW1 = float3(i.tangentWS.y, i.bitangentWS.y, i.normalWS.y);
				float3 TtoW2 = float3(i.tangentWS.z, i.bitangentWS.z, i.normalWS.z);
				float3 viewDirTS = TtoW0 * viewDir.x + TtoW1 * viewDir.y + TtoW2 * viewDir.z;
				viewDirTS = SafeNormalize(viewDirTS);
				float3 Out183 = float3(0,0,0);
				ViewDirectionParallax_half(viewDirTS, Out183);
				float2 ShadowOffset = (saturate(heightSubtract3 / _SurfaceFoamShadowProjection) * (LightDirTS + Out183)).xy;
				half4 SurfaceFoamSampleColor = float4(0,0,0,0);
				FoamSample(SurfaceFoamUV, _SurfaceFoamSampling, _SurfaceFoamTexture, ShadowOffset, SurfaceFoamSampleColor);
				half4 SurfaceFoamColor = SurfaceFoamMask * SurfaceFoamSampleColor;
				half SurfaceShadow = 0.0;
				half4 SurfaceFoam = half4(0,0,0,0);
				FoamColor_half(_SurfaceFoamColor1, _SurfaceFoamColor2, SurfaceFoamColor.x, SurfaceFoamColor.y, SurfaceFoamColor.z, SurfaceFoamColor.w, SurfaceShadow, SurfaceFoam);

				half NdotL = dot(SafeNormalize(_MainLightPosition.xyz), i.normalWS.xyz);
				
				#ifdef _SURFACEFOAM_ON
					half4 SurfaceFoamShadow = float4(0.0, 0.0, 0.0, saturate(_ShadowStrength * (NdotL * (1.0 - saturate(heightSubtract3 / _SurfaceFoamShadowDepth)))) * SurfaceShadow);
					SurfaceFoamShadow = lerp(col, SurfaceFoamShadow, saturate(SurfaceFoamShadow.w));
				#else
					half4 SurfaceFoamShadow = col;
				#endif
								
				float4 Directional233 = float4( 0,0,0,0 );
				float4 ByDepth233 = float4( 0,0,0,0 );
				IntersectionFoamUV_half(uv, _IntersectionFoamMovement, _IntersectionFoamScale, Directional233, ByDepth233);
				float4 UVs238 = Directional233;
				
				float IntersectionDepth_Linear = saturate(heightSubtract3 / _IntersectionDepth);
				float2 Sampling238 = _IntersectionFoamSampling * IntersectionDepth_Linear;
				
				float2 ShadowOffset238 = (saturate(heightSubtract3 / _IntersectionFoamShadowProjection) * (LightDirTS + Out183)).xy;
				half4 IntersectionFoamSampleColor = half4(0,0,0,0);
				FoamSample(UVs238, Sampling238, _IntersectionFoamTexture, ShadowOffset238, IntersectionFoamSampleColor);
				
				half IntersectionEffectMask = smoothstep(1.0 - _IntersectionWaterBlend, 1.0, IntersectionDepth_Linear + 0.1);
				IntersectionEffectMask = 1.0 - IntersectionEffectMask;
				
				#ifdef _FOAMSHADOWS_ON
					#ifdef _INTERSECTIONEFFECTS_ON
						#ifdef _SHOREMOVEMENT_ON
							half4 FoamShadow = SurfaceFoamShadow;
						#else
							half4 IntersectionFoamShadow = float4(0.0, 0.0, 0.0, saturate(_ShadowStrength * (NdotL * (1.0 - saturate(heightSubtract3 / _FoamShadowDepth))))
								* (saturate(IntersectionFoamSampleColor.x + IntersectionFoamSampleColor.y) * _IntersectionFoamColor.a) * IntersectionEffectMask);
							half4 FoamShadow = lerp(SurfaceFoamShadow, IntersectionFoamShadow, saturate(IntersectionFoamShadow.w));
						#endif
					#else
						half4 FoamShadow = SurfaceFoamShadow;
					#endif
				#else
					half4 FoamShadow = col;
				#endif
				
				#ifdef _SURFACEFOAM_ON
					SurfaceFoamMask = saturate(SurfaceFoam.w);
					half4 SurfaceFoamColor1 = lerp(FoamShadow, SurfaceFoam, SurfaceFoamMask);
					half4 SurfaceFoamColor2 = saturate(lerp(FoamShadow, SurfaceFoam + FoamShadow, SurfaceFoamMask));
					half4 SurfaceFoamResult = lerp(SurfaceFoamColor1, SurfaceFoamColor2, _SurfaceFoamBlend);
				#else
					half4 SurfaceFoamResult = FoamShadow;
				#endif
				
				#ifdef _INTERSECTIONEFFECTS_ON
					#ifdef _SHOREMOVEMENT_ON
						half temp_output_305_0 = 1.0 - IntersectionDepth_Linear;
						half gradientNoise = UnityGradientNoise(i.uv.xy, _ShoreFoamBreakupScale);
						gradientNoise = gradientNoise * 0.5 + 0.5;
						IntersectionFoamColor = step(1.0 - (_ShoreFoamWidth - temp_output_305_0 + 0.01), temp_output_305_0 + (sin(temp_output_305_0 * _ShoreFoamFrequency + _ShoreFoamSpeed * _Time.y)
								+ (gradientNoise - _ShoreFoamBreakupStrength))) * IntersectionEffectMask * _IntersectionFoamColor;
					#else
						half4 IntersectionFoamColor = _IntersectionFoamColor * saturate(IntersectionFoamSampleColor.z + IntersectionFoamSampleColor.w);
						IntersectionFoamColor = half4(IntersectionFoamColor.rgb, IntersectionEffectMask * IntersectionFoamColor.a);
					#endif
					half IntersectionFoamMask = saturate(IntersectionFoamColor.w);
					half4 IntersectionFoamColor1 = lerp(SurfaceFoamResult, IntersectionFoamColor, IntersectionFoamMask);
					half4 IntersectionFoamColor2 = saturate(lerp(SurfaceFoamResult, IntersectionFoamColor + SurfaceFoamResult, IntersectionFoamMask));
					half4 IntersectionFoamResult = lerp(IntersectionFoamColor1, IntersectionFoamColor2, _IntersectionFoamBlend);
				#else
					half4 IntersectionFoamResult = SurfaceFoamResult;
				#endif
				
				#ifdef _LIGHTING_ON
					half3 normal = NormalStrength358(SurfaceNormal, _NormalStrength);
					half3 normalWS = normalize(TransformTangentToWorld(normal, tangentToWorld));
					
					half Specular = 0.0;
					half Diffuse = 0.0;
					MainLighting_half(i.positionWS, normalWS, viewDir, _Smoothness, Specular, Diffuse);
					Specular = lerp(Specular, step(0.5, Specular), _Hardness);
					Diffuse = lerp(Diffuse, step(0.5, Diffuse), _Hardness);
					
					half3 AddLightSpecular = float3( 0,0,0 );
					half3 AddLightDiffuse = float3( 0,0,0 );
					AdditionalLighting_half(i.positionWS, normalWS, viewDir, _Smoothness, _Hardness, AddLightSpecular, AddLightDiffuse);
					half4 LightingResult = _SpecularColor * Specular + Diffuse * _DiffuseColor + float4(AddLightSpecular, 0.0);
					half4 color = LightingResult + IntersectionFoamResult;
				#else
					half4 color = IntersectionFoamResult;
				#endif

				color.rgb = MixFog(color.rgb, i.fogFactor);

				return half4(color.rgb, 1);
			}
			ENDHLSL
		}

		Pass
        {
            Name "PBRBase DepthOnly"
            Tags
            {   
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            struct a2v
            {
                float4 positionOS     : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f DepthOnlyVertex(a2v v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            	
            	 o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 DepthOnlyFragment(v2f i) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return 0;
            }

            ENDHLSL
        }
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}