using UnityEngine;
using UnityEditor;
using System.IO;

namespace LZ
{
    /// <summary>
    /// 右键 Humanoid AnimationClip → "Create Mirrored Clip"
    /// 生成一份左右互换的镜像动画资产（.anim），
    /// 替代 AnimatorController State 上的 Mirror 勾选框。
    ///
    /// 原理：交换 Left/Right 肌肉通道，取反中心骨骼横向通道和根运动。
    /// </summary>
    public static class AnimationClipMirrorTool
    {
        [MenuItem("Assets/Create Mirrored Clip", validate = true)]
        private static bool Validate()
        {
            return Selection.activeObject is AnimationClip clip && clip.isHumanMotion;
        }

        [MenuItem("Assets/Create Mirrored Clip")]
        private static void Execute()
        {
            var source = Selection.activeObject as AnimationClip;
            if (source == null) return;

            var mirrored = new AnimationClip { frameRate = source.frameRate };

            foreach (var binding in AnimationUtility.GetCurveBindings(source))
            {
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                var (newBinding, negate) = MirrorBinding(binding);

                if (negate)
                    curve = NegateCurve(curve);

                AnimationUtility.SetEditorCurve(mirrored, newBinding, curve);
            }

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
            {
                AnimationUtility.SetObjectReferenceCurve(
                    mirrored, binding,
                    AnimationUtility.GetObjectReferenceCurve(source, binding));
            }

            var settings = AnimationUtility.GetAnimationClipSettings(source);
            AnimationUtility.SetAnimationClipSettings(mirrored, settings);
            AnimationUtility.SetAnimationEvents(mirrored, AnimationUtility.GetAnimationEvents(source));
            mirrored.EnsureQuaternionContinuity();

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string dir = Path.GetDirectoryName(sourcePath);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(dir, source.name + "_Mirror.anim"));

            AssetDatabase.CreateAsset(mirrored, newPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = mirrored;

            Debug.Log($"Mirrored clip created: {newPath}");
        }

        private static (EditorCurveBinding binding, bool negate) MirrorBinding(EditorCurveBinding binding)
        {
            var result = binding;
            bool negate = false;
            string prop = binding.propertyName;

            if (binding.type != typeof(Animator))
                return (result, false);

            // 1) 交换左右肢体肌肉通道（值不变）
            if (prop.StartsWith("Left "))
                result.propertyName = "Right " + prop.Substring(5);
            else if (prop.StartsWith("Right "))
                result.propertyName = "Left " + prop.Substring(6);
            // 2) 中心骨骼横向 / 扭转通道取反
            else if (prop.Contains("Left-Right") || prop.Contains("Twist"))
                negate = true;

            // 3) 根运动镜像：反转横向位移和 Y/Z 旋转分量
            if (prop == "RootT.x" || prop == "MotionT.x")
                negate = true;
            if (prop == "RootQ.y" || prop == "RootQ.z" ||
                prop == "MotionQ.y" || prop == "MotionQ.z")
                negate = true;

            return (result, negate);
        }

        private static AnimationCurve NegateCurve(AnimationCurve curve)
        {
            var keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = -keys[i].value;
                keys[i].inTangent = -keys[i].inTangent;
                keys[i].outTangent = -keys[i].outTangent;
            }
            return new AnimationCurve(keys);
        }
    }
}
