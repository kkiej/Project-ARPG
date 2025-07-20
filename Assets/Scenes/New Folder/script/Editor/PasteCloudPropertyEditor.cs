﻿// @author : xue
// @created : 2023,09,01,14:26
// @desc:

using UnityEditor;
using UnityEngine;

namespace Scenes.shaderDevelop_2023_08_10.原神云渲染.script.Editor
{
    [CustomEditor(typeof(PasteCloudProperty))]
    public class PasteCloudPropertyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Paste"))
            {
                PasteCloudProperty pasteCloudProperty = target as PasteCloudProperty;
                pasteCloudProperty.Paste();
            }
        }
    }
}