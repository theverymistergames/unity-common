using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Data;
using MisterGames.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintRunner))]
    public sealed class BlueprintRunnerEditor : UnityEditor.Editor {

        private readonly HashSet<BlueprintAsset> _visitedBlueprintAssets = new HashSet<BlueprintAsset>();

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner runner) return;

            serializedObject.Update();

            DrawBlueprintRunner(runner);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBlueprintRunner(BlueprintRunner runner) {
            var blueprintAssetProperty = serializedObject.FindProperty("_blueprintAsset");
            EditorGUILayout.PropertyField(blueprintAssetProperty);

            var blueprint = runner.BlueprintAsset;
            if (blueprint == null) {
                runner.BlackboardOverridesMap.Clear();

                if (runner.IsRunningRuntimeBlueprint && GUILayout.Button("Interrupt Blueprint")) {
                    runner.InterruptRuntimeBlueprint();
                }
                return;
            }

            if (GUILayout.Button("Edit")) {
                BlueprintsEditorWindow.OpenAsset(blueprint);
            }

            if (runner.IsRunningRuntimeBlueprint) {
                if (GUILayout.Button("Interrupt Blueprint")) {
                    runner.InterruptRuntimeBlueprint();
                }
            }
            else {
                if (GUILayout.Button("Compile & Start Blueprint")) {
                    runner.CompileAndStartRuntimeBlueprint();
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("Blackboards", EditorStyles.boldLabel);

            _visitedBlueprintAssets.Clear();

            var blackboardOverridesMap = runner.BlackboardOverridesMap;
            DrawBlackboardPropertiesRecursively(blueprint, blackboardOverridesMap);

            var keys = new BlueprintAsset[blackboardOverridesMap.Keys.Count];
            blackboardOverridesMap.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++) {
                var blueprintAsset = keys[i];
                if (!_visitedBlueprintAssets.Contains(blueprintAsset)) blackboardOverridesMap.Remove(blueprintAsset);
            }

            _visitedBlueprintAssets.Clear();
        }

        private void DrawBlackboardPropertiesRecursively(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, Blackboard> blackboardOverridesMap
        ) {
            if (blueprint == null) return;

            _visitedBlueprintAssets.Add(blueprint);

            DrawBlackboardProperties(blueprint, blackboardOverridesMap);

            GUILayout.Space(10);

            foreach (var subgraphAsset in blueprint.BlueprintMeta.SubgraphReferencesMap.Values) {
                DrawBlackboardPropertiesRecursively(subgraphAsset, blackboardOverridesMap);
            }
        }

        private static void DrawBlackboardProperties(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, Blackboard> blackboardOverridesMap
        ) {
            var blackboard = blueprint.Blackboard;
            var propertiesMap = blackboard.PropertiesMap;

            if (propertiesMap.Count == 0) {
                if (blackboardOverridesMap.ContainsKey(blueprint)) blackboardOverridesMap.Remove(blueprint);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Blueprint Asset", blueprint, typeof(BlueprintAsset), false);
            EditorGUI.EndDisabledGroup();

            if (blackboardOverridesMap.TryGetValue(blueprint, out var blackboardOverride)) {
                var overridePropertiesMap = blackboardOverride.PropertiesMap;

                int[] hashes = new int[overridePropertiesMap.Count];
                overridePropertiesMap.Keys.CopyTo(hashes, 0);

                for (int i = 0; i < hashes.Length; i++) {
                    int hash = hashes[i];
                    if (!propertiesMap.ContainsKey(hash)) blackboardOverride.RemoveProperty(hash);
                }
            }

            var properties = propertiesMap.OrderBy(p => p.Value.index).ToList();

            for (int i = 0; i < properties.Count; i++) {
                (int hash, var property) = properties[i];
                var type = Blackboard.GetPropertyType(property);

                bool hasOverride = blackboardOverride != null && blackboardOverride.PropertiesMap.ContainsKey(hash);

                blackboard.TryGetPropertyValue(hash, out object blackboardValue);
                object value = hasOverride && blackboardOverride.TryGetPropertyValue(hash, out object blackboardOverrideValue)
                    ? blackboardOverrideValue
                    : blackboardValue;

                var currentFontStyle = EditorStyles.label.fontStyle;
                if (hasOverride) EditorStyles.label.fontStyle = FontStyle.Bold;

                object result = BlackboardUtils.BlackboardField(type, value, property.name);

                if (hasOverride) EditorStyles.label.fontStyle = currentFontStyle;

                var comparer = BlackboardUtils.GetEqualityComparer(type);

                if (hasOverride && comparer.Equals(result, blackboardValue)) {
                    blackboardOverride.RemoveProperty(hash);
                    continue;
                }

                if (comparer.Equals(result, value)) continue;

                if (blackboardOverride == null) {
                    blackboardOverride = new Blackboard();
                    blackboardOverridesMap[blueprint] = blackboardOverride;
                }

                if (!blackboardOverride.PropertiesMap.ContainsKey(hash)) {
                    blackboardOverride.TryAddProperty(property.name, type, out _);
                }

                blackboardOverride.TrySetPropertyValue(hash, result);
            }

            if (blackboardOverridesMap.ContainsKey(blueprint) && (blackboardOverride == null || blackboardOverride.PropertiesMap.Count == 0)) {
                blackboardOverridesMap.Remove(blueprint);
            }
        }
    }

}
