#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace MisterGames.Logic.ShadersWarmup {
    
    internal static class ShadersWarmupMenu {
        
        [MenuItem("MisterGames/Shaders Warmup/Collect shaders and vfx assets for release collection")]
        private static void CollectTracedShadersAndVfxAssetsIntoReleaseCollection() {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ShaderWarmupSettings)}");

            if (guids == null || guids.Length == 0) {
                Debug.LogError($"{nameof(ShaderWarmupSettings)} is not found. " +
                               $"Create {nameof(ShaderWarmupSettings)} in Create/MisterGames/Shaders/{nameof(ShaderWarmupSettings)}");
                return;
            }
            
            if (guids.Length > 1) {
                Debug.LogError($"Found multiple instances of {nameof(ShaderWarmupSettings)}. " +
                               $"There should be only one instance of {nameof(ShaderWarmupSettings)}.");
                return;
            }
            
            var asset = AssetDatabase.LoadAssetAtPath<ShaderWarmupSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

            asset.SearchVisualEffectAssets();
            asset.AppendTracedShadersToReleaseCollection();
        }
    }
    
}

#endif