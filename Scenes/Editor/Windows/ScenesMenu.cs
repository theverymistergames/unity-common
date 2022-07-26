using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;

namespace MisterGames.Scenes.Editor.Windows {
    
    public static class ScenesMenu {

        [MenuItem("MisterGames/Tools/Refresh Scenes Storage")]
        private static void RefreshScenesStorage() {
            ScenesStorage.Instance.Refresh();
        }

        [MenuItem("MisterGames/Tools/Include all scenes in build settings")]
        internal static void IncludeAllScenesInBuildSettings() {
            EditorBuildSettings.scenes = AssetDatabase
                .FindAssets($"a:assets t:{nameof(SceneAsset)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
        }
    }
    
}