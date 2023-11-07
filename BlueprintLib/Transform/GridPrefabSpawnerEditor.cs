#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [CustomEditor(typeof(GridPrefabSpawner))]
    public sealed class GridPrefabSpawnerEditor : Editor {

        public override void OnInspectorGUI() {
            if (target is not GridPrefabSpawner spawner) return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_grid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridStep"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spawnAmount"));

            if (GUILayout.Button("Spawn")) {
                spawner.Spawn();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_spawnedCount"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif
