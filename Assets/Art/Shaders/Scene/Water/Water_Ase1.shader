// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water_Ase1"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_NormalTexture("Normal Texture", 2D) = "white" {}
		[Toggle]_WorldSpaceUV("World Space UV", Float) = 0
		_NormalMovement("Normal Movement", Vector) = (0.04,0.36,0,0)
		_Refraction("Refraction", Float) = 0.022
		_GradientTexture("Gradient Texture", 2D) = "white" {}
		[Toggle]_Gradient("Gradient", Float) = 0
		_HorizonColor("Horizon Color", Color) = (0,0.227451,0.8,1)
		_ShallowColor("Shallow Color", Color) = (0,0.772549,0.6235294,0.5019608)
		_DeepColor("Deep Color", Color) = (0.01176471,0.2862745,0.509804,1)
		_ColorDepth("Color Depth", Float) = 0.84
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
		[ASEEnd][Toggle(_LIGHTING_ON)] _Lighting("Lighting", Float) = 1


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

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

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
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

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

			#include "Includes/StylizedWaterForURP.hlsl"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_NORMAL
			#pragma shader_feature_local _LIGHTING_ON
			#pragma shader_feature_local _INTERSECTIONEFFECTS_ON
			#pragma shader_feature_local _SURFACEFOAM_ON
			#pragma shader_feature_local _FOAMSHADOWS_ON
			#pragma shader_feature_local _SHOREMOVEMENT_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
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
				float4 ase_texcoord8 : TEXCOORD8;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
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
			

			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				
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
				o.ase_texcoord8 = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				
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

				float3 vertexValue = position151;

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
				float4 ase_color : COLOR;
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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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
				float2 _Vector0 = float2(-1,0);
				float2 temp_output_2_0_g51 = _NormalMovement;
				float temp_output_12_0_g51 = (temp_output_2_0_g51).x;
				float2 texCoord8_g52 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult2_g52 = (float2(WorldPosition.x , WorldPosition.z));
				float2 temp_output_4_0_g51 = (( _WorldSpaceUV )?( -( appendResult2_g52 * 0.1 ) ):( texCoord8_g52 ));
				float temp_output_13_0_g51 = (temp_output_2_0_g51).y;
				float3 SurfaceNormals34 = BlendNormal( UnpackNormalScale( tex2D( _NormalTexture, ( ( _Vector0 * ( _TimeParameters.x ) * ( temp_output_12_0_g51 * -0.5 ) ) + ( temp_output_4_0_g51 / ( temp_output_13_0_g51 * 0.5 ) ) ) ), 1.0f ) , UnpackNormalScale( tex2D( _NormalTexture, ( ( _Vector0 * ( _TimeParameters.x ) * temp_output_12_0_g51 ) + ( temp_output_4_0_g51 / temp_output_13_0_g51 ) ) ), 1.0f ) );
				float3 ase_worldTangent = IN.ase_texcoord5.xyz;
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord7.xyz;
				float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
				float3 tangentToViewDir37 = mul( UNITY_MATRIX_V, float4( mul( ase_tangentToWorldFast, SurfaceNormals34 ), 0 ) ).xyz;
				float4 temp_output_41_0 = ( ase_screenPosNorm + float4( ( tangentToViewDir37 * ( _Refraction * 0.2 ) ) , 0.0 ) );
				float eyeDepth27_g24 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_41_0.xy ),_ZBufferParams);
				float3 objToView14_g24 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g24 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g24 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g24 : ( 1.0 - clampDepth4_g24 ) ));
				float4 appendResult15_g24 = (float4(objToView14_g24.x , objToView14_g24.y , -lerpResult8_g24 , 0.0));
				float3 viewToWorld16_g24 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g24.xyz, 1 ) ).xyz;
				float3 lerpResult17_g24 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g24 ) + _WorldSpaceCameraPos ) , viewToWorld16_g24 , unity_OrthoParams.w);
				float2 RefractUV73 = (( (( WorldPosition - lerpResult17_g24 )).y >= 0.0 ? temp_output_41_0 : ase_screenPosNorm )).xy;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float fresnelNdotV134 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode134 = ( 0.0 + _ReflectionStrength * pow( 1.0 - fresnelNdotV134, _ReflectionFresnel ) );
				float4 lerpResult133 = lerp( float4( 0,0,0,0 ) , tex2D( _PlanarReflectionTexture, RefractUV73 ) , fresnelNode134);
				float4 ReflectionColor135 = lerpResult133;
				float4 temp_output_1_0_g27 = float4( RefractUV73, 0.0 , 0.0 );
				float eyeDepth27_g28 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g27.xy ),_ZBufferParams);
				float3 objToView14_g28 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g28 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g28 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g28 : ( 1.0 - clampDepth4_g28 ) ));
				float4 appendResult15_g28 = (float4(objToView14_g28.x , objToView14_g28.y , -lerpResult8_g28 , 0.0));
				float3 viewToWorld16_g28 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g28.xyz, 1 ) ).xyz;
				float3 lerpResult17_g28 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g28 ) + _WorldSpaceCameraPos ) , viewToWorld16_g28 , unity_OrthoParams.w);
				float3 break9_g27 = ( WorldPosition - lerpResult17_g28 );
				float temp_output_14_0_g27 = ( ( IN.ase_normal.x * break9_g27.x ) + ( IN.ase_normal.y * break9_g27.y ) + ( IN.ase_normal.z * break9_g27.z ) );
				float temp_output_2_0_g27 = _ColorDepth;
				float4 lerpResult84 = lerp( _DeepColor , _ShallowColor , saturate( exp( ( -temp_output_14_0_g27 / temp_output_2_0_g27 ) ) ));
				float2 temp_cast_5 = (saturate( ( temp_output_14_0_g27 / temp_output_2_0_g27 ) )).xx;
				float4 DeepColor119 = (( _Gradient )?( tex2D( _GradientTexture, temp_cast_5 ) ):( lerpResult84 ));
				float fresnelNdotV90 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode90 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV90, _HorizonDistance ) );
				float4 lerpResult91 = lerp( DeepColor119 , _HorizonColor , fresnelNode90);
				float4 HorizonColor121 = lerpResult91;
				float4 temp_output_1_0_g29 = ase_screenPosNorm;
				float eyeDepth27_g30 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g29.xy ),_ZBufferParams);
				float3 objToView14_g30 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g30 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g30 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g30 : ( 1.0 - clampDepth4_g30 ) ));
				float4 appendResult15_g30 = (float4(objToView14_g30.x , objToView14_g30.y , -lerpResult8_g30 , 0.0));
				float3 viewToWorld16_g30 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g30.xyz, 1 ) ).xyz;
				float3 lerpResult17_g30 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g30 ) + _WorldSpaceCameraPos ) , viewToWorld16_g30 , unity_OrthoParams.w);
				float3 break9_g29 = ( WorldPosition - lerpResult17_g30 );
				float temp_output_14_0_g29 = ( ( IN.ase_normal.x * break9_g29.x ) + ( IN.ase_normal.y * break9_g29.y ) + ( IN.ase_normal.z * break9_g29.z ) );
				float temp_output_2_0_g29 = _ShoreDepth;
				float temp_output_99_3 = saturate( ( temp_output_14_0_g29 / temp_output_2_0_g29 ) );
				float smoothstepResult101 = smoothstep( ( 1.0 - _ShoreFade ) , 1.0 , ( temp_output_99_3 + 0.1 ));
				float smoothstepResult107 = smoothstep( _ShoreBlend , 0.0 , ( temp_output_99_3 + 0.3 ));
				float lerpResult105 = lerp( ( ( 1.0 - smoothstepResult101 ) * _ShoreColor.a ) , 0.0 , smoothstepResult107);
				float4 lerpResult109 = lerp( HorizonColor121 , _ShoreColor , saturate( lerpResult105 ));
				float4 ShoreColor124 = lerpResult109;
				float4 fetchOpaqueVal110 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( RefractUV73 ), 1.0 );
				float4 UnderWaterColor125 = ( fetchOpaqueVal110 * ( 1.0 - (HorizonColor121).a ) );
				float4 temp_output_207_0 = ( ReflectionColor135 + ( ShoreColor124 + UnderWaterColor125 ) );
				float4 _Vector1 = float4(0,0,0,0);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult191 = dot( SafeNormalize(_MainLightPosition.xyz) , normalizedWorldNormal );
				float4 temp_output_1_0_g34 = ase_screenPosNorm;
				float eyeDepth27_g35 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g34.xy ),_ZBufferParams);
				float3 objToView14_g35 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g35 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g35 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g35 : ( 1.0 - clampDepth4_g35 ) ));
				float4 appendResult15_g35 = (float4(objToView14_g35.x , objToView14_g35.y , -lerpResult8_g35 , 0.0));
				float3 viewToWorld16_g35 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g35.xyz, 1 ) ).xyz;
				float3 lerpResult17_g35 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g35 ) + _WorldSpaceCameraPos ) , viewToWorld16_g35 , unity_OrthoParams.w);
				float3 break9_g34 = ( WorldPosition - lerpResult17_g35 );
				float temp_output_14_0_g34 = ( ( IN.ase_normal.x * break9_g34.x ) + ( IN.ase_normal.y * break9_g34.y ) + ( IN.ase_normal.z * break9_g34.z ) );
				float temp_output_2_0_g34 = _SurfaceFoamShadowDepth;
				float localFoamColor_half195 = ( 0.0 );
				float4 FoamColor1195 = _SurfaceFoamColor1;
				float4 FoamColor2195 = _SurfaceFoamColor2;
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = WorldPosition;
				float3 appendResult142 = (float3(( IN.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float Height150 = saturate( (Offset145).y );
				float smoothstepResult167 = smoothstep( _HeightMask , ( _HeightMask + _HeightMaskSmoothness ) , Height150);
				float lerpResult170 = lerp( 1.0 , smoothstepResult167 , step( 0.0 , _HeightMask ));
				float localFoamSample174 = ( 0.0 );
				float localSurfaceFoamUV_half173 = ( 0.0 );
				float2 texCoord8_g31 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult2_g31 = (float2(WorldPosition.x , WorldPosition.z));
				float2 UV173 = (( _WorldSpaceUV )?( -( appendResult2_g31 * 0.1 ) ):( texCoord8_g31 ));
				float4 Movement173 = _SurfaceFoamMovement;
				float4 TillingandOffset173 = _SurfaceFoamTillingandOffset;
				float4 Out173 = float4( 0,0,0,0 );
				SurfaceFoamUV_half( UV173 , Movement173 , TillingandOffset173 , Out173 );
				float4 UVs174 = Out173;
				float2 Sampling174 = _SurfaceFoamSampling;
				sampler2D Texture174 = _SurfaceFoamTexture;
				float4 temp_output_1_0_g32 = ase_screenPosNorm;
				float eyeDepth27_g33 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g32.xy ),_ZBufferParams);
				float3 objToView14_g33 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g33 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g33 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g33 : ( 1.0 - clampDepth4_g33 ) ));
				float4 appendResult15_g33 = (float4(objToView14_g33.x , objToView14_g33.y , -lerpResult8_g33 , 0.0));
				float3 viewToWorld16_g33 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g33.xyz, 1 ) ).xyz;
				float3 lerpResult17_g33 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g33 ) + _WorldSpaceCameraPos ) , viewToWorld16_g33 , unity_OrthoParams.w);
				float3 break9_g32 = ( WorldPosition - lerpResult17_g33 );
				float temp_output_14_0_g32 = ( ( IN.ase_normal.x * break9_g32.x ) + ( IN.ase_normal.y * break9_g32.y ) + ( IN.ase_normal.z * break9_g32.z ) );
				float temp_output_2_0_g32 = _SurfaceFoamShadowProjection;
				float3x3 ase_worldToTangent = float3x3(ase_worldTangent,ase_worldBitangent,ase_worldNormal);
				float3 worldToTangentDir180 = mul( ase_worldToTangent, SafeNormalize(_MainLightPosition.xyz));
				float localViewDirectionParallax_half183 = ( 0.0 );
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 ase_tanViewDir =  tanToWorld0 * ase_worldViewDir.x + tanToWorld1 * ase_worldViewDir.y  + tanToWorld2 * ase_worldViewDir.z;
				ase_tanViewDir = SafeNormalize( ase_tanViewDir );
				float3 tangentViewDir183 = ase_tanViewDir;
				float3 Out183 = float3( 0,0,0 );
				ViewDirectionParallax_half( tangentViewDir183 , Out183 );
				float2 ShadowOffset174 = ( saturate( ( temp_output_14_0_g32 / temp_output_2_0_g32 ) ) * ( worldToTangentDir180 + Out183 ) ).xy;
				float4 Out174 = float4( 0,0,0,0 );
				FoamSample( UVs174 , Sampling174 , Texture174 , ShadowOffset174 , Out174 );
				float4 break185 = ( lerpResult170 * Out174 );
				float Foam1195 = break185.x;
				float Foam2195 = break185.y;
				float Shadow1195 = break185.z;
				float Shadow2195 = break185.w;
				float Shadow195 = 0.0;
				float4 Foam195 = float4( 0,0,0,0 );
				FoamColor_half( FoamColor1195 , FoamColor2195 , Foam1195 , Foam2195 , Shadow1195 , Shadow2195 , Shadow195 , Foam195 );
				float4 appendResult197 = (float4(0.0 , 0.0 , 0.0 , ( saturate( ( _ShadowStrength * ( dotResult191 * ( 1.0 - saturate( ( temp_output_14_0_g34 / temp_output_2_0_g34 ) ) ) ) ) ) * Shadow195 )));
				float4 SurfaceFoamShadow198 = appendResult197;
				#ifdef _SURFACEFOAM_ON
				float4 staticSwitch205 = SurfaceFoamShadow198;
				#else
				float4 staticSwitch205 = _Vector1;
				#endif
				float4 lerpResult208 = lerp( temp_output_207_0 , staticSwitch205 , saturate( (staticSwitch205).w ));
				float ShadowStrength235 = _ShadowStrength;
				float dotResult256 = dot( SafeNormalize(_MainLightPosition.xyz) , normalizedWorldNormal );
				float4 temp_output_1_0_g44 = ase_screenPosNorm;
				float eyeDepth27_g45 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g44.xy ),_ZBufferParams);
				float3 objToView14_g45 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g45 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g45 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g45 : ( 1.0 - clampDepth4_g45 ) ));
				float4 appendResult15_g45 = (float4(objToView14_g45.x , objToView14_g45.y , -lerpResult8_g45 , 0.0));
				float3 viewToWorld16_g45 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g45.xyz, 1 ) ).xyz;
				float3 lerpResult17_g45 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g45 ) + _WorldSpaceCameraPos ) , viewToWorld16_g45 , unity_OrthoParams.w);
				float3 break9_g44 = ( WorldPosition - lerpResult17_g45 );
				float temp_output_14_0_g44 = ( ( IN.ase_normal.x * break9_g44.x ) + ( IN.ase_normal.y * break9_g44.y ) + ( IN.ase_normal.z * break9_g44.z ) );
				float temp_output_2_0_g44 = _FoamShadowDepth;
				float localFoamSample238 = ( 0.0 );
				float localIntersectionFoamUV_half233 = ( 0.0 );
				float2 texCoord8_g43 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult2_g43 = (float2(WorldPosition.x , WorldPosition.z));
				float2 UV233 = (( _WorldSpaceUV )?( -( appendResult2_g43 * 0.1 ) ):( texCoord8_g43 ));
				float2 Movement233 = _IntersectionFoamMovement;
				float Scale233 = _IntersectionFoamScale;
				float4 Directional233 = float4( 0,0,0,0 );
				float4 ByDepth233 = float4( 0,0,0,0 );
				IntersectionFoamUV_half( UV233 , Movement233 , Scale233 , Directional233 , ByDepth233 );
				float4 UVs238 = Directional233;
				float4 temp_output_1_0_g36 = ase_screenPosNorm;
				float eyeDepth27_g37 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g36.xy ),_ZBufferParams);
				float3 objToView14_g37 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g37 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g37 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g37 : ( 1.0 - clampDepth4_g37 ) ));
				float4 appendResult15_g37 = (float4(objToView14_g37.x , objToView14_g37.y , -lerpResult8_g37 , 0.0));
				float3 viewToWorld16_g37 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g37.xyz, 1 ) ).xyz;
				float3 lerpResult17_g37 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g37 ) + _WorldSpaceCameraPos ) , viewToWorld16_g37 , unity_OrthoParams.w);
				float3 break9_g36 = ( WorldPosition - lerpResult17_g37 );
				float temp_output_14_0_g36 = ( ( IN.ase_normal.x * break9_g36.x ) + ( IN.ase_normal.y * break9_g36.y ) + ( IN.ase_normal.z * break9_g36.z ) );
				float temp_output_2_0_g36 = _IntersectionDepth;
				float temp_output_214_3 = saturate( ( temp_output_14_0_g36 / temp_output_2_0_g36 ) );
				float2 Sampling238 = ( _IntersectionFoamSampling * temp_output_214_3 );
				sampler2D Texture238 = _IntersectionFoamTexture;
				float4 temp_output_1_0_g41 = ase_screenPosNorm;
				float eyeDepth27_g42 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( temp_output_1_0_g41.xy ),_ZBufferParams);
				float3 objToView14_g42 = mul( UNITY_MATRIX_MV, float4( IN.ase_texcoord8.xyz, 1 ) ).xyz;
				float clampDepth4_g42 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				float lerpResult8_g42 = lerp( _ProjectionParams.y , _ProjectionParams.z , ( _ProjectionParams.x > 0.0 ? clampDepth4_g42 : ( 1.0 - clampDepth4_g42 ) ));
				float4 appendResult15_g42 = (float4(objToView14_g42.x , objToView14_g42.y , -lerpResult8_g42 , 0.0));
				float3 viewToWorld16_g42 = mul( UNITY_MATRIX_I_V, float4( appendResult15_g42.xyz, 1 ) ).xyz;
				float3 lerpResult17_g42 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth27_g42 ) + _WorldSpaceCameraPos ) , viewToWorld16_g42 , unity_OrthoParams.w);
				float3 break9_g41 = ( WorldPosition - lerpResult17_g42 );
				float temp_output_14_0_g41 = ( ( IN.ase_normal.x * break9_g41.x ) + ( IN.ase_normal.y * break9_g41.y ) + ( IN.ase_normal.z * break9_g41.z ) );
				float temp_output_2_0_g41 = _IntersectionFoamShadowProjection;
				float3 worldToTangentDir243 = mul( ase_worldToTangent, SafeNormalize(_MainLightPosition.xyz));
				float localViewDirectionParallax_half247 = ( 0.0 );
				float3 tangentViewDir247 = ase_tanViewDir;
				float3 Out247 = float3( 0,0,0 );
				ViewDirectionParallax_half( tangentViewDir247 , Out247 );
				float2 ShadowOffset238 = ( saturate( ( temp_output_14_0_g41 / temp_output_2_0_g41 ) ) * ( worldToTangentDir243 + Out247 ) ).xy;
				float4 Out238 = float4( 0,0,0,0 );
				FoamSample( UVs238 , Sampling238 , Texture238 , ShadowOffset238 , Out238 );
				float4 break249 = Out238;
				float IntersectionDepth_Linear225 = temp_output_214_3;
				float smoothstepResult228 = smoothstep( ( 1.0 - _IntersectionWaterBlend ) , 1.0 , ( IntersectionDepth_Linear225 + 0.1 ));
				float IntersectionEffectMask265 = ( 1.0 - smoothstepResult228 );
				float4 appendResult268 = (float4(0.0 , 0.0 , 0.0 , ( ( saturate( ( ShadowStrength235 * ( dotResult256 * ( 1.0 - saturate( ( temp_output_14_0_g44 / temp_output_2_0_g44 ) ) ) ) ) ) * ( saturate( ( break249.x + break249.y ) ) * _IntersectionFoamColor.a ) ) * IntersectionEffectMask265 )));
				float4 IntersectionFoamShadow269 = appendResult268;
				#ifdef _SHOREMOVEMENT_ON
				float4 staticSwitch280 = _Vector1;
				#else
				float4 staticSwitch280 = IntersectionFoamShadow269;
				#endif
				#ifdef _INTERSECTIONEFFECTS_ON
				float4 staticSwitch281 = staticSwitch280;
				#else
				float4 staticSwitch281 = _Vector1;
				#endif
				float4 lerpResult282 = lerp( lerpResult208 , staticSwitch281 , saturate( (staticSwitch281).w ));
				#ifdef _FOAMSHADOWS_ON
				float4 staticSwitch285 = lerpResult282;
				#else
				float4 staticSwitch285 = temp_output_207_0;
				#endif
				float4 temp_output_1_0_g49 = staticSwitch285;
				float4 SurfaceFoam199 = Foam195;
				float4 temp_output_2_0_g49 = SurfaceFoam199;
				float temp_output_6_0_g49 = saturate( (temp_output_2_0_g49).w );
				float4 lerpResult4_g49 = lerp( temp_output_1_0_g49 , temp_output_2_0_g49 , temp_output_6_0_g49);
				float4 blendOpSrc7_g49 = temp_output_2_0_g49;
				float4 blendOpDest7_g49 = temp_output_1_0_g49;
				float4 lerpBlendMode7_g49 = lerp(blendOpDest7_g49,( blendOpSrc7_g49 + blendOpDest7_g49 ),temp_output_6_0_g49);
				float4 lerpResult8_g49 = lerp( lerpResult4_g49 , ( saturate( lerpBlendMode7_g49 )) , _SurfaceFoamBlend);
				#ifdef _SURFACEFOAM_ON
				float4 staticSwitch290 = lerpResult8_g49;
				#else
				float4 staticSwitch290 = staticSwitch285;
				#endif
				float4 temp_output_1_0_g50 = staticSwitch290;
				float4 temp_output_272_0 = ( _IntersectionFoamColor * saturate( ( break249.z + break249.w ) ) );
				float4 appendResult274 = (float4((temp_output_272_0).rgb , ( IntersectionEffectMask265 * (temp_output_272_0).a )));
				float4 IntersectionFoam277 = appendResult274;
				float temp_output_305_0 = ( 1.0 - IntersectionDepth_Linear225 );
				float2 texCoord313 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float gradientNoise312 = UnityGradientNoise(texCoord313,_ShoreFoamBreakupScale);
				gradientNoise312 = gradientNoise312*0.5 + 0.5;
				float4 IntersectionFoamColor323 = _IntersectionFoamColor;
				float4 ShoreFoam325 = ( ( step( ( 1.0 - ( ( _ShoreFoamWidth - temp_output_305_0 ) + 0.01 ) ) , ( temp_output_305_0 + ( sin( ( ( temp_output_305_0 * _ShoreFoamFrequency ) + ( _ShoreFoamSpeed * ( _TimeParameters.x ) ) ) ) + ( gradientNoise312 - _ShoreFoamBreakupStrength ) ) ) ) * IntersectionEffectMask265 ) * IntersectionFoamColor323 );
				#ifdef _SHOREMOVEMENT_ON
				float4 staticSwitch328 = ShoreFoam325;
				#else
				float4 staticSwitch328 = IntersectionFoam277;
				#endif
				float4 temp_output_2_0_g50 = staticSwitch328;
				float temp_output_6_0_g50 = saturate( (temp_output_2_0_g50).w );
				float4 lerpResult4_g50 = lerp( temp_output_1_0_g50 , temp_output_2_0_g50 , temp_output_6_0_g50);
				float4 blendOpSrc7_g50 = temp_output_2_0_g50;
				float4 blendOpDest7_g50 = temp_output_1_0_g50;
				float4 lerpBlendMode7_g50 = lerp(blendOpDest7_g50,( blendOpSrc7_g50 + blendOpDest7_g50 ),temp_output_6_0_g50);
				float4 lerpResult8_g50 = lerp( lerpResult4_g50 , ( saturate( lerpBlendMode7_g50 )) , _IntersectionFoamBlend);
				#ifdef _INTERSECTIONEFFECTS_ON
				float4 staticSwitch293 = lerpResult8_g50;
				#else
				float4 staticSwitch293 = staticSwitch290;
				#endif
				float4 Foam330 = staticSwitch293;
				float localMainLighting_half350 = ( 0.0 );
				float3 Position350 = WorldPosition;
				float3 In358 = SurfaceNormals34;
				float Strength358 = _NormalStrength;
				float3 localNormalStrength358 = NormalStrength358( In358 , Strength358 );
				float3 tangentToWorldDir359 = normalize( mul( ase_tangentToWorldFast, localNormalStrength358 ) );
				float3 Normal350 = tangentToWorldDir359;
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 View350 = ase_worldViewDir;
				float Smoothness350 = _Smoothness;
				float Specular350 = 0.0;
				float Diffuse350 = 0.0;
				MainLighting_half( Position350 , Normal350 , View350 , Smoothness350 , Specular350 , Diffuse350 );
				float lerpResult354 = lerp( Specular350 , step( 0.5 , Specular350 ) , _Hardness);
				float lerpResult361 = lerp( Diffuse350 , step( 0.5 , Diffuse350 ) , _Hardness);
				float localAdditionalLighting_half365 = ( 0.0 );
				float3 Position365 = WorldPosition;
				float3 Normal365 = tangentToWorldDir359;
				float3 View365 = ase_worldViewDir;
				float Smoothness365 = _Smoothness;
				float Hardness365 = _Hardness;
				float3 Specular365 = float3( 0,0,0 );
				float3 Diffuse365 = float3( 0,0,0 );
				AdditionalLighting_half( Position365 , Normal365 , View365 , Smoothness365 , Hardness365 , Specular365 , Diffuse365 );
				float4 Lighting366 = ( ( ( _SpecularColor * lerpResult354 ) + ( lerpResult361 * _DiffuseColor ) ) + float4( Specular365 , 0.0 ) );
				#ifdef _LIGHTING_ON
				float4 staticSwitch371 = ( Lighting366 + Foam330 );
				#else
				float4 staticSwitch371 = Foam330;
				#endif
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = staticSwitch371.rgb;
				float Alpha = 1;
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
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#include "Includes/StylizedWaterForURP.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = position151;

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
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

				

				float Alpha = 1;
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

		
		Pass
		{
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }

			Cull Off

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107


			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Includes/StylizedWaterForURP.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = position151;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107


			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Includes/StylizedWaterForURP.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			float4 _SelectionID;


			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = position151;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On


			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107


			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Includes/StylizedWaterForURP.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = position151;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

				o.clipPos = TransformWorldToHClip(positionWS);
				o.normalWS.xyz =  normalWS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				float3 normalWS = IN.normalWS;

				return half4(NormalizeNormalPerPixel(normalWS), 0.0);
			}

			ENDHLSL
		}

		
		Pass
		{
			
            Name "DepthNormalsOnly"
            Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_SRP_VERSION 120107


			#pragma exclude_renderers glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define ATTRIBUTES_NEED_TEXCOORD1
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Includes/StylizedWaterForURP.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _SurfaceFoamMovement;
			float4 _SpecularColor;
			float4 _IntersectionFoamColor;
			float4 _SurfaceFoamTillingandOffset;
			float4 _SurfaceFoamColor2;
			float4 _SurfaceFoamColor1;
			float4 _ShoreColor;
			float4 _HorizonColor;
			float4 _ShallowColor;
			float4 _DiffuseColor;
			float4 _WaveDirections;
			float4 _DeepColor;
			float3 _WaveVisuals;
			float2 _NormalMovement;
			float2 _IntersectionFoamSampling;
			float2 _SurfaceFoamSampling;
			float2 _IntersectionFoamMovement;
			float _ShadowStrength;
			float _Smoothness;
			float _NormalStrength;
			float _Gradient;
			float _IntersectionFoamBlend;
			float _ShoreFoamBreakupStrength;
			float _ShoreFoamBreakupScale;
			float _ShoreFoamSpeed;
			float _ShoreFoamFrequency;
			float _ShoreFoamWidth;
			float _SurfaceFoamBlend;
			float _IntersectionWaterBlend;
			float _HorizonDistance;
			float _IntersectionFoamShadowProjection;
			float _IntersectionDepth;
			float _ReflectionFresnel;
			float _IntersectionFoamScale;
			float _ShoreFade;
			float _FoamShadowDepth;
			float _SurfaceFoamShadowProjection;
			float _ShoreDepth;
			float _WorldSpaceUV;
			float _Hardness;
			float _HeightMaskSmoothness;
			float _HeightMask;
			float _Refraction;
			float _ReflectionStrength;
			float _SurfaceFoamShadowDepth;
			float _ShoreBlend;
			float _ColorDepth;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				float localGerstnerWaves_float145 = ( 0.0 );
				float3 Position145 = ase_worldPos;
				float3 appendResult142 = (float3(( v.ase_color.g * _WaveVisuals.x ) , _WaveVisuals.y , _WaveVisuals.z));
				float3 Visuals145 = appendResult142;
				float4 Directions145 = _WaveDirections;
				float3 Offset145 = float3( 0,0,0 );
				float3 Normal145 = float3( 0,0,0 );
				GerstnerWaves_float( Position145 , Visuals145 , Directions145 , Offset145 , Normal145 );
				float3 worldToObj147 = mul( GetWorldToObjectMatrix(), float4( ( ase_worldPos + Offset145 ), 1 ) ).xyz;
				float3 position151 = worldToObj147;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = position151;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

				o.clipPos = TransformWorldToHClip(positionWS);
				o.normalWS.xyz =  normalWS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;

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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				float3 normalWS = IN.normalWS;

				return half4(NormalizeNormalPerPixel(normalWS), 0.0);
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
Node;AmplifyShaderEditor.CommentaryNode;367;-2611.37,1110.061;Inherit;False;3057.666;901.4501;Comment;22;351;352;355;354;356;344;349;348;359;346;353;360;361;362;345;363;364;347;350;365;358;366;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;326;-4471.671,3590.537;Inherit;False;2327.006;1071.292;Comment;26;304;305;296;306;307;298;309;308;310;300;313;312;314;299;295;316;317;311;315;318;319;322;320;324;321;325;Shore Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;294;-3394.883,-167.044;Inherit;False;1935.333;635.9628;Comment;10;330;293;292;329;327;328;290;285;289;288;Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;286;-5120,-368.4201;Inherit;False;1581.42;723.1328;Comment;12;206;203;208;205;279;280;209;210;282;284;283;281;Foam Shadows;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;278;-8480.705,4483.385;Inherit;False;3145.402;1953.361;Comment;45;243;248;245;247;244;246;241;242;220;223;224;233;234;239;238;249;250;251;252;253;254;255;256;257;258;260;261;262;263;221;236;264;267;268;269;270;271;272;273;274;275;276;277;219;323;Intersection Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;266;-7895.723,6777.463;Inherit;False;1260.179;331.0002;Comment;7;226;227;228;229;230;231;265;Intersection Effect Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;218;-9641.523,5255.302;Inherit;False;932;500;Comment;6;225;216;217;215;214;213;Intersection Foam Sampling;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;202;-8732.42,1815.626;Inherit;False;3359.946;2490.543;Comment;44;168;166;167;169;170;163;164;155;156;172;157;160;177;178;179;180;181;182;184;171;185;158;159;190;192;191;189;193;165;194;188;186;161;187;196;197;198;199;175;195;174;173;183;235;Surface Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;153;-8591.403,830.5212;Inherit;False;1964.263;642.6427;Comment;13;138;141;139;142;144;143;145;146;148;149;147;151;150;Wave;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;136;-8170.039,9.692928;Inherit;False;1250.826;573.5786;Comment;7;130;131;128;133;134;129;135;Reflection Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;117;-8917.617,-1253.187;Inherit;False;2551.361;658.648;Comment;18;124;122;103;109;108;105;107;95;101;106;96;102;97;104;100;99;94;98;Shore Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;-8120.803,-499.1333;Inherit;False;1096.19;350.9501;Comment;7;125;112;123;111;113;114;110;Under Water Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;93;-10627.24,1022.428;Inherit;False;1121.124;534.2599;Comment;6;120;88;89;90;91;121;Horizon Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;92;-10908.26,132.858;Inherit;False;1705.685;739.6274;Comment;11;118;85;86;84;76;77;75;82;83;78;119;Deep Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;74;-11444.8,-679.9155;Inherit;False;2261.782;722.1991;Comment;15;73;132;79;72;71;70;69;68;42;41;38;40;39;37;36;Refraction UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;35;-10884.37,-1234.492;Inherit;False;894.9808;453;Comment;4;21;26;33;34;Surface Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;0,0;Float;False;True;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;Water_Ase1;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;23;Surface;1;638771752776900037;  Blend;0;0;Two Sided;1;0;Forward Only;0;0;Cast Shadows;0;638771752793554995;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;0;638771753146174945;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;0;638777921950579337;0;10;False;True;False;True;False;False;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;6;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;7;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;9;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TexturePropertyNode;21;-10834.37,-1184.492;Inherit;True;Property;_NormalTexture;Normal Texture;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;26;-10814.37,-945.4921;Inherit;False;Property;_NormalMovement;Normal Movement;6;0;Create;True;0;0;0;False;0;False;0.04,0.36;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-10213.39,-1066.029;Inherit;False;SurfaceNormals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-11394.8,-292.7167;Inherit;False;34;SurfaceNormals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;37;-11096.8,-290.7167;Inherit;False;Tangent;View;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;39;-11241.8,-85.71655;Inherit;False;Property;_Refraction;Refraction;7;0;Create;True;0;0;0;False;0;False;0.022;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-11026.8,-79.71653;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-10767.8,-191.7165;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-10588.88,-328.4129;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;42;-10852.87,-439.4129;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;68;-10430.21,-629.9155;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;69;-10148.11,-571.4158;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;70;-9953.784,-577.4767;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;71;-9746.885,-382.8769;Inherit;False;3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;72;-10678.54,-255.3453;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;79;-10420.58,-449.011;Inherit;False;ScenePosition;-1;;24;53a3baa0c12e0254da606eeb60f3788d;0;1;1;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-10858.26,521.3671;Inherit;False;Property;_ColorDepth;Color Depth;13;0;Create;True;0;0;0;False;0;False;0.84;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;83;-10603.26,446.3672;Inherit;False;CustomDepthFade;-1;;27;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.SamplerNode;82;-10321.46,182.858;Inherit;True;Property;_TextureSample2;Texture Sample 2;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;75;-10583.5,183.8878;Inherit;True;Property;_GradientTexture;Gradient Texture;8;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode;77;-10291.75,391.8052;Inherit;False;Property;_DeepColor;Deep Color;12;0;Create;True;0;0;0;False;0;False;0.01176471,0.2862745,0.509804,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;76;-10279.54,560.1778;Inherit;False;Property;_ShallowColor;Shallow Color;11;0;Create;True;0;0;0;False;0;False;0,0.772549,0.6235294,0.5019608;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;84;-9960.556,545.8094;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;86;-10262.32,788.4011;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;85;-9670.4,393.6526;Inherit;False;Property;_Gradient;Gradient;9;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;109;-6770.269,-1046.219;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;118;-10865.86,404.5711;Inherit;False;73;RefractUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;119;-9389.037,392.9723;Inherit;False;DeepColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;91;-9903.315,1141.165;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;90;-10328.32,1346.165;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-10571.31,1441.166;Inherit;False;Property;_HorizonDistance;Horizon Distance;14;0;Create;True;0;0;0;False;0;False;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-10282.51,1071.479;Inherit;False;119;DeepColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-9706.247,1135.162;Inherit;False;HorizonColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-6573.15,-1049.285;Inherit;False;ShoreColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;105;-7174.087,-876.556;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;108;-6977.269,-873.2192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-7022.483,-1097.38;Inherit;False;121;HorizonColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-7261.583,-345.0926;Inherit;False;UnderWaterColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-7428.621,-339.9332;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;113;-7670.441,-251.1827;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-7905.809,-449.1331;Inherit;False;73;RefractUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;98;-8867.617,-898.673;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-8858.863,-714.5389;Inherit;False;Property;_ShoreDepth;Shore Depth;15;0;Create;True;0;0;0;False;0;False;0.74;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;99;-8604.766,-864.8871;Inherit;False;CustomDepthFade;-1;;29;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.SimpleAddOpNode;100;-8264.067,-1203.187;Inherit;False;2;2;0;FLOAT;0.1;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-7431.263,-1143.104;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;97;-7796.177,-1026.239;Inherit;False;Property;_ShoreColor;Shore Color;19;0;Create;True;0;0;0;False;0;False;0,0.9803922,0.3019608,0.3411765;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;102;-8177.253,-1055.104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-8359.864,-1059.339;Inherit;False;Property;_ShoreFade;Shore Fade;18;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-8260.647,-864.3311;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;101;-7937.076,-1200.187;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-8244.665,-746.0389;Inherit;False;Property;_ShoreBlend;Shore Blend;17;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;107;-7989.654,-828.3311;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;103;-7671.262,-1201.704;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-8070.907,-256.1867;Inherit;False;121;HorizonColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;112;-7854.615,-257.2116;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;132;-9575.02,-382.3918;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-9393.022,-381.8715;Inherit;False;RefractUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenColorNode;110;-7699.002,-441.1053;Inherit;False;Global;_GrabScreen0;Grab Screen 0;15;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;130;-7954.04,466.0251;Inherit;False;Property;_ReflectionFresnel;Reflection Fresnel;22;0;Create;True;0;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-8120.039,105.0249;Inherit;False;73;RefractUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;128;-7889.042,82.0249;Inherit;True;Global;_PlanarReflectionTexture;_PlanarReflectionTexture;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;133;-7368.438,64.27087;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;134;-7670.439,376.2712;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-7962.04,341.0252;Inherit;False;Property;_ReflectionStrength;Reflection Strength;20;0;Create;True;0;0;0;False;0;False;0.82;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;135;-7147.213,59.69293;Inherit;False;ReflectionColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;138;-8496.496,880.5212;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;141;-8541.403,1084.163;Inherit;False;Property;_WaveVisuals;Wave Visuals;21;0;Create;True;0;0;0;False;0;False;0.132,7.9,1.38;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-8217.405,1003.163;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;142;-7973.405,1113.164;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;143;-8026.405,1261.164;Inherit;False;Property;_WaveDirections;Wave Directions;23;0;Create;True;0;0;0;False;0;False;0,0.64,1,0.33;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;146;-7331.73,933.6332;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;151;-6848.143,929.9235;Inherit;False;position;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;148;-7311.73,1084.633;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;149;-7137.73,1090.633;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;168;-8010.28,2758.126;Inherit;False;150;Height;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-7978.28,2887.126;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;167;-7733.28,2779.126;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;169;-7568.28,2870.126;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;170;-7391.28,2753.126;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;163;-8384.146,2781.309;Inherit;False;Property;_HeightMask;Height Mask;31;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;164;-8351.145,2897.309;Inherit;False;Property;_HeightMaskSmoothness;Height Mask Smoothness;32;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;155;-8236.362,3087.563;Inherit;False;Property;_SurfaceFoamMovement;Surface Foam Movement;24;0;Create;True;0;0;0;False;0;False;0.7,0,0.47,0.01;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;156;-8247.363,3303.563;Inherit;False;Property;_SurfaceFoamTillingandOffset;Surface Foam Tilling and Offset;25;0;Create;True;0;0;0;False;0;False;-1,-1,0.57,0.22;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;172;-8161.063,2992.818;Inherit;False;WaterUV;1;;31;a9e8fcbf9e71e2e4993054503fdbab40;0;0;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;157;-7815.668,3221.781;Inherit;False;Property;_SurfaceFoamSampling;Surface Foam Sampling;26;0;Create;True;0;0;0;False;0;False;0.62,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;-7047.082,2988.404;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;185;-6811.835,2988.677;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ColorNode;158;-6885.706,2484.741;Inherit;False;Property;_SurfaceFoamColor1;Surface Foam Color 1;27;0;Create;True;0;0;0;False;0;False;1,1,1,0.3215686;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;159;-6878.706,2698.74;Inherit;False;Property;_SurfaceFoamColor2;Surface Foam Color 2;28;0;Create;True;0;0;0;False;0;False;1,1,1,0.7294118;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;165;-6650.854,1974.02;Inherit;False;Property;_ShadowStrength;Shadow Strength;33;0;Create;True;0;0;0;False;0;False;0.45;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-5951.66,2400.912;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;197;-5794.677,2328.606;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;175;-7803.566,3393.712;Inherit;True;Property;_SurfaceFoamTexture;Surface Foam Texture;39;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CustomExpressionNode;195;-6459.322,2761.641;Inherit;False; ;7;File;8;True;FoamColor1;FLOAT4;0,0,0,0;In;;Inherit;False;True;FoamColor2;FLOAT4;0,0,0,0;In;;Inherit;False;True;Foam1;FLOAT;0;In;;Inherit;False;True;Foam2;FLOAT;0;In;;Inherit;False;True;Shadow1;FLOAT;0;In;;Inherit;False;True;Shadow2;FLOAT;0;In;;Inherit;False;True;Shadow;FLOAT;0;Out;;Inherit;False;True;Foam;FLOAT4;0,0,0,0;Out;;Inherit;False;FoamColor_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;9;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT4;0,0,0,0;False;3;FLOAT;0;FLOAT;8;FLOAT4;9
Node;AmplifyShaderEditor.CustomExpressionNode;173;-7867.872,3048.313;Inherit;False; ;7;File;4;True;UV;FLOAT2;0,0;In;;Inherit;False;True;Movement;FLOAT4;0,0,0,0;In;;Inherit;False;True;TillingandOffset;FLOAT4;0,0,0,0;In;;Inherit;False;True;Out;FLOAT4;0,0,0,0;Out;;Inherit;False;SurfaceFoamUV_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;5;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;2;FLOAT;0;FLOAT4;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;235;-6404.67,1932.079;Inherit;False;ShadowStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;174;-7413.078,3224.238;Inherit;False; ;7;File;5;True;UVs;FLOAT4;0,0,0,0;In;;Inherit;False;True;Sampling;FLOAT2;0,0;In;;Inherit;False;True;Texture;SAMPLER2D;;In;;Inherit;False;True;ShadowOffset;FLOAT2;0,0;In;;Inherit;False;True;Out;FLOAT4;0,0,0,0;Out;;Inherit;False;FoamSample;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;6;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT2;0,0;False;3;SAMPLER2D;;False;4;FLOAT2;0,0;False;5;FLOAT4;0,0,0,0;False;2;FLOAT;0;FLOAT4;6
Node;AmplifyShaderEditor.RangedFloatNode;160;-8682.42,3754.773;Inherit;False;Property;_SurfaceFoamShadowProjection;Surface Foam Shadow Projection;29;0;Create;True;0;0;0;False;0;False;15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;177;-8232.241,3676.521;Inherit;False;CustomDepthFade;-1;;32;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.ScreenPosInputsNode;178;-8567.948,3543.17;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;180;-8221.94,3870.17;Inherit;False;World;Tangent;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;181;-7850.938,3918.17;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;182;-8514.948,4118.169;Inherit;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-7642.938,3855.17;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;183;-8254.942,4098.169;Inherit;False; ;7;File;2;True;tangentViewDir;FLOAT3;0,0,0;In;;Inherit;False;True;Out;FLOAT3;0,0,0;Out;;Inherit;False;ViewDirectionParallax_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;2;FLOAT;0;FLOAT3;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;179;-8514.948,3876.17;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;190;-7088.048,1865.626;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;192;-7055.048,2038.627;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;191;-6789.051,1934.627;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;188;-6805.618,2248.827;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;186;-7074.18,2248.423;Inherit;False;CustomDepthFade;-1;;34;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.RangedFloatNode;161;-7372.818,2402.649;Inherit;False;Property;_SurfaceFoamShadowDepth;Surface Foam Shadow Depth;30;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;187;-7321.577,2192.065;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;189;-6604.051,2106.626;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;-6425.051,2051.627;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;194;-6244.872,2051.936;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;226;-7794.723,6827.463;Inherit;False;225;IntersectionDepth_Linear;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;227;-7547.722,6833.463;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;228;-7327.722,6880.463;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;229;-7845.724,6992.463;Inherit;False;Property;_IntersectionWaterBlend;Intersection Water Blend;46;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;230;-7555.722,6997.463;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;231;-7115.724,6880.463;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;199;-6093.394,2806.193;Inherit;False;SurfaceFoam;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;198;-5596.474,2324.173;Inherit;False;SurfaceFoamShadow;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;213;-9556.213,5418.641;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;214;-9275.522,5519.302;Inherit;False;CustomDepthFade;-1;;36;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.RangedFloatNode;215;-9591.523,5642.302;Inherit;False;Property;_IntersectionDepth;Intersection Depth;35;0;Create;True;0;0;0;False;0;False;0.72;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;217;-9282.522,5305.302;Inherit;False;Property;_IntersectionFoamSampling;Intersection Foam Sampling;40;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;216;-8934.523,5420.302;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TransformDirectionNode;243;-8137.701,6000.748;Inherit;False;World;Tangent;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;248;-8430.705,6006.748;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;245;-8383.218,6248.747;Inherit;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CustomExpressionNode;247;-8165.426,6228.747;Inherit;False; ;7;File;2;True;tangentViewDir;FLOAT3;0,0,0;In;;Inherit;False;True;Out;FLOAT3;0,0,0;Out;;Inherit;False;ViewDirectionParallax_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;2;FLOAT;0;FLOAT3;3
Node;AmplifyShaderEditor.SimpleAddOpNode;244;-7828.255,6112.063;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;246;-7609.702,5961.126;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;241;-7966.85,5773.683;Inherit;False;CustomDepthFade;-1;;41;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.ScreenPosInputsNode;242;-8226.93,5666.713;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;220;-8328.455,5862.931;Inherit;False;Property;_IntersectionFoamShadowProjection;Intersection Foam Shadow Projection;42;0;Create;True;0;0;0;False;0;False;4.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;223;-8092.766,5195.349;Inherit;False;Property;_IntersectionFoamMovement;Intersection Foam Movement;44;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;224;-8064.768,5338.048;Inherit;False;Property;_IntersectionFoamScale;Intersection Foam Scale;45;0;Create;True;0;0;0;False;0;False;0.19;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;233;-7773.714,5152.649;Inherit;False; ;7;File;5;True;UV;FLOAT2;0,0;In;;Inherit;False;True;Movement;FLOAT2;0,0;In;;Inherit;False;True;Scale;FLOAT;0;In;;Inherit;False;True;Directional;FLOAT4;0,0,0,0;Out;;Inherit;False;True;ByDepth;FLOAT4;0,0,0,0;Out;;Inherit;False;IntersectionFoamUV_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;6;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT;0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;3;FLOAT;0;FLOAT4;5;FLOAT4;6
Node;AmplifyShaderEditor.FunctionNode;234;-7978.747,5090.644;Inherit;False;WaterUV;1;;43;a9e8fcbf9e71e2e4993054503fdbab40;0;0;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;239;-7790.931,5436.078;Inherit;True;Property;_IntersectionFoamTexture;Intersection Foam Texture;47;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CustomExpressionNode;238;-7407.597,5365.866;Inherit;False; ;7;File;5;True;UVs;FLOAT4;0,0,0,0;In;;Inherit;False;True;Sampling;FLOAT2;0,0;In;;Inherit;False;True;Texture;SAMPLER2D;;In;;Inherit;False;True;ShadowOffset;FLOAT2;0,0;In;;Inherit;False;True;Out;FLOAT4;0,0,0,0;Out;;Inherit;False;FoamSample;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;6;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT2;0,0;False;3;SAMPLER2D;;False;4;FLOAT2;0,0;False;5;FLOAT4;0,0,0,0;False;2;FLOAT;0;FLOAT4;6
Node;AmplifyShaderEditor.BreakToComponentsNode;249;-7118.497,5391.723;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;250;-6818.497,5193.724;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;251;-6658.497,5193.724;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-6464.359,5192.755;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;253;-6245.186,5169.722;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;254;-7320.771,4533.385;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;255;-7287.771,4706.385;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;256;-7021.774,4602.386;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;257;-7038.341,4916.586;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;258;-7306.904,4916.183;Inherit;False;CustomDepthFade;-1;;44;cbca4ad544188e0499701d45225c5bb5;0;2;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;3;FLOAT;0;FLOAT;27
Node;AmplifyShaderEditor.ScreenPosInputsNode;260;-7554.301,4859.824;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;-6836.774,4774.385;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;262;-6657.775,4719.386;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;263;-6477.595,4719.695;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;221;-7576.882,5049.962;Inherit;False;Property;_FoamShadowDepth;Foam Shadow Depth;43;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;236;-6884.671,4577.209;Inherit;False;235;ShadowStrength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;264;-6021.521,5170.439;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;267;-6342.307,5309.462;Inherit;False;265;IntersectionEffectMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;268;-5814.307,5098.462;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;270;-6805.784,5613.307;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;271;-6648.784,5613.307;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;272;-6428.785,5563.306;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;273;-6242.784,5559.306;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;274;-5892.784,5567.306;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;275;-6242.784,5681.307;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;276;-6056.784,5668.307;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;219;-6796.972,5358.124;Inherit;False;Property;_IntersectionFoamColor;Intersection Foam Color;41;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;269;-5616.307,5094.462;Inherit;False;IntersectionFoamShadow;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;277;-5665.784,5561.306;Inherit;False;IntersectionFoam;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-6904.543,6873.61;Inherit;False;IntersectionEffectMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;288;-3288.102,19.2449;Inherit;False;199;SurfaceFoam;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;289;-3344.883,128.9389;Inherit;False;Property;_SurfaceFoamBlend;Surface Foam Blend;48;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;285;-3336.783,-117.044;Inherit;False;Property;_FoamShadows;Foam Shadows;36;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;290;-2674.883,-113.0611;Inherit;False;Property;_FoamShadows1;Foam Shadows;34;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Reference;205;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-4799.309,-666.7264;Inherit;False;124;ShoreColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-4829.596,-571.7206;Inherit;False;125;UnderWaterColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-4577.797,-603.0559;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;207;-4366.092,-672.0387;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;206;-5060.486,-318.42;Inherit;False;Constant;_Vector1;Vector 1;32;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;208;-4148.67,-241.1168;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;205;-4782.487,-220.42;Inherit;False;Property;_SurfaceFoam;Surface Foam;34;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;209;-4541.669,-92.11687;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;210;-4367.67,-88.11687;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;282;-3720.581,72.7121;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;284;-3922.582,242.7129;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;283;-4092.582,238.7129;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;281;-4416.581,89.71207;Inherit;False;Property;_IntersectionEffects;Intersection Effects;38;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;-5070,113.5886;Inherit;False;269;IntersectionFoamShadow;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;225;-8980.574,5605.035;Inherit;False;IntersectionDepth_Linear;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;323;-6493.071,5359.252;Inherit;False;IntersectionFoamColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;304;-4421.671,3792.481;Inherit;False;225;IntersectionDepth_Linear;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;305;-4115.958,3798.034;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;296;-4198.326,3898.829;Inherit;False;Property;_ShoreFoamFrequency;Shore Foam Frequency;51;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;306;-3917.96,3843.034;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;307;-3727.216,3945.074;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;298;-4167.327,4032.829;Inherit;False;Property;_ShoreFoamSpeed;Shore Foam Speed;52;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;309;-4156.871,4133.783;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;308;-3909.873,4068.784;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;310;-3555.873,3944.784;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;300;-4153.171,4459.179;Inherit;False;Property;_ShoreFoamBreakupScale;Shore Foam Breakup Scale;54;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;313;-4126.97,4315.397;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;312;-3803.972,4365.397;Inherit;False;Gradient;True;True;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;314;-3561.972,4455.397;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;299;-3891.83,4545.834;Inherit;False;Property;_ShoreFoamBreakupStrength;Shore Foam Breakup Strength;53;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;295;-4129.581,3640.537;Inherit;False;Property;_ShoreFoamWidth;Shore Foam Width;50;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;316;-3829.583,3698.798;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;317;-3598.114,3698.896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;311;-3367.574,4190.386;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;315;-3220.981,3890.399;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;318;-3252.114,3698.896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;319;-3013.114,3766.896;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;322;-3122.907,4006.181;Inherit;False;265;IntersectionEffectMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;320;-2803.114,3854.896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;324;-2887.527,4074.963;Inherit;False;323;IntersectionFoamColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;321;-2558.115,3935.896;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;325;-2368.671,3929.617;Inherit;False;ShoreFoam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;280;-4741.579,113.712;Inherit;False;Property;_ShoreMovement;Shore Movement;37;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;328;-2674.052,184.9206;Inherit;False;Property;_ShoreMovement1;Shore Movement;37;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;280;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;327;-2903.502,253.1174;Inherit;False;325;ShoreFoam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;329;-2929.052,134.9206;Inherit;False;277;IntersectionFoam;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;292;-2665.884,357.9388;Inherit;False;Property;_IntersectionFoamBlend;Intersection Foam Blend;49;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;293;-2040.884,-108.0611;Inherit;False;Property;_FoamShadows2;Foam Shadows;38;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Reference;281;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;330;-1708.62,-107.1297;Inherit;False;Foam;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-5052.536,-119.786;Inherit;False;198;SurfaceFoamShadow;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;-4661.658,-770.9667;Inherit;False;135;ReflectionColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-6902.143,1084.923;Inherit;False;Height;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;147;-7103.73,929.6333;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;144;-8018.405,932.1637;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;342;-2973.382,2.309335;Inherit;False;Overlay;-1;;49;41174f205ba525240b7c587772ce6c54;0;3;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;343;-2309.884,165.9387;Inherit;False;Overlay;-1;;50;41174f205ba525240b7c587772ce6c54;0;3;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;351;-1697.165,1266.112;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;352;-1711.165,1489.112;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;355;-1114.88,1341.71;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;354;-729.165,1358.112;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;356;-502.8796,1313.71;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;344;-778.2468,1160.061;Inherit;False;Property;_SpecularColor;Specular Color;55;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;349;-2561.37,1391.013;Inherit;False;34;SurfaceNormals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;348;-2545.075,1495.251;Inherit;False;Property;_NormalStrength;Normal Strength;59;0;Create;True;0;0;0;False;0;False;0.29;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;359;-2004.518,1426.394;Inherit;False;Tangent;World;True;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;346;-1802.744,1655.5;Inherit;False;Property;_Smoothness;Smoothness;57;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;353;-972.165,1382.112;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;360;-933.7266,1666.096;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;361;-727.0549,1663.482;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;362;-476.0549,1669.482;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;345;-760.7654,1799.511;Inherit;False;Property;_DiffuseColor;Diffuse Color;56;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;363;-210.0549,1552.482;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;364;26.94507,1791.482;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;347;-991.7646,1510.311;Inherit;False;Property;_Hardness;Hardness;58;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;350;-1396.165,1385.112;Inherit;False; ;7;File;6;True;Position;FLOAT3;0,0,0;In;;Inherit;False;True;Normal;FLOAT3;0,0,0;In;;Inherit;False;True;View;FLOAT3;0,0,0;In;;Inherit;False;True;Smoothness;FLOAT;0;In;;Inherit;False;True;Specular;FLOAT;0;Out;;Inherit;False;True;Diffuse;FLOAT;0;Out;;Inherit;False;MainLighting_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;7;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;3;FLOAT;0;FLOAT;6;FLOAT;7
Node;AmplifyShaderEditor.CustomExpressionNode;365;-1380.738,1778.861;Inherit;False; ;7;File;7;True;Position;FLOAT3;0,0,0;In;;Inherit;False;True;Normal;FLOAT3;0,0,0;In;;Inherit;False;True;View;FLOAT3;0,0,0;In;;Inherit;False;True;Smoothness;FLOAT;0;In;;Inherit;False;True;Hardness;FLOAT;0;In;;Inherit;False;True;Specular;FLOAT3;0,0,0;Out;;Inherit;False;True;Diffuse;FLOAT3;0,0,0;Out;;Inherit;False;AdditionalLighting_half;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;8;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;3;FLOAT;0;FLOAT3;7;FLOAT3;8
Node;AmplifyShaderEditor.CustomExpressionNode;358;-2298.55,1428.399;Inherit;False;float3 Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)))@$return Out@;3;Create;2;True;In;FLOAT3;0,0,0;In;;Inherit;False;True;Strength;FLOAT;0;In;;Inherit;False;Normal Strength;True;False;0;;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;366;222.2957,1786.508;Inherit;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;368;-1376.359,-95.06995;Inherit;False;330;Foam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;369;-1063.359,-177.0699;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;370;-1379.359,-245.0699;Inherit;False;366;Lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;371;-837.3586,-130.0699;Inherit;False;Property;_Lighting;Lighting;60;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-300.6471,-124.3721;Inherit;False;330;Foam;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-282.4327,97.51849;Inherit;False;151;position;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;33;-10530.37,-1060.492;Inherit;False;Blended Normals;3;;51;0a23339061bb7494d94a079a17678a51;0;2;1;SAMPLER2D;0;False;2;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;88;-10309.71,1161.791;Inherit;False;Property;_HorizonColor;Horizon Color;10;0;Create;True;0;0;0;False;0;False;0,0.227451,0.8,1;0,0.227451,0.8,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;145;-7632.409,1066.163;Inherit;False; ;7;File;5;True;Position;FLOAT3;0,0,0;In;;Inherit;False;True;Visuals;FLOAT3;0,0,0;In;;Inherit;False;True;Directions;FLOAT4;0,0,0,0;In;;Inherit;False;True;Offset;FLOAT3;0,0,0;Out;;Inherit;False;True;Normal;FLOAT3;0,0,0;Out;;Inherit;False;GerstnerWaves_float;False;False;0;1a65bcea829b38d45a44f98ef7e8ff25;False;6;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;3;FLOAT;0;FLOAT3;5;FLOAT3;6
WireConnection;1;2;371;0
WireConnection;1;5;152;0
WireConnection;34;0;33;0
WireConnection;37;0;36;0
WireConnection;40;0;39;0
WireConnection;38;0;37;0
WireConnection;38;1;40;0
WireConnection;41;0;42;0
WireConnection;41;1;38;0
WireConnection;69;0;68;0
WireConnection;69;1;79;0
WireConnection;70;0;69;0
WireConnection;71;0;70;0
WireConnection;71;2;41;0
WireConnection;71;3;72;0
WireConnection;72;0;42;0
WireConnection;79;1;41;0
WireConnection;83;1;118;0
WireConnection;83;2;78;0
WireConnection;82;0;75;0
WireConnection;82;1;83;3
WireConnection;84;0;77;0
WireConnection;84;1;76;0
WireConnection;84;2;86;0
WireConnection;86;0;83;0
WireConnection;85;0;84;0
WireConnection;85;1;82;0
WireConnection;109;0;122;0
WireConnection;109;1;97;0
WireConnection;109;2;108;0
WireConnection;119;0;85;0
WireConnection;91;0;120;0
WireConnection;91;1;88;0
WireConnection;91;2;90;0
WireConnection;90;3;89;0
WireConnection;121;0;91;0
WireConnection;124;0;109;0
WireConnection;105;0;104;0
WireConnection;105;2;107;0
WireConnection;108;0;105;0
WireConnection;125;0;114;0
WireConnection;114;0;110;0
WireConnection;114;1;113;0
WireConnection;113;0;112;0
WireConnection;99;1;98;0
WireConnection;99;2;94;0
WireConnection;100;0;99;3
WireConnection;104;0;103;0
WireConnection;104;1;97;4
WireConnection;102;0;96;0
WireConnection;106;0;99;3
WireConnection;101;0;100;0
WireConnection;101;1;102;0
WireConnection;107;0;106;0
WireConnection;107;1;95;0
WireConnection;103;0;101;0
WireConnection;112;0;123;0
WireConnection;132;0;71;0
WireConnection;73;0;132;0
WireConnection;110;0;111;0
WireConnection;128;1;131;0
WireConnection;133;1;128;0
WireConnection;133;2;134;0
WireConnection;134;2;129;0
WireConnection;134;3;130;0
WireConnection;135;0;133;0
WireConnection;139;0;138;2
WireConnection;139;1;141;1
WireConnection;142;0;139;0
WireConnection;142;1;141;2
WireConnection;142;2;141;3
WireConnection;146;0;144;0
WireConnection;146;1;145;5
WireConnection;151;0;147;0
WireConnection;148;0;145;5
WireConnection;149;0;148;0
WireConnection;166;0;163;0
WireConnection;166;1;164;0
WireConnection;167;0;168;0
WireConnection;167;1;163;0
WireConnection;167;2;166;0
WireConnection;169;1;163;0
WireConnection;170;1;167;0
WireConnection;170;2;169;0
WireConnection;171;0;170;0
WireConnection;171;1;174;6
WireConnection;185;0;171;0
WireConnection;196;0;194;0
WireConnection;196;1;195;8
WireConnection;197;3;196;0
WireConnection;195;1;158;0
WireConnection;195;2;159;0
WireConnection;195;3;185;0
WireConnection;195;4;185;1
WireConnection;195;5;185;2
WireConnection;195;6;185;3
WireConnection;173;1;172;0
WireConnection;173;2;155;0
WireConnection;173;3;156;0
WireConnection;235;0;165;0
WireConnection;174;1;173;5
WireConnection;174;2;157;0
WireConnection;174;3;175;0
WireConnection;174;4;184;0
WireConnection;177;1;178;0
WireConnection;177;2;160;0
WireConnection;180;0;179;0
WireConnection;181;0;180;0
WireConnection;181;1;183;3
WireConnection;184;0;177;3
WireConnection;184;1;181;0
WireConnection;183;1;182;0
WireConnection;191;0;190;0
WireConnection;191;1;192;0
WireConnection;188;0;186;3
WireConnection;186;1;187;0
WireConnection;186;2;161;0
WireConnection;189;0;191;0
WireConnection;189;1;188;0
WireConnection;193;0;165;0
WireConnection;193;1;189;0
WireConnection;194;0;193;0
WireConnection;227;0;226;0
WireConnection;228;0;227;0
WireConnection;228;1;230;0
WireConnection;230;0;229;0
WireConnection;231;0;228;0
WireConnection;199;0;195;9
WireConnection;198;0;197;0
WireConnection;214;1;213;0
WireConnection;214;2;215;0
WireConnection;216;0;217;0
WireConnection;216;1;214;3
WireConnection;243;0;248;0
WireConnection;247;1;245;0
WireConnection;244;0;243;0
WireConnection;244;1;247;3
WireConnection;246;0;241;3
WireConnection;246;1;244;0
WireConnection;241;1;242;0
WireConnection;241;2;220;0
WireConnection;233;1;234;0
WireConnection;233;2;223;0
WireConnection;233;3;224;0
WireConnection;238;1;233;5
WireConnection;238;2;216;0
WireConnection;238;3;239;0
WireConnection;238;4;246;0
WireConnection;249;0;238;6
WireConnection;250;0;249;0
WireConnection;250;1;249;1
WireConnection;251;0;250;0
WireConnection;252;0;251;0
WireConnection;252;1;219;4
WireConnection;253;0;263;0
WireConnection;253;1;252;0
WireConnection;256;0;254;0
WireConnection;256;1;255;0
WireConnection;257;0;258;3
WireConnection;258;1;260;0
WireConnection;258;2;221;0
WireConnection;261;0;256;0
WireConnection;261;1;257;0
WireConnection;262;0;236;0
WireConnection;262;1;261;0
WireConnection;263;0;262;0
WireConnection;264;0;253;0
WireConnection;264;1;267;0
WireConnection;268;3;264;0
WireConnection;270;0;249;2
WireConnection;270;1;249;3
WireConnection;271;0;270;0
WireConnection;272;0;219;0
WireConnection;272;1;271;0
WireConnection;273;0;272;0
WireConnection;274;0;273;0
WireConnection;274;3;276;0
WireConnection;275;0;272;0
WireConnection;276;0;267;0
WireConnection;276;1;275;0
WireConnection;269;0;268;0
WireConnection;277;0;274;0
WireConnection;265;0;231;0
WireConnection;285;1;207;0
WireConnection;285;0;282;0
WireConnection;290;1;285;0
WireConnection;290;0;342;0
WireConnection;115;0;126;0
WireConnection;115;1;127;0
WireConnection;207;0;137;0
WireConnection;207;1;115;0
WireConnection;208;0;207;0
WireConnection;208;1;205;0
WireConnection;208;2;210;0
WireConnection;205;1;206;0
WireConnection;205;0;203;0
WireConnection;209;0;205;0
WireConnection;210;0;209;0
WireConnection;282;0;208;0
WireConnection;282;1;281;0
WireConnection;282;2;284;0
WireConnection;284;0;283;0
WireConnection;283;0;281;0
WireConnection;281;1;206;0
WireConnection;281;0;280;0
WireConnection;225;0;214;3
WireConnection;323;0;219;0
WireConnection;305;0;304;0
WireConnection;306;0;305;0
WireConnection;306;1;296;0
WireConnection;307;0;306;0
WireConnection;307;1;308;0
WireConnection;308;0;298;0
WireConnection;308;1;309;2
WireConnection;310;0;307;0
WireConnection;312;0;313;0
WireConnection;312;1;300;0
WireConnection;314;0;312;0
WireConnection;314;1;299;0
WireConnection;316;0;295;0
WireConnection;316;1;305;0
WireConnection;317;0;316;0
WireConnection;311;0;310;0
WireConnection;311;1;314;0
WireConnection;315;0;305;0
WireConnection;315;1;311;0
WireConnection;318;0;317;0
WireConnection;319;0;318;0
WireConnection;319;1;315;0
WireConnection;320;0;319;0
WireConnection;320;1;322;0
WireConnection;321;0;320;0
WireConnection;321;1;324;0
WireConnection;325;0;321;0
WireConnection;280;1;279;0
WireConnection;280;0;206;0
WireConnection;328;1;329;0
WireConnection;328;0;327;0
WireConnection;293;1;290;0
WireConnection;293;0;343;0
WireConnection;330;0;293;0
WireConnection;150;0;149;0
WireConnection;147;0;146;0
WireConnection;342;1;285;0
WireConnection;342;2;288;0
WireConnection;342;3;289;0
WireConnection;343;1;290;0
WireConnection;343;2;328;0
WireConnection;343;3;292;0
WireConnection;355;0;350;6
WireConnection;354;0;355;0
WireConnection;354;1;353;0
WireConnection;354;2;347;0
WireConnection;356;0;344;0
WireConnection;356;1;354;0
WireConnection;359;0;358;0
WireConnection;353;1;350;6
WireConnection;360;1;350;7
WireConnection;361;0;350;7
WireConnection;361;1;360;0
WireConnection;361;2;347;0
WireConnection;362;0;361;0
WireConnection;362;1;345;0
WireConnection;363;0;356;0
WireConnection;363;1;362;0
WireConnection;364;0;363;0
WireConnection;364;1;365;7
WireConnection;350;1;351;0
WireConnection;350;2;359;0
WireConnection;350;3;352;0
WireConnection;350;4;346;0
WireConnection;365;1;351;0
WireConnection;365;2;359;0
WireConnection;365;3;352;0
WireConnection;365;4;346;0
WireConnection;365;5;347;0
WireConnection;358;0;349;0
WireConnection;358;1;348;0
WireConnection;366;0;364;0
WireConnection;369;0;370;0
WireConnection;369;1;368;0
WireConnection;371;1;368;0
WireConnection;371;0;369;0
WireConnection;33;1;21;0
WireConnection;33;2;26;0
WireConnection;145;1;144;0
WireConnection;145;2;142;0
WireConnection;145;3;143;0
ASEEND*/
//CHKSM=8C45BDE44162C924E113F2905325F3BFBE92C168