/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : Colorful.hlsl                            		   *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2021/1/20                                       *
 *                                                                                             *
 *---------------------------------------------------------------------------------------------*
 * Functions:                                                                                  *
 *   1.0 luminance                                                                       	   *
 *   2.0 mod                                                                      			   *
 *   3.0 ROT                                                                 				   *
 *   4.0 SIMPLE NOISE                                                               		   *
 *   5.0 SIMPLE NOISE FRACLESS                                                                 *
 *   6.0 INV LERP                                                        					   *
 *   7.0 PIXELATE                                                                              *
 *   8.0 BARREL DISTORTION                                                                     *
 *   9.0 HSVtoRGB                                                            				   *
 *  10.0 RGBtoHSV                                                                              *
 *  11.0 RGBtoHUE                                                                              *
 *  12.0 HUEtoRGB                                                                              *
 *  13.0 RGBtoYUV                                                                              *
 *  14.0 YUVtoRGB                                                                              *
 *  15.0 RGBtoCMYK                                                                             *
 *  16.0 CMYKtoRGB                                                                             * 
 *  17.0 sRGB                                                                                  *
 *  18.0 Linear                                                                                *
 *  19.0 CheapContrast_RGB                                                                     *
 *  20.0 CheapBright_RGB                                                                       *
 *  21.0 FastAdjustHsv                                                                         *
 *  22.0 OverlayBlendMode                                                                      *
 *  23.0 OverlayBlendRGB                                                                       *
 *  24.0 OverlayFromPS                                                                         *
 *  25.0 SoftLightFromPS                                                                       *
 *  26.0 HardLightFromPS                                                                       *
 *  27.0 LightModelFromPS                                                                      *
 *  28.0 SpotLightModelFromPS                                                                  *
 *  29.0 ColorFilterFromPS                                                                     *
 *  30.0 ColorDeepenedFromPS                                                                   *
 *  31.0 ColorFadeFromPS                                                                       *
 * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#include "SimplexNoise3D.hlsl"
#define PI 3.14159265 

/// <summary>
/// LUMINANCE
/// </summary>
half luminance(half3 color) 
{
	return dot(color, half3(0.222, 0.707, 0.071));
}

/// <summary>
/// OPENGL MOD
/// </summary>
half3 mod(half3 x, half3 y)
{
	return x - y * floor(x / y);
}

half2 mod(half2 x, half2 y)
{
	return x - y * floor(x / y); 
}

half mod(half x, half y)
{
	return x - y * floor(x / y);
}

/// <summary>
/// ROT
/// </summary>
half rot(half value, half low, half hi)
{
	return (value < low) ? value + hi : (value > hi) ? value - hi : value;
}

half rot10(half value)
{
	return rot(value, 0.0, 1.0);
}

/// <summary>
/// SIMPLE NOISE
/// </summary>
float simpleNoise(half2 uv)
{
	return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

/// <summary>
/// SIMPLE NOISE FRACLESS
/// </summary>
float simpleNoise_fracLess(half2 uv)
{
	return sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453;
}

/// <summary>
/// INV LERP
/// </summary>
half invlerp(half from, half to, half value)
{
	return (value - from) / (to - from);
}

/// <summary>
/// PIXELATE
/// </summary>
half4 pixelate(sampler2D tex, half2 uv, half scale, half ratio)
{
	half ds = 1.0 / scale;
	half2 coord = half2(ds * ceil(uv.x / ds), (ds * ratio) * ceil(uv.y / ds / ratio));
	return half4(tex2D(tex, coord).xyzw);
}

half4 pixelate(sampler2D tex, half2 uv, half2 scale)
{
	half2 ds = 1.0 / scale;
	half2 coord = ds * ceil(uv / ds);
	return half4(tex2D(tex, coord).xyzw);
}

/// <summary>
/// BARREL DISTORTION
/// </summary>
half2 barrelDistortion(half2 coord, half spherical, half barrel, half scale) 
{
	half2 h = coord.xy - half2(0.5, 0.5);
	half r2 = dot(h, h);
	half f = 1.0 + r2 * (spherical + barrel * sqrt(r2));
	return f * scale * h + 0.5;
}

/// <summary>
/// HSVtoRGB
/// </summary>
half3 HSVtoRGB(half3 c)
{
	half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

/// <summary>
/// RGBtoHSV
/// </summary>
half3 RGBtoHSV(half3 c)
{
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	half d = q.x - min(q.w, q.y);
	half e = 1.0e-10;
	return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

/// <summary>
/// RGBtoHUE
/// </summary>
half RGBtoHUE(half3 c)
{
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	half d = q.x - min(q.w, q.y);
	return abs(q.z + (q.w - q.y) / (6.0 * d + 1.0e-10));
}

/// <summary>
/// HUEtoRGB
/// </summary>
half3 HUEtoRGB(half h)
{
	half h6 = h * 6.0;
	half r = abs(h6 - 3.0) - 1.0;
	half g = 2.0 - abs(h6 - 2.0);
	half b = 2.0 - abs(h6 - 4.0);
	return saturate(half3(r, g, b));
}

/// <summary>
/// RGBtoHSL
/// </summary>
half3 RGBtoHSL(half3 c)
{
	half3 hsv = RGBtoHSV(c);
	half l = hsv.z - hsv.y * 0.5;
	half s = hsv.y / (1.0 - abs(l * 2.0 - 1.0) + 1e-10);
	return half3(hsv.x, s, l);
}

/// <summary>
/// HSLtoRGB
/// </summary>
half3 HSLtoRGB(half3 c)
{
   half3 rgb = HUEtoRGB(c.x);
   half C = (1.0 - abs(2.0 * c.z - 1.0)) * c.y;
   return (rgb - 0.5) * C + c.z;
}

/// <summary>
/// RGBtoYUV
/// </summary>
half3 RGBtoYUV(half3 c)
{
	half3 yuv;
	yuv.x = dot(c, half3(0.299, 0.587, 0.114));
	yuv.y = dot(c, half3(-0.14713, -0.28886, 0.436));
	yuv.z = dot(c, half3(0.615, -0.51499, -0.10001));
	return yuv;
}

/// <summary>
/// YUVtoRGB
/// </summary>
half3 YUVtoRGB(half3 c)
{
	half3 rgb;
	rgb.r = c.x + c.z * 1.13983;
	rgb.g = c.x + dot(half2(-0.39465, -0.58060), c.yz);
	rgb.b = c.x + c.y * 2.03211;
	return rgb;
}

/// <summary>
/// RGBtoCMYK
/// </summary>
half4 RGBtoCMYK(half3 c)
{
	half k = max(max(c.r, c.g), c.b);
	return min(half4(c.rgb / k, k), 1.0);
}

/// <summary>
/// CMYKtoRGB
/// </summary>
half3 CMYKtoRGB(half4 c)
{
	return c.rgb * c.a;
}

/// <summary>
/// sRGB
/// </summary>
half3 sRGB(half3 color)
{
	color = (color <= half3(0.0031308, 0.0031308, 0.0031308)) ? color * 12.9232102 : 1.055 * pow(color, 0.41666) - 0.055;
	return color;
}

half4 sRGB(half4 color)
{
	color.rgb = (color.rgb <= half3(0.0031308, 0.0031308, 0.0031308)) ? color.rgb * 12.9232102 : 1.055 * pow(color.rgb, 0.41666) - 0.055;
	return color;
}

/// <summary>
/// Linear
/// </summary>
half4 Linear(half4 color)
{
	color.rgb = (color.rgb <= half3(0.0404482, 0.0404482, 0.0404482)) ? color.rgb / 12.9232102 : pow((color.rgb + 0.055) * 0.9478672, 2.4);
	return color;
}

/// <summary>
/// CheapContrast_RGB
/// </summary>
half3 CheapContrast_RGB(half3 rgb, half contrast)
{
	return saturate(lerp(-contrast.xxx, rgb, 1 + contrast));
}

/// <summary>
/// CheapBright_RGB
/// </summary>
half3 CheapBright_RGB(half3 rgb, half bright)
{
	return rgb * bright;
}

/// <summary>
/// FastAdjustHsv
/// </summary>
float3 FastAdjustHsv(float3 color, float brt, float sat, float cont)
{
	//构造色彩中值
	float3 avgLum = float3(0.5,0.5,0.5);
	//构造调节系数
	float3 lumcoeff = float3(0.2125,0.7154,0.0721);

	//控制亮度Brightness
	float3 brtColor = color * brt;
	float intensityf = dot(brtColor,lumcoeff);
	float3 intensity = float3(intensityf,intensityf,intensityf);

	//控制饱和度
	float3 satColor = lerp(intensity, brtColor, sat);

	//控制对比度
	float3 conColor = lerp(avgLum, satColor, cont);
	return conColor;
}

/// <summary>
/// OverlayBlendMode
/// </summary>
half OverlayBlendMode(half basePixel, half blendPixel, half dec) 
{  
	//half blend = step(basePixel, 0.9);
	if (basePixel < 0.9)
		return (basePixel + blendPixel);

	return (basePixel + blendPixel - dec);
}

/// <summary>
/// OverlayBlendRGB
/// </summary>
half3 OverlayBlendRGB(half3 source, half3 blend) 
{  
	half3 normal = source + blend;
	half3 decontrast = source;
	decontrast.r = decontrast.r + 20.0 / 255.0;
	decontrast.g = decontrast.g + 8.0 / 255.0;
	decontrast += blend * 0.3;

	half sum = source.r + source.g + source.b;
	sum *= 1 - step(blend.r + blend.g + blend.b, 0.01);

	half factor = step(sum, 2.823);
	return factor * normal + (1 - factor) * decontrast;
}

/// <summary>
/// OverlayFromPS
/// </summary>
half3 OverlayFromPS(half3 source, half3 blend)
{
	half3 ifFlag = step(source,half3(0.5,0.5,0.5));
	return ifFlag * source * blend * 2 + (1 - ifFlag) * (1 - (1 - source) * (1 - blend) * 2);
}

/// <summary>
/// SoftLightFromPS
/// </summary>
half3 SoftLightFromPS(half3 source,half3 blend)
{
	half3 ifFlag= step(blend,half3(0.5,0.5,0.5));
	return ifFlag * (source * blend * 2 + source * source *(1 - blend * 2)) + ( 1 - ifFlag ) * ( source * (1 - blend) * 2 + sqrt(source) * (2 * blend - 1)); //柔光
}

/// <summary>
/// HardLightFromPS
/// </summary>
half3 HardLightFromPS(half3 source,half3 blend)
{
	half3 ifFlag = step(blend,half3(0.5,0.5,0.5));
	return ifFlag * source * blend * 2 + (1 - ifFlag) * (1 - (1 - source) * (1 - blend) * 2); //强光
}

/// <summary>
/// LightModelFromPS
/// </summary>
half3 LightModelFromPS(half3 source, half3 blend)
{
	half3 ifFlag = step(blend,half3(0.5,0.5,0.5));
	return ifFlag*(source-(1-source)*(1-2*blend)/(2*blend))+(1-ifFlag)*(source+source*(2*blend-1)/(2*(1-blend))); //亮光
}

/// <summary>
/// SpotLightModelFromPS
/// </summary>
half3 SpotLightModelFromPS(half3 source, half3 blend)
{
	half3 ifFlag = step(blend,half3(0.5,0.5,0.5));
	return ifFlag*(min(source,2*blend))+(1-ifFlag)*(max(source,( blend*2-1))); //点光
}

/// <summary>
/// ColorFilterFromPS
/// </summary>
half3 ColorFilterFromPS(half3 source, half3 blend)
{
	return 1 - ((1 - source) * (1 - blend));
}

/// <summary>
/// ColorDeepenedFromPS
/// </summary>
half3 ColorDeepenedFromPS(half3 source,half3 blend)
{
	return source - ((1 - source) * (1 - blend))/blend;
}

/// <summary>
/// ColorFadeFromPS
/// </summary>
half3 ColorFadeFromPS(half3 source, half3 blend)
{
	return source + (source * blend) / (1 - blend);
}

//------------------------------绕轴旋转旋转----------------------------------
float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
{
	original -= center;
	float C = cos( angle );
	float S = sin( angle );
	float t = 1 - C;
	float m00 = t * u.x * u.x + C;
	float m01 = t * u.x * u.y - S * u.z;
	float m02 = t * u.x * u.z + S * u.y;
	float m10 = t * u.x * u.y + S * u.z;
	float m11 = t * u.y * u.y + C;
	float m12 = t * u.y * u.z - S * u.x;
	float m20 = t * u.x * u.z - S * u.y;
	float m21 = t * u.y * u.z + S * u.x;
	float m22 = t * u.z * u.z + C;
	float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
	return mul( finalMatrix, original ) + center;
}

//-------------------------------HUE 偏移--------------------------------------
float3 HueShift(float HueShiftValue, float3 Texture)
{
	return RotateAroundAxis(float3(0,0,0), Texture, normalize(float3(1,1,1)), HueShiftValue);
}

//---------------------------------Rodrigues_Rotation_Formula-------------------
float RodriguesRotationFormula(float3 t, float3 c)
{
	float t_cos = cos(t * 3);
	float t_sin = sin(t * 3);

	float c_cross = cross(c, normalize(float3(1,1,1)));
	float c_dot = dot(c, normalize(float3(1,1,1)));

	float t_cos_c = t_cos * c;
	float t_sin_c_cross = t_sin * c_cross;
	float t_cos_sub = 1 - t_cos;
	float c_dot_c = c_dot * c;

	return t_cos_sub * c_dot_c + t_sin_c_cross + t_cos_c;
}
