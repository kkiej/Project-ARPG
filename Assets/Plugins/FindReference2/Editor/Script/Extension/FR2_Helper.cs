using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace vietlabs.fr2
{
    internal static class FR2_Helper
    {






        public static int StringMatch(string pattern, string input)
        {
            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(input)) return 0;
            
            pattern = pattern.ToLower();
            input = input.ToLower();
            
            if (input.Contains(pattern)) return 100;
            
            int score = 0;
            int patternIdx = 0;
            
            for (int i = 0; i < input.Length && patternIdx < pattern.Length; i++)
            {
                if (input[i] == pattern[patternIdx])
                {
                    score += 10;
                    patternIdx++;
                }
            }
            
            return patternIdx == pattern.Length ? score : 0;
        }

        public static string GetfileSizeString(long fileSize)
        {
            if (fileSize < 1024) return fileSize + " B";
            if (fileSize < 1024 * 1024) return (fileSize / 1024f).ToString("F1") + " KB";
            if (fileSize < 1024 * 1024 * 1024) return (fileSize / (1024f * 1024f)).ToString("F1") + " MB";
            return (fileSize / (1024f * 1024f * 1024f)).ToString("F1") + " GB";
        }
    }
}
