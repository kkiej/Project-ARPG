using System;
using UnityEditor;
using UnityEngine;
using static vietlabs.fr2.FR2_Scope;

namespace vietlabs.fr2
{
    internal class FR2_DeleteButton
    {
        public string confirmMessage;
        public GUIContent deleteLabel;
        public bool hasConfirm;
        public string warningMessage;

        public bool Draw(Action onConfirmDelete)
        {
            using (HzLayout())
            {
                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
                using (VtLayout())
                {
                    GUILayout.Space(2f);
                    hasConfirm = GUILayout.Toggle(hasConfirm, confirmMessage);
                    using (GUIEnable(hasConfirm))
                    {
                        using (FR2_Scope.BGColor(GUI2.Alpha(GUI2.darkRed, 0.8f)))
                        {
                            if (GUILayout.Button(deleteLabel, EditorStyles.miniButton))
                            {
                                hasConfirm = false;
                                onConfirmDelete();
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
