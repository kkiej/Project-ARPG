using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public class BardToonFaceShaderGUI : ShaderGUI
{
    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    public GUILayoutOption[] middleButtonStyle = new GUILayoutOption[]{ GUILayout.Width(130) }; 
    public GUILayoutOption[] bigButtonStyle = new GUILayoutOption[]{ GUILayout.Width(180) }; 

    private  MaterialEditor m_MaterialEditor;
    
    static bool _TextureSettings_Foldout = false;

    static bool _OutLineSettings_Foldout = false;
    
    static bool _ShadowSettings_Foldout = false;

    static bool _HUESettings_Foldout = false;
    
    private MaterialProperty MainColor = null;
    private MaterialProperty MainTex = null;
    private MaterialProperty SDFTex = null;

    private MaterialProperty UseSmoothNormal = null;
    private MaterialProperty OutlineColor = null;
    private MaterialProperty OutlineWidth = null;
    private MaterialProperty OutLineOffsetZ = null;
    
    private MaterialProperty ShadowColor = null;
    private MaterialProperty EyeShadowColor = null;
    private MaterialProperty defaultShadowStrength = null;
    private MaterialProperty HairShadowDistace = null;
    private MaterialProperty UseDarkLuminance = null;
    private MaterialProperty DarkLuminance = null;
    
    private MaterialProperty Hue = null;
    private MaterialProperty Saturation = null;
    private MaterialProperty Value = null;
    
    public void FindProperties(MaterialProperty[] props)
    {
        MainColor = FindProperty(BardToonFaceIDs.MainColor, props, false);
        MainTex = FindProperty(BardToonFaceIDs.MainTex, props, false);
        SDFTex = FindProperty(BardToonFaceIDs.SDFTex, props, false);

        UseSmoothNormal = FindProperty(BardToonFaceIDs.UseSmoothNormal, props, false);
        OutlineColor = FindProperty(BardToonFaceIDs.OutlineColor, props, false);
        OutlineWidth = FindProperty(BardToonFaceIDs.OutlineWidth, props, false);
        OutLineOffsetZ = FindProperty(BardToonFaceIDs.OutLineOffsetZ, props, false);
        
        ShadowColor = FindProperty(BardToonFaceIDs.ShadowColor, props, false);
        EyeShadowColor = FindProperty(BardToonFaceIDs.EyeShadowColor, props, false);
        defaultShadowStrength = FindProperty(BardToonFaceIDs.defaultShadowStrength, props, false);
        HairShadowDistace = FindProperty(BardToonFaceIDs.HairShadowDistace, props, false);
        UseDarkLuminance = FindProperty(BardToonBodyIDs.UseDarkLuminance, props, false);
        DarkLuminance = FindProperty(BardToonBodyIDs.DarkLuminance, props, false);
        
        Hue = FindProperty(BardToonFaceIDs.Hue, props, false);
        Saturation = FindProperty(BardToonFaceIDs.Saturation, props, false);
        Value = FindProperty(BardToonFaceIDs.Value, props, false);
    }
    
    private static class Styles
    {
        public static GUIContent MainTexText = new GUIContent("Base Texture","Base Color : Texture(RGB) × Color(RGB) Default:White");
        public static GUIContent SDFTexText = new GUIContent("SDF Texture","SDF Texture : Texture(R) Default:Red");
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

        //HUESetting 
        _HUESettings_Foldout = Foldout(_HUESettings_Foldout, "【HUE Settings】");
        if(_HUESettings_Foldout)
        {
            EditorGUI.indentLevel++;
            GUI_HUESettings(material);
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
        m_MaterialEditor.TexturePropertySingleLine(Styles.SDFTexText, SDFTex);
        m_MaterialEditor.TextureScaleOffsetProperty(SDFTex); 
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
        if(material.GetFloat(BardToonFaceIDs.UseSmoothNormal) > 0.5f)
        {
            if (GUILayout.Button("Off",shortButtonStyle))
            {
                material.SetFloat(BardToonFaceIDs.UseSmoothNormal,0);
            }
        }
        else
        {
            if (GUILayout.Button("Active",shortButtonStyle))
            {
                material.SetFloat(BardToonFaceIDs.UseSmoothNormal,1);
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
        m_MaterialEditor.ColorProperty(ShadowColor, "Shadow Color");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.ColorProperty(EyeShadowColor, "Eye Shadow Color");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(defaultShadowStrength, "Default Shadow Strength");
        GUILayout.Space(60);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_MaterialEditor.RangeProperty(HairShadowDistace, "Front Shadow Offset");
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
}

public class BardToonFaceIDs
{
    //Texture Settings 
    public static readonly string MainColor = "_MainColor";
    public static readonly string MainTex = "_MainTex";
    public static readonly string SDFTex = "_SDFTexture";

    //OutLine Setting
    public static readonly string UseSmoothNormal = "_UseSmoothNormal";
    public static readonly string OutlineColor = "_OutlineColor";
    public static readonly string OutlineWidth = "_OutlineWidth";
    public static readonly string OutLineOffsetZ = "_Offset_Z";
    
    //Shadow Setting
    public static readonly string ShadowColor = "_ShadowColor";
    public static readonly string EyeShadowColor = "_EyeShadowColor";
    public static readonly string defaultShadowStrength = "_defaultShadowStrength";
    public static readonly string HairShadowDistace = "_HairShadowDistace";
    public static readonly string UseDarkLuminance = "_UseDarkLuminance";
    public static readonly string DarkLuminance = "_DarkLuminance";
    
    //HUE Setting
    public static readonly string Hue = "_Hue";
    public static readonly string Saturation = "_Saturation";
    public static readonly string Value = "_Value";
}