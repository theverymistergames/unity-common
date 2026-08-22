using UnityEditor;

namespace MisterGames.Logic.Editor.ShadersWarmup {

    internal static class ShadersWarmupMenu {

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Collect shaders and vfx assets for release collection", priority = 0)]
        private static void CollectTracedShadersAndVfxAssetsIntoReleaseCollection() {
            if (!ShaderWarmupEditorUtils.TryFindSettings(out var settings)) return;

            settings.SearchVisualEffectAssets();
            settings.AppendTracedShadersToReleaseCollection();

            // New variants bring new shaders: keys are collected right away, otherwise warmup takes them
            // by a direct reference, that is not the copy the game renders with.
            ShaderWarmupAddressableKeys.Collect(settings);
        }
    }

}
