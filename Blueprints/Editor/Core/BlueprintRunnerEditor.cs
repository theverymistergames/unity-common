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

            DrawBlueprintRunner(runner);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBlueprintRunner(BlueprintRunner runner) {
            var blueprintAssetProperty = serializedObject.FindProperty("_blueprintAsset");
            EditorGUILayout.PropertyField(blueprintAssetProperty);

            var blueprint = runner.BlueprintAsset;
            if (blueprint == null) {
                runner.SceneReferencesMap.Clear();
                return;
            }

            if (GUILayout.Button("Edit")) {
                BlueprintsEditorWindow.OpenAsset(blueprint);
            }

            GUILayout.Space(10);

            GUILayout.Label("Blackboard scene references", EditorStyles.boldLabel);

            _visitedBlueprintAssets.Clear();

            var sceneReferencesMap = runner.SceneReferencesMap;
            DrawBlackboardGameObjectPropertiesRecursively(blueprint, sceneReferencesMap);

            var keys = new BlueprintAsset[sceneReferencesMap.Keys.Count];
            sceneReferencesMap.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++) {
                var blueprintAsset = keys[i];
                if (!_visitedBlueprintAssets.Contains(blueprintAsset)) sceneReferencesMap.Remove(blueprintAsset);
            }

            _visitedBlueprintAssets.Clear();
        }

        private void DrawBlackboardGameObjectPropertiesRecursively(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> sceneReferencesMap
        ) {
            if (blueprint == null) return;

            _visitedBlueprintAssets.Add(blueprint);

            DrawBlackboardGameObjectProperties(blueprint, sceneReferencesMap);

            GUILayout.Space(10);

            foreach (var subgraphAsset in blueprint.BlueprintMeta.SubgraphReferencesMap.Values) {
                DrawBlackboardGameObjectPropertiesRecursively(subgraphAsset, sceneReferencesMap);
            }
        }

        private static void DrawBlackboardGameObjectProperties(
            BlueprintAsset blueprint,
            SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> sceneReferencesMap
        ) {
            var blackboard = blueprint.Blackboard;
            var blackboardGameObjectsMap = blackboard.GameObjects;

            if (blackboardGameObjectsMap.Count == 0) {
                if (sceneReferencesMap.ContainsKey(blueprint)) sceneReferencesMap.Remove(blueprint);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Blackboard of Blueprint", blueprint, typeof(BlueprintAsset), false);
            EditorGUI.EndDisabledGroup();

            if (sceneReferencesMap.TryGetValue(blueprint, out var gameObjectsMap)) {
                int[] keys = new int[gameObjectsMap.Keys.Count];
                gameObjectsMap.Keys.CopyTo(keys, 0);

                for (int i = 0; i < keys.Length; i++) {
                    int hash = keys[i];
                    if (!blackboardGameObjectsMap.ContainsKey(hash)) gameObjectsMap.Remove(hash);
                }
            }

            var blackboardPropertiesMap = blackboard.PropertiesMap;

            foreach ((int hash, var gameObject) in blackboardGameObjectsMap) {
                string propertyName = blackboardPropertiesMap[hash].name;

                if (gameObject != null) {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(propertyName, gameObject, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();

                    if (gameObjectsMap != null && gameObjectsMap.ContainsKey(hash)) gameObjectsMap.Remove(hash);
                    continue;
                }

                var gameObjectRef = gameObjectsMap != null && gameObjectsMap.TryGetValue(hash, out var go) ? go : null;
                var obj = EditorGUILayout.ObjectField(propertyName, gameObjectRef, typeof(GameObject), true) as GameObject;

                if (obj == null) {
                    if (gameObjectsMap != null && gameObjectsMap.ContainsKey(hash)) gameObjectsMap.Remove(hash);
                    continue;
                }

                if (gameObjectsMap == null) {
                    gameObjectsMap = new SerializedDictionary<int, GameObject>();
                    sceneReferencesMap[blueprint] = gameObjectsMap;
                }

                gameObjectsMap[hash] = obj;
            }

            if (sceneReferencesMap.ContainsKey(blueprint) && (gameObjectsMap == null || gameObjectsMap.Count == 0)) {
                sceneReferencesMap.Remove(blueprint);
            }
        }
    }

}
