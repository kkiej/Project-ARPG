Shader "Unlit/Skybox"
{
    Properties
    {
        [HDR]_SunColor ("Sun Color", Color) = (1, 1, 1, 1)
        _SunSize ("Sun Size", Range(0, 1)) = 1
        _SunInnerBound ("Sun Inner Bound", Range(0, 1)) = 0.5
        _SunOuterBound ("Sun Outer Bound", Range(0, 1)) = 1
        [HDR]_SunInfColor ("Sun Inflence Color", Color) = (1, 1, 1, 1)
        _SunInfScale ("Sun Inflence Scale", Float) = 1
        [HDR]_MoonColor ("Moon Color", Color) = (1, 1, 1, 1)
        _MoonTex ("Moon Tex", 2D) = "white" {}
        _MoonSize ("Moon Size", Float) = 1
        _MoonHaloSize ("Moon Halo Size", Float) = 0.5
        [HideInInspector]_PlanetRadius ("Planet Radius", Float) = 0.5
        [HideInInspector]_AtmosphereHeight ("Atmosphere Height", Float) = 1
        _MieColor ("Mie Color", Color) = (1, 1, 1, 1)
        _MieStrength ("Mie Strength", Float) = 1
        _DayTopColor ("Day Top Color", Color) = (1, 1, 1, 1)
        _DayMidColor ("Day Mid Color", Color) = (1, 1, 1, 1)
        _DayBottomColor ("Day Bottom Color", Color) = (1, 1, 1, 1)
        _NightTopColor ("Night Top Color", Color) = (1, 1, 1, 1)
        _NightBottomColor ("Night Bottom Color", Color) = (1, 1, 1, 1)
        _DayHorColor ("Day Hor Color", Color) = (1, 1, 1, 1)
        _DayHorWidth ("Day Hor Width", Float) = 1
        _DayHorStrength ("Day Hor Strength", Float) = 1
        _NightHorColor ("Night Hor Color", Color) = (1, 1, 1, 1)
        _NightHorWidth ("Night Hor Width", Float) = 1
        _NightHorStrength ("Night Hor Strength", Float) = 1
    	[HideInInspector]_DensityScaleHeight ("Density Scale Height", Vector) = (1, 1, 1, 0)
    	[HideInInspector]_MieG ("Mie G", Float) = 1
    	[HideInInspector]_ExtinctionM ("Extinction Mie", Vector) = (1, 1, 1, 0)
    	[HideInInspector]_ScatteringM (" Scattering Mie", Vector) = (1, 1, 1, 0)
    	[Header(Galaxy)] _GalaxyNoiseTex ("Galaxy Noise Tex", 2D) = "white" {}
    	_GalaxyTex ("Galaxy Tex", 2D) = "white" {}
    	_GalaxyColor ("First Galaxy Color", Color) = (1, 1, 1, 1)
    	_GalaxyColor1 ("Second Galaxy Color", Color) = (1, 1, 1, 1)
    	[Header(Star)] _StarTex ("Star Tex", 2D) = "white" {}
    	_StarNoise3D ("Star Noise", 3D) = "white" {}
    	_CloudHighTex ("Cloud High Tex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" "Queue"="Background" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		
        CBUFFER_START(UnityPerMaterial)
        half4 _SunColor;
        half _SunSize;
        half _SunInnerBound;
        half _SunOuterBound;
        half4 _SunInfColor;
        half _SunInfScale;
        half4 _MoonColor;
        float4 _MoonTex_ST;
        half _MoonSize;
        half _MoonHaloSize;
        half _PlanetRadius;
        half _AtmosphereHeight;
        half4 _MieColor;
        half _MieStrength;
        half4 _DayTopColor;
        half4 _DayMidColor;
        half4 _DayBottomColor;
        half4 _NightTopColor;
        half4 _NightBottomColor;
        half4 _DayHorColor;
        half _DayHorWidth;
        half _DayHorStrength;
        half4 _NightHorColor;
        half _NightHorWidth;
        half _NightHorStrength;
        half4 _DensityScaleHeight;
        half _MieG;
        half4 _ExtinctionM;
        half4 _ScatteringM;
        float4 _GalaxyNoiseTex_ST;
        float4 _GalaxyTex_ST;
        half4 _GalaxyColor;
        half4 _GalaxyColor1;
        float4 _StarTex_ST;
        float4 _StarNoise3D_ST;
        float4 _CloudHighTex_ST;
        CBUFFER_END
        
        ENDHLSL
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MoonTex);
            SAMPLER(sampler_MoonTex);
            TEXTURE2D(_GalaxyNoiseTex);
            SAMPLER(sampler_GalaxyNoiseTex);
            TEXTURE2D(_GalaxyTex);
            SAMPLER(sampler_GalaxyTex);
            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);
            TEXTURE3D(_StarNoise3D);
            SAMPLER(sampler_StarNoise3D);
            TEXTURE2D(_CloudHighTex);
            SAMPLER(sampler_CloudHighTex);
            float4x4 _LToW;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float2 RaySphereIntersection(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius)
			{
				rayOrigin -= sphereCenter;
				float a = dot(rayDir, rayDir);
				float b = 2.0 * dot(rayOrigin, rayDir);
				float c = dot(rayOrigin, rayOrigin) - (sphereRadius * sphereRadius);
				float d = b * b - 4 * a * c;
				if (d < 0)
				{
					return -1;
				}
				else
				{
					d = sqrt(d);
					return float2(-b - d, -b + d) / (2 * a);
				}
			}
            
			void ComputeOutLocalDensity(float3 position, out float localDPA, out float DCP)
			{
				float3 planetCenter = float3(0, -_PlanetRadius, 0);
				float height = distance(position, planetCenter) - _PlanetRadius;
				localDPA = exp(-(height / _DensityScaleHeight.x));
				
				DCP = 0;
			}

            float MiePhaseFunction(float cosAngle)
            {
	            float g = _MieG;
            	float g2 = g * g;
            	float phase = (1 / (4 * PI)) * ((3 * (1 - g2)) / (2 * (2 + g2))) * ((1 + cosAngle * cosAngle) / (pow((1 + g2 - 2 * g * cosAngle), 3 / 2)));
            	return  phase;
            }
            
			float4 IntegrateInscattering(float3 rayStart,float3 rayDir,float rayLength, float3 lightDir,float sampleCount)
			{
				float3 stepVector = rayDir * (rayLength / sampleCount);
				float stepSize = length(stepVector);
			
				float scatterMie = 0;
			
				float densityCP = 0;
				float densityPA = 0;
				float localDPA = 0;
			
				float prevLocalDPA = 0;
				float prevTransmittance = 0;
				
				ComputeOutLocalDensity(rayStart, localDPA, densityCP);
				
				densityPA += localDPA*stepSize;
				prevLocalDPA = localDPA;
			
				float Transmittance = exp(-(densityCP + densityPA) * _ExtinctionM.x) * localDPA;
				
				prevTransmittance = Transmittance;
			
				for(float i = 1.0; i < sampleCount; ++i)
				{
					float3 P = rayStart + stepVector * i;
					
					ComputeOutLocalDensity(P, localDPA, densityCP);
					densityPA += (prevLocalDPA + localDPA) * stepSize / 2;
			
					Transmittance = exp(-(densityCP + densityPA) * _ExtinctionM) * localDPA;
			
					scatterMie += (prevTransmittance + Transmittance) * stepSize / 2;
					
					prevTransmittance = Transmittance;
					prevLocalDPA = localDPA;
				}
			
				scatterMie *= MiePhaseFunction(dot(rayDir, -lightDir.xyz));
			
				float3 lightInscatter = _ScatteringM * scatterMie;
			
				return float4(lightInscatter,1);
			}

            //void ComputeLocalInscattering(float2 localDensity, float2 densityPA, float2 densityCP, out float3 localInscatterR, out float3 localInscatterM)
			//{
			//	float2 densityCPA = densityCP + densityPA;
			//
			//	float3 Tr = densityCPA.x * _ExtinctionR;
			//	float3 Tm = densityCPA.y * _ExtinctionM;
			//
			//	float3 extinction = exp(-(Tr + Tm));
			//
			//	localInscatterR = localDensity.x * extinction;
			//	localInscatterM = localDensity.y * extinction;
			//}

            float3 ACESFilm(float3 x)
			{
				float a = 2.51f;
				float b = 0.03f;
				float c = 2.43f;
				float d = 0.59f;
				float e = 0.14f;
				return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
			}
			
            half4 frag(v2f i):SV_Target
            {                
                float verticalPos = i.uv.y * 0.5 + 0.5;
                float sunNightStep = smoothstep(-0.3, 0.25, _MainLightPosition.y);
                float3 sunUV = mul(i.uv.xyz, _LToW);
                
                //SUN
                //dist of skybox suface pixel and sun point
                float sunDist = distance(i.uv.xyz, _MainLightPosition.xyz);
	            float sunArea = 1 - sunDist / _SunSize;
	            sunArea = smoothstep(_SunInnerBound, _SunOuterBound, sunArea);
                float3 fallSunColor = _SunColor.rgb * 0.4;
                float3 finalSunColor = lerp(fallSunColor, _SunColor.rgb, smoothstep(-0.03, 0.03, _MainLightPosition.y)) * sunArea;
                
                //MOON
                float2 moonUV = sunUV.xy * _MoonTex_ST.xy * (1 / _MoonSize + 0.001) + _MoonTex_ST.zw;
                float4 moonTex = SAMPLE_TEXTURE2D(_MoonTex, sampler_MoonTex, moonUV);
                float moonHaloArea = max(0, 1 - length((moonUV - 0.5) * 1 / _MoonHaloSize));
                float lerpmoonHaloArea = lerp(0, moonHaloArea, smoothstep(0.02, 0.1, -_MainLightPosition.y));
                                
                //step z 两个月亮问题
                float3 finalMoonColor = (_MoonColor * moonTex.rgb * moonTex.a) * step(0, sunUV.z) * lerpmoonHaloArea;
                            	
                //sunInfluence
                float sunMask2 = smoothstep(-0.4, 0.4, -mul(i.uv.xyz, _LToW).z) - 0.3;
                float sunInfScaleMask = smoothstep(-0.01, 0.1,_MainLightPosition.y) * smoothstep(-0.4, -0.01, -_MainLightPosition.y);
                float3 finalSunInfColor = _SunInfColor * sunMask2 * _SunInfScale * sunInfScaleMask;
                
                //Mie scattering
                float3 scatteringColor = 0;
                
                float3 rayStart = float3(0, 10, 0);
                rayStart.y = saturate(rayStart.y);
                float3 rayDir = normalize(i.uv.xyz);
                
                float3 planetCenter = float3(0, -_PlanetRadius, 0);
                float2 intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius + _AtmosphereHeight);
                float rayLength = intersection.y;
                
                intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius);
	            if (intersection.x > 0)
	            	rayLength = min(rayLength, intersection.x * 1000);
				
                float4 inscattering = IntegrateInscattering(rayStart, rayDir, rayLength, -_MainLightPosition.xyz, 16);
                //scatteringColor = _MieColor * _MieStrength * ACESFilm(inscattering);
                scatteringColor = _MieColor * _MieStrength * inscattering;
                
                //DAY NIGHT
                float3 gradientDay = lerp(_DayBottomColor, _DayMidColor, saturate(i.uv.y)) * step(0, -i.uv.y)
                                    + lerp(_DayMidColor, _DayTopColor, saturate(i.uv.y)) * step(0, i.uv.y);
                
	            float3 gradientNight = lerp(_NightBottomColor, _NightTopColor, verticalPos);
	            float3 skyGradients = lerp(gradientNight, gradientDay, sunNightStep);
                
                //HORIZONTAL
                float horWidth = lerp(_NightHorWidth, _DayHorWidth, sunNightStep);
                float horStrenth = lerp(_NightHorStrength, _DayHorStrength, sunNightStep);
                float horLineMask = smoothstep(-horWidth, 0, i.uv.y) * smoothstep(-horWidth, 0, -i.uv.y);
                float3 horLineGradients = lerp(_NightHorColor, _DayHorColor, sunNightStep).rgb;

            	//FINAL
                float3 finalSkyColor = skyGradients * (1 - horLineMask) + horLineGradients * horLineMask * horStrenth;
            	
                //GALAXY
                float4 galaxyNoiseTex = SAMPLE_TEXTURE2D(_GalaxyNoiseTex, sampler_GalaxyNoiseTex, i.uv.xz * _GalaxyNoiseTex_ST.xy + _GalaxyNoiseTex_ST.zw + float2(0, _Time.x * 0.15));
                    
                float4 galaxy = SAMPLE_TEXTURE2D(_GalaxyTex, sampler_GalaxyTex, (i.uv.xz + (galaxyNoiseTex - 0.5) * 0.3) * _GalaxyTex_ST.xy + _GalaxyTex_ST.zw);
                
                float4 galaxyColor = (_GalaxyColor * (-galaxy.r + galaxy.g) + _GalaxyColor1 * galaxy.r) * smoothstep(0, 0.2, 1 - galaxy.g);
                
                galaxyNoiseTex = SAMPLE_TEXTURE2D(_GalaxyNoiseTex, sampler_GalaxyNoiseTex, i.uv.xz * _GalaxyNoiseTex_ST.xy + _GalaxyNoiseTex_ST.zw - float2(_Time.x * 0.2, _Time.x * 0.1));
                galaxy = SAMPLE_TEXTURE2D(_GalaxyTex, sampler_GalaxyTex, (i.uv.xz + (galaxyNoiseTex - 0.5) * 0.3) * _GalaxyTex_ST.xy + _GalaxyTex_ST.zw);
                
                galaxyColor += (_GalaxyColor * (-galaxy.r + galaxy.g) + _GalaxyColor1 * galaxy.r) * smoothstep(0, 0.3, 1 - galaxy.g);
                galaxyColor *= 0.5;
                
                //STAR
                float4 starTex = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, i.uv.xz * _StarTex_ST.xy + _StarTex_ST.zw);
                float4 starNoiseTex = SAMPLE_TEXTURE3D(_StarNoise3D, sampler_StarNoise3D, i.uv.xyz * _StarNoise3D_ST.x + _Time.x * 0.2);
                
                float starPos = smoothstep(0.21, 0.31, starTex.r);
                float starBright = smoothstep(0.5, 0.64, starNoiseTex.r);

            	float starAreaMask = (galaxy.r * 4 + (1 - galaxy.r) * 0.1) * starBright;
                float starColor = starPos * starAreaMask;
            	
                //float starMask = lerp((1 - smoothstep(-0.7, -0.2, -i.uv.y)), 0, sunNightStep) * step(lerpmoonHaloArea, 0);
                float starMask = lerp((1 - smoothstep(-0.7, -0.2, -i.uv.y)), 0, sunNightStep);
                
                float3 finalColor = finalSunColor + finalSunInfColor + finalMoonColor + finalSkyColor + (starColor + galaxyColor) * starMask + scatteringColor;

            	//Cloud
                //float cosRh = cos(_CloudRotate + _CloudHighSpeed * _Time.x);
                //float sinRh = sin(_CloudRotate + _CloudHighSpeed * _Time.x);
                //float4x4 rotateh = float4x4(float4(cosRh, 0, sinRh, 0), float4(0, 1, 0, 0), float4(-sinRh, 0, cosRh, 0), float4(0, 0, 0, 1));
                //float2 rotateUVh = mul(i.uv.xz, rotateh);
                //float4 cloudHighTex = SAMPLE_TEXTURE2D(_CloudHighTex, sampler_CloudHighTex, rotateUVh * _CloudHighTex_ST.xy + _CloudHighTex_ST.zw);
            	half2 highCloudUV = TRANSFORM_TEX(i.uv.xy, _CloudHighTex);
				highCloudUV.x = i.uv.z < 0 ? 1 - highCloudUV.x : highCloudUV.x;
				highCloudUV.x += _Time / 2;
				highCloudUV.y = smoothstep(0.4, 0.8, highCloudUV.y);
				half4 highCloudTex = SAMPLE_TEXTURE2D(_CloudHighTex, sampler_CloudHighTex, highCloudUV);
				//highCloudTex = smoothstep(0.04, 0.1, highCloudTex) * abs(i.uv.z);
                
            	finalColor += highCloudTex.r * saturate(i.uv.y * 0.5);
                
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}
