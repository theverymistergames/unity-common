using System.Collections.Generic;
using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {
    
    [CustomEditor(typeof(ScenesStorage))]
    public class SceneStorageEditor : UnityEditor.Editor {

        private SceneAsset[] _sceneAssetsCache;
        private readonly List<SceneAsset> _sceneAssetsBuffer = new List<SceneAsset>();

        private void OnEnable() {
            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }

        private void OnDisable() {
            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not ScenesStorage scenesStorage) return;

            GUILayout.Space(10);

            if (GUILayout.Button("Include all scenes in build settings")) {
                ScenesMenu.IncludeAllScenesInBuildSettings();
            }

            GUILayout.Space(10);

            GUILayout.Label("Scenes", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);

            _sceneAssetsCache ??= scenesStorage.GetAllSceneAssets().ToArray();

            _sceneAssetsBuffer.Clear();
            _sceneAssetsBuffer.AddRange(_sceneAssetsCache);

            for (int i = 0; i < _sceneAssetsBuffer.Count; i++) {
                var sceneAsset = _sceneAssetsBuffer[i];
                EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            _sceneAssetsCache = ScenesStorage.Instance.GetAllSceneAssets().ToArray();
        }
    }
    
}
