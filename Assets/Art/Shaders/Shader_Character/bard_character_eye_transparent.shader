//--------------------------------------------------------------
//-----------------------writer:zh 2020/8/24--------------------
//--------------------------------------------------------------
Shader "bard/role/eye(transparent)"
{
    Properties
    {
		_MainTex("Base Texture", 2D) = "white" {}
    	[HideInInspector]_MainColor("Color",color) = (1,1,1,1)
    	uv_x("UV(x)",float) = 0
    	uv_y("UV(y)",float) = 0
		[HideInInspector] _Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
        [HideInInspector] _LightColor("Light Color",Color) = (1,1,1,1)
        [HideInInspector] [Toggle]_UseRoleFlickerEffect("使用黑白闪烁特效",int) = 0
    	
	    [HideInInspector] _UseIce("Use Ice",float) = 0
    	
		[HideInInspector] _UseTransparentMask("Use Transparency Clip",float) = 1
    } 
    
    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
    	
        pass
        {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"	
			#pragma vertex																		VS_Eye
			#pragma fragment																	PS_Eye

			struct VertexIn_Eye
			{
				float4 vertex           														: POSITION;
				float2 texcoord0        														: TEXCOORD0;
				float3 normal           														: NORMAL;
			};

			struct VertexOut_Eye
			{
				float4 pos              														: SV_POSITION;
				float2 uv0              														: TEXCOORD0;
				float4 screenPos        														: TEXCOORD1;
				float3 normal           														: TEXCOORD2;
				float3 posWS																	: TEXCOORD3;
			}; 

			VertexOut_Eye VS_Eye(VertexIn_Eye vin)
			{
				VertexOut_Eye vout 																= (VertexOut_Eye)0;
				vout.uv0 																		= vin.texcoord0;
				vout.pos 																		= TransformObjectToHClip(vin.vertex);
				vout.screenPos 																	= ComputeScreenPos(vout.pos);
				vout.normal 																	= TransformObjectToWorldNormal(vin.normal);
				vout.posWS																		= TransformObjectToWorld(vin.vertex);
				return vout;
			}

            
			//表情
			uniform TEXTURE2D(_MainTex);                                    uniform SAMPLER(sampler_MainTex);
			uniform TEXTURE2D(_IceTexture);                                 uniform SAMPLER(sampler_IceTexture);
            uniform TEXTURE2D(_CameraOpaqueTexture);						uniform SAMPLER(sampler_CameraOpaqueTexture);
			uniform sampler2D _NoiseTex;  
			uniform     float4                                              _NoiseTex_ST;

			uniform     half2                                               _ScreenUV;
			uniform     half                                                _UseWhiteBalance;
			uniform     half3                                               _kRGB;
			uniform     half                                                _ToonLightStrength;
			uniform     half                                                _UseDissolve;
			uniform     half                                                _DissolveScale;
			uniform     half                                                _EdgeWidth;
			uniform     half4                                               _EdgeColor;
            
            CBUFFER_START(UnityPerMaterial)
			uniform     half                                                _Transparency;
			uniform     half4                                               _LightColor;
            uniform     half                                                _UseIce;
			uniform     half                                                _UseTransparentMask;
            uniform		half4												_MainColor;
			uniform     half                                                uv_x;
			uniform     half                                                uv_y;
			CBUFFER_END
            
			inline void CHARACTER_MASK_TRANSPARENT_SET(half UseTransparentMask,half Transparency,float4 PosSS)
			{
			    UNITY_BRANCH
			    if(UseTransparentMask < 0.5)
			        return;
			    
			    PosSS.xy                                    = PosSS.xy / PosSS.w;
			    PosSS.xy                                   *= _ScreenParams.xy;

			    //阈值矩阵
			    float4x4 thresholdMatrix ={  
			        1.0 / 17.0,			9.0 / 17.0,			3.0 / 17.0,			11.0 / 17.0,
			        13.0 / 17.0,		5.0 / 17.0,			15.0 / 17.0,		7.0 / 17.0,
			        4.0 / 17.0,			12.0 / 17.0,		2.0 / 17.0,			10.0 / 17.0,
			        16.0 / 17.0,		8.0 / 17.0,			14.0 / 17.0,		6.0 / 17.0
			    };

			    //单位矩阵
			    float4x4 _RowAccess = {
			        1,  0,  0,  0, 
			        0,  1,  0,  0,
			        0,  0,  1,  0, 
			        0,  0,  0,  1 
			    };
			    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
			    clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4)] * _RowAccess[fmod(PosSS.y, 4)]);
			    #else
			    clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4.00055)] * _RowAccess[fmod(PosSS.y, 4.00055)]);
			    #endif
			}

			inline void SetCharacterWhiteBalance(half UseWhiteBalance, half3 kRGB,inout half4 color)
			{
			    UNITY_BRANCH
			    if (UseWhiteBalance < 0.5)
			        return;
			    
			    color.r                                        *= kRGB.r;
			    color.g                                        *= kRGB.g;
			    color.b                                        *= kRGB.b;
			}

			inline half remap(half x, half t1, half t2, half s1, half s2)
			{
				return																(x - t1) / (t2 - t1) * (s2 - s1) + s1;
			}
            
			inline void SetCharacterIce(half UseIce, half2 uv, half strength, inout half4 outColor)
			{
			    UNITY_BRANCH
			    if(UseIce < 0.5)
			        return;
			    
			    half mask                                       = tex2D(_NoiseTex,TRANSFORM_TEX(uv,_NoiseTex)).g;
			    mask                                            = remap(mask,0.85,0.1,1,0.2) * strength;
			    half4 iceColor                                  = SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv * half2(0.5,0.5)) * half4(0.4313726,1.32549,2,1);
			    iceColor                                        = lerp(iceColor,outColor,mask);
			    outColor                                        = iceColor;
			}

			inline void CHARACTER_DISSOLVE_SET(half UseDissolve, half DissolveScale, half EdgeWidth, half4 EdgeColor, half4 uv, inout half4 color)
			{
			    UNITY_BRANCH
			    if(UseDissolve < 0.5)
			        return;

			    half2 PosS                              = (uv.xy / uv.w) * _ScreenUV;
			    half noiseValue                         = saturate(tex2D(_NoiseTex,PosS).r);

			    if(noiseValue < DissolveScale)
			        discard;

			    half EdgeFactor                         = saturate((noiseValue - DissolveScale)/(EdgeWidth * DissolveScale));
			    half EdgeArea                           = step(EdgeFactor,0.5);
			    color                                   = lerp(color,EdgeColor,EdgeArea);
			}

            inline void SetTransparent(float4 PosS,inout half4 color)
			{
			    half2 screenUV                                                  = PosS.xy / PosS.w;
			    half4 opaqueColor											    = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV);
			    color.rgb                                                       = lerp(opaqueColor.rgb,color.rgb,_MainColor.a);
			}
            
			half4 PS_Eye(VertexOut_Eye pin) : SV_Target
			{
				CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.screenPos);

				#if defined BLACK_WHITE_ON
					half3 V																		= normalize(_WorldSpaceCameraPos.xyz - pin.posWS.xyz);
					return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,pin.normal, V);
				#endif

				half2 uv  																		= half2(pin.uv0.x + uv_x,pin.uv0.y + uv_y);
				float4 finalColor 																= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv);
				clip(finalColor.a - 0.5);
				finalColor.rgb 																	*= _LightColor.rgb * _ToonLightStrength;

				SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
				SetCharacterIce(_UseIce,pin.uv0,1,finalColor);
				CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.screenPos, finalColor);
				SetTransparent(pin.screenPos,finalColor);
				return finalColor;
			}
            ENDHLSL
        }
    }
}
 