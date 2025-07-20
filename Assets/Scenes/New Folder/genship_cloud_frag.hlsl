#include "genship_cloud_common.hlsl"

#define _CloudMainColorAddition 0.11   // _45._m0
#define _AlphaDiv10_Smooth 1.00   // _45._m1
// #define _DisturbanceScale 0.0123 // _45._m2
float _DisturbanceScale;
#define _Alpha 1.00   // _45._m3
#define _Alpha_Numerator_Bias 1.00   // _45._m4

// 扰动 mask uv 噪声图，其中 rg 表示 uv.xy -0.5~0.5 的基础扰动，b 是扰动缩放
sampler2D _NoiseMapRGB;

// r cloud 染色，1 代表完全 CloudColor_1，0 代表完全 CloudColor_2
// y 云浓度低时的透射区域，一般是边缘区域
// z alpha 分子，
// w alpha 全局
sampler2D _MaskMapRGBA;

// float _FeatherFactor;
// float _FeatherFactorA;
// float _DisappearFactor;

fixed4 frag (v2f i) : SV_Target
{
    float4 Output_0;

    // o.Varying_2.w 是 动画时间百分比 01 映射到 1 \ 0 / 1 的平滑图案，其中 0.4 处达到最低位 0, 0.6 处离开最低位 0
    // 动画时间百分比 0.4 处完全淡入，0.6 处完全淡出
    float _disappearFactor = i.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.w;
    // _disappearFactor = _DisappearFactor;

    // 截帧 _featherFactor  _featherFactorA  是一样的 都是 0.09804
    float _featherFactor = i.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.y;
    float _featherFactorA = i.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.z;

    // _featherFactor = _FeatherFactor;
    // _featherFactorA = _FeatherFactorA;
    
    // _featherFactor 都是 0.09804
    // _alpha_denominator alpha 分母是台阶状，0~0.5 是 0.09804 变大到 2*0.09804 保持，0.5~1 则镜像
    float _alpha_denominator =  min(_disappearFactor + _featherFactor, 1.0) - max(_disappearFactor - _featherFactor, 0.0);

    float2 _disturbanceNoiseUv = i.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.zw;
    float2 _maskMapUv = i.Varying_MaskMapUvXY_DisturbanceNoiseUvZW.xy;
    
    
    float3 _NoiseMapSample = tex2D(_NoiseMapRGB, _disturbanceNoiseUv).xyz;
    float2 _NoiseRG_n0d5_to_0d5 = _NoiseMapSample.xy - 0.5;
    // #define _DisturbanceScale 0.0123 // _45._m2
    float2 _maskUvAfterDisturbance = _NoiseMapSample.z * _NoiseRG_n0d5_to_0d5 * _DisturbanceScale + _maskMapUv;
    float4 _maskMapSample = tex2D(_MaskMapRGBA, _maskUvAfterDisturbance);

    // #define _Alpha_Numerator_Bias 1.00   // _45._m4
    // _alpha_numerator alpha 分子
    float _alpha_numerator_part = _maskMapSample.z - max(_disappearFactor - _featherFactor, 0.0);
    float _alpha_numerator = _alpha_numerator_part + (_Alpha_Numerator_Bias - 1.0);

    
    float _angle_up_to_down_1_n1_bias_0d1 = i.Varying_ViewDirAndAngle1_n1.w + 0.1;
    float _angle_up_to_down_1_n1_scale_smooth = smoothstep(0, 1, _angle_up_to_down_1_n1_bias_0d1 * 5.0);

    // (x- max(x - 0.09804, 0.0) + 1-1) / (min(x+0.09804, 1.0) - max(x-0.09804, 0.0))
    // _alpha_div 说明：
    //   其中 _disappearFactor 即 1 \ 0 / 1 的平滑图案影响最常见，在其输出 0 时，最终是 0，其输出 1 时，最终是 1，
    //   但从 0 到 0.09804 是渐入到 0.5，在 0.09804 ~ 0.90196 保持 0.5, 0.90196 到 1 快入到 1
    // 总结:
    //   _disappearFactor       自变量，1 \ 0 / 1 的平滑图案
    //   _featherFactor 0.09804 可以理解成自变量起作用影响范围宽度
    float _alpha_div = smoothstep(0, 1, _alpha_numerator / _alpha_denominator) * _angle_up_to_down_1_n1_scale_smooth  * _maskMapSample.w;


    // #define _AlphaDiv10_Smooth 1.00   // _45._m1
    // #define _Alpha 1.00   // _45._m3
    float _alphaControl = smoothstep(0, 1, _AlphaDiv10_Smooth * 10.0) * _Alpha;
    float _output_alpha = _alphaControl * _alpha_div;

    // 原神云 _disappearFactor 表示云的消失程度，1 时完全消失，解决出现消失时突兀问题
    _output_alpha *= saturate( (1 - _disappearFactor) / 0.01);
    
    if (_output_alpha < 0.01)
    {
        discard;
    }


    // (x- max(x - 0.09804, 0.0) ) / (min(x+0.09804, 1.0) - max(x-0.09804, 0.0))
    // 同上 但变成 1-smoothstep(0, 1, ?)，即 1 \ 0.5 - 0.5 - 0.5 \ 0 这样
    float _alpha_denominator_2 =  min(_disappearFactor + _featherFactorA, 1.0) - max(_disappearFactor - _featherFactor, 0.0);
    float _alpha_div_oneMinus = 1.0 - smoothstep(0, 1, _alpha_numerator_part / _alpha_denominator_2);

    // 当 _disappearFactor 输入接近 0，则使用描边状 _maskMapSample.y 作为透射系数，否则保持为 2，最后变成 0
    float _transmission = lerp(_maskMapSample.y, _alpha_div_oneMinus * 4.0, _disappearFactor);

    float3 _cloudMainColor = lerp(i.Varying_CloudColor_Dark,  i.Varying_CloudColor_Bright, _maskMapSample.x);

    float3 _sumCloudColor;
        _sumCloudColor = _cloudMainColor;
        _sumCloudColor += i.Varying_TransmissionColor * _transmission;
        _sumCloudColor += i.Varying_CloudColor_Bright * _CloudMainColorAddition * 0.4;
        _sumCloudColor += i.Varying_ShineColor * _maskMapSample.x;
    
    
    float _cloudMoreBright = i.Varying_LDotDir01FixX_FeatherYZ_DisappearFactorW.x + 1.0;
    
    
    float _angle_up_to_down_1_n1_scale_smooth_2 = min(1.0, smoothstep(0, 1, i.Varying_ViewDirAndAngle1_n1.w * 10.0));

    
    float _cloudVisableFactor = lerp(_angle_up_to_down_1_n1_scale_smooth_2, 1.0, smoothstep(0, 1, (_CloudMainColorAddition - 0.4) * 10/3.0));
    
    Output_0.xyz = lerp( i.Varying_DayPartColor, _sumCloudColor * _cloudMoreBright, _cloudVisableFactor);
    Output_0.w = _output_alpha;
    
    fixed4 col = Output_0.xyzw;
    return col;
}