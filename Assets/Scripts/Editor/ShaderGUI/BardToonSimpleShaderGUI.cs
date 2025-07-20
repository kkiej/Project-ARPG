using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public class BardToonSimpleShaderGUI : ShaderGUI
{
    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] {GUILayout.Width(130)};
    public GUILayoutOption[] middleButtonStyle = new GUILayoutOption[] {GUILayout.Width(130)};
    public GUILayoutOption[] bigButtonStyle = new GUILayoutOption[] {GUILayout.Width(180)};

    private MaterialEditor m_MaterialEditor;

    static bool _TextureSettings_Foldout = false;

    static bool _OutLineSettings_Foldout = false;

    static bool _ShadowSettings_Foldout = false;

    static bool _RimLightSettings_Foldout = false;

    static bool _EmissionSettings_Foldout = false;

    static bool _SpecularSettings_Foldout = false;

    private MaterialProperty MainTex = null;
    private MaterialProperty MainTex02 = null;
    private MaterialProperty CelTex = null;
    private MaterialProperty CelTex02 = null;
    private MaterialProperty BlendTex = null;
    private MaterialProperty BlendTex02 = null;

    private MaterialProperty UseSmoothNormal = null;
    private MaterialProperty OutlineColor = null;
    private MaterialProperty OutlineWidth = null;
    private MaterialProperty OutLineOffsetZ = null;

    private MaterialProperty EmissionColor = null;

    private MaterialProperty RimLightColor = null;
    private MaterialProperty RimRangeStep = null;
    private MaterialProperty RimFeather = null;

    private MaterialProperty SpecColor = null;
    private MaterialProperty SpecFalloff = null;

    private MaterialProperty ShadowRangeStep = null;
    private MaterialProperty ShadowFeather = null;

    private MaterialProperty Dissolve_power1 = null;
    private MaterialProperty Dissolve_power2 = null;
    private MaterialProperty EdgeColor2 = null;
        
    public void FindProperties(MaterialProperty[] props)
    {
        MainTex = FindProperty(BardToonSimpleIDs.MainTex, props, false);
        MainTex02 = FindProperty(BardToonSimpleIDs.MainTex02, props, false);
        CelTex = FindProperty(BardToonSimpleIDs.CelTex, props, false);
        CelTex02 = FindProperty(BardToonSimpleIDs.CelTex02, props, false);
        BlendTex = FindProperty(BardToonSimpleIDs.BlendTex, props, false);
        BlendTex02 = FindProperty(BardToonSimpleIDs.BlendTex02, props, false);

        UseSmoothNormal = FindProperty(BardToonSimpleIDs.UseSmoothNormal, props, false);
        OutlineColor = FindProperty(BardToonSimpleIDs.OutlineColor, props, false);
        OutlineWidth = FindProperty(BardToonSimpleIDs.OutlineWidth, props, false);
        OutLineOffsetZ = FindProperty(BardToonSimpleIDs.OutLineOffsetZ, props, false);

        EmissionColor = FindProperty(BardToonSimpleIDs.EmissionColor, props, false);

        RimLightColor = FindProperty(BardToonSimpleIDs.RimLightColor, props, false);
        RimRangeStep = FindProperty(BardToonSimpleIDs.RimRangeStep, props, false);
        RimFeather = FindProperty(BardToonSimpleIDs.RimFeather, props, false);

        SpecFalloff = FindProperty(BardToonSimpleIDs.SpecFalloff, props, false);
        SpecColor = FindProperty(BardToonSimpleIDs.SpecColor, props, false);

        ShadowRangeStep = FindProperty(BardToonSimpleIDs.ShadowRangeStep, props, false);
        ShadowFeather = FindProperty(BardToonSimpleIDs.ShadowFeather, props, false);

        Dissolve_power1 = FindProperty(BardToonSimpleIDs.Dissolve_power1, props, false);
        Dissolve_power2 = FindProperty(BardToonSimpleIDs.Dissolve_power2, props, false);
        EdgeColor2 = FindProperty(BardToonSimpleIDs.EdgeColor2, props, false);
    }

    private static class Styles
    {
        public static GUIContent MainTexText = new GUIContent("Base Texture", "Base Color : Texture(RGB) × Color(RGB) Default:White");

        public static GUIContent MainTex02Text = new GUIContent("Base Texture(02)", "Base Color : Texture(RGB) × Color(RGB) Default:White");

        public static GUIContent CelTexText = new GUIContent("Cel Texture", "Cel Texture : Texture(RGBA) Default:White");
        
        public static GUIContent CelTex02Text = new GUIContent("Cel Texture(02)", "Cel Texture : Texture(RGBA) Default:White");

        public static GUIContent BlendTexText = new GUIContent("Blend Texture", "Blend Texture : Texture(RGBA)");
        
        public static GUIContent BlendTex02Text = new GUIContent("Blend Texture(02)", "Blend Texture : Texture(RGBA)");
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
        if (_TextureSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_TextureSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        //OutLineSetting 
        _OutLineSettings_Foldout = Foldout(_OutLineSettings_Foldout, "【OutLine Settings】");
        if (_OutLineSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_OutLineSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        //ShadowSetting 
        _ShadowSettings_Foldout = Foldout(_ShadowSettings_Foldout, "【Shadow Settings】");
        if (_ShadowSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_ShadowSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        //RimLightSetting 
        _RimLightSettings_Foldout = Foldout(_RimLightSettings_Foldout, "【RimLight Settings】");
        if (_RimLightSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_RimLightSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        //EmissionSetting 
        _EmissionSettings_Foldout = Foldout(_EmissionSettings_Foldout, "【Emission Settings】");
        if (_EmissionSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_EmissionSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        //SpecularSetting 
        _SpecularSettings_Foldout = Foldout(_SpecularSettings_Foldout, "【Specular Settings】");
        if (_SpecularSettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_SpecularSettings(material);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        
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
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Alpha Clip");
        
        if(material.GetFloat("_UseAlphaClip") > 0.5){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat("_UseAlphaClip",0);
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat("_UseAlphaClip",1);
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
        
        EditorGUI.indentLevel++;
        m_MaterialEditor.TexturePropertySingleLine(Styles.MainTexText, MainTex);
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
        GUILayout.Space(20);
        
        GUILayout.Label("Texture(02)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Texture(02) Active");
        
        GUILayout.Space(10);
        if(material.IsKeywordEnabled("_USE_MAIN_TEXTURE_LERP")){
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.DisableKeyword("_USE_MAIN_TEXTURE_LERP");
            }
        }else{
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.EnableKeyword("_USE_MAIN_TEXTURE_LERP");
            }
        }
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();

        if (material.IsKeywordEnabled("_USE_MAIN_TEXTURE_LERP"))
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.MainTex02Text, MainTex02);
            // m_MaterialEditor.TextureScaleOffsetProperty(MainTex02);  
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
            
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.CelTex02Text, CelTex02);
            // m_MaterialEditor.TextureScaleOffsetProperty(CelTex02);  
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
        
            EditorGUI.indentLevel++;
            m_MaterialEditor.TexturePropertySingleLine(Styles.BlendTex02Text, BlendTex02);
            // m_MaterialEditor.TextureScaleOffsetProperty(BlendTex02);  
            EditorGUI.indentLevel--;
            GUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Dissolve_power1, "Dissolve Scale");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.RangeProperty(Dissolve_power2, "Edge Scale");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.ColorProperty(EdgeColor2, "Edge Color");
            GUILayout.Space(60);
            EditorGUILayout.EndHorizontal();
        }
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
    }
    #endregion
    
    #region GUI_RimLightSettings
    void GUI_RimLightSettings(Material material)
    {
        GUILayout.Label("RimLight Settings : Color × Bool × Float", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(RimLightColor, "RimLight Color");
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        EditorGUI.indentLevel++;
        m_MaterialEditor.RangeProperty(RimRangeStep, "RimLight Step");
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel++;
        m_MaterialEditor.RangeProperty(RimFeather, "RimLight Feather");
        EditorGUI.indentLevel--;
    }
    #endregion
    
    #region GUI_EmissionSettings
    void GUI_EmissionSettings(Material material)
    {
        EditorGUI.indentLevel++;
        m_MaterialEditor.ColorProperty(EmissionColor,"Emission Color");
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
    }
    #endregion
}

public class BardToonSimpleIDs
{
    //Texture Settings 
    public static readonly string MainTex = "_MainTex";
    public static readonly string MainTex02 = "_MainTex02";
    public static readonly string CelTex = "_CelTex";
    public static readonly string CelTex02 = "_CelTex02";
    public static readonly string BlendTex = "_BlendTex";
    public static readonly string BlendTex02 = "_BlendTex02";

    //OutLine Setting
    public static readonly string UseSmoothNormal = "_UseSmoothNormal";
    public static readonly string OutlineColor = "_OutlineColor";
    public static readonly string OutlineWidth = "_OutlineWidth";
    public static readonly string OutLineOffsetZ = "_Offset_Z";

    //Specular Setting
    public static readonly string EmissionColor = "_EmissionColor";

    //RimLight Setting
    public static readonly string RimLightColor = "_RimLightColor";
    public static readonly string RimRangeStep = "_RimRangeStep";
    public static readonly string RimFeather = "_RimFeather";

    //Specular Setting
    public static readonly string SpecFalloff = "_SpecFalloff";
    public static readonly string SpecColor = "_SpecColor";

    //Shadow Setting
    public static readonly string ShadowRangeStep = "_ShadowRangeStep";
    public static readonly string ShadowFeather = "_ShadowFeather";

    //MainTex Lerp
    public static readonly string Dissolve_power1 = "_Dissolve_power1";
    public static readonly string Dissolve_power2 = "_Dissolve_power2";
    public static readonly string EdgeColor2 = "_EdgeColor2";
}