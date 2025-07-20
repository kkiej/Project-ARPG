//--------------------------------------------------------------
//-----------------------writer:zh 2020/8/24--------------------
//--------------------------------------------------------------
Shader "bard/role/eye(dove)"
{
    Properties
    {
		_MainTex_1("Base Texture1", 2D) = "white" {}
		_MainTex_2("Base Texture2", 2D) = "white" {}
		_MainTex_3("Base Texture3", 2D) = "white" {}
		_MainTex_4("Base Texture4", 2D) = "white" {}
    	
    	uv_x("UV(x)",float) = 0
    	uv_y("UV(y)",float) = 0
    	_PicIndex("PicInex",float) = 1
    	
		[HideInInspector] _Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
        [HideInInspector] _LightColor("Light Color",Color) = (1,1,1,1)
        [HideInInspector] [Toggle]_UseRoleFlickerEffect("使用黑白闪烁特效",int) = 0
    	
    	[HideInInspector]_UseIce("Use Ice",float) = 0
    	[HideInInspector]_IceVFXColor("Ice Color",Color) = (1,1,1,1)
		[HideInInspector]_NormalMap("NormalMap",2D) = "bump"{}
    	[HideInInspector]_IceNormalScale("IceNormalScale",Range(0,1)) = 1
        [HideInInspector]_IceRimColor("Ice Rim Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceTexture("Ice Tex",2D) = "White"{}
        [HideInInspector]_Ice_Fresnel_Feather("Fresnel Feather",Range(0,1)) = 0.5
        [HideInInspector]_Ice_Fresnel_Step("Fresnel Step",Range(0,1)) = 0.5
    	[HideInInspector]_IceSpecColor("Spec Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceSpecPower("Spec Power",float) = 1
    	
		[HideInInspector] _UseTransparentMask("Use Transparency Clip",float) = 1
    	[HideInInspector] _UseUnityFog("Use Unity Fog",float) = 1
    	//死亡溶解
	    [HideInInspector]_NoiseTex("NoiseTex",2D) = "white"{}
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
		[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
    } 
    
    SubShader
    {
		Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
    	
        pass
        {
            Tags{"LightMode" = "UniversalForward"}
        	Blend SrcAlpha  OneMinusSrcAlpha
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
				float4 uv0              														: TEXCOORD0;
				float4 screenPos        														: TEXCOORD1;
				float3 normal           														: TEXCOORD2;
				float3 posWS																	: TEXCOORD3;
			}; 

			VertexOut_Eye VS_Eye(VertexIn_Eye vin)
			{
				VertexOut_Eye vout 																= (VertexOut_Eye)0;
				vout.uv0.xy 																	= vin.texcoord0;
				vout.pos 																		= TransformObjectToHClip(vin.vertex);
				vout.screenPos 																	= ComputeScreenPos(vout.pos);
				vout.normal 																	= TransformObjectToWorldNormal(vin.normal);
				vout.posWS																		= TransformObjectToWorld(vin.vertex);
				vout.uv0.w																		= ComputeFogFactor(vout.pos.z);
				return vout;
			}

            
			//表情
			uniform TEXTURE2D(_MainTex_1);                                  uniform SAMPLER(sampler_MainTex_1);
			uniform TEXTURE2D(_MainTex_2);                                  uniform SAMPLER(sampler_MainTex_2);
			uniform TEXTURE2D(_MainTex_3);                                  uniform SAMPLER(sampler_MainTex_3);
            uniform TEXTURE2D(_MainTex_4);									uniform SAMPLER(sampler_MainTex_4);
            
			uniform TEXTURE2D(_IceTexture);                                 uniform SAMPLER(sampler_IceTexture);
			uniform sampler2D _NoiseTex;  

			uniform     half2                                               _ScreenUV;
			uniform     half                                                _UseWhiteBalance;
			uniform     half3                                               _kRGB;
			uniform     half                                                _ToonLightStrength;
            
            CBUFFER_START(UnityPerMaterial)
			uniform     float4                                              _NoiseTex_ST;
			uniform     half                                                _UseDissolve;
			uniform     half                                                _DissolveScale;
			uniform     half                                                _EdgeWidth;
			uniform     half4                                               _EdgeColor;
            
			uniform     half                                                _Transparency;
			uniform     half4                                               _LightColor;
            uniform     half                                                _UseIce;
			uniform     half                                                _UseTransparentMask;
            uniform		half4												_IceVFXColor;
			uniform     half                                                uv_x;
			uniform     half                                                uv_y;
            uniform		half												_UseUnityFog;
            uniform		half												_PicIndex;
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
			    half4 iceColor                                  = SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv * half2(0.5,0.5)) * _IceVFXColor;
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

			/// <summary>
			/// 设置角色Unity雾
			/// </summary>
			inline void SetCharacterUnityFog(half fogCoord, inout half4 outColor)
			{
			    UNITY_BRANCH
			    if(_UseUnityFog < 0.5)
			        return;

			    outColor.rgb                                                    = MixFog(outColor.rgb, fogCoord);
			}
            
			half4 PS_Eye(VertexOut_Eye pin) : SV_Target
			{
				CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.screenPos);

				#if defined BLACK_WHITE_ON
					half3 V																		= normalize(_WorldSpaceCameraPos.xyz - pin.posWS.xyz);
					return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,pin.normal, V);
				#endif

				half2 uv  																		= half2(pin.uv0.x + uv_x,pin.uv0.y + uv_y);

				_PicIndex																		= clamp(_PicIndex,1,4);
				half isTex1																		= abs(_PicIndex - 1) > 0.1 ? 0 : 1;
				half isTex2																		= abs(_PicIndex - 2) > 0.1 ? 0 : 1;
				half isTex3																		= abs(_PicIndex - 3) > 0.1 ? 0 : 1;
				half isTex4																		= abs(_PicIndex - 4) > 0.1 ? 0 : 1;
				
				float4 color1 																	= SAMPLE_TEXTURE2D(_MainTex_1,sampler_MainTex_1, uv) * isTex1;
				float4 color2 																	= SAMPLE_TEXTURE2D(_MainTex_2,sampler_MainTex_2, uv) * isTex2;
				float4 color3 																	= SAMPLE_TEXTURE2D(_MainTex_3,sampler_MainTex_3, uv) * isTex3;
				float4 color4 																	= SAMPLE_TEXTURE2D(_MainTex_4,sampler_MainTex_4, uv) * isTex4;
				half4 finalColor																= color1 + color2 + color3 + color4;
				
				finalColor.rgb 																	*= _LightColor.rgb * _ToonLightStrength;

				SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
				SetCharacterIce(_UseIce,pin.uv0,1,finalColor);
				CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.screenPos, finalColor);

				SetCharacterUnityFog(pin.uv0.w, finalColor);
				return finalColor;
			}
            ENDHLSL
        }
    }
}
 