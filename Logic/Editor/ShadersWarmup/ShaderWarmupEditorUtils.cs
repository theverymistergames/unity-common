using System.Collections.Generic;
using System.Linq;
using MisterGames.Logic.ShadersWarmup;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace MisterGames.Logic.Editor.ShadersWarmup {

    /// <summary>
    /// Shared bits of the shaders warmup editor tools: where their menu lives, how the single settings asset
    /// is found and where addressable entries for shaders are put.
    /// </summary>
    internal static class ShaderWarmupEditorUtils {

        public const string MenuRoot = "MisterGames/Shaders Warmup/";

        /// <summary>
        /// One group for all shaders gives one bundle: while a shader lies in a group next to content, it goes
        /// into that group bundle, and the same shader mentioned in another group goes there as well. The player
        /// then holds two copies of one shader with different instance ids, and warmup warms up the wrong one.
        /// </summary>
        public const string ShaderGroupName = "Shaders";

        /// <summary> How many names to print in a report before cutting the list off with an ellipsis. </summary>
        public const int ReportSampleSize = 12;

        private const string SettingsFilter = "t:" + nameof(ShaderWarmupSettings);

        public static bool TryFindSettings(out ShaderWarmupSettings settings) {
            settings = null;

            string[] guids = AssetDatabase.FindAssets(SettingsFilter);

            if (guids == null || guids.Length == 0) {
                Debug.LogError($"{nameof(ShaderWarmupSettings)} is not found. " +
                               $"Create {nameof(ShaderWarmupSettings)} in Create/MisterGames/Shaders/{nameof(ShaderWarmupSettings)}");
                return false;
            }

            if (guids.Length > 1) {
                Debug.LogError($"Found multiple instances of {nameof(ShaderWarmupSettings)}. " +
                               $"There should be only one instance of {nameof(ShaderWarmupSettings)}.");
                return false;
            }

            settings = AssetDatabase.LoadAssetAtPath<ShaderWarmupSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return settings != null;
        }

        public static AddressableAssetGroup GetOrCreateShaderGroup(AddressableAssetSettings addressableSettings) {
            return addressableSettings.FindGroup(ShaderGroupName)
                   ?? addressableSettings.CreateGroup(
                       ShaderGroupName,
                       false, // setAsDefaultGroup: the default group stays as it was
                       false, // readOnly
                       false, // postEvent: the event is sent once, in the end
                       null,  // schemasToCopy
                       typeof(BundledAssetGroupSchema),
                       typeof(ContentUpdateGroupSchema));
        }

        public static string FormatSample(IEnumerable<string> names) {
            var all = names as IList<string> ?? names.ToList();
            if (all.Count == 0) return "none";

            var sample = all.Take(ReportSampleSize);

            return all.Count > ReportSampleSize
                ? $"{string.Join(", ", sample)}, ..."
                : string.Join(", ", sample);
        }

        public static string FormatSample(IEnumerable<Shader> shaders) {
            return FormatSample(shaders.Select(shader => shader != null ? shader.name : "null"));
        }
    }

}
