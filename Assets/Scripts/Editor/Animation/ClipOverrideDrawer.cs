using LZ;
using UnityEditor;
using UnityEngine;

namespace LZEditor
{
    [CustomPropertyDrawer(typeof(ClipOverride))]
    public class ClipOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var originalProp = property.FindPropertyRelative("original");
            var replacementProp = property.FindPropertyRelative("replacement");

            var originalClip = originalProp.objectReferenceValue as AnimationClip;
            string clipName = originalClip != null ? originalClip.name : "(empty)";

            float totalWidth = position.width;
            float labelWidth = totalWidth * 0.4f;
            float arrowWidth = 20f;
            float fieldWidth = totalWidth - labelWidth - arrowWidth;

            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            var arrowRect = new Rect(position.x + labelWidth, position.y, arrowWidth, position.height);
            var fieldRect = new Rect(position.x + labelWidth + arrowWidth, position.y, fieldWidth, position.height);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(labelRect, originalClip, typeof(AnimationClip), false);
            EditorGUI.EndDisabledGroup();

            EditorGUI.LabelField(arrowRect, "→");

            EditorGUI.PropertyField(fieldRect, replacementProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
