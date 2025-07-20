Shader "bard/role/simpleLit"
{
    Properties
    {
        _MainTex("MainTex",2D) = "White"{}
        _OutColor("OutLine Color",Color) = (1,1,1,1)
		_Width("Outline Width", Range (0, 1)) = .07
        _ShadowRangeStep("Shadow Range Step",Range(0,1)) = 0.8
        _ShadowFeather("Shadow Feather",Range(0.001,1)) = 0.3
        _ShadowColor("Shadow Color",color) = (1,1,1,1)

        [HideInInspector] _LightColor("Light Color",Color) = (1,1,1,1)
    	//死亡溶解
	    [HideInInspector]_NoiseTex("NoiseTex",2D) = "white"{}
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
		[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
    	
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
        [HideInInspector] _Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
    	
    	[HideInInspector] _UseUnityFog("Use Unity Fog",float) = 1
    }
    
    SubShader
    {
    
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
    	
    	Stencil
	    {
	        Ref 2
	        Comp Always
	        Pass Replace 
	    } 
    	
        Pass 
    	{
            Name "OutLine"
            Tags{"LightMode"					= "OutLine"}
			Cull Front

            HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            
			uniform TEXTURE2D(_MainTex);     uniform SAMPLER(sampler_MainTex);
			uniform TEXTURE2D(_IceTexture);  uniform SAMPLER(sampler_IceTexture);
			uniform sampler2D _NoiseTex;
            
            uniform half2   _ScreenUV;
			uniform half    _UseWhiteBalance;
			uniform half3   _kRGB;
			uniform half    _ToonLightStrength;
            CBUFFER_START(UnityPerMaterial)
			half _Width;
			half4 _OutColor;
            uniform half _ShadowRangeStep;
            uniform half _ShadowFeather;
            uniform half4 _ShadowColor;
            
			uniform float4  _NoiseTex_ST;
			uniform half    _UseDissolve;
			uniform half    _DissolveScale;
			uniform half    _EdgeWidth;
			uniform half4   _EdgeColor;
            uniform half    _Transparency;
			uniform half4   _LightColor;
            uniform half    _UseIce;
			uniform half    _UseTransparentMask;
            uniform	half4	_IceVFXColor;
            uniform	half	_UseUnityFog;
            CBUFFER_END
            
			struct a2v 
            {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
            	float2 texcoord : TEXCOORD0;
			}; 
			
			struct v2f
            {
			    float4 pos : SV_POSITION;
            	float4 texcoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
            {
				v2f o;
				v.vertex.xyz += normalize(v.normal) * _Width * 0.1;
				o.pos = TransformObjectToHClip(v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				o.texcoord.xy = v.texcoord;
				o.texcoord.w = ComputeFogFactor(o.pos.z);
				return o;
			}

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
            
			float4 frag(v2f i) : SV_Target
            {
            	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,i.screenPos);
            	UNITY_BRANCH
				if(_UseDissolve > 0.5)
				{
					clip(_OutColor.r - 2);
				}

            	SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,_OutColor);
            	SetCharacterIce(_UseIce,i.texcoord,1,_OutColor);

            	//---------- 距离雾 ----------
		
				SetCharacterUnityFog(i.texcoord.w, _OutColor);
            	
				float4 c = _OutColor * _LightColor;
				return c;               
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
                float4 PosL                     : POSITION;
                float2 TexC                     : TEXCOORD0;
                float3 NormalL                  : NORMAL;
            };

            struct VertexOut
            {
                float4 PosH                     : SV_POSITION;
                float4 TexC                     : TEXCOORD0;
                float3 NormalW                  : TEXCOORD1;
            	float4 PosS						: TEXCOORD2;
            };
			uniform TEXTURE2D(_MainTex);     uniform SAMPLER(sampler_MainTex);
			uniform TEXTURE2D(_IceTexture);  uniform SAMPLER(sampler_IceTexture);
			uniform sampler2D _NoiseTex;
            
            uniform half2   _ScreenUV;
			uniform half    _UseWhiteBalance;
			uniform half3   _kRGB;
			uniform half    _ToonLightStrength;
            uniform half3	_DirectionLightPos;
            CBUFFER_START(UnityPerMaterial)
			half _Width;
			half4 _OutColor;
            uniform half _ShadowRangeStep;
            uniform half _ShadowFeather;
            uniform half4 _ShadowColor;
            
			uniform float4  _NoiseTex_ST;
			uniform half    _UseDissolve;
			uniform half    _DissolveScale;
			uniform half    _EdgeWidth;
			uniform half4   _EdgeColor;
            uniform half    _Transparency;
			uniform half4   _LightColor;
            uniform half    _UseIce;
			uniform half    _UseTransparentMask;
            uniform	half4	_IceVFXColor;
            uniform	half	_UseUnityFog;
            CBUFFER_END
            
            VertexOut VS (VertexIn vin)
            {
                VertexOut                       vout;
                vout.PosH                       = TransformObjectToHClip(vin.PosL);
                vout.NormalW                    = TransformObjectToWorldNormal(vin.NormalL);
            	vout.PosS						= ComputeScreenPos(vout.PosH);
                vout.TexC.xy                    = vin.TexC;
				vout.TexC.w						= ComputeFogFactor(vout.PosH.z);
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
            
            half4 PS (VertexOut pin) : SV_Target
            {
            	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.PosS);
            	
                half4 color                     = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.TexC);
                half3 Light                     = normalize(_DirectionLightPos);
                half3 NormalW                   = normalize(pin.NormalW);
                half NoL                        = saturate(dot(Light,NormalW) * 0.5 + 0.5);
                half shadow                     = saturate(((NoL - (_ShadowRangeStep-_ShadowFeather)) * - 1.0 ) / (_ShadowRangeStep - (_ShadowRangeStep-_ShadowFeather)) + 1 );
                color                           = lerp(color,_ShadowColor * color,shadow);

            	SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,color);
            	CHARACTER_DISSOLVE_SET(pin.PosS, color.rgb);
            	SetCharacterIce(_UseIce,pin.TexC,1,color);

            	//---------- 距离雾 ----------
		
				SetCharacterUnityFog(pin.TexC.w, color);
                return color * _LightColor;
            }
            ENDHLSL
        }
    	
		pass 
    	{
			Tags{ "LightMode" = "ShadowCaster" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
			struct appdata
			{
				float4 vertex : POSITION;
			};
 
			struct v2f
			{
				float4 pos : SV_POSITION;
			};
 
			sampler2D _MainTex;
			float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
			half _Width;
			half4 _OutColor;
            uniform half _ShadowRangeStep;
            uniform half _ShadowFeather;
            uniform half4 _ShadowColor;
 			half  _DissolveScale;
			half2 _ScreenUV;
			half  _EdgeWidth;
			half4 _EdgeColor;

			//冰冻效果
			half  _UseIce;
			half4 _IceVFXColor;
            CBUFFER_END
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				return o;
			}
			float4 frag(v2f i) : SV_Target
			{
				float4 color;
				color.xyz = float3(0.0, 0.0, 0.0);
				return color;
			}
			ENDHLSL
		}
    }
}
