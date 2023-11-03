using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Editors {

    [CustomEditor(typeof(BlueprintRunner2))]
    public sealed class BlueprintRunnerEditor2 : UnityEditor.Editor {

        private readonly HashSet<BlueprintAsset2> _visitedBlueprintAssets = new HashSet<BlueprintAsset2>();

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner2 runner) return;

            serializedObject.Update();

            FetchBlackboards(runner);
            DrawBlueprintRunner(runner);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBlueprintRunner(BlueprintRunner2 runner) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_blueprintAsset"));

            var blueprint = runner.BlueprintAsset;

            if (blueprint != null && GUILayout.Button("Edit")) {
                BlueprintEditorWindow.OpenAsset(blueprint);
            }

            if (runner.RuntimeBlueprint != null) {
                if (GUILayout.Button("Interrupt Blueprint")) runner.InterruptRuntimeBlueprint();

                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
                GUILayout.Label("External enter ports", EditorStyles.boldLabel);

                var rootPorts = runner.RuntimeBlueprint.rootPorts;

                foreach ((int sign, var port) in rootPorts) {
                    if (port.IsData() || !port.IsInput()) continue;

                    if (GUILayout.Button(port.Name)) {
                        runner.RuntimeBlueprint.CallRoot(runner.RuntimeBlueprint.Root, sign);
                    }
                }
            }
            else {
                if (blueprint != null && GUILayout.Button("Compile & Start Blueprint")) {
                    runner.RestartBlueprint();
                }
            }

            var blackboardOverrides = serializedObject.FindProperty("_blackboardOverridesMap").FindPropertyRelative("_entries");

            int count = blackboardOverrides.arraySize;
            if (count == 0) return;

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            GUILayout.Label("Blackboards", EditorStyles.boldLabel);

            for (int i = 0; i < count; i++) {
                var entry = blackboardOverrides.GetArrayElementAtIndex(i);

                var ownerBlueprintProperty = entry.FindPropertyRelative("key");
                var blackboardProperty = entry.FindPropertyRelative("value");

                EditorGUILayout.PropertyField(blackboardProperty, new GUIContent("Host"));

                var blackboardRect = GUILayoutUtility.GetLastRect();
                var headerRect = new Rect(
                    blackboardRect.x + EditorGUIUtility.labelWidth,
                    blackboardRect.y,
                    blackboardRect.width - EditorGUIUtility.labelWidth,
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(headerRect, ownerBlueprintProperty, GUIContent.none);
                EditorGUI.EndDisabledGroup();

                if (i < count - 1) GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        private void FetchBlackboards(BlueprintRunner2 runner) {
            var blueprint = runner.BlueprintAsset;

            if (blueprint == null) {
                runner.BlackboardOverridesMap.Clear();
                return;
            }

            _visitedBlueprintAssets.Clear();

            var blackboardOverridesMap = runner.BlackboardOverridesMap;
            FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(runner, blueprint, blackboardOverridesMap);

            var keys = new BlueprintAsset2[blackboardOverridesMap.Keys.Count];
            blackboardOverridesMap.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++) {
                var blueprintAsset = keys[i];
                if (!_visitedBlueprintAssets.Contains(blueprintAsset)) blackboardOverridesMap.Remove(blueprintAsset);
            }

            _visitedBlueprintAssets.Clear();
        }

        private void FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(
            BlueprintRunner2 runner,
            BlueprintAsset2 blueprint,
            SerializedDictionary<BlueprintAsset2, Blackboard> blackboardOverridesMap
        ) {
            if (blueprint == null) {
                if (blackboardOverridesMap.ContainsKey(blueprint)) blackboardOverridesMap.Remove(blueprint);
                return;
            }

            if (_visitedBlueprintAssets.Contains(blueprint)) return;
            _visitedBlueprintAssets.Add(blueprint);

            FetchBlackboardOfBlueprint(runner, blueprint, blackboardOverridesMap);

            foreach (var subgraphAsset in blueprint.BlueprintMeta.SubgraphAssets) {
                FetchBlackboardOfBlueprintAndItsSubgraphsRecursively(runner, subgraphAsset, blackboardOverridesMap);
            }
        }

        private static void FetchBlackboardOfBlueprint(
            BlueprintRunner2 runner,
            BlueprintAsset2 blueprint,
            SerializedDictionary<BlueprintAsset2, Blackboard> blackboardOverridesMap
        ) {
            if (!blackboardOverridesMap.TryGetValue(blueprint, out var blackboardOverride)) {
                blackboardOverride = new Blackboard(blueprint.Blackboard);
                blackboardOverridesMap[blueprint] = blackboardOverride;
                EditorUtility.SetDirty(runner);
                return;
            }

            if (!blackboardOverride.OverrideBlackboard(blueprint.Blackboard)) return;

            EditorUtility.SetDirty(runner.gameObject);
        }
    }

}
