// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NMJJ/Scene/Water_ASE"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Header(Base)]_ShallowColor("Shallow Color", Color) = (0.8766465,0.9433962,0.9401634,0)
		_DeepColor("Deep Color", Color) = (0.1764418,0.2801822,0.5754717,0)
		_WaterColorRange("Water Color Range", Range( 0 , 5)) = 0.3058824
		_WaterDepth("Water Depth", Range( 0 , 1)) = 0.3058824
		[NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
		_NormalScale("NormalScale", Float) = 1
		_NormalMovement1("Normal Movement", Vector) = (0,0,0,0)
		[Header(Reflection And Refraction)]_ReflectionStrength("Reflection Strength", Range( 0 , 1)) = 0.5
		_ReflectDistort("Reflect Distort", Range( 0 , 0.1)) = 0.03
		_ReflectionRange("Reflection Range", Range( 0 , 5)) = 1.882353
		[Header(Caustics)]_CausticsTilling("Caustics Tilling", Float) = 9.78
		_CausticsColor("Caustics Color", Color) = (1,1,1,1)
		[NoScaleOffset]_CausticsTex("Caustics Tex", 2D) = "white" {}
		_CausticsSpeed("Caustics Speed", Vector) = (-0.1,-0.05,0,0)
		_CausticsIntensity("Caustics Intensity", Range( 0 , 2)) = 1
		[Header(Foam)]_FoamColor("Foam Color", Color) = (0.9716981,0.9304467,0.9304467,0)
		_FoamNoise("Foam Noise", 2D) = "white" {}
		_FoamCutOff("Foam CutOff", Range( 0 , 0.5)) = 0
		_SecondFoamNoiseTAS("Second Foam Noise TAS", Vector) = (1,1,0,0)
		_SecondFoamCutOff("Second Foam CutOff", Range( 0 , 0.1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_FoamDistort("Foam Distort", Range( 0 , 1)) = 0
		[HDR][Header(Specular)]_SpecularColor("Specular Color", Color) = (0.990566,0.9578586,0.9578586,1)
		_SpecularPower("Specular Power", Float) = 20
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		_Hardness("Hardness", Range( 0 , 1)) = 0
		[ASEEnd]_NormalTilling("Normal Tilling", Float) = 0.1


		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-30" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma shader_feature _ _SAMPLE_GI
			#pragma multi_compile _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShallowColor;
			float4 _DeepColor;
			float4 _FoamNoise_ST;
			float4 _Noise_ST;
			float4 _SecondFoamNoiseTAS;
			float4 _FoamColor;
			float4 _CausticsColor;
			float4 _SpecularColor;
			float2 _CausticsSpeed;
			float2 _NormalMovement1;
			float _ReflectionRange;
			float _ReflectDistort;
			float _CausticsIntensity;
			float _CausticsTilling;
			float _Hardness;
			float _NormalScale;
			float _Smoothness;
			float _ReflectionStrength;
			float _NormalTilling;
			float _SecondFoamCutOff;
			float _FoamDistort;
			float _FoamCutOff;
			float _WaterColorRange;
			float _SpecularPower;
			float _WaterDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _FoamNoise;
			sampler2D _Noise;
			float3 _LightDir;
			sampler2D _NormalMap;
			sampler2D _CausticsTex;
			sampler2D _PlanarReflectionTexture;


			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g14( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			real3 ASESafeNormalize(float3 inVec)
			{
				real dp3 = max(FLT_MIN, dot(inVec, inVec));
				return inVec* rsqrt( dp3);
			}
			

			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord5.xyz = ase_worldTangent;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord6.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord7.xyz = ase_worldBitangent;
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				o.ase_texcoord7.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif

				o.clipPos = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 UV22_g15 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g15 = UnStereo( UV22_g15 );
				float2 break64_g14 = localUnStereo22_g15;
				float clampDepth69_g14 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g14 = ( 1.0 - clampDepth69_g14 );
				#else
				float staticSwitch38_g14 = clampDepth69_g14;
				#endif
				float3 appendResult39_g14 = (float3(break64_g14.x , break64_g14.y , staticSwitch38_g14));
				float4 appendResult42_g14 = (float4((appendResult39_g14*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g14 = mul( unity_CameraInvProjection, appendResult42_g14 );
				float3 temp_output_46_0_g14 = ( (temp_output_43_0_g14).xyz / (temp_output_43_0_g14).w );
				float3 In75_g14 = temp_output_46_0_g14;
				float3 localInvertDepthDirURP75_g14 = InvertDepthDirURP75_g14( In75_g14 );
				float4 appendResult49_g14 = (float4(localInvertDepthDirURP75_g14 , 1.0));
				float4 ReconstructWorldPosition748 = mul( unity_CameraToWorld, appendResult49_g14 );
				float YDeltha759 = ( WorldPosition.y - (ReconstructWorldPosition748).y );
				float smoothstepResult762 = smoothstep( 0.0 , _WaterColorRange , YDeltha759);
				float WaterColorRange763 = smoothstepResult762;
				float4 lerpResult496 = lerp( _ShallowColor , _DeepColor , WaterColorRange763);
				float3 temp_output_499_0 = (lerpResult496).rgb;
				float3 WaterColor139 = temp_output_499_0;
				float smoothstepResult766 = smoothstep( 0.0 , _FoamCutOff , YDeltha759);
				float2 temp_output_582_0 = ( _FoamNoise_ST.zw * ( _TimeParameters.x * 0.05 ) );
				float2 texCoord778 = IN.ase_texcoord4.xy * _Noise_ST.xy + ( _Noise_ST.zw * ( _TimeParameters.x * 0.05 ) );
				float Noise794 = tex2D( _Noise, texCoord778 ).r;
				float2 lerpResult793 = lerp( temp_output_582_0 , ( temp_output_582_0 + Noise794 ) , _FoamDistort);
				float2 texCoord579 = IN.ase_texcoord4.xy * _FoamNoise_ST.xy + ( temp_output_582_0 + lerpResult793 );
				float smoothstepResult771 = smoothstep( 0.0 , _SecondFoamCutOff , YDeltha759);
				float2 appendResult632 = (float2(_SecondFoamNoiseTAS.x , _SecondFoamNoiseTAS.y));
				float2 appendResult634 = (float2(_SecondFoamNoiseTAS.z , _SecondFoamNoiseTAS.w));
				float2 temp_output_633_0 = ( ( _TimeParameters.x * 0.05 ) * appendResult634 );
				float2 lerpResult781 = lerp( temp_output_633_0 , ( temp_output_633_0 + Noise794 ) , _FoamDistort);
				float2 texCoord631 = IN.ase_texcoord4.xy * appendResult632 + lerpResult781;
				float4 temp_output_615_0 = ( ( step( smoothstepResult766 , tex2D( _FoamNoise, texCoord579 ).r ) * step( smoothstepResult771 , tex2D( _FoamNoise, texCoord631 ).r ) ) * _FoamColor );
				float4 Foam567 = temp_output_615_0;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 normalizeResult369 = ASESafeNormalize( ( _LightDir + ase_worldViewDir ) );
				float2 _Vector0 = float2(-1,0);
				float2 appendResult181 = (float2(WorldPosition.x , WorldPosition.z));
				float2 UV178 = -( appendResult181 * _NormalTilling );
				float3 unpack248 = UnpackNormalScale( tex2D( _NormalMap, ( ( _Vector0 * ( _TimeParameters.x ) * ( _NormalMovement1.x * -0.5 ) ) + ( UV178 * ( 1.0 / ( _NormalMovement1.y * 0.5 ) ) ) ) ), _NormalScale );
				unpack248.z = lerp( 1, unpack248.z, saturate(_NormalScale) );
				float3 unpack243 = UnpackNormalScale( tex2D( _NormalMap, ( ( UV178 * ( 1.0 / _NormalMovement1.y ) ) + ( _Vector0 * ( _TimeParameters.x ) * _NormalMovement1.x ) ) ), _NormalScale );
				unpack243.z = lerp( 1, unpack243.z, saturate(_NormalScale) );
				float3 temp_output_245_0 = BlendNormal( unpack248 , unpack243 );
				float3 ase_worldTangent = IN.ase_texcoord5.xyz;
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord7.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal246 = temp_output_245_0;
				float3 worldNormal246 = normalize( float3(dot(tanToWorld0,tanNormal246), dot(tanToWorld1,tanNormal246), dot(tanToWorld2,tanNormal246)) );
				float3 Normal247 = worldNormal246;
				float dotResult445 = dot( normalizeResult369 , Normal247 );
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult610 = dot( normalizeResult369 , normalizedWorldNormal );
				float temp_output_609_0 = ( pow( saturate( dotResult445 ) , exp2( ( ( _Smoothness * 10.0 ) + 1.0 ) ) ) * pow( saturate( dotResult610 ) , _SpecularPower ) );
				float lerpResult600 = lerp( temp_output_609_0 , step( 0.5 , temp_output_609_0 ) , _Hardness);
				float4 Specular472 = ( lerpResult600 * _SpecularColor );
				float FoamAlpha625 = (temp_output_615_0).a;
				float temp_output_651_0 = ( 1.0 - FoamAlpha625 );
				float2 temp_output_76_0 = ( (ReconstructWorldPosition748).xz / _CausticsTilling );
				float2 temp_output_381_0 = ( _CausticsSpeed * _TimeParameters.x );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult30 = dot( ase_worldViewDir , Normal247 );
				float smoothstepResult407 = smoothstep( 0.2 , 1.0 , dotResult30);
				float CausticsAreaMask64 = smoothstepResult407;
				float4 Caustics239 = ( _CausticsColor * ( min( tex2D( _CausticsTex, ( temp_output_76_0 + temp_output_381_0 ) ) , tex2D( _CausticsTex, ( ( -temp_output_76_0 * 1.5 ) + temp_output_381_0 ) ) ) * _CausticsIntensity ) * CausticsAreaMask64 );
				float2 appendResult308 = (float2(ase_screenPosNorm.x , ase_screenPosNorm.y));
				float3 SurfaceNormal241 = temp_output_245_0;
				float2 temp_output_128_0 = ( (SurfaceNormal241).xy * _ReflectDistort );
				float4 tex2DNode425 = tex2D( _PlanarReflectionTexture, ( ( appendResult308 / ase_screenPosNorm.w ) + temp_output_128_0 ) );
				float dotResult696 = dot( ase_worldViewDir , normalizedWorldNormal );
				float ReflectionMask35 = pow( max( ( 1.0 - dotResult696 ) , 0.001 ) , _ReflectionRange );
				float4 ReflectColor143 = ( tex2DNode425 * ReflectionMask35 * _ReflectionStrength );
				
				float smoothstepResult758 = smoothstep( 0.0 , _WaterDepth , YDeltha759);
				float WaterDepth751 = smoothstepResult758;
				float temp_output_648_0 = ( ( 1.0 - WaterDepth751 ) * FoamAlpha625 );
				float lerpResult629 = lerp( WaterDepth751 , temp_output_648_0 , temp_output_648_0);
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = ( float4( WaterColor139 , 0.0 ) + Foam567 + ( Specular472 * temp_output_651_0 ) + ( temp_output_651_0 * Caustics239 ) + ReflectColor143 ).rgb;
				float Alpha = lerpResult629;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.clipPos, Color);
				#endif

				#if defined(_ALPHAPREMULTIPLY_ON)
				Color *= Alpha;
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShallowColor;
			float4 _DeepColor;
			float4 _FoamNoise_ST;
			float4 _Noise_ST;
			float4 _SecondFoamNoiseTAS;
			float4 _FoamColor;
			float4 _CausticsColor;
			float4 _SpecularColor;
			float2 _CausticsSpeed;
			float2 _NormalMovement1;
			float _ReflectionRange;
			float _ReflectDistort;
			float _CausticsIntensity;
			float _CausticsTilling;
			float _Hardness;
			float _NormalScale;
			float _Smoothness;
			float _ReflectionStrength;
			float _NormalTilling;
			float _SecondFoamCutOff;
			float _FoamDistort;
			float _FoamCutOff;
			float _WaterColorRange;
			float _SpecularPower;
			float _WaterDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _FoamNoise;
			sampler2D _Noise;


			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g14( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.worldPos = positionWS;
				#endif

				o.clipPos = TransformWorldToHClip( positionWS );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.worldPos;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 screenPos = IN.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 UV22_g15 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g15 = UnStereo( UV22_g15 );
				float2 break64_g14 = localUnStereo22_g15;
				float clampDepth69_g14 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g14 = ( 1.0 - clampDepth69_g14 );
				#else
				float staticSwitch38_g14 = clampDepth69_g14;
				#endif
				float3 appendResult39_g14 = (float3(break64_g14.x , break64_g14.y , staticSwitch38_g14));
				float4 appendResult42_g14 = (float4((appendResult39_g14*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g14 = mul( unity_CameraInvProjection, appendResult42_g14 );
				float3 temp_output_46_0_g14 = ( (temp_output_43_0_g14).xyz / (temp_output_43_0_g14).w );
				float3 In75_g14 = temp_output_46_0_g14;
				float3 localInvertDepthDirURP75_g14 = InvertDepthDirURP75_g14( In75_g14 );
				float4 appendResult49_g14 = (float4(localInvertDepthDirURP75_g14 , 1.0));
				float4 ReconstructWorldPosition748 = mul( unity_CameraToWorld, appendResult49_g14 );
				float YDeltha759 = ( WorldPosition.y - (ReconstructWorldPosition748).y );
				float smoothstepResult758 = smoothstep( 0.0 , _WaterDepth , YDeltha759);
				float WaterDepth751 = smoothstepResult758;
				float smoothstepResult766 = smoothstep( 0.0 , _FoamCutOff , YDeltha759);
				float2 temp_output_582_0 = ( _FoamNoise_ST.zw * ( _TimeParameters.x * 0.05 ) );
				float2 texCoord778 = IN.ase_texcoord3.xy * _Noise_ST.xy + ( _Noise_ST.zw * ( _TimeParameters.x * 0.05 ) );
				float Noise794 = tex2D( _Noise, texCoord778 ).r;
				float2 lerpResult793 = lerp( temp_output_582_0 , ( temp_output_582_0 + Noise794 ) , _FoamDistort);
				float2 texCoord579 = IN.ase_texcoord3.xy * _FoamNoise_ST.xy + ( temp_output_582_0 + lerpResult793 );
				float smoothstepResult771 = smoothstep( 0.0 , _SecondFoamCutOff , YDeltha759);
				float2 appendResult632 = (float2(_SecondFoamNoiseTAS.x , _SecondFoamNoiseTAS.y));
				float2 appendResult634 = (float2(_SecondFoamNoiseTAS.z , _SecondFoamNoiseTAS.w));
				float2 temp_output_633_0 = ( ( _TimeParameters.x * 0.05 ) * appendResult634 );
				float2 lerpResult781 = lerp( temp_output_633_0 , ( temp_output_633_0 + Noise794 ) , _FoamDistort);
				float2 texCoord631 = IN.ase_texcoord3.xy * appendResult632 + lerpResult781;
				float4 temp_output_615_0 = ( ( step( smoothstepResult766 , tex2D( _FoamNoise, texCoord579 ).r ) * step( smoothstepResult771 , tex2D( _FoamNoise, texCoord631 ).r ) ) * _FoamColor );
				float FoamAlpha625 = (temp_output_615_0).a;
				float temp_output_648_0 = ( ( 1.0 - WaterDepth751 ) * FoamAlpha625 );
				float lerpResult629 = lerp( WaterDepth751 , temp_output_648_0 , temp_output_648_0);
				

				float Alpha = lerpResult629;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

	
	}
	
	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19100
Node;AmplifyShaderEditor.CommentaryNode;616;-8464.508,-2205.905;Inherit;False;1035.031;330.123;Comment;6;182;181;179;177;169;178;UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;607;-2302.228,-2168.301;Inherit;False;2587.337;1335.713;Comment;33;625;626;567;615;614;584;637;634;633;636;632;630;631;585;641;577;580;582;581;579;578;767;766;770;771;791;781;782;783;795;792;793;796;Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;26;-8454.298,-128.8565;Inherit;False;2297.185;981.2186;Comment;26;619;485;484;483;495;763;764;762;761;494;751;758;760;759;714;713;712;750;139;481;470;549;499;496;497;498;水体颜色和深度差;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;27;-7982.217,-871.8879;Inherit;False;1216.857;596.9003;;14;64;539;537;538;659;35;695;694;32;697;30;407;696;29;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;39;-5317.695,-667.8009;Inherit;False;1885.521;681.4766;Comment;17;425;384;355;340;319;318;308;304;272;143;128;112;108;78;572;554;613;相机反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;43;-8862.269,-1797.869;Inherit;False;2802.099;833.2032;Comment;26;247;246;512;248;241;245;243;257;176;251;320;263;262;261;260;259;258;255;254;253;252;250;249;244;192;804;扰乱波纹法线;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;44;-2862.75,-674.465;Inherit;False;2544.812;677.6535;Comment;21;698;239;288;397;466;377;404;453;419;418;381;365;334;228;154;138;133;100;76;73;769;焦散;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;45;-5805.084,-2165.017;Inherit;False;3327.338;1375.018;Refraction;38;482;479;468;420;216;214;213;212;211;210;209;208;207;206;205;204;203;202;201;200;199;198;197;196;195;194;193;191;189;180;175;174;173;172;171;170;168;803;Refraction;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;50;-5889.729,135.3936;Inherit;False;2747.997;592.912;Comment;24;472;432;448;605;362;611;603;600;609;601;457;594;596;597;599;598;602;595;610;608;445;369;296;668;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;1194.407,-289.6948;Float;False;True;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;NMJJ/Scene/Water_ASE;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=-30;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;False;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;23;Surface;1;638380359598952102;  Blend;0;0;Two Sided;1;0;Forward Only;0;0;Cast Shadows;0;638380359535794059;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;0;638380359555038483;LOD CrossFade;0;0;Built-in Fog;1;638384772544710700;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;False;False;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;9;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.SimpleAddOpNode;297;884.671,-480.9325;Inherit;False;5;5;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;-5195.676,-1560.651;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;170;-5431.697,-1073.001;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;171;-5409.697,-1268.001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;173;-5641.698,-1188.002;Inherit;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ProjectionParams;174;-5721.087,-1049.227;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;180;-5137.938,-2118.841;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;189;-3212.938,-1662.28;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;191;-5188.396,-1939.069;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OrthoParams;193;-4367.688,-932.0005;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;194;-4348.688,-1153.002;Inherit;False;View;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;195;-4239.035,-1726.413;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;198;-4007.987,-1173.103;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;199;-3747.81,-1315.989;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;200;-4569.327,-1540.41;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GrabScreenPosition;202;-3616.313,-1778.676;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;-4462.328,-1880.957;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;204;-4786.688,-1028.001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;205;-4540.688,-1148.002;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;206;-4646.114,-1938.663;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;207;-5234.697,-1206.002;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;208;-4710.054,-1753.105;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;209;-4863.69,-1216.003;Inherit;False;World;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;210;-5043.697,-1212.002;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;211;-4974.759,-1643.097;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;212;-4906.05,-1896.776;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;213;-4866.943,-2038.081;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;214;-5014.696,-1028.001;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;420;-2979.856,-1662.995;Inherit;False;Global;_GrabScreen1;Grab Screen 0;9;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;482;-5727.653,-1475.172;Inherit;False;241;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;216;-5495.509,-1473.402;Inherit;False;Tangent;View;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;172;-5414.477,-1621.15;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;479;-5264.004,-1757.96;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;182;-8414.509,-2155.905;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;181;-8162.757,-2117.924;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;177;-7983.272,-2067.963;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;169;-7820.956,-2067.488;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;249;-7497.319,-1631.622;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-8007.18,-1631.757;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-7740.836,-1478.015;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;253;-8005.617,-1308.959;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TimeNode;254;-8315.889,-1420.792;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;259;-8375.187,-1635.757;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CosOpNode;261;-8556.188,-1672.757;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;262;-8740.326,-1291.624;Inherit;False;Property;_NormalMovement1;Normal Movement;6;0;Create;True;0;0;0;False;0;False;0,0;-0.05,-0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;308;-4864.974,-527.7509;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;320;-7498.131,-1330.984;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;355;-4560.75,-384.7446;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;251;-7743.587,-1216.524;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;498;-7469.829,562.0786;Inherit;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0.1764418,0.2801822,0.5754717,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;496;-7180.609,541.7747;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;499;-6980.55,535.6498;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;78;-4729.972,-477.7508;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;549;-6655.07,520.5197;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;470;-6697.338,381.6445;Inherit;False;Darken;True;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;257;-8216.18,-1634.757;Inherit;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;112;-3970.477,-484.5249;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;512;-7431.865,-1450.236;Inherit;False;Property;_NormalScale;NormalScale;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-2221.455,-375.2027;Inherit;False;Constant;_Float14;Float 14;43;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;76;-2375.498,-591.2659;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1351.5,-239.5197;Inherit;False;Property;_CausticsIntensity;Caustics Intensity;16;0;Create;True;0;0;0;False;0;False;1;0.61;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;133;-1844.199,-585.2659;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;-1832.718,-280.2068;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;154;-2475.003,-178.6868;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;228;-1221.915,-423.0067;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;334;-1001.899,-333.3775;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;365;-2203.716,-496.2076;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;381;-2218.274,-256.7455;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;418;-2030.456,-391.2028;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;419;-2573.699,-624.4648;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;453;-2488.13,-329.3636;Inherit;False;Property;_CausticsSpeed;Caustics Speed;15;0;Create;True;0;0;0;False;0;False;-0.1,-0.05;-0.2,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;404;-1663.79,-307.0276;Inherit;True;Property;_CausticsTex2;Caustics Tex2;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;466;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;582;-1950.09,-1707.406;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureTransformNode;580;-2252.228,-1813.699;Inherit;False;577;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.SamplerNode;466;-1665.8,-612.6228;Inherit;True;Property;_CausticsTex;Caustics Tex;14;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;621;55.01221,9.700914;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;622;256.0121,-58.2991;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;481;-6995.454,412.5105;Inherit;False;468;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;474;500.9225,-661.4378;Inherit;False;139;WaterColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IndirectSpecularLight;318;-4310.42,-599.6906;Inherit;False;World;3;0;FLOAT3;0,0,1;False;1;FLOAT;1;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-4543.511,-604.8128;Inherit;False;247;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;554;-3930.338,-221.9841;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;572;-4617.363,-214.6503;Inherit;False;Normaldistort;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;139;-6384.196,515.9892;Inherit;False;WaterColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PiNode;263;-8804.187,-1631.757;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;260;-8556.188,-1593.757;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;241;-6525.75,-1485.838;Inherit;False;SurfaceNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;243;-7171.893,-1360.476;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Instance;248;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;245;-6831.198,-1479.029;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;246;-6537.938,-1302.076;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;620;-174.9879,-65.2991;Inherit;False;619;ScreenPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;108;-5234.064,-558.5997;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;629;901.2884,159.5096;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;507.5858,-561.9637;Inherit;False;567;Foam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;652;434.312,-479.8795;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;653;435.4481,-295.7315;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;617;455.8255,-87.79791;Inherit;True;Property;_WaterWave;Water Wave;28;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;29;-7924.281,-750.7874;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;696;-7662.269,-743.9577;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;407;-7465.572,-522.6759;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;30;-7647.971,-522.3999;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;697;-7939.447,-504.9475;Inherit;False;247;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;32;-7487.272,-744.4219;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;694;-7304.448,-744.6019;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;695;-7141.448,-743.6019;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;659;-7446.242,-630.9518;Inherit;False;Property;_ReflectionRange;Reflection Range;10;0;Create;True;0;0;0;False;0;False;1.882353;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;538;-7450.672,-355.4809;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;537;-7626.671,-361.4819;Inherit;False;-1;;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;539;-7239.671,-439.4819;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-6958.022,-747.868;Float;False;ReflectionMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-7033.772,-444.275;Inherit;False;CausticsAreaMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;225;149.5184,-275.825;Inherit;False;239;Caustics;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;288;-748.6104,-358.4047;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;239;-528.9361,-363.5817;Float;False;Caustics;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;698;-1071.359,-564.2395;Inherit;False;Property;_CausticsColor;Caustics Color;13;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;247;-6310.127,-1307.549;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;505.6054,-194.6312;Inherit;False;143;ReflectColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;624;371.7714,365.4499;Inherit;False;625;FoamAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;744;577.4019,371.8812;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;647;571.7811,226.6514;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;378.1646,158.4036;Inherit;False;751;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;648;752.7813,288.6512;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;340;-4276.641,-199.7713;Inherit;False;35;ReflectionMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;750;-8425.051,97.90071;Inherit;False;748;ReconstructWorldPosition;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;712;-8137.132,96.64729;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;713;-8173.133,-77.35278;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;714;-7963.134,22.64727;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;759;-7778.188,17.14891;Inherit;False;YDeltha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;760;-8301.626,255.6391;Inherit;False;759;YDeltha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;758;-8059.986,283.8906;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;751;-7868.53,279.5406;Inherit;False;WaterDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;494;-8395.201,360.9184;Inherit;False;Property;_WaterDepth;Water Depth;3;0;Create;True;0;0;0;False;0;False;0.3058824;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;761;-8310.145,501.1396;Inherit;False;759;YDeltha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;762;-8068.505,529.3908;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;764;-8403.72,606.4185;Inherit;False;Property;_WaterColorRange;Water Color Range;2;0;Create;True;0;0;0;False;0;False;0.3058824;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;763;-7877.048,525.0408;Inherit;False;WaterColorRange;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;495;-7470.624,759.1722;Inherit;False;763;WaterColorRange;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;483;-7473.203,27.22715;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;484;-7250.212,28.22718;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComputeScreenPosHlpNode;485;-7023.332,26.7672;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;619;-6739.642,19.38141;Inherit;False;ScreenPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;143;-3670.69,-225.5317;Inherit;False;ReflectColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;668;-5836.397,187.9941;Inherit;False;Global;_LightDir;_Light Dir;28;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;296;-5819.534,379.0245;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;369;-5433.6,274.0741;Inherit;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;445;-5216.584,275.6778;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;608;-5464.21,547.2842;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;610;-5208.557,525.3098;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;595;-4367.47,272.5849;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;602;-4158.57,491.7719;Inherit;False;Property;_Hardness;Hardness;27;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;598;-4650.471,381.5853;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Exp2OpNode;599;-4508.471,381.5853;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;597;-4808.471,381.5853;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;596;-5100.471,375.5853;Inherit;False;Property;_Smoothness;Smoothness;26;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;594;-4992.471,272.5849;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;457;-4607.693,640.8821;Inherit;False;Property;_SpecularPower;Specular Power;25;0;Create;True;0;0;0;False;0;False;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;601;-3993.063,362.6705;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;609;-4163.239,271.6742;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;600;-3795.87,272.1684;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;603;-3549.469,365.2701;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;611;-5005.874,524.7077;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;362;-4370.33,518.6479;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;432;-5594.6,269.0741;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;472;-3365.395,359.7769;Inherit;False;Specular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-4779.849,-311.0446;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;319;-5125.542,-245.9404;Inherit;False;Property;_ReflectDistort;Reflect Distort;9;0;Create;True;0;0;0;False;0;False;0.03;0.64;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;272;-4935.851,-334.0446;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;304;-5171.542,-334.9406;Inherit;False;241;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;651;207.8182,-385.3117;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;650;-1.8284,-389.088;Inherit;False;625;FoamAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;632;-2001.9,-1363.411;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;795;-1880.464,-1094.259;Inherit;False;794;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;782;-1642.428,-1116.842;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;781;-1420.858,-1134.46;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;631;-1266.901,-1387.411;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;636;-1027.901,-1415.411;Inherit;True;Property;_TextureSample1;Texture Sample 1;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;577;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;637;-664.9,-1408.411;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;578;-2252.522,-1588.239;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;770;-1176.34,-1607.889;Inherit;False;759;YDeltha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;771;-885.3426,-1602.889;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;641;-1308.697,-1509.769;Inherit;False;Property;_SecondFoamCutOff;Second Foam CutOff;21;0;Create;True;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;791;-473.332,-1694.728;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;584;-681.2381,-1915.56;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;577;-1039.444,-1864.096;Inherit;True;Property;_FoamNoise;Foam Noise;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;579;-1288.514,-1832.427;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;767;-1204.63,-2072.956;Inherit;False;759;YDeltha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;766;-913.6304,-2067.956;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;585;-1279.045,-1959.577;Inherit;False;Property;_FoamCutOff;Foam CutOff;19;0;Create;True;0;0;0;False;0;False;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;581;-1450.324,-1704.525;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;793;-1588.715,-1588.429;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;792;-1774.708,-1566.123;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;796;-1975.508,-1548.688;Inherit;False;794;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;634;-1998.101,-1249.273;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;633;-1832.802,-1276.12;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;630;-2280.506,-1385.411;Inherit;False;Property;_SecondFoamNoiseTAS;Second Foam Noise TAS;20;0;Create;True;0;0;0;False;0;False;1,1,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;783;-1949.225,-988.4287;Inherit;False;Property;_FoamDistort;Foam Distort;23;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;778;-1604.518,-2511.056;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureTransformNode;777;-2079.521,-2494.056;Inherit;False;776;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.TimeNode;779;-2070.521,-2337.056;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;780;-1803.519,-2361.056;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;776;-1284.262,-2540.566;Inherit;True;Property;_Noise;Noise;22;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;794;-920.725,-2518.571;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;626;-60.88345,-1487.734;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;615;-218.4743,-1695.226;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;567;91.58371,-1701.037;Inherit;False;Foam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;497;-7482.194,363.0143;Inherit;False;Property;_ShallowColor;Shallow Color;0;1;[Header];Create;True;1;Base;0;0;False;0;False;0.8766465,0.9433962,0.9401634,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;613;-4321.941,-92.66714;Inherit;False;Property;_ReflectionStrength;Reflection Strength;7;1;[Header];Create;True;1;Reflection And Refraction;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;377;-2608.336,-525.6607;Inherit;False;Property;_CausticsTilling;Caustics Tilling;12;1;[Header];Create;True;1;Caustics;0;0;False;0;False;9.78;19.78;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;614;-488.183,-1522.348;Inherit;False;Property;_FoamColor;Foam Color;17;1;[Header];Create;True;1;Foam;0;0;False;0;False;0.9716981,0.9304467,0.9304467,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;605;-3828.464,457.4719;Inherit;False;Property;_SpecularColor;Specular Color;24;2;[HDR];[Header];Create;True;1;Specular;0;0;False;0;False;0.990566,0.9578586,0.9578586,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;176;-8026.836,-1425.015;Inherit;False;178;UV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;244;-8444.89,-1501.792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;255;-8165.312,-1081.134;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;192;-8400.328,-1193.624;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;258;-8171.837,-1216.015;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-8197.967,-1996.781;Inherit;False;Property;_NormalTilling;Normal Tilling;29;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;799;1112.573,-561.2629;Inherit;False;468;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;798;-8182.199,-569.2185;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;175;-5721.676,-1626.15;Inherit;False;Property;_RefractionPower1;Refraction Power;11;0;Create;True;0;0;0;False;0;False;0.1;1.81;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;197;-3600.508,-1321.889;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;196;-4008.83,-1566.319;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StepOpNode;201;-3339.407,-1409.087;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;803;-3671.526,-1481.679;Inherit;False;Property;_Float0;Float 0;30;0;Create;True;0;0;0;False;0;False;0;0.05;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;153.2711,-484.9289;Inherit;False;472;Specular;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;448;-5454.777,387.5262;Inherit;False;247;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector2Node;804;-8391.155,-1768.044;Inherit;False;Constant;_Vector0;Vector 0;31;0;Create;True;0;0;0;False;0;False;-1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;178;-7653.475,-2073.279;Inherit;False;UV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;248;-7174.668,-1660.046;Inherit;True;Property;_NormalMap;NormalMap;4;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;625;95.71648,-1488.335;Inherit;False;FoamAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;769;-2845.02,-624.1132;Inherit;False;748;ReconstructWorldPosition;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;397;-1062.156,-97.92774;Inherit;False;64;CausticsAreaMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;468;-2749.3,-1667.157;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;748;-6716.502,-2074.85;Inherit;False;ReconstructWorldPosition;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;256;-7090.503,-2068.103;Inherit;False;Reconstruct World Position From Depth;-1;;14;e7094bcbcc80eb140b2a3dbe6a861de8;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;425;-4373.997,-412.7379;Inherit;True;Global;_PlanarReflectionTexture;_PlanarReflectionTexture;8;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;1;2;297;0
WireConnection;1;3;629;0
WireConnection;297;0;474;0
WireConnection;297;1;74;0
WireConnection;297;2;652;0
WireConnection;297;3;653;0
WireConnection;297;4;106;0
WireConnection;168;0;172;0
WireConnection;168;1;216;0
WireConnection;170;1;174;1
WireConnection;171;0;173;0
WireConnection;189;0;202;0
WireConnection;189;1;211;0
WireConnection;189;2;201;0
WireConnection;194;0;205;0
WireConnection;195;0;203;0
WireConnection;195;1;200;0
WireConnection;198;0;195;0
WireConnection;198;1;194;0
WireConnection;198;2;193;4
WireConnection;199;0;196;0
WireConnection;199;1;195;0
WireConnection;203;0;206;0
WireConnection;203;1;208;0
WireConnection;204;0;214;0
WireConnection;205;0;209;1
WireConnection;205;1;209;2
WireConnection;205;2;204;0
WireConnection;206;0;213;0
WireConnection;206;1;212;4
WireConnection;207;0;171;0
WireConnection;207;1;173;0
WireConnection;207;2;170;0
WireConnection;208;0;211;0
WireConnection;209;0;210;0
WireConnection;211;0;479;0
WireConnection;211;1;168;0
WireConnection;213;0;180;0
WireConnection;213;1;191;0
WireConnection;214;0;174;2
WireConnection;214;1;174;3
WireConnection;214;2;207;0
WireConnection;420;0;189;0
WireConnection;216;0;482;0
WireConnection;172;0;175;0
WireConnection;181;0;182;1
WireConnection;181;1;182;3
WireConnection;177;0;181;0
WireConnection;177;1;179;0
WireConnection;169;0;177;0
WireConnection;249;0;250;0
WireConnection;249;1;252;0
WireConnection;250;0;804;0
WireConnection;250;1;254;2
WireConnection;250;2;244;0
WireConnection;252;0;176;0
WireConnection;252;1;258;0
WireConnection;253;0;804;0
WireConnection;253;1;254;2
WireConnection;253;2;262;1
WireConnection;259;0;261;0
WireConnection;259;1;260;0
WireConnection;261;0;263;0
WireConnection;308;0;108;1
WireConnection;308;1;108;2
WireConnection;320;0;251;0
WireConnection;320;1;253;0
WireConnection;355;0;78;0
WireConnection;355;1;128;0
WireConnection;251;0;176;0
WireConnection;251;1;255;0
WireConnection;496;0;497;0
WireConnection;496;1;498;0
WireConnection;496;2;495;0
WireConnection;499;0;496;0
WireConnection;78;0;308;0
WireConnection;78;1;108;4
WireConnection;549;0;481;0
WireConnection;549;1;499;0
WireConnection;470;0;481;0
WireConnection;470;1;499;0
WireConnection;257;0;804;0
WireConnection;112;0;318;0
WireConnection;112;1;425;0
WireConnection;112;2;340;0
WireConnection;76;0;419;0
WireConnection;76;1;377;0
WireConnection;133;0;76;0
WireConnection;133;1;381;0
WireConnection;138;0;418;0
WireConnection;138;1;381;0
WireConnection;228;0;466;0
WireConnection;228;1;404;0
WireConnection;334;0;228;0
WireConnection;334;1;100;0
WireConnection;365;0;76;0
WireConnection;381;0;453;0
WireConnection;381;1;154;0
WireConnection;418;0;365;0
WireConnection;418;1;73;0
WireConnection;419;0;769;0
WireConnection;404;1;138;0
WireConnection;582;0;580;1
WireConnection;582;1;578;1
WireConnection;466;1;133;0
WireConnection;621;0;620;0
WireConnection;622;0;620;0
WireConnection;622;1;621;0
WireConnection;318;0;384;0
WireConnection;554;0;425;0
WireConnection;554;1;340;0
WireConnection;554;2;613;0
WireConnection;572;0;128;0
WireConnection;139;0;499;0
WireConnection;260;0;263;0
WireConnection;241;0;245;0
WireConnection;243;1;320;0
WireConnection;243;5;512;0
WireConnection;245;0;248;0
WireConnection;245;1;243;0
WireConnection;246;0;245;0
WireConnection;629;0;42;0
WireConnection;629;1;648;0
WireConnection;629;2;648;0
WireConnection;652;0;294;0
WireConnection;652;1;651;0
WireConnection;653;0;651;0
WireConnection;653;1;225;0
WireConnection;617;1;622;0
WireConnection;696;0;29;0
WireConnection;696;1;798;0
WireConnection;407;0;30;0
WireConnection;30;0;29;0
WireConnection;30;1;697;0
WireConnection;32;0;696;0
WireConnection;694;0;32;0
WireConnection;695;0;694;0
WireConnection;695;1;659;0
WireConnection;538;0;537;0
WireConnection;539;0;407;0
WireConnection;539;1;538;0
WireConnection;35;0;695;0
WireConnection;64;0;407;0
WireConnection;288;0;698;0
WireConnection;288;1;334;0
WireConnection;288;2;397;0
WireConnection;239;0;288;0
WireConnection;247;0;246;0
WireConnection;744;0;624;0
WireConnection;647;0;42;0
WireConnection;648;0;647;0
WireConnection;648;1;624;0
WireConnection;712;0;750;0
WireConnection;714;0;713;2
WireConnection;714;1;712;0
WireConnection;759;0;714;0
WireConnection;758;0;760;0
WireConnection;758;2;494;0
WireConnection;751;0;758;0
WireConnection;762;0;761;0
WireConnection;762;2;764;0
WireConnection;763;0;762;0
WireConnection;484;0;483;0
WireConnection;485;0;484;0
WireConnection;619;0;485;0
WireConnection;143;0;554;0
WireConnection;369;0;432;0
WireConnection;445;0;369;0
WireConnection;445;1;448;0
WireConnection;610;0;369;0
WireConnection;610;1;608;0
WireConnection;595;0;594;0
WireConnection;595;1;599;0
WireConnection;598;0;597;0
WireConnection;599;0;598;0
WireConnection;597;0;596;0
WireConnection;594;0;445;0
WireConnection;601;1;609;0
WireConnection;609;0;595;0
WireConnection;609;1;362;0
WireConnection;600;0;609;0
WireConnection;600;1;601;0
WireConnection;600;2;602;0
WireConnection;603;0;600;0
WireConnection;603;1;605;0
WireConnection;611;0;610;0
WireConnection;362;0;611;0
WireConnection;362;1;457;0
WireConnection;432;0;668;0
WireConnection;432;1;296;0
WireConnection;472;0;603;0
WireConnection;128;0;272;0
WireConnection;128;1;319;0
WireConnection;272;0;304;0
WireConnection;651;0;650;0
WireConnection;632;0;630;1
WireConnection;632;1;630;2
WireConnection;782;0;633;0
WireConnection;782;1;795;0
WireConnection;781;0;633;0
WireConnection;781;1;782;0
WireConnection;781;2;783;0
WireConnection;631;0;632;0
WireConnection;631;1;781;0
WireConnection;636;1;631;0
WireConnection;637;0;771;0
WireConnection;637;1;636;1
WireConnection;771;0;770;0
WireConnection;771;2;641;0
WireConnection;791;0;584;0
WireConnection;791;1;637;0
WireConnection;584;0;766;0
WireConnection;584;1;577;1
WireConnection;577;1;579;0
WireConnection;579;0;580;0
WireConnection;579;1;581;0
WireConnection;766;0;767;0
WireConnection;766;2;585;0
WireConnection;581;0;582;0
WireConnection;581;1;793;0
WireConnection;793;0;582;0
WireConnection;793;1;792;0
WireConnection;793;2;783;0
WireConnection;792;0;582;0
WireConnection;792;1;796;0
WireConnection;634;0;630;3
WireConnection;634;1;630;4
WireConnection;633;0;578;1
WireConnection;633;1;634;0
WireConnection;778;0;777;0
WireConnection;778;1;780;0
WireConnection;780;0;777;1
WireConnection;780;1;779;1
WireConnection;776;1;778;0
WireConnection;794;0;776;1
WireConnection;626;0;615;0
WireConnection;615;0;791;0
WireConnection;615;1;614;0
WireConnection;567;0;615;0
WireConnection;244;0;262;1
WireConnection;255;1;262;2
WireConnection;192;0;262;2
WireConnection;258;1;192;0
WireConnection;197;0;199;0
WireConnection;201;0;803;0
WireConnection;201;1;197;0
WireConnection;178;0;169;0
WireConnection;248;1;249;0
WireConnection;248;5;512;0
WireConnection;625;0;626;0
WireConnection;468;0;420;0
WireConnection;748;0;256;0
WireConnection;425;1;355;0
ASEEND*/
//CHKSM=073B4362387AB89BEF77D27F7558AF14BEB74A25