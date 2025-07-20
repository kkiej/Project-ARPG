using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 角色类型
/// </summary>
public enum CharacterType
{
    Player,
    Enemy,
    HumanEnemy,
}

public enum DissolvePart
{
    All,
    Weapon
}

public class RoleShaderFunctions : MonoBehaviour
{
    public List<CharacterRender> MaterialList = new List<CharacterRender>();

    //更新角色点阵化透明 material
    public void UpdateMaskTransparent(Material[] materials, bool PerCharacterMaskTransparentState,
        float MaskTransparent)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(ShaderIDs.useTransparentMask,
                Convert.ToInt32((PerCharacterMaskTransparentState && GlobalConfig.useTransparentMask)));
            materials[i].SetFloat(ShaderIDs.Transparency, MaskTransparent);
        }
    }

    //更新角色点阵化透明 MaterialPropertyBlock
    public void UpdateMaskTransparent(MaterialPropertyBlock propertyBlock, bool PerCharacterMaskTransparentState,
        float MaskTransparent)
    {
        propertyBlock.SetFloat(ShaderIDs.useTransparentMask,
            Convert.ToInt32((PerCharacterMaskTransparentState && GlobalConfig.useTransparentMask)));
        propertyBlock.SetFloat(ShaderIDs.Transparency, MaskTransparent);
    }

    /// <summary>
    /// 更新角色屏幕空间边缘光
    /// </summary>
    /// <param name="useCustomRimLightWidth"></param>
    /// <param name="material"></param>
    /// <param name="CustomRimLightWidth"></param>
    public void UpdateRoleRimScale(bool useCustomRimLightWidth, Material[] materials, float CustomRimLightWidth)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (useCustomRimLightWidth)
            {
                materials[i].SetFloat(ShaderIDs.SSRimScale, (CustomRimLightWidth + 0.01f) * 5);
                materials[i].SetFloat(ShaderIDs.RimRangeStep, (1 - CustomRimLightWidth));
            }
            else
            {
                materials[i].SetFloat(ShaderIDs.useSSRimLight, Convert.ToInt32(GlobalConfig.useSSRimLight));

                if (GlobalConfig.useSSRimLight)
                    materials[i].SetFloat(ShaderIDs.SSRimScale, ShaderUtils.SSRimWidth);
                else
                    materials[i].SetFloat(ShaderIDs.RimRangeStep, ShaderUtils.SimpleRimWidth);
            }
        }
    }

    public void UpdateRoleRimScale(bool useCustomRimLightWidth, MaterialPropertyBlock propertyBlock,
        float CustomRimLightWidth)
    {
        if (useCustomRimLightWidth)
        {
            propertyBlock.SetFloat(ShaderIDs.SSRimScale, (CustomRimLightWidth + 0.01f) * 5);
            propertyBlock.SetFloat(ShaderIDs.RimRangeStep, (1 - CustomRimLightWidth));
        }
    }

    /// <summary>
    /// 更新边缘光颜色
    /// </summary>
    /// <param name="useCustomRimLightColor"></param>
    /// <param name="materials"></param>
    /// <param name="CustomRimLightColor"></param>
    public void UpdateRoleRimColor(bool useCustomRimLightColor, Material[] materials, Color CustomRimLightColor)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (useCustomRimLightColor && materials[i].HasProperty(ShaderIDs.RimColor))
            {
                materials[i].SetColor(ShaderIDs.RimColor, CustomRimLightColor);
            }
        }
    }

    public void UpdateRoleRimColor(bool useCustomRimLightColor, MaterialPropertyBlock propertyBlock,
        Color CustomRimLightColor)
    {
        if (useCustomRimLightColor)
        {
            propertyBlock.SetColor(ShaderIDs.RimColor, CustomRimLightColor);
        }
    }

    /// <summary>
    /// 更新敌人边缘光颜色
    /// </summary>
    /// <param name="useCustomRimLightColor"></param>
    /// <param name="materials"></param>
    /// <param name="CustomRimLightColor"></param>
    public void UpdateEnemyRimColor(bool useCustomRimLightColor, Material[] materials, Color CustomRimLightColor)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (useCustomRimLightColor && materials[i].HasProperty(ShaderIDs.RimColor))
            {
                materials[i].SetColor(ShaderIDs.EnemyRimColor, CustomRimLightColor);
            }
        }
    }

    public void UpdateEnemyRimColor(bool useCustomRimLightColor, MaterialPropertyBlock propertyBlock,
        Color CustomRimLightColor)
    {
        if (useCustomRimLightColor)
        {
            propertyBlock.SetColor(ShaderIDs.EnemyRimColor, CustomRimLightColor);
        }
    }

    /// <summary>
    /// 使用UnityFog
    /// </summary>
    /// <param name="useUnityFog"></param>
    /// <param name="material"></param>
    public void UpdateUnityFogEnable(Material[] materials, bool useUnityFog)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(ShaderIDs.UseUnityFog, Convert.ToInt32(useUnityFog));
        }
    }

    public void UpdateUnityFogEnable(MaterialPropertyBlock propertyBlock, bool useUnityFog)
    {
        propertyBlock.SetFloat(ShaderIDs.UseUnityFog, Convert.ToInt32(useUnityFog));
    }

    /// <summary>
    /// 更新角色描边
    /// </summary>
    public void UpdateRoleOutLine(bool isFace, Material[] materials, bool isShowScene, bool useCustomOutLineWidth,
        float CustomOutLineWidth)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            // 自定义描边
            if (useCustomOutLineWidth)
            {
                materials[i].SetFloat(ShaderIDs.OutLineWidth, math.pow(CustomOutLineWidth, 0.66f));
            }
            else
            {
                if (isShowScene)
                {
                    materials[i].SetFloat(ShaderIDs.OutLineWidth, ShaderUtils.showSceneOutLine);
                }
                else
                {
                    if (isFace)
                    {
                        materials[i].SetFloat(ShaderIDs.OutLineWidth, ShaderUtils.showSceneOutLine);
                    }
                    else
                    {
                        materials[i].SetFloat(ShaderIDs.OutLineWidth, ShaderUtils.commonOutLine);
                    }

                    materials[i].SetFloat(ShaderIDs.ShineOutLineWidth, ShaderUtils.ShineOutLine);
                    materials[i].SetColor(ShaderIDs.ShineOutLineColor, new Color(0.1801f, 1.1802f, 1.7207f, 1));
                }
            }
        }
    }

    public void UpdateRoleOutLine(bool isFace, MaterialPropertyBlock propertyBlock, bool isShowScene,
        bool useCustomOutLineWidth, float CustomOutLineWidth)
    {
        // 自定义描边
        if (useCustomOutLineWidth)
        {
            propertyBlock.SetFloat(ShaderIDs.OutLineWidth, math.pow(CustomOutLineWidth, 0.66f));
        }
    }

    /// <summary>
    /// 更新暗部亮度控制
    /// </summary>
    /// <param name="materials"></param>
    /// <param name="UseDarkLuminance"></param>
    /// <param name="DarkLuminance"></param>
    public void UpdateRoleDarkLuminance(Material[] materials, bool UseDarkLuminance, float DarkLuminance)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(ShaderIDs.UseDarkLuminance, Convert.ToInt32(UseDarkLuminance));

            if (UseDarkLuminance)
                materials[i].SetFloat(ShaderIDs.DarkLuminance, DarkLuminance);
        }
    }

    /// <summary>
    /// 更新暗部亮度控制
    /// </summary>
    /// <param name="propertyBlock"></param>
    /// <param name="UseDarkLuminance"></param>
    /// <param name="DarkLuminance"></param>
    public void UpdateRoleDarkLuminance(MaterialPropertyBlock propertyBlock, bool UseDarkLuminance, float DarkLuminance)
    {
        propertyBlock.SetFloat(ShaderIDs.UseDarkLuminance, Convert.ToInt32(UseDarkLuminance));

        if (UseDarkLuminance)
            propertyBlock.SetFloat(ShaderIDs.DarkLuminance, DarkLuminance);
    }

    /// <summary>
    /// 更新角色灯光 material
    /// </summary>
    public void UpdateRoleLightParams(Material[] materials, Color LightColor, Color AddColor, bool UseWhiteBalance,
        bool UseAddColor, bool UseAdditionLight, float addShadowRangeStep, float addShadowFeather)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor(ShaderIDs.LightColor, LightColor);
            materials[i].SetColor(ShaderIDs.AddColor, AddColor);
            materials[i].SetFloat(ShaderIDs.UseWhiteBalance, Convert.ToInt32(UseWhiteBalance));
            materials[i].SetFloat(ShaderIDs.UseAddColor, Convert.ToInt32(UseAddColor));
            materials[i].SetFloat(ShaderIDs.UseAddLight, Convert.ToInt32(UseAdditionLight));
            materials[i].SetFloat(ShaderIDs.addShadowRangeStep, addShadowRangeStep);
            materials[i].SetFloat(ShaderIDs.addShadowFeather, addShadowFeather);
            materials[i].SetVector(ShaderIDs.WorldPos, this.transform.position);
        }
    }

    /// <summary>
    /// 更新角色灯光 MaterialPropertyBlock
    /// </summary>
    public void UpdateRoleLightParams(MaterialPropertyBlock propertyBlock, Color LightColor, Color AddColor,
        bool UseWhiteBalance, bool UseAddColor, bool UseAdditionLight, float addShadowRangeStep, float addShadowFeather)
    {
        propertyBlock.SetColor(ShaderIDs.LightColor, LightColor);
        propertyBlock.SetColor(ShaderIDs.AddColor, AddColor);
        propertyBlock.SetFloat(ShaderIDs.UseWhiteBalance, Convert.ToInt32(UseWhiteBalance));
        propertyBlock.SetFloat(ShaderIDs.UseAddColor, Convert.ToInt32(UseAddColor));
        propertyBlock.SetFloat(ShaderIDs.UseAddLight, Convert.ToInt32(UseAdditionLight));
        propertyBlock.SetFloat(ShaderIDs.addShadowRangeStep, addShadowRangeStep);
        propertyBlock.SetFloat(ShaderIDs.addShadowFeather, addShadowFeather);
        propertyBlock.SetVector(ShaderIDs.WorldPos, this.transform.position);
    }

    /// <summary>
    /// 更新卡通灯光 material
    /// </summary>
    public void UpdateToonLight(Material[] materials, bool useCameraLight, Camera mainCamera,
        Vector3 cameraLightForward, float LightStrength)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetInt(ShaderIDs.UseCameraLight, Convert.ToInt32(useCameraLight));
            if (mainCamera != null)
                materials[i].SetFloat(ShaderIDs.CameraFOV, mainCamera.fieldOfView);
            else
                materials[i].SetFloat(ShaderIDs.CameraFOV, 45);

            if (cameraLightForward != null)
            {
                Vector3 toonLightDir = cameraLightForward;
                materials[i].SetVector(ShaderIDs.ToonLightDirection, toonLightDir);
            }
            else
                materials[i].SetVector(ShaderIDs.ToonLightDirection, new Vector4(0, 0, 0, 0));

            materials[i].SetFloat(ShaderIDs.ToonLightStrength, LightStrength);
        }
    }

    /// <summary>
    /// 更新卡通灯光 MaterialPropertyBlock
    /// </summary>
    public void UpdateToonLight(MaterialPropertyBlock propertyBlock, bool useCameraLight, Camera mainCamera,
        Vector3 cameraLightForward, float LightStrength)
    {
        propertyBlock.SetInt(ShaderIDs.UseCameraLight, Convert.ToInt32(useCameraLight));
        if (mainCamera != null)
        {
            if (useCameraLight && cameraLightForward != null)
            {
                Vector3 toonLightDir = cameraLightForward;
                propertyBlock.SetVector(ShaderIDs.ToonLightDirection, toonLightDir);
            }

            propertyBlock.SetFloat(ShaderIDs.CameraFOV, mainCamera.fieldOfView);
        }
        else
        {
            propertyBlock.SetFloat(ShaderIDs.CameraFOV, 45);
        }

        propertyBlock.SetFloat(ShaderIDs.ToonLightStrength, LightStrength);
    }

    /// <summary>
    /// 更新敌人灯光参数 Material
    /// </summary>
    public void UpdateEnemyLightParams(Material[] materials, float LightStrength, Color LightColor, Color AddColor,
        bool UseWhiteBalance, bool UseAddColor, bool UseAdditionLight)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(ShaderIDs.ToonLightStrength, LightStrength);
            materials[i].SetColor(ShaderIDs.LightColor, LightColor);
            materials[i].SetColor(ShaderIDs.AddColor, AddColor);
            materials[i].SetFloat(ShaderIDs.UseWhiteBalance, Convert.ToInt32(UseWhiteBalance));
            materials[i].SetFloat(ShaderIDs.UseAddColor, Convert.ToInt32(UseAddColor));
            materials[i].SetFloat(ShaderIDs.UseAddLight, Convert.ToInt32(UseAdditionLight));
            materials[i].SetVector(ShaderIDs.WorldPos, this.transform.position);
        }
    }

    /// <summary>
    /// 更新敌人灯光参数 MaterialPropertyBlock
    /// </summary>
    public void UpdateEnemyLightParams(MaterialPropertyBlock propertyBlock, float LightStrength, Color LightColor,
        Color AddColor, bool UseWhiteBalance, bool UseAddColor, bool UseAdditionLight)
    {
        propertyBlock.SetFloat(ShaderIDs.ToonLightStrength, LightStrength);
        propertyBlock.SetColor(ShaderIDs.LightColor, LightColor);
        propertyBlock.SetColor(ShaderIDs.AddColor, AddColor);
        propertyBlock.SetFloat(ShaderIDs.UseWhiteBalance, Convert.ToInt32(UseWhiteBalance));
        propertyBlock.SetFloat(ShaderIDs.UseAddColor, Convert.ToInt32(UseAddColor));
        propertyBlock.SetFloat(ShaderIDs.UseAddLight, Convert.ToInt32(UseAdditionLight));
        propertyBlock.SetVector(ShaderIDs.WorldPos, this.transform.position);
    }

    /// <summary>
    /// 更新敌人描边宽度    Material
    /// </summary>
    public void UpdateEnemyOutLine(Material[] materials, bool isShowScene, CharacterType type,
        bool useCustomOutLineWidth, float CustomOutLineWidth)
    {
        if (!isShowScene)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (useCustomOutLineWidth)
                {
                    materials[i].SetFloat(ShaderIDs.OutLineWidth, CustomOutLineWidth);
                }
                else
                {
                    if (type == CharacterType.Enemy)
                        materials[i].SetFloat(ShaderIDs.OutLineWidth, ShaderUtils.EnemyOutLine);
                    else
                        materials[i].SetFloat(ShaderIDs.OutLineWidth, ShaderUtils.HumanEnemyOutLine);
                }
            }
        }
    }

    /// <summary>
    /// 更新敌人描边宽度  MaterialPropertyBlock
    /// </summary>
    public void UpdateEnemyOutLine(MaterialPropertyBlock propertyBlock, bool useCustomOutLineWidth,
        float CustomOutLineWidth)
    {
        if (useCustomOutLineWidth)
        {
            propertyBlock.SetFloat(ShaderIDs.OutLineWidth, CustomOutLineWidth);
        }
    }

    /// <summary>
    /// 更新Material
    /// </summary>
    public void UpdateCameraFOV(Material[] materials, Camera mainCamera)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (mainCamera != null)
            {
                materials[i].SetFloat(ShaderIDs.CameraFOV, mainCamera.fieldOfView);
            }
            else
            {
                materials[i].SetFloat(ShaderIDs.CameraFOV, 45);
            }
        }
    }

    /// <summary>
    /// 更新点阵透明Scale
    /// </summary>
    public void UpdatMaskTransparentScale(UniversalAdditionalCameraData mainCameraData)
    {
        if (mainCameraData != null)
        {
            /*if (mainCameraData.EnablePerRenderScale)
                Shader.SetGlobalFloat(ShaderIDs.RenderScale,
                    mainCameraData.PerRenderScale * ShaderUtils.GetGlobalAsset().renderScale);
            else
                Shader.SetGlobalFloat(ShaderIDs.RenderScale, ShaderUtils.GetGlobalAsset().renderScale);*/
        }
        else
            Shader.SetGlobalFloat(ShaderIDs.RenderScale, ShaderUtils.GetGlobalAsset().renderScale);
    }

    /// <summary>
    /// 设置角色半透明
    /// </summary>
    public void SetCharacterMaskTransparentState(ref bool PerCharacterMaskTransparentState, bool CanInvisibility,
        bool isSelect, Camera mainCamera, ref float MaskTransparent)
    {
        if (CanInvisibility && !isSelect)
        {
            if (PerCharacterMaskTransparentState == false)
                PerCharacterMaskTransparentState = true;
            if (mainCamera == null)
                return;
            var distance = Vector3.Distance(transform.position + new Vector3(0, 1, 0), mainCamera.transform.position);
            if (distance < 4)
            {
                MaskTransparent = math.clamp(distance / 4, 0, 1) * 0.4f;
            }
            else
            {
                MaskTransparent = 1;
            }
        }
        else
        {
            MaskTransparent = 1;
        }
    }

    /// <summary>
    /// 更新MaterialPropertyBlock
    /// </summary>
    public void UpdateCameraFOV(MaterialPropertyBlock propertyBlock, Camera mainCamera)
    {
        if (mainCamera != null)
        {
            propertyBlock.SetFloat(ShaderIDs.CameraFOV, mainCamera.fieldOfView);
        }
        else
        {
            propertyBlock.SetFloat(ShaderIDs.CameraFOV, 45);
        }
    }

    /// <summary>
    /// 设置脸部HSV
    /// </summary>
    /// <param name="HSV"></param>
    public void SetFaceHSV(Vector3 HSV)
    {
        if (GlobalConfig.useCharacterHSV == false)
            return;

        foreach (CharacterRender child in MaterialList)
        {
            if (child.isFace)
            {
                child._renderer.material.SetFloat(ShaderIDs.UseHSV, 1);
                child._renderer.material.SetFloat(ShaderIDs.HUE, HSV.x);
                child._renderer.material.SetFloat(ShaderIDs.Saturation, HSV.y);
                child._renderer.material.SetFloat(ShaderIDs.Value, HSV.z);
            }
        }
    }

    /// <summary>
    /// 设置头发HSV
    /// </summary>
    /// <param name="HSV"></param>
    public void SetHairHSV(Vector3 HSV)
    {
        if (GlobalConfig.useCharacterHSV == false)
            return;

        foreach (CharacterRender child in MaterialList)
        {
            if (child.isHair)
            {
                child._renderer.material.SetFloat(ShaderIDs.UseHSV, 1);
                child._renderer.material.SetFloat(ShaderIDs.HUE, HSV.x);
                child._renderer.material.SetFloat(ShaderIDs.Saturation, HSV.y);
                child._renderer.material.SetFloat(ShaderIDs.Value, HSV.z);
            }
        }
    }

    /// <summary>
    /// 设置身体HSV
    /// </summary>
    /// <param name="HSV"></param>
    public void SetBodyHSV(Vector3 HSV)
    {
        if (GlobalConfig.useCharacterHSV == false)
            return;

        foreach (CharacterRender child in MaterialList)
        {
            if (child.isBody)
            {
                child._renderer.material.SetFloat(ShaderIDs.UseHSV, 1);
                child._renderer.material.SetFloat(ShaderIDs.HUE, HSV.x);
                child._renderer.material.SetFloat(ShaderIDs.Saturation, HSV.y);
                child._renderer.material.SetFloat(ShaderIDs.Value, HSV.z);
            }
        }
    }

    /// <summary>
    /// 阴影设置
    /// </summary>
    public void SetShadow(bool useShadow)
    {
        //  遍历child
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterial != null)
            {
                if (useShadow && GlobalConfig.useRealTimeShadow)
                {
                    //  看一下ShadowCaster通道开了没有  没开的话开启一下
                    if (!child.sharedMaterial.GetShaderPassEnabled("ShadowCaster"))
                        child.sharedMaterial.SetShaderPassEnabled("ShadowCaster", true);
                }
                else
                {
                    //  看一下ShadowCaster通道开了没有  开启的话就关闭了
                    if (child.sharedMaterial.GetShaderPassEnabled("ShadowCaster"))
                        child.sharedMaterial.SetShaderPassEnabled("ShadowCaster", false);
                }
            }
        }
    }

    /// <summary>
    /// 设置小怪表情
    /// </summary>
    /// <param name="index"></param>
    public void SetEnemyEye(int index)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child._renderer.gameObject.name.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                switch (index)
                {
                    case (int) EnemyExpression.Default:
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetX, 0f);
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetY, 0);
                        break;
                    case (int) EnemyExpression.Dead:
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetX, 0f);
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetY, 0.33f);
                        break;
                    case (int) EnemyExpression.Play:
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetX, 0f);
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetY, 0.66f);
                        break;
                    case (int) EnemyExpression.Attack:
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetX, 0.5f);
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetY, 0);
                        break;
                    case (int) EnemyExpression.Hit:
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetX, 0.5f);
                        child.sharedMaterial.SetFloat(ShaderIDs.UVOffsetY, 0.66f);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 设置武器流光出现
    /// </summary>
    public void SetWeaponShine(bool useWeaponShine, float showRimScale, float showObjectScale, Color rimColor,
        float dissolveWidth, Color dissolveEdgeColor)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterials.Length > 0)
            {
                for (int i = 0; i < child.sharedMaterials.Length; i++)
                {
                    if (useWeaponShine)
                    {
                        if (child.isWeapon)
                        {
                            child.sharedMaterials[i]
                                .SetFloat(ShaderIDs.useDissolveCut, Convert.ToInt32(useWeaponShine));
                            child.sharedMaterials[i].SetFloat(ShaderIDs.useAdditionFresnel,
                                Convert.ToInt32(useWeaponShine));
                            child.sharedMaterials[i].SetFloat(ShaderIDs.fresnelFlowRange, showRimScale);
                            child.sharedMaterials[i].SetFloat(ShaderIDs.dissolveCut, showObjectScale);
                            child.sharedMaterials[i].SetColor(ShaderIDs.fresnelColor, rimColor);
                            child.sharedMaterials[i].SetFloat(ShaderIDs.DissolveWidth, dissolveWidth);
                            child.sharedMaterials[i].SetColor(ShaderIDs.DissolveColor, dissolveEdgeColor);
                        }
                    }
                    else
                    {
                        if (child.isWeapon)
                        {
                            child.sharedMaterials[i]
                                .SetFloat(ShaderIDs.useDissolveCut, Convert.ToInt32(useWeaponShine));
                            child.sharedMaterials[i].SetFloat(ShaderIDs.useAdditionFresnel,
                                Convert.ToInt32(useWeaponShine));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 程序用 设置额外菲尼尔
    /// </summary>
    public void SetAdditionFresnel(bool UseAdditionFresnel, Color rimColor, float FresnelRange, float FresnelFeather,
        float showRimScale)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (UseAdditionFresnel)
            {
                child.sharedMaterial.SetFloat(ShaderIDs.useAdditionFresnel, Convert.ToInt32(UseAdditionFresnel));
                child.sharedMaterial.SetColor(ShaderIDs.fresnelColor, rimColor);
                child.sharedMaterial.SetFloat(ShaderIDs.FresnelRange, FresnelRange);
                child.sharedMaterial.SetFloat(ShaderIDs.FresnelFeather, FresnelFeather);
                child.sharedMaterial.SetFloat(ShaderIDs.fresnelFlowRange, showRimScale);
            }
            else
            {
                child.sharedMaterial.SetFloat(ShaderIDs.useAdditionFresnel, Convert.ToInt32(UseAdditionFresnel));
            }
        }
    }

    /// <summary>
    /// 美术看的 程序不调用这里  额外菲涅尔
    /// </summary>
    /// <param name="child"></param>
    /// <param name="propertyBlock"></param>
    public void SetAdditionFresnelTest(MaterialPropertyBlock propertyBlock, bool UseAdditionFresnel, Color rimColor,
        float FresnelRange, float FresnelFeather, float showRimScale)
    {
        if (UseAdditionFresnel)
        {
            propertyBlock.SetFloat(ShaderIDs.useAdditionFresnel, Convert.ToInt32(UseAdditionFresnel));
            propertyBlock.SetColor(ShaderIDs.fresnelColor, rimColor);
            propertyBlock.SetFloat(ShaderIDs.FresnelRange, FresnelRange);
            propertyBlock.SetFloat(ShaderIDs.FresnelFeather, FresnelFeather);
            propertyBlock.SetFloat(ShaderIDs.fresnelFlowRange, showRimScale);
        }
        else
        {
            propertyBlock.SetFloat(ShaderIDs.useAdditionFresnel, Convert.ToInt32(UseAdditionFresnel));
        }
    }

    /// <summary>
    /// 使用前发投影
    /// </summary>
    public void SetCharacterCelHairShadow(CharacterType _characterType, bool useHairShadow)
    {
        if (_characterType == CharacterType.Player)
        {
            if (useHairShadow)
            {
                Shader.SetGlobalFloat(ShaderIDs.UseHairShadow, 1);
            }
            else
            {
                Shader.SetGlobalFloat(ShaderIDs.UseHairShadow, 0);
            }
        }
    }

    /// <summary>
    /// 设置Common状态
    /// </summary>
    public void SetRoleOutLineCommon(out bool isSelect)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterials.Length > 0)
            {
                for (int i = 0; i < child.sharedMaterials.Length; i++)
                {
                    if (child.sharedMaterials[i] != null)
                    {
                        //打开普通外勾边
                        child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.OutLinePass, true);
                        //关闭发光外勾边
                        child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.ShineOutLinePass, false);
                    }
                }
            }
        }

        isSelect = false;
    }

    /// <summary>
    /// 设置角色选中 蓝色描边
    /// </summary>
    public void SetRoleSelectON(out bool isSelect)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterials.Length > 0)
            {
                for (int i = 0; i < child.sharedMaterials.Length; i++)
                {
                    //关闭普通外勾边
                    child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.OutLinePass, false);
                    //打开发光外勾边
                    child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.ShineOutLinePass, true);
                }
            }
        }

        isSelect = true;
    }

    /// <summary>
    /// 设置角色未选中
    /// </summary>
    public void SetRoleSelectOFF(out bool isSelect)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterials.Length > 0)
            {
                for (int i = 0; i < child.sharedMaterials.Length; i++)
                {
                    //打开普通外勾边
                    child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.OutLinePass, true);
                    //关闭发光外勾边
                    child.sharedMaterials[i].SetShaderPassEnabled(ShaderIDs.ShineOutLinePass, false);
                }
            }
        }

        isSelect = false;
    }

    /// <summary>
    /// 设置灯光颜色
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="color"></param>
    public void SetLightColor(out Color LightColor, Color color)
    {
        LightColor = color;
    }

    /// <summary>
    /// 关闭黑白闪
    /// </summary>
    public void SetBlackWhiteFlaskOff()
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.sharedMaterials.Length > 0)
            {
                for (int i = 0; i < child.sharedMaterials.Length; i++)
                {
                    child.sharedMaterials[i].DisableKeyword("BLACK_WHITE_ON");
                }
            }
        }
    }

    /// <summary>
    /// 更新角色流光剔除
    /// </summary>
    /// <param name="mat"></param>
    public void UpdateCharacterClip(Material[] materials, bool UsePosWDissolve, float PosWDissolveScale,
        float PosWDissolveWidth, float MaskScale, Color PosWDissolveColor, bool useTop2Bottom)
    {
        if (materials.Length <= 0)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            if (UsePosWDissolve)
            {
                materials[i].EnableKeyword("ROLE_POSW_DISSOLVE");
                materials[i].SetFloat(ShaderIDs.PosWDissolveScale, PosWDissolveScale);
                materials[i].SetFloat(ShaderIDs.PosWDissolveWidth, PosWDissolveWidth);
                materials[i].SetFloat(ShaderIDs.PosWMaskScale, MaskScale);
                materials[i].SetColor(ShaderIDs.PosWDissolveColor, PosWDissolveColor);
                materials[i].SetFloat(ShaderIDs.UseTop2Bottom, Convert.ToInt32(useTop2Bottom));
            }
            else
            {
                if (materials[i] != null)
                {
                    materials[i].DisableKeyword("ROLE_POSW_DISSOLVE");
                }
            }
        }
    }

    /// <summary>
    /// 更新冰冻效果
    /// </summary>
    public void UpdateIceVFX(Material[] mats, bool UseIce, Color IceColor, float IceNormalScale, Color IceRimColor,
        float IceFresnelFeather, float IceFresnelStep, Color IceSpecColor, float IceSpecPower)
    {
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetFloat(ShaderIDs.UseIce, Convert.ToInt32(UseIce));
            if (UseIce)
            {
                mats[i].SetColor(ShaderIDs.IceVFXColor, IceColor);
                mats[i].SetFloat(ShaderIDs.IceNormalScale, IceNormalScale);
                mats[i].SetColor(ShaderIDs.IceRimColor, IceRimColor);
                mats[i].SetFloat(ShaderIDs.IceFresnelFeather, IceFresnelFeather);
                mats[i].SetFloat(ShaderIDs.IceFresnelStep, IceFresnelStep);
                mats[i].SetColor(ShaderIDs.IceSpecColor, IceSpecColor);
                mats[i].SetFloat(ShaderIDs.IceSpecPower, IceSpecPower);
            }
        }
    }

    public void UpdateIceVFX(MaterialPropertyBlock materialPropertyBlock, bool UseIce, Color IceColor,
        float IceNormalScale, Color IceRimColor, float IceFresnelFeather, float IceFresnelStep, Color IceSpecColor,
        float IceSpecPower)
    {
        materialPropertyBlock.SetFloat(ShaderIDs.UseIce, Convert.ToInt32(UseIce));
        materialPropertyBlock.SetColor(ShaderIDs.IceVFXColor, IceColor);
        materialPropertyBlock.SetFloat(ShaderIDs.IceNormalScale, IceNormalScale);
        materialPropertyBlock.SetColor(ShaderIDs.IceRimColor, IceRimColor);
        materialPropertyBlock.SetFloat(ShaderIDs.IceFresnelFeather, IceFresnelFeather);
        materialPropertyBlock.SetFloat(ShaderIDs.IceFresnelStep, IceFresnelStep);
        materialPropertyBlock.SetColor(ShaderIDs.IceSpecColor, IceSpecColor);
        materialPropertyBlock.SetFloat(ShaderIDs.IceSpecPower, IceSpecPower);
    }

    /// <summary>
    /// 更新脸部冰冻溶解
    /// </summary>
    public void UpdateFaceIceQuad(Material[] materials, bool UseFaceIce, float IceQuadDensity,
        float IceQuadDissolveScale)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(ShaderIDs.UseFaceIceQuad, Convert.ToInt32(UseFaceIce));
            materials[i].SetFloat(ShaderIDs.IceQuadDensity, IceQuadDensity);
            materials[i].SetFloat(ShaderIDs.IceQuadDissolveScale, IceQuadDissolveScale);
        }
    }

    public void UpdateFaceIceQuad(MaterialPropertyBlock materialPropertyBlock, bool UseFaceIce, float IceQuadDensity,
        float IceQuadDissolveScale)
    {
        materialPropertyBlock.SetFloat(ShaderIDs.UseFaceIceQuad, Convert.ToInt32(UseFaceIce));
        materialPropertyBlock.SetFloat(ShaderIDs.IceQuadDensity, IceQuadDensity);
        materialPropertyBlock.SetFloat(ShaderIDs.IceQuadDissolveScale, IceQuadDissolveScale);
    }

    /// <summary>
    /// 更新溶解
    /// </summary>
    /// <param name="material"></param>
    public void UpdateDossolve(Material[] mats, bool UseDossolve, float dissolveScale, Color edgeColor,
        Vector2 ScreenUV)
    {
        for (int i = 0; i < mats.Length; i++)
        {
            if (UseDossolve)
            {
                mats[i].SetFloat(ShaderIDs.UseDissolve, 1);
                mats[i].SetFloat(ShaderIDs.DissolveScale, dissolveScale);
                mats[i].SetFloat(ShaderIDs.EdgeWidth, GlobalConfig.DieDissolveWidth);
                mats[i].SetColor(ShaderIDs.EdgeColor, edgeColor);
                mats[i].SetVector(ShaderIDs.DissolveUV, ScreenUV);
            }
            else
            {
                mats[i].SetFloat(ShaderIDs.UseDissolve, 0);
            }
        }
    }

    /// <summary>
    /// 角色阴影的Stencil
    /// </summary>
    /// <param name="mat"></param>
    public void SetRoleLitDirStencilRef(Material mat)
    {
        if (ShaderUtils.GetGlobalPostprocessingData().renderingMode == RenderingMode.Deferred)
        {
            mat.SetFloat(ShaderIDs.LitDirStencilRef, 32);
        }
        else
        {
            mat.SetFloat(ShaderIDs.LitDirStencilRef, 0);
        }
    }

    public void SetRoleLitDirStencilRef(MaterialPropertyBlock propertyBlock)
    {
        if (ShaderUtils.GetGlobalPostprocessingData().renderingMode == RenderingMode.Deferred)
        {
            propertyBlock.SetFloat(ShaderIDs.LitDirStencilRef, 32);
        }
        else
        {
            propertyBlock.SetFloat(ShaderIDs.LitDirStencilRef, 0);
        }
    }

    /// <summary>
    /// 敌人透明度控制
    /// </summary>
    public void UpdateTransparentAlpha(bool useEnemyInvisible, Material[] materials, float Alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (useEnemyInvisible)
            {
                materials[i].SetColor("_MainColor", new Color(1, 1, 1, Alpha));
            }
        }
    }

    public void UpdateTransparentAlpha(bool useEnemyInvisible, MaterialPropertyBlock propertyBlock, float Alpha)
    {
        if (useEnemyInvisible)
        {
            propertyBlock.SetColor("_MainColor", new Color(1, 1, 1, Alpha));
        }
    }


    /// <summary>
    /// 因为半透明不写入深度值，我们将RimLight改成普通边缘光
    /// </summary>
    public void SetCharacterTranspatentRimCtrl(bool state, ref bool useCustomRimLightWidth, Material material)
    {
        if (state)
        {
            //  透明
            useCustomRimLightWidth = true;
            material.SetFloat(ShaderIDs.useSSRimLight, 0);
        }
        else
        {
            //  不透明
            useCustomRimLightWidth = false;
            material.SetFloat(ShaderIDs.useSSRimLight, Convert.ToInt32(GlobalConfig.useSSRimLight));
        }
    }

    /// <summary>
    /// 设置角色自发光颜色
    /// </summary>
    /// <param name="material"></param>
    public void SetEnemyEmissionStrength(float TargetStrength)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.isBody)
            {
                Color baseColor;
                float strength = 0;
                ShaderUtils.DecomposeHdrColor(child.sharedMaterial.GetColor(ShaderIDs.EmissionColor), out baseColor,
                    out strength);

                float PreColorFactory = math.pow(2, strength);
                baseColor = new Color(
                    child.sharedMaterial.GetColor(ShaderIDs.EmissionColor).r / PreColorFactory,
                    child.sharedMaterial.GetColor(ShaderIDs.EmissionColor).g / PreColorFactory,
                    child.sharedMaterial.GetColor(ShaderIDs.EmissionColor).b / PreColorFactory,
                    1
                );

                float ColorFactory = math.pow(2, TargetStrength);
                Color emission = new Color(
                    baseColor.r * ColorFactory,
                    baseColor.g * ColorFactory,
                    baseColor.b * ColorFactory,
                    baseColor.a * ColorFactory
                );

                child.sharedMaterial.SetColor(ShaderIDs.EmissionColor, emission);
            }
        }
    }

    public float GetCurrentEnemyEmissionStrength()
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (child.isBody)
            {
                Color baseColor;
                float strength = 0;
                ShaderUtils.DecomposeHdrColor(child.sharedMaterial.GetColor(ShaderIDs.EmissionColor), out baseColor,
                    out strength);
                return strength;
            }
        }

        return 0;
    }

    public void SetCharacterNoiseTexture()
    {
        if (Application.isPlaying)
        {
            Texture2D dissolve = Resources.Load<Texture2D>(ShaderIDs.dissolveTex);

            foreach (CharacterRender child in MaterialList)
            {
                if (child.sharedMaterials.Length > 0)
                {
                    for (int i = 0; i < child.sharedMaterials.Length; i++)
                    {
                        if (dissolve != null)
                            child._renderer.materials[i].SetTexture(ShaderIDs.NoiseTex, dissolve);
                    }
                }
            }
        }
    }
}

public class CharacterRender
{
    public Renderer _renderer;
    public Material sharedMaterial;
    public Material[] sharedMaterials;

    public bool isBody;
    public bool isHair;
    public bool isFace;
    public bool isWeapon;
    public bool isShadow;
    public bool isEye;
}