//--------------------------------------------------------------
//-----------------------writer:zh 2020/8/24--------------------
//--------------------------------------------------------------
Shader "bard/role/eye"
{
    Properties
    {
        [Header(BaseTex)]
		_MainTex("Base Texture", 2D) = "white" {}
    	uv_x("UV(x)",float) = 0
    	uv_y("UV(y)",float) = 0
		
        [Header(Texteure(02))]
    	[Toggle(_USE_MAIN_TEXTURE_LERP)]USE_MAIN_TEXTURE_LERP("Texteure(02)插值",int) = 0
    	_MainTex02("Base Texture(02)", 2D) = "white"{}
		_Dissolve_power1("溶解强度", Range( 0 , 1)) = 0
		_Dissolve_power2("亮边范围", Range( 0 , 1)) = 0.65
		[HDR]_EdgeColor2("亮边颜色", Color) = (0,0.438499,1,1)
    	
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
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
    	
        pass
        {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
			#pragma multi_compile_fog
            #pragma shader_feature _ _USE_MAIN_TEXTURE_LERP
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

			#if defined(_USE_MAIN_TEXTURE_LERP)
			uniform TEXTURE2D(_MainTex02);                                  uniform SAMPLER(sampler_MainTex02);
			#endif
            
			//表情
			uniform TEXTURE2D(_MainTex);                                    uniform SAMPLER(sampler_MainTex);
			uniform TEXTURE2D(_IceTexture);                                 uniform SAMPLER(sampler_IceTexture);
			uniform sampler2D _NoiseTex;  

			uniform     half2                                               _ScreenUV;
			uniform     half                                                _UseWhiteBalance;
			uniform     half3                                               _kRGB;
			uniform     half                                                _ToonLightStrength;
            
            CBUFFER_START(UnityPerMaterial)
			uniform     half                                                _Dissolve_power1;
			uniform     half                                                _Dissolve_power2;
			uniform     half4                                               _EdgeColor2;
            
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

			/// <summary>
			/// SIMPLE NOISE 
			/// </summary>
			inline half2 hash_simple( half2 p ) // replace this by something better
			{
				p																	= half2( dot(p,half2(127.1,311.7)), dot(p,half2(269.5,183.3)) );
				return																-1.0 + 2.0*frac(sin(p)*43758.5453123);
			}
            
			/// <summary>
			/// SIMPLEX
			/// </summary>
			inline half SimpleX_Noise( in half2 p )
			{
			    const half K1 														= 0.366025404; // (sqrt(3)-1)/2;
			    const half K2 														= 0.211324865; // (3-sqrt(3))/6;

				half2  i 															= floor( p + (p.x+p.y)*K1 );
			    half2  a 															= p - i + (i.x+i.y)*K2;
			    half   m 															= step(a.y,a.x); 
			    half2  o 															= half2(m,1.0-m);
			    half2  b 															= a - o + K2;
				half2  c 															= a - 1.0 + 2.0*K2;
			    half3  h 															= max( 0.5-half3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
				half3  n 															= h*h*h*h*half3( dot(a,hash_simple(i+0.0)), dot(b,hash_simple(i+o)), dot(c,hash_simple(i+1.0)));
			    return																dot(n, half3(70,70,70) );
			}
            
			half4 PS_Eye(VertexOut_Eye pin) : SV_Target
			{
				CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.screenPos);

				#if defined BLACK_WHITE_ON
					half3 V																		= normalize(_WorldSpaceCameraPos.xyz - pin.posWS.xyz);
					return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,pin.normal, V);
				#endif

				half2 uv  																		= half2(pin.uv0.x + uv_x,pin.uv0.y + uv_y);

			    #if defined(_USE_MAIN_TEXTURE_LERP)
			    half2 screenPos = pin.screenPos.xy / pin.screenPos.w;
			    float noiseValue = SimpleX_Noise(screenPos * half2(32,16)) * 0.5 + 0.5;
			    half EdgeFactor								= saturate((noiseValue - _Dissolve_power1)/(_Dissolve_power2 * _Dissolve_power1));

			    float4 finalColor							= (noiseValue > _Dissolve_power1) ? SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) : SAMPLE_TEXTURE2D(_MainTex02, sampler_MainTex, uv);
			    half EdgeArea								= step(_Dissolve_power1, noiseValue) * step(EdgeFactor, 0.5);
			    finalColor									= _EdgeColor2 * EdgeArea + (1 - EdgeArea) * finalColor;
			    #else
			    float4 finalColor                               = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
			    #endif

				clip(finalColor.a - 0.5);
				finalColor.rgb 																	*= _LightColor.rgb * _ToonLightStrength;

				SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
				SetCharacterIce(_UseIce,pin.uv0,1,finalColor);
				CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.screenPos, finalColor);
				//---------- 距离雾 ----------
		
				SetCharacterUnityFog(pin.uv0.w, finalColor);
				return finalColor;
			}
            ENDHLSL
        }
    }
}
 