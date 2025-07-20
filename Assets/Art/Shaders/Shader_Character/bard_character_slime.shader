Shader "bard/role/slime"
{
    Properties
    {
		[Header(OutLine)]
    	[Toggle]_UseSmoothNormal("Use SmoothNormal",float) = 1
		_Outline_Color ("OutLine Color", Color) = (0.106,0.0902,0.0784,1)
		_Outline_Width("Outline Width", Range (0, 1)) = 0.07

		[Header(MatCap)]
        _MatCapMap("MatCap Map", 2D) = "white"{}
    	[HDR]_MainColor("Main Color",color) = (1,1,1,1)
        [HDR]_MatCapColor("MatCap Color", Color) = (1,1,1,1)
    	
		[Header(Dark)]
    	[HDR]_DarkColor("Dark Color",Color) = (1,1,1,1)
	    _DarkRangeStep("Dark Range Step", Range(0, 1)) = 0.3
		_DarkFeather("Dark Feather", Range(0.001,1)) = 0.3
    	
		[Header(Bottom)]
    	[HDR]_BottomColor("Bottom Color",Color) = (1,1,1,1)
	    _BottomRangeStep("Bottom Range Step", Range(0, 1)) = 0.3
		_BottomFeather("Bottom Feather", Range(0.001,1)) = 0.3

		[Header(Rim)]
		[HDR]_RimColor("Rim Color",color) = (1,1,1,1)
	    _RimRangeStep("Rim Range Step", Range(0, 1)) = 0.3
		_RimFeather("Rim Feather", Range(0.001,1)) = 0.3

		[Header(Specular)]
		[HDR]_SpecularColor("Specular Color",color) = (1,1,1,1)
	    _SpecularRangeStep("Specular Range Step", Range(0, 1)) = 0.3
		_SpecularFeather("Specular Feather", Range(0.001,1)) = 0.3

		[Header(VertexOffset)]
    	_VertexOffsetRangeX("VertexOffset Range(X)", Range(0,1)) = 0.3
    	_VertexOffsetRangeY("VertexOffset Range(Y)", Range(0,1)) = 0.3
    	_VertexOffsetStrengthX("VertexOffset Strength(X)",Range(0,1)) = 0.3
    	_VertexOffsetStrengthY("VertexOffset Strength(Y)",Range(0,1)) = 0.3
		_VertexOffsetSpeedX ("VertexOffset Speed(X)", float) = 0.1
		_VertexOffsetSpeedY ("VertexOffset Speed(Y)", float) = 0.1
    	
		//死亡溶解
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
		[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
		[HideInInspector]_UseIce("Use Ice",float) = 0
    	 
    }
    
    SubShader
    {
    
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
        
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
			uniform TEXTURE2D(_MatCapMap);							uniform SAMPLER(sampler_MatCapMap);
            
			uniform float4											_NoiseTex_ST;
			uniform half4                                           _IceVFXColor;
            
            CBUFFER_START(UnityPerMaterial)
            
			uniform	half 											_Outline_Width;
			uniform	half4 											_Outline_Color;
			uniform	half											_UseSmoothNormal;
	
            uniform half4											_MainColor;

            uniform half4											_MatCapColor;
            uniform float4											_MatCapMap_ST;
            
            uniform half4											_DarkColor;
			uniform half											_DarkRangeStep;
			uniform half											_DarkFeather;

			uniform half4											_BottomColor;
			uniform half											_BottomRangeStep;
			uniform half											_BottomFeather;
            
			uniform half4											_RimColor;
			uniform half											_RimRangeStep;
			uniform half											_RimFeather;
            
			uniform half4											_SpecularColor;
			uniform half											_SpecularRangeStep;
			uniform half											_SpecularFeather;

			uniform	half											_UseDissolve;
			uniform half                                            _DissolveScale;
			uniform half                                            _EdgeWidth;
			uniform half4                                           _EdgeColor;
			uniform half                                            _UseIce;

			uniform half 											_VertexOffsetRangeX;
			uniform half 											_VertexOffsetRangeY;
			uniform half											_VertexOffsetStrengthX;
			uniform half											_VertexOffsetStrengthY;
            uniform half											_VertexOffsetSpeedX;
            uniform half											_VertexOffsetSpeedY;
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

            inline void VertexAnimation(inout half3 PosL)
            {
            	half strengthX = saturate(PosL.y + _VertexOffsetRangeX);
            	half strengthY = saturate(PosL.y + _VertexOffsetRangeY);
	            half vertexOffsetX = sin(_Time.y * _VertexOffsetSpeedX) * _VertexOffsetStrengthX * strengthX;
            	half vertexOffsetY = sin(_Time.y * _VertexOffsetSpeedY * 2) * _VertexOffsetStrengthY * strengthY;
            	PosL.x += vertexOffsetX;
            	PosL.y += vertexOffsetY;
            }
            
			v2f_outline vert_outline(outline_data v)
			{
				v2f_outline o 										= (v2f_outline)0;
            	VertexAnimation(v.vertex.xyz);
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
    			o.pos.xy                                  			+= _Outline_Width * ndcNormal.xy * v.color.x * outlineScaleBalance;
				o.screenPos 										= ComputeScreenPos(o.pos);
			    o.color 											= v.color;
				o.color.a 											= v.color.x;
			    o.uv0 												= v.texcoord0;
				o.PosL												= v.vertex;
				return												o;
			}
            
			inline half remap(half x, half t1, half t2, half s1, half s2)
			{
				return																(x - t1) / (t2 - t1) * (s2 - s1) + s1;
			}
            
			/// <summary>
			/// 角色冰冻
			/// </summary>
			inline void SetCharacterIce(half UseIce, half2 uv, half strength, inout half4 outColor)
			{
			    UNITY_BRANCH
			    if(UseIce < 0.5)
			        return;
			    
			    half mask                                       = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;
			    mask                                            = remap(mask,0.85,0.1,1,0.2) * strength;
			    half4 iceColor                                  = SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv * half2(0.5,0.5)) * _IceVFXColor;
			    iceColor                                        = lerp(iceColor,outColor,mask);
			    outColor                                        = iceColor;
			}
            
			half4 frag_outline(v2f_outline i) : COLOR
			{
				UNITY_BRANCH
				if(_UseDissolve > 0.5)
				{
					clip(i.color.x - 2);
					return 0;
				}
				float4 c											= _Outline_Color;
				SetCharacterIce(_UseIce,i.uv0,1,c);
				return												c;               
			}
            ENDHLSL
        }
    	
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex		VS
            #pragma fragment	PS

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
				float4 TexC											: TEXCOORD2;
            };

			uniform sampler2D _NoiseTex;
			uniform TEXTURE2D(_IceTexture);                         uniform SAMPLER(sampler_IceTexture);
			uniform TEXTURE2D(_MatCapMap);							uniform SAMPLER(sampler_MatCapMap);
			uniform half3                                           _DirectionLightPos;
			uniform half2                                           _ScreenUV;
			uniform float4											_NoiseTex_ST;
			uniform half4                                           _IceVFXColor;
            
            CBUFFER_START(UnityPerMaterial)
            
			uniform	half 											_Outline_Width;
			uniform	half4 											_Outline_Color;
			uniform	half											_UseSmoothNormal;
	
            uniform half4											_MainColor;

            uniform half4											_MatCapColor;
            uniform float4											_MatCapMap_ST;
            
            uniform half4											_DarkColor;
			uniform half											_DarkRangeStep;
			uniform half											_DarkFeather;

			uniform half4											_BottomColor;
			uniform half											_BottomRangeStep;
			uniform half											_BottomFeather;
            
			uniform half4											_RimColor;
			uniform half											_RimRangeStep;
			uniform half											_RimFeather;
            
			uniform half4											_SpecularColor;
			uniform half											_SpecularRangeStep;
			uniform half											_SpecularFeather;

			uniform	half											_UseDissolve;
			uniform half                                            _DissolveScale;
			uniform half                                            _EdgeWidth;
			uniform half4                                           _EdgeColor;
			uniform half                                            _UseIce;

			uniform half 											_VertexOffsetRangeX;
			uniform half 											_VertexOffsetRangeY;
			uniform half											_VertexOffsetStrengthX;
			uniform half											_VertexOffsetStrengthY;
            uniform half											_VertexOffsetSpeedX;
            uniform half											_VertexOffsetSpeedY;
            
            CBUFFER_END

            inline void VertexAnimation(inout half3 PosL)
            {
            	half strengthX = saturate(PosL.y + _VertexOffsetRangeX);
            	half strengthY = saturate(PosL.y + _VertexOffsetRangeY);
	            half vertexOffsetX = sin(_Time.y * _VertexOffsetSpeedX) * _VertexOffsetStrengthX * strengthX;
            	half vertexOffsetY = sin(_Time.y * _VertexOffsetSpeedY * 2) * _VertexOffsetStrengthY * strengthY;
            	PosL.x += vertexOffsetX;
            	PosL.y += vertexOffsetY;
            }
            
            VertexOut VS (VertexIn vin)
            {
                VertexOut                       					vout;
            	VertexAnimation(vin.PosL.xyz);
                vout.PosH                       					= TransformObjectToHClip(vin.PosL);
            	float3 PosW											= TransformObjectToWorld(vin.PosL);
            	vout.NormalW										= TransformObjectToWorldNormal(vin.NormalL);
            	vout.V												= normalize(_WorldSpaceCameraPos.xyz - PosW);
				vout.PosS											= ComputeScreenPos(vout.PosH);
				vout.TexC.xy										= vin.TexC;
			    vout.TexC.z                                         = dot(normalize(UNITY_MATRIX_IT_MV[0].xyz), normalize(vin.NormalL));
			    vout.TexC.w                                         = dot(normalize(UNITY_MATRIX_IT_MV[1].xyz), normalize(vin.NormalL));
                return vout;
            }
            
			inline void CHARACTER_DISSOLVE_SET(half4 uv, inout half4 color)
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
			inline void SetCharacterIce(half UseIce, half2 uv, half strength, inout half4 outColor)
			{
			    UNITY_BRANCH
			    if(UseIce < 0.5)
			        return;
			    
			    half mask                                       = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;
			    mask                                            = remap(mask,0.85,0.1,1,0.2) * strength;
			    half4 iceColor                                  = SAMPLE_TEXTURE2D(_IceTexture,sampler_IceTexture,uv * half2(0.5,0.5)) * _IceVFXColor;
			    iceColor                                        = lerp(iceColor,outColor,mask);
			    outColor                                        = iceColor;
			}
            
			half4 PS (VertexOut pin) : SV_Target
            {
            	half4 finalColor								= _MainColor;

            	//	Direction Lighting
            	half NoL										= saturate(dot(pin.NormalW,_DirectionLightPos) * 0.5 + 0.5);
				half ShadowMask									= saturate(((NoL - (_DarkRangeStep-_DarkFeather)) * - 1.0 ) / (_DarkRangeStep - (_DarkRangeStep-_DarkFeather)) + 1 );
				finalColor										= lerp(finalColor,_DarkColor,ShadowMask);

            	//	Bottom Lighting
				half bottomFactor                              	= saturate(dot(pin.NormalW,half3(0,-1,0)) * 0.5 + 0.5);
				half BottomMask                            		= 1 - saturate(((bottomFactor - (_BottomRangeStep - _BottomFeather)) * - 1.0 ) / (_BottomRangeStep - (_BottomRangeStep - _BottomFeather)) + 1 );
            	finalColor										= lerp(finalColor,_BottomColor,BottomMask);

            	//	MatCap
				half matCapMask                                 = (SAMPLE_TEXTURE2D(_MatCapMap,sampler_MatCapMap, pin.TexC.zw * _MatCapMap_ST.xy * half2(0.5,0.5) + _MatCapMap_ST.zw + half2(0.5,0.5)) * _MatCapColor).r;
				finalColor										= lerp(_MatCapColor,finalColor,matCapMask);
            	
            	//	Rim Lighting
				half rimFactor                              	= pow(1 - saturate(dot(pin.NormalW,pin.V) * 0.5 + 0.5),1);
				half rimMask                            		= 1 - saturate(((rimFactor - (_RimRangeStep - _RimFeather)) * - 1.0 ) / (_RimRangeStep - (_RimRangeStep - _RimFeather)) + 1 );
            	finalColor									   += rimMask * _RimColor;

            	//	Specular Lighting
				half NoH										= saturate(dot(pin.NormalW,normalize(pin.V + _DirectionLightPos)) * 0.5 + 0.5);
            	half specMask									= 1 - saturate(((NoH - (_SpecularRangeStep - _SpecularFeather)) * - 1.0 ) / (_SpecularRangeStep - (_SpecularRangeStep - _SpecularFeather)) + 1 );
				finalColor									   += _SpecularColor * specMask;

            	
				SetCharacterIce(_UseIce,pin.TexC,1,finalColor);
				CHARACTER_DISSOLVE_SET(pin.PosS, finalColor);
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
