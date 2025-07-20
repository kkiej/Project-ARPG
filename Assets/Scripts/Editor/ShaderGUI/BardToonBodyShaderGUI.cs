using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
public class BardToonBodyShaderGUI : ShaderGUI
{
    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    public GUILayoutOption[] middleButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    public GUILayoutOption[] bigButtonStyle = new GUILayoutOption[]{ GUILayout.Width(180) }; 
    
    private  MaterialEditor m_MaterialEditor;
    
    static bool _TextureSettings_Foldout = false;
    
    static bool _LightSettings_Foldout = false;

    static bool _OutLineSettings_Foldout = false;
    
    static bool _ShadowSettings_Foldout = false;
    
    static bool _RimLightSettings_Foldout = false;
    
    static bool _EmissionSettings_Foldout = false;
    
    static bool _SpecularSettings_Foldout = false;
    
    static bool _HUESettings_Foldout = false;

    static bool _Stocking_Foldout = false;
    
    static bool _Effect_Foldout = false;

    private MaterialProperty MainColor = null;
    private MaterialProperty MainTex = null;
    private MaterialProperty CelTex = null;
    private MaterialProperty BlendTex = null;
    private MaterialProperty EmissionTex = null;
    
    
    private MaterialProperty UseSmoothNormal = null;
    private MaterialProperty OutlineColor = null;
    private MaterialProperty OutlineSkinColor = null;
    private MaterialProperty OutlineWidth = null;
    private MaterialProperty OutLineOffsetZ = null;
    
    private MaterialProperty ShadowRangeStep = null;
    private MaterialProperty ShadowFeather = null;
    private MaterialProperty defaultShadowStrength = null;
    private MaterialProperty UseDarkLuminance = null;
    private MaterialProperty DarkLuminance = null;

    private MaterialProperty RimColor = null;
    private MaterialProperty RimRangeStep = null;
    private MaterialProperty RimFeather = null;
    private MaterialProperty UseSSRim = null;
    private MaterialProperty SSRimScale = null;
    
    private MaterialProperty EmissionColor = null;
    
    private MaterialProperty SpecFalloff = null;
    private MaterialProperty SpecColor = null;
    
    private MaterialProperty MetalScale = null;
    private MaterialProperty SmoothScale = null;
    private MaterialProperty Shiness = null;
    private MaterialProperty CheckLine = null;
    private MaterialProperty SpecStep = null;
    
    private MaterialProperty Hue = null;
    private MaterialProperty Saturation = null;
    private MaterialProperty Value = null;

    private MaterialProperty StockingTex = null;
    private MaterialProperty First_Fresnel_Shadow_Step = null;
    private MaterialProperty First_Fresnel_Shadow_Feather = null;
    private MaterialProperty First_Fresnel_Shadow_Color = null;
    private MaterialProperty Second_Fresnel_Shadow_Step = null;
    private MaterialProperty Second_Fresnel_Shadow_Feather = null;
    private MaterialProperty Second_Fresnel_Shadow_Color = null;
    private MaterialProperty Fresnel_Light_Step = null;
    private MaterialProperty Fresnel_Light_Feather = null;
    private MaterialProperty Fresnel_Light_Color = null;
    
    public void FindProperties(MaterialProperty[] props)
    {
        MainColor = FindProperty(BardToonBodyIDs.MainColor, props, false);
        MainTex = FindProperty(BardToonBodyIDs.MainTex, props, false);
        CelTex = FindProperty(BardToonBodyIDs.CelTex, props, false);
        BlendTex = FindProperty(BardToonBodyIDs.BlendTex, props, false);
        EmissionTex = FindProperty(BardToonBodyIDs.EmissionTex, props, false);

        UseSmoothNormal = FindProperty(BardToonBodyIDs.UseSmoothNormal, props, false);
        OutlineColor = FindProperty(BardToonBodyIDs.OutlineColor, props, false);
        OutlineSkinColor = FindProperty(BardToonBodyIDs.OutlineSkinColor, props, false);
        OutlineWidth = FindProperty(BardToonBodyIDs.OutlineWidth, props, false);
        OutLineOffsetZ = FindProperty(BardToonBodyIDs.OutLineOffsetZ, props, false);
        
        ShadowRangeStep = FindProperty(BardToonBodyIDs.ShadowRangeStep, props, false);
        ShadowFeather = FindProperty(BardToonBodyIDs.ShadowFeather, props, false);
        defaultShadowStrength = FindProperty(BardToonBodyIDs.defaultShadowStrength, props, false);
        UseDarkLuminance = FindProperty(BardToonBodyIDs.UseDarkLuminance, props, false);
        DarkLuminance = FindProperty(BardToonBodyIDs.DarkLuminance, props, false);
        
        RimColor = FindProperty(BardToonBodyIDs.RimColor, props, false);
        RimRangeStep = FindProperty(BardToonBodyIDs.RimRangeStep, props, false);
        RimFeather = FindProperty(BardToonBodyIDs.RimFeather, props, false);
        UseSSRim = FindProperty(BardToonBodyIDs.UseSSRim, props, false);
        SSRimScale = FindProperty(BardToonBodyIDs.SSRimScale, props, false);
        
        EmissionColor = FindProperty(BardToonBodyIDs.EmissionColor, props, false);
        
        SpecFalloff = FindProperty(BardToonBodyIDs.SpecFalloff, props, false);
        SpecColor = FindProperty(BardToonBodyIDs.SpecColor, props, false);
        
        MetalScale = FindProperty(BardToonBodyIDs.MetalScale, props, false);
        SmoothScale = FindProperty(BardToonBodyIDs.SmoothScale, props, false);
        Shiness = FindProperty(BardToonBodyIDs.Shiness, props, false);
        CheckLine = FindProperty(BardToonBodyIDs.CheckLine, props, false);
        SpecStep = FindProperty(BardToonBodyIDs.SpecStep, props, false);
        
        Hue = FindProperty(BardToonBodyIDs.Hue, props, false);
        Saturation = FindProperty(BardToonBodyIDs.Saturation, props, false);
        Value = FindProperty(BardToonBodyIDs.Value, props, false);
        
        StockingTex = FindProperty(BardToonBodyIDs.StockingTex, props, false);
        First_Fresnel_Shadow_Step = FindProperty(BardToonBodyIDs.First_Fresnel_Shadow_Step, props, false);
        First_Fresnel_Shadow_Feather = FindProperty(BardToonBodyIDs.First_Fresnel_Shadow_Feather, props, false);
        First_Fresnel_Shadow_Color = FindProperty(BardToonBodyIDs.First_Fresnel_Shadow_Color, props, false);
        Second_Fresnel_Shadow_Step = FindProperty(BardToonBodyIDs.Second_Fresnel_Shadow_Step, props, false);
        Second_Fresnel_Shadow_Feather = FindProperty(BardToonBodyIDs.Second_Fresnel_Shadow_Feather, props, false);
        Second_Fresnel_Shadow_Color = FindProperty(BardToonBodyIDs.Second_Fresnel_Shadow_Color, props, false);
        Fresnel_Light_Step = FindProperty(BardToonBodyIDs.Fresnel_Light_Step, props, false);
        Fresnel_Light_Feather = FindProperty(BardToonBodyIDs.Fresnel_Light_Feather, props, false);
        Fresnel_Light_Color = FindProperty(BardToonBodyIDs.Fresnel_Light_Color, props, false);
    }
    
    private static class Styles
    {
        public static GUIContent MainTexText = new GUIContent("Base Texture","Base Color : Texture(RGB) × Color(RGB) Default:White");
        public static GUIContent CelTexText = new GUIContent("Cel Texture","Cel Texture : Texture(RGBA) Default:White");
        public static GUIContent BlendTexText = new GUIContent("Blend Texture","Blend Texture : Texture(RGBA)");
        public static GUIContent EmissionTex = new GUIContent("Emission: ","Emission Texture : Texture(R) × Color(RGB)");
        public static GUIContent StockingTex = new GUIContent("Stocking: ","Stocking Texture : Texture(R)");
    }
    
    #region Foldout
    /// <summary>
    /// Foldout
    /// </summary>
    /// <param name="display"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    static bool Foldout(bool display, string title)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 22;
        style.contentOffset = new Vector2(20f, -2f);

        var rect = GUILayoutUtility.GetRect(16f, 22f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }
    #endregion
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        EditorGUIUtility.fieldWidth = 0;
        FindProperties(props);
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;
        
        //LinkButton
        EditorGUILayout.BeginHorizontal();
        OpenManualLink();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        //BasicSettings
        _TextureSettings_Foldout = Foldout(_TextureSettings_Foldout, "【Texture Settings】");
        if(_TextureSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_TextureSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //OutLineSetting 
        _OutLineSettings_Foldout = Foldout(_OutLineSettings_Foldout, "【OutLine Settings】");
        if(_OutLineSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_OutLineSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //ShadowSetting 
        _ShadowSettings_Foldout = Foldout(_ShadowSettings_Foldout, "【Shadow Settings】");
        if(_ShadowSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_ShadowSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //RimLightSetting 
        _RimLightSettings_Foldout = Foldout(_RimLightSettings_Foldout, "【RimLight Settings】");
        if(_RimLightSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_RimLightSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //EmissionSetting 
        _EmissionSettings_Foldout = Foldout(_EmissionSettings_Foldout, "【Emission Settings】");
        if(_EmissionSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_EmissionSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //SpecularSetting 
        _SpecularSettings_Foldout = Foldout(_SpecularSettings_Foldout, "【Specular Settings】");
        if(_SpecularSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_SpecularSettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //HUESetting 
        _HUESettings_Foldout = Foldout(_HUESettings_Foldout, "【HUE Settings】");
        if(_HUESettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_HUESettings(material);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        
        //StockingSetting 
        _Stocking_Foldout = Foldout(_Stocking_Foldout, "【Stocking Settings】");
        if (_Stocking_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_StockingSettings(material);
            EditorGUI.indentLevel--;
        }

        m_MaterialEditor.RenderQueueField();
    }
    
    #region OpenManualLink
    /// <summary>
    /// OpenManualLink
    /// </summary>
    void OpenManualLink()
    {
        if (GUILayout.Button("Toon INTRODUCE",middleButtonStyle))
        {
            Application.OpenURL("http://10.234.80.155:3000/BardStudio/Bard_Client/src/develop/Assets/Art/Bard_3D_Share/Renderer/README.md");
        }
        if (GUILayout.Button("BARD Toon INTRODUCE",bigButtonStyle))
        {
            Application.OpenURL("http://10.234.80.155:3000/BardStudio/Bard_Client/src/develop/Assets/Art/Bard_3D_Share/Renderer/README.md");
        }
        if (GUILayout.Button("Repository Link",middleButtonStyle))
        {
            Application.OpenURL("http://10.234.80.155:3000/");
        }
    }
    #endregion

    #region GUI_TextureSettings
    void GUI_TextureSettings(Material material)
    {
        EditorGUI.indentLevel++;
        m_MaterialEditor.TexturePropertySingleLine(Styles.MainTexText, MainTex, MainColor);
        m_MaterialEditor.TextureScaleOffsetProperty(MainTex);   
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
        
        EditorGUI.indentLevel++;
        m_MaterialEditor.TexturePropertySingleLine(Styles.CelTexText, CelTex);
        m_MaterialEditor.TextureScaleOffsetProperty(CelTex); 
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
        
        EditorGUI.indentLevel++;
        m_MaterialEditor.TexturePropertySingleLine(Styles.BlendTexText, BlendTex);
        m_MaterialEditor.TextureScaleOffsetProperty(BlendTex);   
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Night Color Active");
        if(material.IsKeywordEnabled("NIGHT_ON")){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.DisableKeyword("NIGHT_ON");
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.EnableKeyword("NIGHT_ON");
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_OutLineSettings
    void GUI_OutLineSettings(Material material)
    {
        GUILayout.Label("Outline Settings : Bool × Float", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("SmoothNormal Active");
        if(material.GetFloat(BardToonBodyIDs.UseSmoothNormal) > 0.5f)
        {
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseSmoothNormal,0);
            }
        }
        else
        {
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseSmoothNormal,1);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(OutlineColor, "OutlineColor");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(OutlineSkinColor, "OutlineSkinColor");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(OutlineWidth, "OutlineWidth");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.FloatProperty(OutLineOffsetZ, "OutLineOffsetZ");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_ShadowSettings
    void GUI_ShadowSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(ShadowRangeStep, "Shadow Step");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(ShadowFeather, "Shadow Feather");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(defaultShadowStrength, "Default Shadow Strength");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Dark Part Luminance ");
        if(material.GetFloat(BardToonBodyIDs.UseDarkLuminance) > 0.5f)
        {
            if (GUILayout.Button("Off", shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseDarkLuminance,0);
            }
        }
        else
        {
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseDarkLuminance,1);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        if(material.GetFloat(BardToonBodyIDs.UseDarkLuminance) > 0.5f)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.RangeProperty(DarkLuminance, "Dark Luminance");
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_RimLightSettings
    void GUI_RimLightSettings(Material material)
    {
        GUILayout.Label("RimLight Settings : Color × Bool × Float", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(RimColor, "RimLight Color");
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        GUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("SS-Rim Active");
        if(material.GetFloat(BardToonBodyIDs.UseSSRim) > 0.5f)
        {
            if (GUILayout.Button("Off", shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseSSRim,0);
            }
        }
        else
        {
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(BardToonBodyIDs.UseSSRim,1);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        if(material.GetFloat(BardToonBodyIDs.UseSSRim) > 0.5f)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.RangeProperty(SSRimScale, "SS-Rim Scale");
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.RangeProperty(RimRangeStep, "RimLight Step");
            EditorGUI.indentLevel--;
                
            EditorGUI.indentLevel++;
            m_MaterialEditor.RangeProperty(RimFeather, "RimLight Feather");
            EditorGUI.indentLevel--;
        }
    }
    #endregion

    #region GUI_EmissionSettings
    void GUI_EmissionSettings(Material material)
    {
        EditorGUI.indentLevel++;
        m_MaterialEditor.TexturePropertySingleLine(Styles.EmissionTex, EmissionTex,EmissionColor);
        EditorGUI.indentLevel--;
    }
    #endregion

    #region GUI_SpecularSettings
    void GUI_SpecularSettings(Material material)
    {
        GUILayout.Label("Universal Specular Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(SpecColor, "Specular Color");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(SpecFalloff, "Specular Fall Off");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        GUILayout.Label("Metallic Specular Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(MetalScale, "Metallic");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(SmoothScale, "Smoothness");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(Shiness, "Shiness");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.FloatProperty(CheckLine, "CheckLine");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.FloatProperty(SpecStep, "SpecStep");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region GUI_HUESettings
    void GUI_HUESettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("HSV Active");
        
        if(material.GetFloat("_UseHSV") > 0.5){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat("_UseHSV",0);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat("_UseHSV",1);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.GetFloat("_UseHSV") > 0.5)
        {
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Hue, "HUE");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Saturation, "Saturation");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Value, "Value");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion

    #region GUI_StockingSettings
    void GUI_StockingSettings(Material material)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Stocking Active");
        
        if(material.IsKeywordEnabled("USE_STOCKING")){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.DisableKeyword("USE_STOCKING");
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.EnableKeyword("USE_STOCKING");
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.IsKeywordEnabled("USE_STOCKING"))
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.StockingTex, StockingTex);
            m_MaterialEditor.TextureScaleOffsetProperty(StockingTex);   
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.ColorProperty(First_Fresnel_Shadow_Color, "First Dark Color");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(First_Fresnel_Shadow_Step, "First Dark Step");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(First_Fresnel_Shadow_Feather, "First Dark Feather");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.ColorProperty(Second_Fresnel_Shadow_Color, "Second Dark Color");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Second_Fresnel_Shadow_Step, "Second Dark Step");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Second_Fresnel_Shadow_Feather, "Second Dark Feather");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.ColorProperty(Fresnel_Light_Color, "Bright Color");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Fresnel_Light_Step, "Bright Step");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Fresnel_Light_Feather, "Bright Feather");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion
    
}

public class BardToonBodyIDs
{
    //Texture Settings 
    public static readonly string MainColor = "_MainColor";
    public static readonly string MainTex = "_MainTex";
    public static readonly string CelTex = "_CelTex";
    public static readonly string BlendTex = "_BlendTex";
    public static readonly string EmissionTex = "_EmissionTex";

    //OutLine Setting
    public static readonly string UseSmoothNormal = "_UseSmoothNormal";
    public static readonly string OutlineColor = "_OutlineColor";
    public static readonly string OutlineSkinColor = "_OutlineSkinColor";
    public static readonly string OutlineWidth = "_OutlineWidth";
    public static readonly string OutLineOffsetZ = "_Offset_Z";
    
    //Shadow Setting
    public static readonly string ShadowRangeStep = "_ShadowRangeStep";
    public static readonly string ShadowFeather = "_ShadowFeather";
    public static readonly string defaultShadowStrength = "_defaultShadowStrength";
    public static readonly string UseDarkLuminance = "_UseDarkLuminance";
    public static readonly string DarkLuminance = "_DarkLuminance";
    
    //RimLight Setting
    public static readonly string RimColor = "_RimColor";
    public static readonly string RimRangeStep = "_RimRangeStep";
    public static readonly string RimFeather = "_RimFeather";
    public static readonly string UseSSRim = "_UseSSRim";
    public static readonly string SSRimScale = "_SSRimScale";
    
    //Specular Setting
    public static readonly string EmissionColor = "_EmissionColor";
    
    //Specular Setting
    public static readonly string SpecFalloff = "_SpecFalloff";
    public static readonly string SpecColor = "_SpecColor";
    
    public static readonly string MetalScale = "_MetalScale";
    public static readonly string SmoothScale = "_SmoothScale";
    public static readonly string Shiness = "_Shiness";
    public static readonly string CheckLine = "_CheckLine";
    public static readonly string SpecStep = "_SpecStep";
    
    //HUE Setting
    public static readonly string Hue = "_Hue";
    public static readonly string Saturation = "_Saturation";
    public static readonly string Value = "_Value";
    
    //Stocking Setting
    public static readonly string StockingTex = "_StockingTex";
    public static readonly string First_Fresnel_Shadow_Step = "_First_Fresnel_Shadow_Step";
    public static readonly string First_Fresnel_Shadow_Feather = "_First_Fresnel_Shadow_Feather";
    public static readonly string First_Fresnel_Shadow_Color = "_First_Fresnel_Shadow_Color";
    public static readonly string Second_Fresnel_Shadow_Step = "_Second_Fresnel_Shadow_Step";
    public static readonly string Second_Fresnel_Shadow_Feather = "_Second_Fresnel_Shadow_Feather";
    public static readonly string Second_Fresnel_Shadow_Color = "_Second_Fresnel_Shadow_Color";
    public static readonly string Fresnel_Light_Step = "_Fresnel_Light_Step";
    public static readonly string Fresnel_Light_Feather = "_Fresnel_Light_Feather";
    public static readonly string Fresnel_Light_Color = "_Fresnel_Light_Color";
}
