Shader "bard/role/simple(fresnel)"
{
    Properties
    {
		[Header(OutLine)]
    	[Toggle]_UseSmoothNormal("Use SmoothNormal",float) = 1
		_OutlineColor ("OutLine Color", Color) = (0.106,0.0902,0.0784,1)
		_OutlineWidth("Outline Width", Range (0, 1)) = .07
    	
		[Header(Base)]
    	[HDR]_BaseColor("Base Color",color) = (1,1,1,1)
    	[HDR]_RimColor("Rim Color",color) = (1,1,1,1)
	    _RimRangeStep("Shadow Range Step", Range(0, 1)) = 0.3
		_RimFeather("Rim Feather", Range(0.001,1)) = 0.3
    	_EmissionStrength("Emission Strength", float) = 1
    	
    	//死亡溶解 
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
		[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
    	
        [HideInInspector]_ShadowInvLen("Shadow InvLen",Range(0,1)) = 1
        [HideInInspector]_ShadowOffset("Shadow Offset(Y)",Range(-3,3)) = 0
        [HideInInspector]_PlaneShadowColor("Plane Shadow Color",color) = (0,0,0,1)
    	[HideInInspector]_WorldPos("WorldPos",vector) = (1,1,1,1)
    	[HideInInspector]_ShadowPlane("ShadowPlane",vector) = (1,1,1,1)
    	[HideInInspector]_ShadowFadeParams("ShadowFadeParams",vector) = (1,1,1,1)

    	//冰冻效果
        [HideInInspector]_UseIce("Use Ice",float) = 0
    	[HideInInspector]_IceVFXColor("Ice Color",Color) = (1,1,1,1)
		[HideInInspector]_IceTexture("Ice Tex",2D) = "White"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
    	
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
    	
	    Pass 
    	{
            Name "OutLine"
            Tags{"LightMode" = "OutLine"}
			Cull Front
			HLSLPROGRAM
			#pragma vertex											vert_outline
			#pragma fragment										frag_outline

			uniform sampler2D _NoiseTex;
			uniform TEXTURE2D(_IceTexture);                         uniform SAMPLER(sampler_IceTexture);
			uniform float4											_NoiseTex_ST;
			
			CBUFFER_START(UnityPerMaterial)
			uniform half4                                           _IceVFXColor;
			
			uniform	half 											_OutlineWidth;
			uniform	half4 											_OutlineColor;
			uniform	half											_UseSmoothNormal;
			
			uniform	half											_UseDissolve;
			uniform half                                            _DissolveScale;
			uniform half                                            _EdgeWidth;
			uniform half4                                           _EdgeColor;

			uniform half4											_BaseColor;
			uniform half4											_RimColor;
			uniform half											_RimRangeStep;
			uniform half											_RimFeather;

			uniform half											_EmissionStrength;
			
			uniform half4                                           _ShadowFadeParams;
			uniform half                                            _ShadowInvLen;
			uniform half                                            _ShadowOffset;
			uniform half4                                           _PlaneShadowColor;
			uniform float4                                          _WorldPos;
			uniform half4                                           _ShadowPlane;

			uniform half                                            _UseIce;
			CBUFFER_END
			
			struct outline_data 
			{
				float4 vertex           							: POSITION;
				float3 normal           							: NORMAL;
			    float3 tangent          							: TANGENT;
				float4 color            							: COLOR;
				float2 texcoord0        							: TEXCOORD0;
			};
			
			struct v2f_outline 
			{
				float4 pos            								: SV_POSITION;
				float4 posWS            							: TEXCOORD0;
				float4 color            							: TEXCOORD1;
				float2 uv0              							: TEXCOORD2;
				float4 screenPos        							: TEXCOORD3;
				float4 PosL											: TEXCOORD4;
				float3 normal										: TEXCOORD5;
			};
			
			v2f_outline vert_outline(outline_data v)
			{
				v2f_outline o 										= (v2f_outline)0;
				o.normal											= TransformObjectToWorldNormal(v.normal);
				o.posWS												= mul(unity_ObjectToWorld, v.vertex);
				o.pos												= TransformObjectToHClip(v.vertex);

				half3 aimNormal 									= _UseSmoothNormal * v.tangent + v.normal - _UseSmoothNormal * v.normal;
			    half3 viewNormal                            		= mul((float3x3)UNITY_MATRIX_IT_MV, aimNormal);
				
			    half3 ndcNormal                             		= normalize(mul((float3x3)UNITY_MATRIX_P, viewNormal)) * o.pos.w;//将法线变换到NDC空间

				half3 posWS 					        			= mul(unity_ObjectToWorld,v.vertex);
    			half4 nearUpperRight                    			= mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
    			half aspect                             			= abs(nearUpperRight.y / nearUpperRight.x);//求得屏幕宽高比
    			ndcNormal.xy                            			 *= aspect;
    			half Dis                                			= distance(_WorldSpaceCameraPos.xyz,posWS);
    			half outlineScaleBalance                			= 0.1 * clamp(lerp(1,0.2,clamp((Dis / 7.3),0,1) ),0,0.4) * 0.4;
    			o.pos.xy                                  			+= _OutlineWidth * ndcNormal.xy * v.color.x * outlineScaleBalance;
				o.screenPos 										= ComputeScreenPos(o.pos);
			    o.color 											= v.color;
				o.color.a 											= v.color.x;
			    o.uv0 												= v.texcoord0;
				o.PosL												= v.vertex;
				return												o;
			}

			/// <summary>
			/// 角色冰冻
			/// </summary>
			inline void SetCharacterIce(half2 uv, inout half3 outColor)
			{
			    UNITY_BRANCH
			    if(_UseIce < 0.5)
			        return;
			    
			    half4 IceColor                                  		= SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv ) * pow(_IceVFXColor,0.4545454545);
			    half mask                                               = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;

			    outColor.rgb                                            = lerp(IceColor,outColor,mask);;
			}
			
			half4 frag_outline(v2f_outline i) : COLOR
			{
				UNITY_BRANCH
				if(_UseDissolve > 0.5)
				{
					clip(i.color.x - 2);
					return 0;
				}
				float4 c											= _OutlineColor;
				SetCharacterIce(i.uv0,c.rgb);
				return												c;               
			}
			
			ENDHLSL
		}
    	
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex VS
            #pragma fragment PS

            struct VertexIn
            {
                float4 PosL											: POSITION;
            	float3 NormalL										: NORMAL;
				float2 TexC											: TEXCOORD0;
            };

            struct VertexOut
            {
                float4 PosH											: SV_POSITION;
            	float3 V											: TEXCOORD0;
            	float3 NormalW										: NORMAL;
            	float4 PosS											: TEXCOORD1;
				float2 TexC											: TEXCOORD2;
            };

			uniform half2                                           _ScreenUV;
			uniform sampler2D _NoiseTex;
			uniform TEXTURE2D(_IceTexture);                         uniform SAMPLER(sampler_IceTexture);
			uniform float4											_NoiseTex_ST;
			CBUFFER_START(UnityPerMaterial)
			uniform half4                                           _IceVFXColor;
            
			uniform	half 											_OutlineWidth;
			uniform	half4 											_OutlineColor;
			uniform	half											_UseSmoothNormal;
            
			uniform	half											_UseDissolve;
			uniform half                                            _DissolveScale;
			uniform half                                            _EdgeWidth;
			uniform half4                                           _EdgeColor;
            
			uniform half4											_BaseColor;
			uniform half4											_RimColor;
			uniform half											_RimRangeStep;
			uniform half											_RimFeather;

			uniform half											_EmissionStrength;
            
			uniform half4                                           _ShadowFadeParams;
			uniform half                                            _ShadowInvLen;
			uniform half                                            _ShadowOffset;
			uniform half4                                           _PlaneShadowColor;
			uniform float4                                          _WorldPos;
			uniform half4                                           _ShadowPlane;
            
			uniform half                                            _UseIce;
            
			CBUFFER_END

            
            VertexOut VS (VertexIn vin)
            {
                VertexOut                       					vout;
                vout.PosH                       					= TransformObjectToHClip(vin.PosL);
            	float3 PosW											= TransformObjectToWorld(vin.PosL);
            	vout.NormalW										= TransformObjectToWorldNormal(vin.NormalL);
            	vout.V												= normalize(_WorldSpaceCameraPos.xyz - PosW);
				vout.PosS											= ComputeScreenPos(vout.PosH);
				vout.TexC											= vin.TexC;
                return vout;
            }
            
			inline void CHARACTER_DISSOLVE_SET(half4 uv, inout half3 color)
			{
			    UNITY_BRANCH
			    if(_UseDissolve < 0.5)
			        return;

			    half2 PosS                              = (uv.xy / uv.w) * _ScreenUV;
			    half noiseValue                         = saturate(tex2D(_NoiseTex,PosS).r);

			    if(noiseValue < _DissolveScale)
			        discard;

			    half EdgeFactor                         = saturate((noiseValue - _DissolveScale)/(_EdgeWidth * _DissolveScale));
			    half EdgeArea                           = step(EdgeFactor,0.5);
			    color                                   = lerp(color,_EdgeColor,EdgeArea);
			}
            
			inline half remap(half x, half t1, half t2, half s1, half s2)
			{
				return																(x - t1) / (t2 - t1) * (s2 - s1) + s1;
			}
            
			/// <summary>
			/// 角色冰冻
			/// </summary>
			inline void SetCharacterIce(half2 uv, inout half3 outColor)
			{
			    UNITY_BRANCH
			    if(_UseIce < 0.5)
			        return;
			    
			    half4 IceColor                                  		= SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv ) * pow(_IceVFXColor,0.4545454545);
			    half mask                                               = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;

			    outColor.rgb                                            = lerp(IceColor,outColor,mask);;
			}
            
            half4 PS (VertexOut pin) : SV_Target
            {
				half rimFactor										= pow(1.0 - max(0, saturate(dot(pin.NormalW,pin.V))), 1);
				half Set_RimLightMask								= saturate(((rimFactor - (_RimRangeStep - _RimFeather)) * - 1.0 ) / (_RimRangeStep - (_RimRangeStep - _RimFeather)) + 1 );
				half4 finalColor									= lerp(_RimColor,_BaseColor,Set_RimLightMask) * _EmissionStrength;
				SetCharacterIce(pin.TexC,finalColor.rgb);
				CHARACTER_DISSOLVE_SET(pin.PosS, finalColor.rgb);
            	return												finalColor;
            }
            ENDHLSL
        }
    	
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"					= "ShadowCaster"}

            HLSLPROGRAM
            #pragma vertex						VS_Shadow
            #pragma fragment					PS_Shadow
			#include "character.hlsl"
            ENDHLSL
        }
    }
}
