using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintRunner))]
    public sealed class BlueprintRunnerEditor : UnityEditor.Editor {

        private readonly HashSet<BlueprintAsset> _visitedBlueprintAssets = new HashSet<BlueprintAsset>();

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner runner) return;

            serializedObject.Update();

            FetchBlackboards(runner);
            DrawBlueprintRunner(runner);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBlueprintRunner(BlueprintRunner runner) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_blueprintAsset"));

            var blueprint = runner.BlueprintAsset;
            if (blueprint == null) {
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

            var blackboardOverrides = serializedObject.FindProperty("_blackboardOverridesMap").FindPropertyRelative("_entries");

            int count = blackboardOverrides.arraySize;
            if (count == 0) return;

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            GUILayout.Label("Blackboards", EditorStyles.boldLabel);

            for (int i = 0; i < count; i++) {
                var entry = blackboardOverrides.GetArrayElementAtIndex(i);

                var ownerBlueprint = entry.FindPropertyRelative("key");
                var blackboard = entry.FindPropertyRelative("value");

                EditorGUILayout.PropertyField(blackboard, new GUIContent("Blackboard Of Blueprint"));

                var blackboardRect = GUILayoutUtility.GetLastRect();
                var headerRect = new Rect(
                    blackboardRect.x + EditorGUIUtility.labelWidth,
                    blackboardRect.y,
                    blackboardRect.width - EditorGUIUtility.labelWidth,
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(headerRect, ownerBlueprint, GUIContent.none);
                EditorGUI.EndDisabledGroup();

                if (i < count - 1) GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        private void FetchBlackboards(BlueprintRunner runner) {
            var blueprint = runner.BlueprintAsset;

            if (blueprint == null) {
                runner.BlackboardOverridesMap.Clear();
                return;
            }

            _visitedBlueprintAssets.Clear();

            var blackboardOverridesMap = runner.BlackboardOverridesMap;
            FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(blueprint, blackboardOverridesMap);

            var keys = new BlueprintAsset[blackboardOverridesMap.Keys.Count];
            blackboardOverridesMap.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++) {
                var blueprintAsset = keys[i];
                if (!_visitedBlueprintAssets.Contains(blueprintAsset)) blackboardOverridesMap.Remove(blueprintAsset);
            }

            _visitedBlueprintAssets.Clear();
        }

        private void FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, Blackboard> blackboardOverridesMap
        ) {
            if (blueprint == null || _visitedBlueprintAssets.Contains(blueprint)) return;

            _visitedBlueprintAssets.Add(blueprint);

            FetchBlackboard(blueprint, blackboardOverridesMap);

            foreach (var subgraphAsset in blueprint.BlueprintMeta.SubgraphReferencesMap.Values) {
                FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(subgraphAsset, blackboardOverridesMap);
            }
        }

        private static void FetchBlackboard(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, Blackboard> blackboardOverridesMap
        ) {
            var blackboard = blueprint.Blackboard;
            var propertiesMap = blackboard.PropertiesMap;

            if (propertiesMap.Count == 0) {
                if (blackboardOverridesMap.ContainsKey(blueprint)) blackboardOverridesMap.Remove(blueprint);
                return;
            }

            if (!blackboardOverridesMap.TryGetValue(blueprint, out var blackboardOverride)) {
                blackboardOverride = new Blackboard();
                blackboardOverridesMap[blueprint] = blackboardOverride;
            }

            var overridenPropertiesMap = blackboardOverride.PropertiesMap;
            int overridenPropertiesCount = overridenPropertiesMap.Count;

            int[] hashes = overridenPropertiesCount > 0 ? new int[overridenPropertiesCount] : Array.Empty<int>();
            overridenPropertiesMap.Keys.CopyTo(hashes, 0);

            for (int i = 0; i < hashes.Length; i++) {
                int hash = hashes[i];

                if (propertiesMap.TryGetValue(hash, out var property) &&
                    overridenPropertiesMap.TryGetValue(hash, out var overridenProperty) &&
                    Blackboard.GetPropertyType(property) == Blackboard.GetPropertyType(overridenProperty)
                ) {
                    continue;
                }

                blackboardOverride.RemoveProperty(hash);
            }

            foreach ((int hash, var property) in propertiesMap) {
                if (!overridenPropertiesMap.ContainsKey(hash) && blackboard.TryGetPropertyValue(hash, out object blackboardValue)) {
                    blackboardOverride.TryAddProperty(property.name, Blackboard.GetPropertyType(property), out _);
                    blackboardOverride.TrySetPropertyValue(hash, blackboardValue);
                }

                blackboardOverride.TrySetPropertyIndex(hash, property.index);
            }
        }
    }

}
