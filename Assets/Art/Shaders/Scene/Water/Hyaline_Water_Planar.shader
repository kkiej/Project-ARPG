// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hyaline/Water/Planar"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_FlowSpeed("FlowSpeed", Float) = 0.5
		_NormalMap("NormalMap", 2D) = "bump" {}
		_normal_in("法线强度", Range( 0 , 1)) = 0.2
		_normal_in1("扰乱法线强度", Range( 0 , 0.2)) = 0.05
		_NormalMovement("Normal Movement", Vector) = (0,0,0,0)
		_FoamEdge("FoamEdge", Range( 0 , 3)) = 1
		_UVMovement_Reflection("反射图片受法线影响强度", Range( 0 , 2)) = 0.64
		_FoamTilling("Foam Tilling", Range( 0 , 5)) = 2
		_RefractionPower("Refraction Power", Range( 0 , 1)) = 0.1
		_CausticsTilling("Caustics Tilling", Float) = 9.78
		_WaveSpeed("WaveSpeed", Float) = -2
		_CausticsTex("Caustics Tex", 2D) = "white" {}
		_CausticsSpeed("Caustics Speed", Vector) = (-0.1,-0.05,0,0)
		_CausticsIntensity("Caustics Intensity", Range( 0 , 2)) = 1
		_SpecularPower("Specular Power", Float) = 20
		[ASEEnd]_Opacity("Opacity", Range( 1 , 5)) = 1


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

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+1" }

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
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_OPAQUE_TEXTURE 1
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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
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
			float2 _NormalMovement;
			float2 _CausticsSpeed;
			float _RefractionPower;
			float _normal_in;
			float _normal_in1;
			float _Opacity;
			float _FoamEdge;
			float _FoamTilling;
			float _WaveSpeed;
			float _FlowSpeed;
			float _SpecularPower;
			float _CausticsTilling;
			float _CausticsIntensity;
			float _UVMovement_Reflection;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _NormalMap;
			uniform float4 _CameraDepthTexture_TexelSize;
			sampler2D _CausticsTex;
			sampler2D _PlanarReflectionTexture;


			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float2 rand1222( float2 st, int seed )
			{
				float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed);
				return -1 + 2 * frac(sin(s) * 43758.5453123);
			}
			
			float2 rand1204( float2 st, int seed )
			{
				float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed);
				return -1 + 2 * frac(sin(s) * 43758.5453123);
			}
			
			float2 rand1225( float2 st, int seed )
			{
				float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed);
				return -1 + 2 * frac(sin(s) * 43758.5453123);
			}
			
			float2 rand1226( float2 st, int seed )
			{
				float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed);
				return -1 + 2 * frac(sin(s) * 43758.5453123);
			}
			
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
				o.ase_texcoord4.xyz = ase_worldTangent;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord5.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord6.xyz = ase_worldBitangent;
				
				o.ase_texcoord7 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;

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
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( screenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float2 appendResult1083 = (float2(cos( PI ) , sin( PI )));
				float2 normalizeResult1085 = normalize( appendResult1083 );
				float2 appendResult1105 = (float2(WorldPosition.x , WorldPosition.z));
				float2 UV1109 = -( appendResult1105 * 0.1 );
				float3 unpack46 = UnpackNormalScale( tex2D( _NormalMap, ( ( normalizeResult1085 * ( _TimeParameters.x * 0.05 ) * ( _NormalMovement.x * -0.5 ) ) + ( UV1109 * ( 1.0 / ( _NormalMovement.y * 0.5 ) ) ) ) ), _normal_in );
				unpack46.z = lerp( 1, unpack46.z, saturate(_normal_in) );
				float3 unpack93 = UnpackNormalScale( tex2D( _NormalMap, ( ( UV1109 * ( 1.0 / _NormalMovement.y ) ) + ( normalizeResult1085 * ( _TimeParameters.x * 0.05 ) * _NormalMovement.x ) ) ), _normal_in1 );
				unpack93.z = lerp( 1, unpack93.z, saturate(_normal_in1) );
				float3 temp_output_54_0 = BlendNormal( unpack46 , unpack93 );
				float3 SurfaceNormal47 = temp_output_54_0;
				float3 ase_worldTangent = IN.ase_texcoord4.xyz;
				float3 ase_worldNormal = IN.ase_texcoord5.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord6.xyz;
				float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
				float3 tangentToViewDir1117 = normalize( mul( UNITY_MATRIX_V, float4( mul( ase_tangentToWorldFast, SurfaceNormal47 ), 0 ) ).xyz );
				float3 break1277 = ( ( _RefractionPower / 10.0 ) * tangentToViewDir1117 );
				float2 appendResult1280 = (float2(( ase_grabScreenPosNorm.r + break1277.x ) , ( ase_grabScreenPosNorm.g + ( break1277.y * 0.05 ) )));
				float eyeDepth1129 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( appendResult1280, 0.0 , 0.0 ).xy ),_ZBufferParams);
				float3 worldToView1128 = mul( UNITY_MATRIX_V, float4( WorldPosition, 1 ) ).xyz;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth1113 = (SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy )*( _ProjectionParams.z - _ProjectionParams.y ));
				float lerpResult1121 = lerp( ( 1.0 - eyeDepth1113 ) , eyeDepth1113 , step( 0.0 , _ProjectionParams.x ));
				float lerpResult1123 = lerp( _ProjectionParams.y , _ProjectionParams.z , lerpResult1121);
				float3 appendResult1132 = (float3(worldToView1128.x , worldToView1128.y , -lerpResult1123));
				float3 viewToWorld1136 = mul( UNITY_MATRIX_I_V, float4( appendResult1132, 1 ) ).xyz;
				float3 lerpResult1139 = lerp( ( ( ( ( WorldPosition - _WorldSpaceCameraPos ) / screenPos.w ) * eyeDepth1129 ) + _WorldSpaceCameraPos ) , viewToWorld1136 , unity_OrthoParams.w);
				float4 lerpResult1144 = lerp( ase_grabScreenPosNorm , float4( appendResult1280, 0.0 , 0.0 ) , step( 0.0 , (( WorldPosition - lerpResult1139 )).y ));
				float4 fetchOpaqueVal1145 = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( lerpResult1144.xy ), 1.0 );
				float4 Refraction1067 = fetchOpaqueVal1145;
				Gradient gradient980 = NewGradient( 0, 3, 2, float4( 0.1492524, 0.8113208, 0.6798155, 0 ), float4( 0.4510995, 0.5976637, 0.8005602, 0.7911803 ), float4( 0.5199804, 0.6453958, 0.8962264, 1 ), 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float eyeDepth797 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos795 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord7.xyz));
				float4 computeScreenPos796 = ComputeScreenPos( unityObjectToClipPos795 );
				float temp_output_798_0 = (computeScreenPos796).w;
				float diffz804 = saturate( ( ( eyeDepth797 - temp_output_798_0 ) / _Opacity ) );
				float smoothstepResult1285 = smoothstep( -1.5 , 2.8 , diffz804);
				float4 blendOpSrc1069 = Refraction1067;
				float4 blendOpDest1069 = float4( (SampleGradient( gradient980, smoothstepResult1285 )).rgb , 0.0 );
				float4 WaterColor24 = ( saturate( (( blendOpDest1069 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1069 ) * ( 1.0 - blendOpSrc1069 ) ) : ( 2.0 * blendOpDest1069 * blendOpSrc1069 ) ) ));
				float temp_output_1187_0 = ( diffz804 / _FoamEdge );
				float clampResult1171 = clamp( temp_output_1187_0 , 0.0 , 1.0 );
				float temp_output_1193_0 = ( 1.0 - clampResult1171 );
				float2 break1239 = ( (WorldPosition).xz * 0.5 );
				float2 appendResult1242 = (float2(break1239.x , ( break1239.y + ( _TimeParameters.x * _FlowSpeed ) )));
				float2 p1209 = floor( appendResult1242 );
				float2 st1222 = p1209;
				int seed1222 = 0;
				float2 localrand1222 = rand1222( st1222 , seed1222 );
				float2 f1206 = frac( appendResult1242 );
				float dotResult1213 = dot( localrand1222 , f1206 );
				float2 st1204 = ( p1209 + float2( 1,0 ) );
				int seed1204 = 0;
				float2 localrand1204 = rand1204( st1204 , seed1204 );
				float dotResult1238 = dot( localrand1204 , ( f1206 - float2( 1,0 ) ) );
				float2 temp_cast_4 = (3.0).xx;
				float2 break1210 = ( ( f1206 * f1206 ) * ( temp_cast_4 - ( f1206 * 2.0 ) ) );
				float lerpResult1240 = lerp( dotResult1213 , dotResult1238 , break1210.x);
				float2 st1225 = ( p1209 + float2( 0,1 ) );
				int seed1225 = 0;
				float2 localrand1225 = rand1225( st1225 , seed1225 );
				float dotResult1249 = dot( localrand1225 , ( f1206 - float2( 0,1 ) ) );
				float2 st1226 = ( p1209 + float2( 1,1 ) );
				int seed1226 = 0;
				float2 localrand1226 = rand1226( st1226 , seed1226 );
				float dotResult1233 = dot( localrand1226 , ( f1206 - float2( 1,1 ) ) );
				float lerpResult1227 = lerp( dotResult1249 , dotResult1233 , break1210.x);
				float lerpResult1224 = lerp( lerpResult1240 , lerpResult1227 , break1210.y);
				float noise1246 = lerpResult1224;
				float FoamColor1179 = saturate( ( ( ( temp_output_1193_0 + ( sin( ( ( temp_output_1193_0 * _FoamTilling * 10.0 ) + ( _WaveSpeed * _TimeParameters.x ) ) ) + noise1246 ) ) - 1.4 ) * ( 1.0 - step( 1.0 , temp_output_1187_0 ) ) ) );
				float3 worldlightdir867 = _MainLightPosition.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 normalizeResult885 = normalize( ( worldlightdir867 + ase_worldViewDir ) );
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal685 = temp_output_54_0;
				float3 worldNormal685 = normalize( float3(dot(tanToWorld0,tanNormal685), dot(tanToWorld1,tanNormal685), dot(tanToWorld2,tanNormal685)) );
				float3 Normal698 = worldNormal685;
				float dotResult878 = dot( normalizeResult885 , Normal698 );
				float3 normalizeResult1152 = normalize( worldlightdir867 );
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult879 = dot( normalizeResult1152 , normalizedWorldNormal );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult790 = dot( ase_worldViewDir , normalizedWorldNormal );
				float smoothstepResult791 = smoothstep( 0.2 , 1.3 , dotResult790);
				float TransmittAreaMask793 = ( 1.0 - smoothstepResult791 );
				float temp_output_880_0 = max( ( dotResult878 * step( 0.0 , dotResult879 ) * TransmittAreaMask793 ) , 0.001 );
				float Specular872 = pow( temp_output_880_0 , _SpecularPower );
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
				float4 PositionFormDepth3 = (mul( unity_CameraToWorld, appendResult49_g14 )).xyzw;
				float2 temp_output_840_0 = ( (PositionFormDepth3).xz / _CausticsTilling );
				float2 temp_output_830_0 = ( _CausticsSpeed * _TimeParameters.x );
				float smoothstepResult787 = smoothstep( 0.2 , 1.0 , dotResult790);
				float CausticsAreaMask783 = smoothstepResult787;
				float4 Caustics831 = ( ( min( tex2D( _CausticsTex, ( temp_output_840_0 + temp_output_830_0 ) ) , tex2D( _CausticsTex, ( ( -temp_output_840_0 * 1.5 ) + temp_output_830_0 ) ) ) * _CausticsIntensity ) * CausticsAreaMask783 );
				half3 reflectVector815 = reflect( -ase_worldViewDir, Normal698 );
				float3 indirectSpecular815 = GlossyEnvironmentReflection( reflectVector815, 1.0 - 1.0, 1.0 );
				float2 appendResult763 = (float2(ase_screenPosNorm.x , ase_screenPosNorm.y));
				float2 temp_output_765_0 = ( appendResult763 / ase_screenPosNorm.w );
				float smoothstepResult785 = smoothstep( 0.4 , 0.6 , dotResult790);
				float RflectAreaMask786 = ( 1.0 - smoothstepResult785 );
				float4 lerpResult823 = lerp( float4( indirectSpecular815 , 0.0 ) , tex2D( _PlanarReflectionTexture, ( temp_output_765_0 + ( (SurfaceNormal47).xy * _UVMovement_Reflection ) ) ) , RflectAreaMask786);
				float4 ReflectColor67 = lerpResult823;
				float dotResult853 = dot( ase_worldViewDir , Normal698 );
				float vReflect1263 = saturate( ( ( 0.02 + ( ( 1.0 - 0.02 ) * pow( ( 1.0 - dotResult853 ) , 3.5 ) ) ) * 0.65 ) );
				float4 lerpResult860 = lerp( ( WaterColor24 + FoamColor1179 + Specular872 + Caustics831 ) , ReflectColor67 , vReflect1263);
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = saturate( ( lerpResult860 + float4( 0,0,0,0 ) ) ).rgb;
				float Alpha = ( diffz804 * TransmittAreaMask793 );
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
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
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
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float2 _NormalMovement;
			float2 _CausticsSpeed;
			float _RefractionPower;
			float _normal_in;
			float _normal_in1;
			float _Opacity;
			float _FoamEdge;
			float _FoamTilling;
			float _WaveSpeed;
			float _FlowSpeed;
			float _SpecularPower;
			float _CausticsTilling;
			float _CausticsIntensity;
			float _UVMovement_Reflection;
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


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord4.xyz = ase_worldNormal;
				
				o.ase_texcoord3 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;

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
				float eyeDepth797 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos795 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord3.xyz));
				float4 computeScreenPos796 = ComputeScreenPos( unityObjectToClipPos795 );
				float temp_output_798_0 = (computeScreenPos796).w;
				float diffz804 = saturate( ( ( eyeDepth797 - temp_output_798_0 ) / _Opacity ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 ase_worldNormal = IN.ase_texcoord4.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult790 = dot( ase_worldViewDir , normalizedWorldNormal );
				float smoothstepResult791 = smoothstep( 0.2 , 1.3 , dotResult790);
				float TransmittAreaMask793 = ( 1.0 - smoothstepResult791 );
				

				float Alpha = ( diffz804 * TransmittAreaMask793 );
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
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


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

			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float2 _NormalMovement;
			float2 _CausticsSpeed;
			float _RefractionPower;
			float _normal_in;
			float _normal_in1;
			float _Opacity;
			float _FoamEdge;
			float _FoamTilling;
			float _WaveSpeed;
			float _FlowSpeed;
			float _SpecularPower;
			float _CausticsTilling;
			float _CausticsIntensity;
			float _UVMovement_Reflection;
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

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord = screenPos;
				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				o.ase_texcoord2.xyz = ase_worldPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;

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
				o.clipPos = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
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

				float4 screenPos = IN.ase_texcoord;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth797 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos795 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord1.xyz));
				float4 computeScreenPos796 = ComputeScreenPos( unityObjectToClipPos795 );
				float temp_output_798_0 = (computeScreenPos796).w;
				float diffz804 = saturate( ( ( eyeDepth797 - temp_output_798_0 ) / _Opacity ) );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult790 = dot( ase_worldViewDir , normalizedWorldNormal );
				float smoothstepResult791 = smoothstep( 0.2 , 1.3 , dotResult790);
				float TransmittAreaMask793 = ( 1.0 - smoothstepResult791 );
				

				surfaceDescription.Alpha = ( diffz804 * TransmittAreaMask793 );
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
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


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

			#define ASE_NEEDS_VERT_NORMAL


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float2 _NormalMovement;
			float2 _CausticsSpeed;
			float _RefractionPower;
			float _normal_in;
			float _normal_in1;
			float _Opacity;
			float _FoamEdge;
			float _FoamTilling;
			float _WaveSpeed;
			float _FlowSpeed;
			float _SpecularPower;
			float _CausticsTilling;
			float _CausticsIntensity;
			float _UVMovement_Reflection;
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

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord = screenPos;
				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				o.ase_texcoord2.xyz = ase_worldPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
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
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
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

				float4 screenPos = IN.ase_texcoord;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth797 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos795 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord1.xyz));
				float4 computeScreenPos796 = ComputeScreenPos( unityObjectToClipPos795 );
				float temp_output_798_0 = (computeScreenPos796).w;
				float diffz804 = saturate( ( ( eyeDepth797 - temp_output_798_0 ) / _Opacity ) );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 ase_worldNormal = IN.ase_texcoord3.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult790 = dot( ase_worldViewDir , normalizedWorldNormal );
				float smoothstepResult791 = smoothstep( 0.2 , 1.3 , dotResult790);
				float TransmittAreaMask793 = ( 1.0 - smoothstepResult791 );
				

				surfaceDescription.Alpha = ( diffz804 * TransmittAreaMask793 );
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
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120107
			#define REQUIRE_DEPTH_TEXTURE 1


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

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float2 _NormalMovement;
			float2 _CausticsSpeed;
			float _RefractionPower;
			float _normal_in;
			float _normal_in1;
			float _Opacity;
			float _FoamEdge;
			float _FoamTilling;
			float _WaveSpeed;
			float _FlowSpeed;
			float _SpecularPower;
			float _CausticsTilling;
			float _CausticsIntensity;
			float _UVMovement_Reflection;
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

				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				float3 ase_worldPos = TransformObjectToWorld( (v.vertex).xyz );
				o.ase_texcoord3.xyz = ase_worldPos;
				
				o.ase_texcoord2 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
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

				float4 screenPos = IN.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth797 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float4 unityObjectToClipPos795 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord2.xyz));
				float4 computeScreenPos796 = ComputeScreenPos( unityObjectToClipPos795 );
				float temp_output_798_0 = (computeScreenPos796).w;
				float diffz804 = saturate( ( ( eyeDepth797 - temp_output_798_0 ) / _Opacity ) );
				float3 ase_worldPos = IN.ase_texcoord3.xyz;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float3 normalizedWorldNormal = normalize( IN.normalWS );
				float dotResult790 = dot( ase_worldViewDir , normalizedWorldNormal );
				float smoothstepResult791 = smoothstep( 0.2 , 1.3 , dotResult790);
				float TransmittAreaMask793 = ( 1.0 - smoothstepResult791 );
				

				surfaceDescription.Alpha = ( diffz804 * TransmittAreaMask793 );
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
	
	Fallback "Hidden/InternalErrorShader"
}
/*ASEBEGIN
Version=19100
Node;AmplifyShaderEditor.CommentaryNode;807;-2668.751,-307.455;Inherit;False;2386.521;1062.614;Comment;27;24;1069;1068;983;982;980;970;979;981;978;972;775;977;803;802;804;801;800;799;798;797;796;795;794;1250;1281;1285;水体颜色和深度差;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;794;-2486.604,-75.43849;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;795;-2263.604,-74.43849;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComputeScreenPosHlpNode;796;-2036.719,-75.89796;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;808;-4763.321,807.8643;Inherit;False;1192.157;699.6003;;11;788;789;790;791;792;793;786;783;784;787;785;遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;798;-1738.719,-80.89797;Inherit;False;FLOAT;3;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;797;-1776.719,-250.8975;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;789;-4713.321,1217.772;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;788;-4703.321,984.7714;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;799;-1477.719,-243.8975;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1250;-1537.5,-150.0325;Inherit;False;Property;_Opacity;Opacity;35;0;Create;True;0;0;0;False;0;False;1;0;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;790;-4441.801,1108.504;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;800;-1253.718,-243.8975;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;792;-4024.417,1109.786;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;801;-1063.718,-243.8975;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;188;-10163.58,8190.232;Inherit;False;2559.208;1579.784;Comment;28;190;189;167;182;184;179;186;185;180;183;172;173;160;157;175;162;174;177;158;161;156;159;155;154;152;151;153;625;海浪;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;200;-10177.9,13831.34;Inherit;False;2342.488;670.0675;Comment;10;214;213;212;211;210;202;216;218;215;217;Wave UV's and Height;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;793;-3821.165,1104.341;Float;False;TransmittAreaMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;804;-871.4953,-249.5542;Inherit;False;diffz;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;139;-10042.84,7356.551;Inherit;False;1824.58;546.3169;Comment;13;148;146;147;144;145;143;132;131;130;129;128;126;127;海岸浪花区域;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;202;-10177.9,13879.34;Inherit;False;1135.412;591.1866;Comment;4;209;208;206;204;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;326;-10209.9,10407.33;Inherit;False;1350.609;402.1709;Comment;8;317;395;366;564;396;295;294;296;泡沫区域遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1201;-5802.164,4518.195;Inherit;False;1969.746;2006.997;;48;1249;1248;1247;1246;1245;1244;1243;1242;1241;1240;1239;1238;1237;1236;1235;1234;1233;1232;1231;1230;1229;1228;1227;1226;1225;1224;1223;1222;1221;1220;1219;1218;1217;1216;1215;1214;1213;1212;1211;1210;1209;1208;1207;1206;1205;1204;1203;1202;Noise;0.3679245,0.3928559,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;998;-5696.05,3471.532;Inherit;False;1810.484;986.6731;Comment;21;882;890;892;884;878;881;880;871;877;869;873;874;894;879;893;883;895;885;875;872;1152;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;846;-2877.058,2696.211;Inherit;False;2681.324;691.5361;Comment;20;826;827;828;829;830;831;832;833;834;835;837;838;839;840;841;842;843;844;845;863;焦散;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;554;-10209.9,11415.34;Inherit;False;1567.448;432.5918;Comment;9;551;550;545;549;548;546;547;553;543;浪花强烈边缘  浪头;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1057;-5009.068,1632.812;Inherit;False;1052.073;348.8972;Comment;5;1052;1053;1054;1055;1056;NdotL;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;198;-10177.9,13479.34;Inherit;False;911.9355;310.1783;Comment;3;203;201;199;World Space Tile;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;533;-8609.898,9879.335;Inherit;False;1467.847;318.3116;Comment;9;526;530;565;531;523;542;527;541;535;岸边颜色明度遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;393;-10177.9,10855.33;Inherit;False;1271.063;499.9648;Comment;8;391;389;390;359;392;342;345;343;海浪岸边动态遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;55;-2840.487,827.8699;Inherit;False;2751.366;856.5919;Comment;26;729;93;47;54;685;698;46;1097;1094;1093;1092;1091;1090;1089;1088;1087;1086;1085;1084;1083;1082;1081;1080;1079;1078;252;扰乱波纹法线;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;183;-9792.623,8240.232;Inherit;False;673.1583;289.1328;Comment;4;166;163;164;165;泡沫区域遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;601;-12113.9,10663.33;Inherit;False;1506.922;767.6074;Comment;13;614;613;612;609;602;604;605;611;606;608;603;607;610;海浪潮汐噪波;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;386;-12129.9,9815.334;Inherit;False;1515.068;774.124;Comment;13;387;328;349;382;329;332;335;355;330;354;383;379;351;海浪潮汐噪波;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;372;-10113.9,9831.334;Inherit;False;1337.872;423.0596;Comment;10;563;368;560;556;557;558;363;365;364;362;泡沫拖尾消失遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;8;-2203.153,-2176.283;Inherit;False;1342.1;355.5708;Comment;7;6;5;4;3;2;9;1077;高度深度;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;567;-10161.9,12407.34;Inherit;False;1271.063;499.9648;Comment;8;590;584;583;581;578;574;572;571;海浪岸边动态遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1058;-3426.214,-1762.5;Inherit;False;3592.024;1360.787;Refraction;41;1129;1279;1280;1278;1126;1277;1120;1117;1067;1125;1111;1110;1145;1140;1127;1112;1118;1123;1128;1115;1139;1116;1143;1141;1114;1135;1130;1144;1119;1137;1132;1138;1136;1122;1131;1113;1134;1133;1121;1124;1142;Refraction;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;568;-10177.9,11959.34;Inherit;False;1350.609;402.1709;Comment;8;600;599;589;586;579;577;573;570;泡沫区域遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;79;-2462.804,1822.559;Inherit;False;1885.521;681.4766;Comment;16;63;765;764;62;57;67;823;56;814;815;61;763;60;976;59;812;相机反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1154;-2980.077,3518.947;Inherit;False;2764.25;675.0417;;23;1197;1196;1195;1193;1192;1190;1189;1187;1184;1182;1179;1177;1176;1173;1172;1171;1169;1165;1164;1163;1160;1159;1156;Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;732;-5372.211,2259.987;Inherit;False;1502.668;297.2681;Comment;12;744;743;742;741;740;739;738;737;736;735;734;733;太阳位置判读时间;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;999;-4925.892,2845.432;Inherit;False;769.2993;340.0461;Comment;2;867;1151;太阳角度;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;569;-10193.9,12967.34;Inherit;False;1567.448;432.5918;Comment;10;594;593;592;588;587;585;582;580;576;575;浪花强烈边缘  浪头;1,1,1,1;0;0
Node;AmplifyShaderEditor.StepOpNode;1142;-755.8702,-937.5574;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1104;-4864.711,218.8894;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1107;-4302.476,292.8321;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;787;-4214.301,1351.465;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1092;-1698.37,1412.822;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;1105;-4554.961,241.8716;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BlendNormalsNode;54;-793.824,1175.312;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-9801.268,7590.894;Inherit;False;Property;_ShoreRange;Shore Range;15;0;Create;True;0;0;0;False;0;False;0.59;0.59;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;733;-4948.5,2325.987;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;1055;-4375.996,1770.63;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;882;-4713.325,4076.302;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1244;-4841.412,4690.585;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1124;-2223.397,-1635.564;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;5;-1426.218,-2141.283;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;877;-5025.3,4343.205;Inherit;False;793;TransmittAreaMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;989;-4643.354,-640.8979;Inherit;False;Constant;_Float31;Float 31;58;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;1080;-2534.405,952.9822;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;763;-2010.083,1962.609;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;252;-1484.758,1131.582;Inherit;False;Property;_normal_in;法线强度;4;0;Create;False;0;0;0;False;0;False;0.2;0.207;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;735;-4824.511,2327.987;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;1108;-4095.158,296.3067;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;740;-4394.64,2369.255;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1086;-2143.536,1544.607;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;1222;-5257.071,4928.422;Inherit;False;float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed)@$return -1 + 2 * frac(sin(s) * 43758.5453123)@;2;Create;2;True;st;FLOAT2;0,0;In;;Inherit;False;True;seed;INT;0;In;;Inherit;False;rand;True;False;0;;False;2;0;FLOAT2;0,0;False;1;INT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalizeNode;1152;-5425.338,4032.51;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;890;-5639.812,4027.79;Inherit;False;867;worldlightdir;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector2Node;843;-2365.927,3041.311;Inherit;False;Property;_CausticsSpeed;Caustics Speed;17;0;Create;True;0;0;0;False;0;False;-0.1,-0.05;-0.2,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;982;-1406.401,346.0833;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1121;-2591.144,-803.4703;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;1133;-1925.783,-1137.885;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;1228;-5752.164,5496.398;Inherit;False;1206;f;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;996;-4506.666,-327.6633;Inherit;False;990;RainON;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;878;-4933.197,3716.661;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1240;-4671.68,5229.236;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;972;-1836.941,140.9765;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FractNode;1202;-4387.996,4689.666;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;979;-2205.355,392.3553;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;978;-2379.355,387.3554;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;1239;-5028.441,4606.556;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ScreenPosInputsNode;57;-2378.173,1935.76;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;840;-2253.294,2779.41;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;131;-9282.134,7492.13;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;127;-9769.269,7453.894;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1232;-5432.009,5198.531;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1134;-1818.784,-1478.438;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;143;-8835.55,7584.38;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1087;-2423.107,1123.948;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;812;-1427.822,2326.73;Inherit;False;786;RflectAreaMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;1113;-2998.144,-785.4703;Inherit;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;1052;-4959.068,1682.812;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;1131;-2002.569,-1536.145;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;872;-4113.566,4139.583;Inherit;False;Specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1156;-1398.859,3800.935;Inherit;False;Constant;_Float11;Float 11;10;0;Create;True;0;0;0;False;0;False;1.4;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;128;-9599.269,7490.894;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1224;-4276.194,5467.074;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;1122;-2544.841,-1536.552;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1705.859,2105.615;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-2316.65,2155.419;Inherit;False;47;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;835;-2099.251,2995.472;Inherit;False;Constant;_Float14;Float 14;43;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;1136;-1705.144,-750.4703;Inherit;False;View;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;734;-4536.64,2442.255;Inherit;False;Constant;_Float41;Float 41;28;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1942.958,2162.315;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1203;-5039.998,4769.039;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;1213;-5009.959,4984.157;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;742;-4550.64,2333.255;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectSpecularLight;815;-1455.531,1890.669;Inherit;False;World;3;0;FLOAT3;0,0,1;False;1;FLOAT;1;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;871;-5454.409,4218.303;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;6;-1211.059,-2059.715;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;894;-4360.038,3956.634;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;1196;-1685.4,3772.001;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1138;-1379.289,-1016.79;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;1217;-5411.11,5913.193;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;884;-5199.121,3827.499;Inherit;False;698;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;997;-4018.762,-462.4682;Inherit;False;RainIN;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;803;-1459.703,-97.16454;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;1210;-4872.166,5492.398;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SamplerNode;46;-1152.893,965.6928;Inherit;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1208;-5186.112,6058.194;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FloorOpNode;1207;-4388.996,4575.666;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1220;-5403.11,6247.193;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1093;-1985.404,993.9822;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;93;-1161.818,1266.564;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Instance;46;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;2;-1813.005,-1957.859;Inherit;False;FLOAT4;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1209;-4213.996,4569.666;Float;False;p;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;823;-1117.588,2057.835;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;874;-4923.357,4100.028;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1132;-1897.144,-745.4703;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1137;-1595.492,-1323.891;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;3;-1643.923,-1957.981;Inherit;False;PositionFormDepth;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;1077;-2153.639,-1951.283;Inherit;False;Reconstruct World Position From Depth;-1;;14;e7094bcbcc80eb140b2a3dbe6a861de8;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1246;-4033.416,5461.884;Float;False;noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1227;-4660.821,5970.814;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1119;-2494.385,-1716.324;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;844;-1543.596,2758.053;Inherit;True;Property;_CausticsTex;Caustics Tex;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;992;-4647.354,-720.8979;Inherit;False;Constant;_Float30;Float 30;58;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1212;-5460.731,5337.715;Inherit;False;1206;f;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;833;-939.9538,3272.747;Inherit;False;783;CausticsAreaMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1097;-1476.356,1294.756;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;869;-5396.197,3618.661;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;827;-1721.995,2785.41;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1144;-569.4025,-1259.758;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1214;-5252.958,5058.157;Inherit;False;1206;f;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;1130;-2143.145,-625.4706;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OrthoParams;1135;-1724.144,-529.4707;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;990;-4256.354,-706.8979;Inherit;False;RainON;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1068;-1209.894,233.9147;Inherit;False;1067;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-533.8774,1087.203;Inherit;False;SurfaceNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;1226;-5231.945,6247.642;Inherit;False;float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed)@$return -1 + 2 * frac(sin(s) * 43758.5453123)@;2;Create;2;True;st;FLOAT2;0,0;In;;Inherit;False;True;seed;INT;0;In;;Inherit;False;rand;True;False;0;;False;2;0;FLOAT2;0,0;False;1;INT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;145;-9050.551,7677.379;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1215;-5496.957,4921.157;Inherit;False;1209;p;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1229;-5433.974,4670.195;Inherit;False;Constant;_Float8;Float 8;3;0;Create;True;0;0;0;False;0;False;0.5;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;1089;-2294.107,1204.948;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1248;-5178.112,6392.193;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;838;-879.6972,3037.297;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;847;1723.436,3605.054;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GradientNode;970;-2232.579,135.0694;Inherit;False;0;3;2;0.02990388,0.1981132,0.17916,0;0.1176471,0.1803922,0.3098039,0.4500038;0.1353685,0.2110844,0.3679245,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.RangedFloatNode;845;-1229.297,3131.155;Inherit;False;Property;_CausticsIntensity;Caustics Intensity;19;0;Create;True;0;0;0;False;0;False;1;0.61;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;848;1677.683,3820.591;Inherit;False;67;ReflectColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;784;-4023.989,863.2883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;860;2070.666,3801.779;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;851;-1369.96,4514.874;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;1141;-920.9703,-917.3584;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;991;-4449.354,-708.8979;Inherit;False;Property;_Keyword4;开启下雨功能;21;0;Create;False;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;False;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;736;-5124.501,2432.987;Inherit;False;Constant;_Float18;Float 18;22;0;Create;True;0;0;0;False;0;False;360;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-9968.845,7447.551;Inherit;False;9;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;995;-4321.662,-458.6633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-873.0843,2052.12;Inherit;False;ReflectColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1216;-5209.977,4606.195;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;837;-2081.513,2874.468;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;834;-2486.131,2845.015;Inherit;False;Property;_CausticsTilling;Caustics Tilling;12;0;Create;True;1;_____________________________________________________________________________________________;0;0;False;0;False;9.78;19.78;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;831;-423.7342,3145.093;Float;False;Caustics;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;855;-1097.96,4562.874;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;1271;1859.935,4239.989;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;858;-873.959,4434.875;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;859;-2057.959,4770.873;Inherit;False;698;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;1265;1079.935,4304.989;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;1270;1598.935,4232.989;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1267;1414.935,4234.989;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1266;1139.935,4106.989;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;1273;2244.935,4239.989;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;200;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;862;1217.001,3890.251;Inherit;False;831;Caustics;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;857;-457.9601,4434.875;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;1143;-972.7762,-1376.155;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;986;1478.306,3872.919;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;987;1223.306,3999.92;Inherit;False;744;sunhig;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;891;1387.169,3681.239;Inherit;False;872;Specular;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;849;-1609.96,4428.875;Inherit;False;Constant;_F0;F0;2;0;Create;True;0;0;0;False;0;False;0.02;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;853;-1785.96,4658.874;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;1116;-2788.144,-670.4706;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DdyOpNode;1272;2039.935,4239.989;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;854;-2057.959,4562.874;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;809;1378.184,3490.544;Inherit;False;24;WaterColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;850;-1577.959,4658.874;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-9264.551,7673.379;Inherit;False;Property;_ShoreEdgeWidth;Shore Edge Width;16;0;Create;True;0;0;0;False;0;False;0.93;0.93;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;1233;-4984.833,6303.376;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1084;-2150.061,1409.725;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1169;-959.1312,4037.67;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1139;-1364.444,-770.5704;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;1115;-2766.144,-865.4703;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1197;-2292.168,4060.554;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;830;-2096.071,3113.929;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1223;-5665.731,5191.715;Inherit;False;1209;p;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1236;-5499.164,5711.398;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GradientSampleNode;983;-1838.596,370.7819;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;802;-1265.508,-99.98461;Float;False;ratioZ;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;729;-1483.397,1472.309;Inherit;False;Property;_normal_in1;扰乱法线强度;5;0;Create;False;0;0;0;False;0;False;0.05;0.207;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;765;-1876.082,2006.609;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;828;-1908.252,2979.472;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GradientNode;980;-2228.2,281.8832;Inherit;False;0;3;2;0.1492524,0.8113208,0.6798155,0;0.4510995,0.5976637,0.8005602,0.7911803;0.5199804,0.6453958,0.8962264,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1241;-5506.164,5491.398;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;883;-5636.197,3707.661;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;981;-1693.201,607.4831;Inherit;False;744;sunhig;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-2250.65,2242.419;Inherit;False;Property;_UVMovement_Reflection;反射图片受法线影响强度;9;0;Create;False;0;0;0;False;0;False;0.64;0.64;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1109;-3864.676,290.5157;Inherit;False;UV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;775;-1198.443,340.2626;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1053;-4915.63,1866.709;Inherit;False;698;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;1128;-2220.145,-813.4703;Inherit;False;World;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;685;-537.054,1268.29;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;1056;-4184.996,1766.63;Inherit;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1123;-2371.144,-625.4706;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;875;-5646.05,3521.532;Inherit;False;867;worldlightdir;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ExpOpNode;130;-9432.268,7490.894;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;147;-8885.594,7795.718;Inherit;False;Property;_ShoreEdgeIntensity;Shore Edge Intensity;18;0;Create;True;0;0;0;False;0;False;-0.27;-0.27;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;1081;-2534.405,1023.982;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;1225;-5239.945,5913.642;Inherit;False;float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed)@$return -1 + 2 * frac(sin(s) * 43758.5453123)@;2;Create;2;True;st;FLOAT2;0,0;In;;Inherit;False;True;seed;INT;0;In;;Inherit;False;rand;True;False;0;;False;2;0;FLOAT2;0,0;False;1;INT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1206;-4207.996,4683.666;Float;False;f;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;764;-2180.083,1986.609;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;832;-1099.713,2947.668;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;1204;-5260.844,5198.98;Inherit;False;float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed)@$return -1 + 2 * frac(sin(s) * 43758.5453123)@;2;Create;2;True;st;FLOAT2;0,0;In;;Inherit;False;True;seed;INT;0;In;;Inherit;False;rand;True;False;0;;False;2;0;FLOAT2;0,0;False;1;INT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;563;-9009.898,9975.335;Inherit;False;foamnoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1242;-4661.441,4605.556;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;829;-2451.494,2746.211;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1218;-5239.998,4830.039;Inherit;False;Property;_FlowSpeed;FlowSpeed;0;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;148;-8444.594,7672.718;Inherit;False;ShoreEdge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1237;-5496.164,5615.398;Inherit;False;Constant;_Float10;Float 10;5;0;Create;True;0;0;0;False;0;False;3;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1234;-5722.164,5727.398;Inherit;False;Constant;_Float9;Float 9;6;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1082;-2378.545,1432.116;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1245;-5207.01,5343.531;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1205;-5431.831,6386.375;Inherit;False;1206;f;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1231;-5078.165,5492.398;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;842;-1710.513,3090.468;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;1230;-5433.974,4568.195;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;1235;-5243.411,4716.585;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;1238;-5013.733,5254.715;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1243;-5439.831,6052.376;Inherit;False;1206;f;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;895;-4521.038,3882.634;Inherit;False;Property;_Float19;Float 19;34;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;1078;-2781.405,965.9822;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;580;-9681.898,13063.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1091;-1719.06,1147.725;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;177;-9585.898,9479.334;Inherit;False;Property;_FoamNoiseSize;岸边波纹噪点uv尺寸;26;0;Create;False;0;0;0;False;0;False;10,40;10,40;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;1083;-2353.405,989.9822;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ProjectionParams;1112;-3077.532,-646.6969;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;582;-9681.898,13159.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;814;-1688.621,1885.547;Inherit;False;698;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1179;-443.8272,4033.455;Float;False;FoamColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;1171;-2472.978,3607.607;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;841;-634.4084,3149.27;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;1085;-2194.405,990.9822;Inherit;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;892;-5010.561,3950.987;Inherit;False;2;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-10001.9,8599.334;Inherit;False;9;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;606;-12097.9,10791.33;Inherit;False;Constant;_Float3;Float 3;45;0;Create;True;0;0;0;False;0;False;0.05;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;994;-4611.991,-467.8652;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.4;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1193;-2242.572,3609.691;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1127;-2400.144,-809.4703;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;1172;-2204.187,3854.286;Inherit;False;Property;_WaveSpeed;WaveSpeed;13;0;Create;True;0;0;0;False;0;False;-2;-5.51;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;603;-12017.9,10871.33;Inherit;False;Constant;_Vector5;Vector 5;35;0;Create;True;0;0;0;False;0;False;0.2,0.7;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;1182;-1736.199,3896.506;Inherit;False;1246;noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;1189;-2206.888,3943.687;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1176;-1182.458,3692.735;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;530;-7553.898,9959.335;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;867;-4384.593,2976.801;Float;False;worldlightdir;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1211;-5689.974,4574.195;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;1163;-1379.714,3618.68;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1184;-2236.361,3769.217;Inherit;False;Constant;_Float7;Float 7;10;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;179;-8609.898,9111.333;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;578;-9585.898,12535.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;543;-8801.898,11463.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;608;-11729.9,10775.33;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;156;-9585.898,9143.334;Inherit;False;Property;_FoamSpeed;岸边波纹速度;25;0;Create;False;0;0;0;False;0;False;-5.51;-5.51;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;863;-2783.722,2746.102;Inherit;False;3;PositionFormDepth;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ClampOpNode;581;-9585.898,12631.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-9329.898,9367.334;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1159;-2837.818,3568.947;Inherit;False;804;diffz;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;387;-10817.9,10087.33;Inherit;False;tide;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1190;-2930.077,3674.607;Inherit;False;Property;_FoamEdge;FoamEdge;8;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1195;-1831.789,3770.386;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1187;-2625.477,3606.607;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1090;-1983.841,1316.781;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1177;-1994.287,3880.086;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;456;-7601.898,10663.33;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;328;-12001.9,10423.33;Inherit;False;Constant;_Vector0;Vector 0;34;0;Create;True;0;0;0;False;0;False;0.5,-0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ClampOpNode;587;-9393.898,13239.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;602;-12001.9,11271.33;Inherit;False;Constant;_Vector4;Vector 4;34;0;Create;True;0;0;0;False;0;False;-0.4,0.1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;318;-8209.898,10791.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;879;-5175.203,4122.184;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;549;-9697.898,11511.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;592;-8945.898,13063.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;556;-9585.898,9879.335;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;161;-9553.898,9015.332;Inherit;False;Property;_FoamFrequency;岸边波纹疏密;23;0;Create;False;0;0;0;False;0;False;27.94;27.94;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-9569.898,13975.34;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;612;-11297.9,11095.33;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1140;-1104.272,-913.4584;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;1192;-2442.51,4060.989;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;605;-12017.9,11047.33;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;893;-4839.561,3950.987;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;315;-8593.898,10535.33;Inherit;True;Property;_Foam_sat;海浪纹理图;28;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;362;-10081.9,10119.33;Inherit;False;Constant;_Vector2;Vector 2;4;0;Create;True;0;0;0;False;0;False;0,0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;593;-8785.898,13015.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;545;-9697.898,11607.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;155;-9505.898,8615.334;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;395;-9233.898,10551.33;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1160;-1528.851,3827.057;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1165;-717.4724,4038.116;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;589;-9041.898,12039.34;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;541;-8081.898,9943.335;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;343;-9105.898,11047.33;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;573;-9857.898,12023.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;396;-9633.898,10551.33;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;586;-9217.898,12039.34;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;351;-12097.9,9975.335;Inherit;False;Constant;_Float1;Float 1;45;0;Create;True;0;0;0;False;0;False;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;209;-9297.898,13975.34;Inherit;False;WaveTileUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;579;-9393.898,12039.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;531;-7905.898,9943.335;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;570;-10081.9,12055.34;Inherit;False;Constant;_foam_Size2;foam_Size2;27;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;174;-9569.898,9335.334;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;599;-10145.9,12263.34;Inherit;False;9;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;609;-11569.9,11095.33;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;546;-9505.898,11575.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;345;-9345.898,11159.33;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;184;-9009.898,8615.334;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;173;-9169.898,9351.334;Inherit;False;Gradient;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;553;-8977.898,11511.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;203;-9537.898,13527.34;Inherit;False;WorldSpaceTile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;625;-8145.898,9063.333;Inherit;False;565;foamlightmask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;785;-4219.49,863.9206;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.4;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;160;-9041.898,9031.333;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;526;-7745.898,9959.335;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;610;-11569.9,10727.33;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;560;-9361.898,9975.335;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;164;-9298.463,8306.793;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;332;-12049.9,10199.33;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;296;-9873.898,10551.33;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;359;-9633.898,10983.33;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;613;-11041.9,10919.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;210;-8897.898,14103.34;Float;False;Property;_WaveHeight;Wave Height;2;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;366;-9409.898,10551.33;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;158;-9377.898,9175.334;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;335;-12097.9,10311.33;Inherit;False;Property;_foamspeed;海浪速度;29;0;Create;False;0;0;0;False;0;False;0.127;0.127;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;624;-7425.898,10583.33;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;218;-8513.898,14343.34;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;389;-10113.9,11095.33;Inherit;False;387;tide;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;739;-4239.81,2371.219;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;162;-9329.898,8967.332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;354;-11297.9,9895.335;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;535;-8593.898,10023.33;Inherit;False;Property;_depth_range;海浪深度宽度;32;0;Create;False;0;0;0;False;0;False;2;2;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;189;-8145.898,8871.332;Inherit;False;Property;_FoamColor;岸边波纹颜色;20;1;[HDR];Create;False;1;_______________________________________________________________________________________________________________;0;0;False;3;Space(10);Header(Foam_____________________________________________________________________________________________________________________________);Space(10);False;1,1,1,1;1.023397,1.137776,1.276236,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;576;-9857.898,13127.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;565;-7377.898,9959.335;Inherit;False;foamlightmask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;368;-9201.898,9975.335;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;206;-9761.898,14087.34;Float;False;Property;_WaveTile;Wave Tile;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;344;-8865.898,10823.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1247;-5644.831,5906.376;Inherit;False;1209;p;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;880;-4514.861,4072.901;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;61;-2080.96,2156.315;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;-9091.134,7486.13;Inherit;False;WaterShore;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;204;-9841.898,13959.34;Inherit;False;203;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;217;-8977.898,14391.34;Inherit;False;Constant;_WaveDirection;Wave Direction;4;0;Create;True;0;0;0;False;0;False;0,0.2;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;390;-9809.898,11047.33;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;216;-8705.898,14359.34;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;379;-11745.9,9943.335;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;588;-9137.898,13127.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;199;-10129.9,13527.34;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;575;-10129.9,13191.34;Inherit;False;Constant;_Float2;Float 2;45;0;Create;True;0;0;0;False;0;False;-0.03;0;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;558;-10065.9,9911.335;Inherit;False;Constant;_Vector3;Vector 3;4;0;Create;True;0;0;0;False;0;False;0.2,0.1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ClampOpNode;154;-9729.898,8631.334;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;551;-10161.9,11639.34;Inherit;False;Constant;_foam_edge;foam_edge;45;0;Create;True;0;0;0;False;0;False;-0.03;0;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;611;-11297.9,10711.33;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;596;-8577.898,12375.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;839;-1541.586,3063.647;Inherit;True;Property;_CausticsTex2;Caustics Tex2;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;844;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;383;-11569.9,9911.335;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;1249;-4992.833,5969.376;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;976;-1708.25,1987.388;Inherit;False;ScreenMask;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;584;-9393.898,12583.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;-8657.898,14071.34;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;-10177.9,10711.33;Inherit;False;9;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;392;-9633.898,11079.33;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;591;-8849.898,12375.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;182;-8465.898,8791.333;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;443;-7905.898,10487.33;Inherit;False;Property;_Color2;海浪颜色;27;1;[HDR];Create;False;1;______________________________________________________________________________________________________________________;0;0;False;3;Space(10);Header(Wave___________________________________________________________________________________________________________);Space(10);False;0.6698113,0.9897224,1,1;0.8990035,1.701685,1.798007,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1219;-5262.165,5688.398;Inherit;False;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1221;-5636.831,6240.376;Inherit;False;1209;p;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalizeNode;885;-5188.197,3617.661;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;527;-7905.898,10039.33;Inherit;False;Constant;_Float5;Float 5;49;0;Create;True;0;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1094;-1475.544,994.1165;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;993;-4936.444,-474.9941;Inherit;False;Global;MyLerpSky;MyLerpSky;17;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;743;-5322.211,2389.47;Inherit;False;Global;_SimLight0EularX;_SimLight0EularX;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;574;-9793.898,12615.34;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1058.622,-2064.475;Inherit;False;WaterDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;881;-4327.861,4144.9;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-8612.594,7675.718;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;737;-5106.501,2319.987;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;826;-2352.799,3191.988;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;1145;-336.3208,-1260.472;Inherit;False;Global;_GrabScreen0;Grab Screen 0;9;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;786;-3814.773,857.8643;Float;False;RflectAreaMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;1151;-4750.18,2982.326;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;159;-9585.898,9239.334;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;163;-9490.877,8340.43;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;585;-9521.898,13127.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;873;-4564.861,4258.9;Inherit;False;Property;_SpecularPower;Specular Power;33;0;Create;True;0;0;0;False;0;False;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;363;-10081.9,10023.33;Inherit;False;209;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;738;-5275.512,2308.987;Inherit;False;Constant;_Float38;Float 38;22;0;Create;True;0;0;0;False;0;False;90;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;698;-301.2431,1262.817;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;555;-8705.898,11575.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;566;-7921.898,10695.33;Inherit;False;565;foamlightmask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-8913.898,9111.333;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-9720.846,8290.231;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1173;-2305.919,3690.113;Inherit;False;Property;_FoamTilling;Foam Tilling;10;0;Create;True;0;0;0;False;0;False;2;27.94;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;600;-9633.898,12167.34;Inherit;False;563;foamnoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;190;-7825.898,8871.332;Inherit;False;5;5;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;152;-9825.898,8615.334;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;614;-10801.9,10903.33;Inherit;False;tide2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;349;-11969.9,10039.33;Inherit;False;Constant;_Vector1;Vector 1;35;0;Create;True;0;0;0;False;0;False;-0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;153;-10129.9,8695.333;Inherit;False;Property;_FoamRange;岸边波纹宽度;22;0;Create;False;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;547;-9169.898,11575.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1153;2682.739,3666.989;Inherit;False;1179;FoamColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;391;-10145.9,11223.33;Inherit;False;Property;_FoamTide_Range;海浪扭曲程度;31;0;Create;False;0;0;0;False;0;False;30;30;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;317;-9073.898,10551.33;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;607;-11745.9,11143.33;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;590;-9073.898,12599.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;604;-12081.9,11159.33;Inherit;False;Constant;_Float6;Float 6;42;0;Create;True;0;0;0;False;0;False;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-11761.9,10295.33;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-8337.898,14135.34;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;571;-10113.9,12775.34;Inherit;False;Property;_FoamTide_Range2;海浪扭曲程度2;30;0;Create;False;0;0;0;False;0;False;30;30;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;382;-11585.9,10247.33;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;214;-8081.898,14199.34;Inherit;False;WaveHight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;583;-9313.898,12727.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;552;-8609.898,10823.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;157;-9185.898,8999.332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;1054;-4612.996,1766.796;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;295;-10081.9,10567.33;Inherit;False;Constant;_foam_Size;foam_Size;26;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;342;-9473.898,11047.33;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;-8232.445,8495.437;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;557;-9793.898,9895.335;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1164;-1999.219,3672.013;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-9780.623,8402.364;Inherit;False;Property;_FoamBlend;岸边波纹明度;24;0;Create;False;0;0;0;False;0;False;-0.37;-0.37;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;564;-9633.898,10679.33;Inherit;False;563;foamnoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;201;-9841.898,13543.34;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;597;-8177.898,12343.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;523;-8529.898,9927.335;Inherit;False;9;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;4;-1394.84,-1956.712;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;211;-8929.898,13943.34;Float;False;Constant;_WaveUp;Wave Up;1;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;180;-8849.898,9223.334;Inherit;False;Constant;_FoamDissolve;FoamDissolve;33;0;Create;True;0;0;0;False;0;False;1.5;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;741;-4706.231,2329.8;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;365;-9585.898,10103.33;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;364;-9809.898,10103.33;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;550;-9873.898,11575.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;594;-8673.898,13127.34;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;572;-10113.9,12647.34;Inherit;False;614;tide2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;330;-11313.9,10247.33;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;215;-8977.898,14311.34;Inherit;False;209;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;355;-11057.9,10087.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;619;-7873.898,11239.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1275;2405.659,3803.632;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;805;2804.931,3990.627;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;186;-8769.898,8935.332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-9201.898,8679.333;Inherit;False;Constant;_FoamWidth;Foam Width;33;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;542;-8289.898,9943.335;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;577;-9617.898,12039.34;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;548;-9409.898,11687.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1072;2699.232,3803.804;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;630;3053.751,3732.435;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1146;4127.198,3791.014;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;False;82;False;;40.55;False;;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;627;2989.264,3800.376;Float;False;True;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;Hyaline/Water/Planar;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=1;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;False;82;False;;40.55;False;;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;23;Surface;1;0;  Blend;0;0;Two Sided;1;0;Forward Only;0;0;Cast Shadows;0;0;  Use Shadow Threshold;0;0;Receive Shadows;0;0;GPU Instancing;1;0;LOD CrossFade;0;0;Built-in Fog;0;0;DOTS Instancing;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;False;True;True;True;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1150;4127.198,3791.014;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;628;3053.751,3732.435;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;629;3053.751,3732.435;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1147;4127.198,3791.014;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1148;4127.198,3791.014;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1149;4127.198,3791.014;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;626;3027.639,3798.901;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;744;-4102.209,2363.633;Inherit;False;sunhig;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1264;1685.967,3970.555;Inherit;False;1263;vReflect;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1263;-281.5828,4425.302;Inherit;False;vReflect;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;861;1373.151,3582.912;Inherit;False;1179;FoamColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;24;-550.278,272.2119;Inherit;False;WaterColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;1125;-2262.505,-1494.259;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;1067;-105.7654,-1264.635;Inherit;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;1069;-849.7795,270.0475;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1088;-2005.06,1200.725;Inherit;False;1109;UV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1106;-4549.17,382.013;Inherit;False;Constant;_Float0;Float 0;29;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;1079;-2718.545,1334.116;Inherit;False;Property;_NormalMovement;Normal Movement;6;0;Create;True;0;0;0;False;0;False;0,0;-0.05,-0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;249;2605.033,3938.244;Inherit;False;804;diffz;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;783;-3955.501,1346.865;Inherit;False;CausticsAreaMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;1129;-1996.51,-1357.583;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1114;-3109.607,-1227.163;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;1118;-2959.135,-1363.976;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1110;-3416.805,-1232.163;Inherit;False;Property;_RefractionPower;Refraction Power;11;0;Create;True;0;0;0;False;0;False;0.1;1.81;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1111;-3422.782,-1081.182;Inherit;False;47;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;1117;-3190.64,-1079.412;Inherit;False;Tangent;View;True;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1120;-2964.807,-1140.663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;1277;-2806.073,-1141.722;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;1278;-2447.97,-1158.327;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1126;-2557.89,-1328.541;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1280;-2229.676,-1274.613;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1279;-2657.073,-1117.722;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;977;-2594.355,387.3554;Inherit;False;976;ScreenMask;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1281;-2370.544,494.6581;Inherit;False;804;diffz;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1285;-2139.168,500.5776;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-1.5;False;2;FLOAT;2.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;806;2558.931,4052.627;Inherit;False;793;TransmittAreaMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;791;-4243.417,1109.786;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;1.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;852;-1369.96,4658.874;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;3.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;856;-681.9602,4434.875;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.65;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;56;-1519.107,2076.622;Inherit;True;Global;_PlanarReflectionTexture;_PlanarReflectionTexture;7;0;Create;True;0;0;0;False;0;False;-1;None;;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;795;0;794;0
WireConnection;796;0;795;0
WireConnection;798;0;796;0
WireConnection;799;0;797;0
WireConnection;799;1;798;0
WireConnection;790;0;788;0
WireConnection;790;1;789;0
WireConnection;800;0;799;0
WireConnection;800;1;1250;0
WireConnection;792;0;791;0
WireConnection;801;0;800;0
WireConnection;793;0;792;0
WireConnection;804;0;801;0
WireConnection;1142;1;1141;0
WireConnection;1107;0;1105;0
WireConnection;1107;1;1106;0
WireConnection;787;0;790;0
WireConnection;1092;0;1088;0
WireConnection;1092;1;1086;0
WireConnection;1105;0;1104;1
WireConnection;1105;1;1104;3
WireConnection;54;0;46;0
WireConnection;54;1;93;0
WireConnection;733;0;737;0
WireConnection;733;1;736;0
WireConnection;1055;0;1054;0
WireConnection;882;0;878;0
WireConnection;882;1;874;0
WireConnection;882;2;877;0
WireConnection;1244;0;1239;1
WireConnection;1244;1;1203;0
WireConnection;1124;0;1119;0
WireConnection;1124;1;1122;0
WireConnection;1080;0;1078;0
WireConnection;763;0;57;1
WireConnection;763;1;57;2
WireConnection;735;0;733;0
WireConnection;1108;0;1107;0
WireConnection;740;0;742;0
WireConnection;740;1;734;0
WireConnection;1086;1;1079;2
WireConnection;1222;0;1215;0
WireConnection;1152;0;890;0
WireConnection;982;0;972;0
WireConnection;982;1;983;0
WireConnection;982;2;981;0
WireConnection;1121;0;1115;0
WireConnection;1121;1;1113;0
WireConnection;1121;2;1116;0
WireConnection;878;0;885;0
WireConnection;878;1;884;0
WireConnection;1240;0;1213;0
WireConnection;1240;1;1238;0
WireConnection;1240;2;1210;0
WireConnection;972;0;970;0
WireConnection;972;1;979;0
WireConnection;1202;0;1242;0
WireConnection;979;0;978;0
WireConnection;978;0;977;0
WireConnection;1239;0;1216;0
WireConnection;840;0;829;0
WireConnection;840;1;834;0
WireConnection;131;0;130;0
WireConnection;127;0;126;0
WireConnection;1232;0;1223;0
WireConnection;1134;0;1131;0
WireConnection;1134;1;1129;0
WireConnection;143;0;132;0
WireConnection;143;1;145;0
WireConnection;1087;0;1079;1
WireConnection;1131;0;1124;0
WireConnection;1131;1;1125;4
WireConnection;872;0;881;0
WireConnection;128;0;127;0
WireConnection;128;1;129;0
WireConnection;1224;0;1240;0
WireConnection;1224;1;1227;0
WireConnection;1224;2;1210;1
WireConnection;59;0;765;0
WireConnection;59;1;62;0
WireConnection;1136;0;1132;0
WireConnection;62;0;61;0
WireConnection;62;1;63;0
WireConnection;1203;0;1235;0
WireConnection;1203;1;1218;0
WireConnection;1213;0;1222;0
WireConnection;1213;1;1214;0
WireConnection;742;0;741;0
WireConnection;815;0;814;0
WireConnection;6;0;5;2
WireConnection;6;1;4;0
WireConnection;894;0;895;0
WireConnection;894;1;880;0
WireConnection;1196;0;1195;0
WireConnection;1217;0;1247;0
WireConnection;997;0;995;0
WireConnection;803;0;797;0
WireConnection;803;1;798;0
WireConnection;1210;0;1231;0
WireConnection;46;1;1094;0
WireConnection;46;5;252;0
WireConnection;1208;0;1243;0
WireConnection;1207;0;1242;0
WireConnection;1220;0;1221;0
WireConnection;1093;0;1085;0
WireConnection;1093;1;1089;1
WireConnection;1093;2;1087;0
WireConnection;93;1;1097;0
WireConnection;93;5;729;0
WireConnection;2;0;1077;0
WireConnection;1209;0;1207;0
WireConnection;823;0;815;0
WireConnection;823;1;56;0
WireConnection;823;2;812;0
WireConnection;874;1;879;0
WireConnection;1132;0;1128;1
WireConnection;1132;1;1128;2
WireConnection;1132;2;1130;0
WireConnection;1137;0;1134;0
WireConnection;1137;1;1133;0
WireConnection;3;0;2;0
WireConnection;1246;0;1224;0
WireConnection;1227;0;1249;0
WireConnection;1227;1;1233;0
WireConnection;1227;2;1210;0
WireConnection;844;1;827;0
WireConnection;1097;0;1092;0
WireConnection;1097;1;1090;0
WireConnection;869;0;875;0
WireConnection;869;1;883;0
WireConnection;827;0;840;0
WireConnection;827;1;830;0
WireConnection;1144;0;1143;0
WireConnection;1144;1;1280;0
WireConnection;1144;2;1142;0
WireConnection;1130;0;1123;0
WireConnection;990;0;991;0
WireConnection;47;0;54;0
WireConnection;1226;0;1220;0
WireConnection;145;0;144;0
WireConnection;1248;0;1205;0
WireConnection;838;0;832;0
WireConnection;838;1;845;0
WireConnection;847;0;809;0
WireConnection;847;1;861;0
WireConnection;847;2;891;0
WireConnection;847;3;862;0
WireConnection;784;0;785;0
WireConnection;860;0;847;0
WireConnection;860;1;848;0
WireConnection;860;2;1264;0
WireConnection;851;0;849;0
WireConnection;1141;0;1140;0
WireConnection;991;1;992;0
WireConnection;991;0;989;0
WireConnection;995;0;994;0
WireConnection;995;1;996;0
WireConnection;67;0;823;0
WireConnection;1216;0;1230;0
WireConnection;1216;1;1229;0
WireConnection;837;0;840;0
WireConnection;831;0;841;0
WireConnection;855;0;851;0
WireConnection;855;1;852;0
WireConnection;1271;0;1270;0
WireConnection;858;0;849;0
WireConnection;858;1;855;0
WireConnection;1270;0;1267;0
WireConnection;1267;0;1266;0
WireConnection;1267;1;1265;0
WireConnection;1273;0;1272;0
WireConnection;857;0;856;0
WireConnection;986;1;862;0
WireConnection;986;2;987;0
WireConnection;853;0;854;0
WireConnection;853;1;859;0
WireConnection;1116;1;1112;1
WireConnection;1272;0;1271;0
WireConnection;850;0;853;0
WireConnection;1233;0;1226;0
WireConnection;1233;1;1248;0
WireConnection;1084;1;1082;0
WireConnection;1169;0;1176;0
WireConnection;1169;1;1197;0
WireConnection;1139;0;1137;0
WireConnection;1139;1;1136;0
WireConnection;1139;2;1135;4
WireConnection;1115;0;1113;0
WireConnection;1197;0;1192;0
WireConnection;830;0;843;0
WireConnection;830;1;826;0
WireConnection;1236;0;1228;0
WireConnection;1236;1;1234;0
WireConnection;983;0;980;0
WireConnection;983;1;1285;0
WireConnection;802;0;803;0
WireConnection;765;0;763;0
WireConnection;765;1;57;4
WireConnection;828;0;837;0
WireConnection;828;1;835;0
WireConnection;1241;0;1228;0
WireConnection;1241;1;1228;0
WireConnection;1109;0;1108;0
WireConnection;775;0;983;0
WireConnection;1128;0;1127;0
WireConnection;685;0;54;0
WireConnection;1056;0;1055;0
WireConnection;1123;0;1112;2
WireConnection;1123;1;1112;3
WireConnection;1123;2;1121;0
WireConnection;130;0;128;0
WireConnection;1081;0;1078;0
WireConnection;1225;0;1217;0
WireConnection;1206;0;1202;0
WireConnection;764;0;57;2
WireConnection;832;0;844;0
WireConnection;832;1;839;0
WireConnection;1204;0;1232;0
WireConnection;563;0;368;0
WireConnection;1242;0;1239;0
WireConnection;1242;1;1244;0
WireConnection;829;0;863;0
WireConnection;148;0;146;0
WireConnection;1082;0;1079;2
WireConnection;1245;0;1212;0
WireConnection;1231;0;1241;0
WireConnection;1231;1;1219;0
WireConnection;842;0;828;0
WireConnection;842;1;830;0
WireConnection;1230;0;1211;0
WireConnection;1238;0;1204;0
WireConnection;1238;1;1245;0
WireConnection;580;0;599;0
WireConnection;580;1;576;0
WireConnection;1091;0;1088;0
WireConnection;1091;1;1084;0
WireConnection;1083;0;1080;0
WireConnection;1083;1;1081;0
WireConnection;582;0;576;0
WireConnection;1179;0;1165;0
WireConnection;1171;0;1187;0
WireConnection;841;0;838;0
WireConnection;841;1;833;0
WireConnection;1085;0;1083;0
WireConnection;892;1;879;0
WireConnection;994;0;993;0
WireConnection;1193;0;1171;0
WireConnection;1176;0;1163;0
WireConnection;1176;1;1156;0
WireConnection;530;0;526;0
WireConnection;867;0;1151;0
WireConnection;1163;0;1193;0
WireConnection;1163;1;1160;0
WireConnection;179;0;186;0
WireConnection;179;1;180;0
WireConnection;578;0;599;0
WireConnection;578;1;574;0
WireConnection;543;0;553;0
WireConnection;543;1;344;0
WireConnection;608;0;605;0
WireConnection;608;1;606;0
WireConnection;608;2;603;0
WireConnection;581;0;574;0
WireConnection;175;0;174;0
WireConnection;175;1;177;0
WireConnection;387;0;355;0
WireConnection;1195;0;1164;0
WireConnection;1195;1;1177;0
WireConnection;1187;0;1159;0
WireConnection;1187;1;1190;0
WireConnection;1090;0;1085;0
WireConnection;1090;1;1089;1
WireConnection;1090;2;1079;1
WireConnection;1177;0;1172;0
WireConnection;1177;1;1189;0
WireConnection;456;0;443;0
WireConnection;456;1;566;0
WireConnection;456;2;443;4
WireConnection;456;3;619;0
WireConnection;587;0;572;0
WireConnection;318;0;315;1
WireConnection;318;1;552;0
WireConnection;879;0;1152;0
WireConnection;879;1;871;0
WireConnection;549;0;294;0
WireConnection;549;1;550;0
WireConnection;592;0;589;0
WireConnection;592;1;588;0
WireConnection;556;0;557;0
WireConnection;208;0;204;0
WireConnection;208;1;206;0
WireConnection;612;0;609;0
WireConnection;1140;0;1138;0
WireConnection;1140;1;1139;0
WireConnection;1192;1;1187;0
WireConnection;893;0;892;0
WireConnection;593;0;592;0
WireConnection;593;1;591;0
WireConnection;545;0;550;0
WireConnection;155;0;154;0
WireConnection;395;0;366;0
WireConnection;1160;0;1196;0
WireConnection;1160;1;1182;0
WireConnection;1165;0;1169;0
WireConnection;589;0;586;0
WireConnection;541;0;542;0
WireConnection;343;0;342;0
WireConnection;343;1;345;0
WireConnection;573;0;599;0
WireConnection;573;1;570;0
WireConnection;396;0;296;0
WireConnection;396;1;390;0
WireConnection;586;0;579;0
WireConnection;209;0;208;0
WireConnection;579;0;577;0
WireConnection;579;1;600;0
WireConnection;531;0;541;0
WireConnection;609;1;607;0
WireConnection;546;0;549;0
WireConnection;546;1;545;0
WireConnection;345;0;389;0
WireConnection;184;0;155;0
WireConnection;184;1;185;0
WireConnection;173;0;175;0
WireConnection;553;0;317;0
WireConnection;553;1;547;0
WireConnection;203;0;201;0
WireConnection;785;0;790;0
WireConnection;160;0;157;0
WireConnection;526;0;531;0
WireConnection;526;1;527;0
WireConnection;610;1;608;0
WireConnection;560;0;556;0
WireConnection;560;1;365;0
WireConnection;164;0;163;0
WireConnection;296;0;294;0
WireConnection;296;1;295;0
WireConnection;359;0;294;0
WireConnection;359;1;390;0
WireConnection;613;0;611;0
WireConnection;613;1;612;0
WireConnection;366;0;396;0
WireConnection;366;1;564;0
WireConnection;158;0;156;0
WireConnection;158;1;159;0
WireConnection;624;0;190;0
WireConnection;624;1;456;0
WireConnection;218;0;216;0
WireConnection;739;0;740;0
WireConnection;162;0;155;0
WireConnection;162;1;161;0
WireConnection;354;0;383;0
WireConnection;576;0;574;0
WireConnection;576;1;575;0
WireConnection;565;0;530;0
WireConnection;368;0;560;0
WireConnection;344;0;317;0
WireConnection;344;1;343;0
WireConnection;880;0;882;0
WireConnection;61;0;60;0
WireConnection;132;0;131;0
WireConnection;390;0;389;0
WireConnection;390;1;389;0
WireConnection;390;2;391;0
WireConnection;216;0;215;0
WireConnection;216;2;217;0
WireConnection;379;0;332;0
WireConnection;379;1;351;0
WireConnection;379;2;349;0
WireConnection;588;0;585;0
WireConnection;588;1;587;0
WireConnection;154;0;152;0
WireConnection;611;0;610;0
WireConnection;596;0;591;0
WireConnection;596;1;594;0
WireConnection;839;1;842;0
WireConnection;383;1;379;0
WireConnection;1249;0;1225;0
WireConnection;1249;1;1208;0
WireConnection;976;0;765;0
WireConnection;584;0;578;0
WireConnection;584;1;581;0
WireConnection;212;0;211;0
WireConnection;212;1;210;0
WireConnection;392;0;390;0
WireConnection;591;0;589;0
WireConnection;591;1;590;0
WireConnection;182;0;184;0
WireConnection;182;1;179;0
WireConnection;1219;0;1237;0
WireConnection;1219;1;1236;0
WireConnection;885;0;869;0
WireConnection;1094;0;1093;0
WireConnection;1094;1;1091;0
WireConnection;574;0;572;0
WireConnection;574;1;572;0
WireConnection;574;2;571;0
WireConnection;9;0;6;0
WireConnection;881;0;880;0
WireConnection;881;1;873;0
WireConnection;146;0;143;0
WireConnection;146;1;147;0
WireConnection;737;0;738;0
WireConnection;737;1;743;0
WireConnection;1145;0;1144;0
WireConnection;786;0;784;0
WireConnection;163;0;166;0
WireConnection;163;1;165;0
WireConnection;585;0;580;0
WireConnection;585;1;582;0
WireConnection;698;0;685;0
WireConnection;555;0;543;0
WireConnection;172;0;160;0
WireConnection;172;1;173;0
WireConnection;166;0;154;0
WireConnection;190;0;167;0
WireConnection;190;1;189;0
WireConnection;190;2;315;1
WireConnection;190;3;625;0
WireConnection;190;4;189;4
WireConnection;152;0;151;0
WireConnection;152;1;153;0
WireConnection;614;0;613;0
WireConnection;547;0;546;0
WireConnection;547;1;548;0
WireConnection;317;0;395;0
WireConnection;607;0;605;0
WireConnection;607;1;604;0
WireConnection;607;2;602;0
WireConnection;590;0;584;0
WireConnection;590;1;583;0
WireConnection;329;0;332;0
WireConnection;329;1;335;0
WireConnection;329;2;328;0
WireConnection;213;0;212;0
WireConnection;213;1;218;0
WireConnection;382;1;329;0
WireConnection;214;0;213;0
WireConnection;583;0;572;0
WireConnection;552;0;344;0
WireConnection;552;1;555;0
WireConnection;157;0;162;0
WireConnection;157;1;158;0
WireConnection;1054;0;1052;0
WireConnection;1054;1;1053;0
WireConnection;342;0;359;0
WireConnection;342;1;392;0
WireConnection;167;0;182;0
WireConnection;167;1;164;0
WireConnection;557;0;363;0
WireConnection;557;2;558;0
WireConnection;1164;0;1193;0
WireConnection;1164;1;1173;0
WireConnection;1164;2;1184;0
WireConnection;201;0;199;1
WireConnection;201;1;199;3
WireConnection;597;0;315;1
WireConnection;597;1;596;0
WireConnection;4;0;3;0
WireConnection;741;0;735;0
WireConnection;365;0;364;0
WireConnection;364;0;363;0
WireConnection;364;2;362;0
WireConnection;550;0;390;0
WireConnection;550;1;551;0
WireConnection;594;0;593;0
WireConnection;330;0;382;0
WireConnection;355;0;354;0
WireConnection;355;1;330;0
WireConnection;619;0;318;0
WireConnection;619;1;597;0
WireConnection;1275;0;860;0
WireConnection;805;0;249;0
WireConnection;805;1;806;0
WireConnection;186;0;155;0
WireConnection;186;1;172;0
WireConnection;542;0;523;0
WireConnection;542;2;535;0
WireConnection;577;0;573;0
WireConnection;577;1;574;0
WireConnection;548;0;389;0
WireConnection;1072;0;1275;0
WireConnection;627;2;1072;0
WireConnection;627;3;805;0
WireConnection;744;0;739;0
WireConnection;1263;0;857;0
WireConnection;24;0;1069;0
WireConnection;1067;0;1145;0
WireConnection;1069;0;1068;0
WireConnection;1069;1;775;0
WireConnection;783;0;787;0
WireConnection;1129;0;1280;0
WireConnection;1114;0;1110;0
WireConnection;1117;0;1111;0
WireConnection;1120;0;1114;0
WireConnection;1120;1;1117;0
WireConnection;1277;0;1120;0
WireConnection;1278;0;1118;2
WireConnection;1278;1;1279;0
WireConnection;1126;0;1118;1
WireConnection;1126;1;1277;0
WireConnection;1280;0;1126;0
WireConnection;1280;1;1278;0
WireConnection;1279;0;1277;1
WireConnection;1285;0;1281;0
WireConnection;791;0;790;0
WireConnection;852;0;850;0
WireConnection;856;0;858;0
WireConnection;56;1;59;0
ASEEND*/
//CHKSM=A9CBBBB1D223C31B15FF7BE1E4789AC20CE30DAA