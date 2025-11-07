Shader "Athena/VolumetricFog/Filter"
{
    Properties
    {
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
        	Name "VolumetricFog Filter"
            ZTest Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature _ _FilterMode_Gaussian _FilterMode_Bilateral _FilterMode_Box4x4
            #pragma multi_compile_local _ _DepthClamp

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScaling.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
			
			#ifdef USE_FULL_PRECISION_BLIT_TEXTURE
			TEXTURE2D_X_FLOAT(_BlitTexture);
			#else
			TEXTURE2D_X(_BlitTexture);
			#endif
			TEXTURECUBE(_BlitCubeTexture);
			
			uniform float4 _BlitScaleBias;
			uniform float4 _BlitScaleBiasRt;
			uniform float4 _BlitTexture_TexelSize;
			uniform float _BlitMipLevel;
			uniform float2 _BlitTextureSize;
			uniform uint _BlitPaddingSize;
			uniform int _BlitTexArraySlice;
			uniform float4 _BlitDecodeInstructions;
			
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				uint vertexID : SV_VertexID;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            	float3 viewDir: TEXCOORD01;
            	UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            //half4 _BlitTexture_TexelSize;
            CBUFFER_END

            half4 _TemporalFilterParam;
            float4 _DepthClampParam;
            float4x4 _ClipToLastClip;

            #define _HistoryWeight _TemporalFilterParam.x
            #define _LastCameraPos _TemporalFilterParam.yzw
            #define _DepthThreshold _DepthClampParam.x
            #define _DepthClampMaxDistance _DepthClampParam.y
            #define _DepthTexelSize _DepthClampParam.zw
            #define _DepthOffsetScale 1

            //TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            TEXTURE2D(_HistoryFogTexture);
            SAMPLER(sampler_HistoryFogTexture);
            TEXTURE2D_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D_FLOAT(_LastDepthTexture);
            SAMPLER(sampler_LastDepthTexture);

            Varyings Vert(Attributes input)
            {
                Varyings output;
            	UNITY_SETUP_INSTANCE_ID(input);
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            	float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
			    float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
			
			    output.positionCS = pos;
			    output.uv = DYNAMIC_SCALING_APPLY_SCALEBIAS(uv);
            	
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                //output.positionCS = vertexInput.positionCS;
                //output.uv = input.uv;
            	output.viewDir = GetCameraPositionWS() - vertexInput.positionWS;
                return output;
            }

            half LuminanceDiff(half4 color0, half4 color1)
            {
                half l0 = Luminance(color0.rgb);
                half l1 = Luminance(color1.rgb);
                return smoothstep(0.5, 1.0, 1.0 - abs(l0 - l1));
            }

            half4 Filter(float2 uv, float2 TexelSize)
            {
                half4 res;
                #if _FilterMode_Box4x4
					// Use a 4x4 box filter because the random texture is tiled 4x4
					res =  SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
					res += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + TexelSize * float2(2, 0));
					res += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + TexelSize * float2(0, 2));
					res += SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + TexelSize * float2(2, 2));
					res *= 0.25;
                #else
                float2 uvR = uv + TexelSize * float2(1, 0);
                float2 uvT = uv + TexelSize * float2(0, 1);
                float2 uvRT = uv + TexelSize * float2(1, 1);
                float2 uvLT = uv + TexelSize * float2(-1, 1);
                float2 uvL = uv + TexelSize * float2(-1, 0);
                float2 uvB = uv + TexelSize * float2(0, -1);
                float2 uvRB = uv + TexelSize * float2(1, -1);
                float2 uvLB = uv + TexelSize * float2(-1, -1);
                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                float4 colorR = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvR);
                float4 colorT = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvT);
                float4 colorRT = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvRT);
                float4 colorLT = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvLT);
                float4 colorL = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvL);
                float4 colorB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvB);
                float4 colorRB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvRB);
                float4 colorLB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvLB);
                #if _FilterMode_Gaussian
						res = (color * 0.147761 +
		 					   colorR * 0.118318 +
		 					   colorT * 0.118318 +
		 					   colorRT * 0.0947416 +
		 					   colorLT * 0.0947416 +
		 					   colorL * 0.118318 +
		 					   colorB * 0.118318 +
		 					   colorRB * 0.0947416 +
		 					   colorLB * 0.0947416);
                #elif _FilterMode_Bilateral
						float W = 0.147761;
						float WR = 0.118318 *   LuminanceDiff(color, colorR);
						float WT = 0.118318 *   LuminanceDiff(color, colorT);
						float WRT = 0.0947416 * LuminanceDiff(color, colorRT);
						float WLT = 0.0947416 * LuminanceDiff(color, colorLT);
						float WL = 0.118318 *   LuminanceDiff(color, colorL);
						float WB = 0.118318 *   LuminanceDiff(color, colorB);
						float WRB = 0.0947416 * LuminanceDiff(color, colorRB);
						float WLB = 0.0947416 * LuminanceDiff(color, colorLB);
						res = (color * W +
						 	   colorR * WR +
						 	   colorT * WT +
						 	   colorRT * WRT +
						 	   colorLT * WLT +
						 	   colorL * WL +
						 	   colorB * WB +
						 	   colorRB * WRB +
						 	   colorLB * WLB );
						res /= W + WR + WT + WRT + WLT + WL + WB + WRB + WLB;
                #else
                res = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                #endif
                #endif
                return res;
            }

            float4 ComputeClipSpacePosition3(float2 positionNDC, float deviceDepth)
            {
                #if defined(SHADER_API_GLCORE) || defined (SHADER_API_GLES) || defined (SHADER_API_GLES3)
					deviceDepth = deviceDepth * 2 - 1;
                #endif

                float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
                return positionCS;
            }

            float2 GetHistoryUV(float2 uv, float depth, float volumetricDepth)
            {
                // depth = max(depth, volumetricDepth);
                float4 curClip = ComputeClipSpacePosition3(uv, depth);
                float4 lastClip = mul(_ClipToLastClip, curClip);
                // return float2(volumetricDepth, 0);
                return (lastClip.xy / lastClip.w + 1.0) * 0.5;
            }

            // #define _DepthClamp_Neighbor 1

            half HistoryClamp(float2 lastUV, float2 uv, float depth, float3 viewDir)
            {
                half history = _HistoryWeight;
                #if _DepthClamp
                float lastDepth = SAMPLE_DEPTH_TEXTURE(_LastDepthTexture, sampler_LastDepthTexture, lastUV);
                lastDepth = LinearEyeDepth(lastDepth, _ZBufferParams);
            	//根据相机的移动修正深度
            	lastDepth = max(lastDepth + dot(_LastCameraPos - GetCameraPositionWS(), normalize(viewDir)), 0);
                float curDepth = LinearEyeDepth(depth, _ZBufferParams);
            	
                #if _DepthClamp_Neighbor
                    
                    half4 lastDepthNeighbor;
                    float2 uv0 = lastUV + _DepthTexelSize * float2(1, 0) * _DepthOffsetScale;
                    float2 uv1 = lastUV + _DepthTexelSize * float2(-1, 0) * _DepthOffsetScale;
                    float2 uv2 = lastUV + _DepthTexelSize * float2(0, -1) * _DepthOffsetScale;
                    float2 uv3 = lastUV + _DepthTexelSize * float2(0, 1) * _DepthOffsetScale;
                    lastDepthNeighbor.x = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_LastDepthTexture, sampler_LastDepthTexture, uv0), _ZBufferParams);
                    lastDepthNeighbor.y = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_LastDepthTexture, sampler_LastDepthTexture, uv1), _ZBufferParams);
                    lastDepthNeighbor.z = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_LastDepthTexture, sampler_LastDepthTexture, uv2), _ZBufferParams);
                    lastDepthNeighbor.w = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_LastDepthTexture, sampler_LastDepthTexture, uv3), _ZBufferParams);

                    if (any(abs(lastDepthNeighbor - curDepth) > _DepthThreshold)
                        || (abs(lastDepth - curDepth) > _DepthThreshold))
                    {
	                    return 0;
                    }
                #else
            		// 远处物体深度精度低，Clamp有可能导致闪烁
            		half minDepth = min(curDepth, lastDepth);
                	if (abs(lastDepth - curDepth) > _DepthThreshold && minDepth < _DepthClampMaxDistance)
                	{
	                    return 0;
                	}
                #endif

                #endif

                if (any(lastUV > float2(1, 1)) || any(lastUV < float2(0, 0)))
                {
	                return 0;
                }

                return history;
            }

            float4 Frag(Varyings input) : SV_Target0
            {
                //float2 uv = input.uv;
                //float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
                //half4 col = Filter(uv, _BlitTexture_TexelSize.xy);
                //if (_HistoryWeight > 0)
                //{
                //    float2 lastUV = GetHistoryUV(uv, depth, input.positionCS.z);
                //    half4 historyCol = SAMPLE_TEXTURE2D(_HistoryFogTexture, sampler_HistoryFogTexture, lastUV);
                //    half history = HistoryClamp(lastUV, uv, depth, input.viewDir);
                //    col = lerp(col, historyCol, history);
                //}
            	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                //return col;
            	float2 uv = input.uv.xy;
            	float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            	half4 col = Filter(uv, _BlitTexture_TexelSize.xy);
            	if (_HistoryWeight > 0)
                {
                    float2 lastUV = GetHistoryUV(uv, depth, input.positionCS.z);
                    half4 historyCol = SAMPLE_TEXTURE2D(_HistoryFogTexture, sampler_HistoryFogTexture, lastUV);
                    half history = HistoryClamp(lastUV, uv, depth, input.viewDir);
                    col = lerp(col, historyCol, history);
                }
				half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);
               
               	// Modify the sampled color
               	return col;
            }
            ENDHLSL
        }
    }
}