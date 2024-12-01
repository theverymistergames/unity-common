using System.Collections.Generic;
using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Core {
    
    [CustomEditor(typeof(SceneLoaderSettings))]
    public sealed class SceneLoaderSettingsEditor : UnityEditor.Editor {

        private static readonly GUIContent ScenesLabel = new GUIContent("Scenes"); 
        private const string SceneNamesProperty = "_sceneNamesCache";
        
        private readonly HashSet<SceneAsset> _buildSceneAssetsCache = new();
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not SceneLoaderSettings sceneLoaderSettings) return;

            GUILayout.Space(10);

            var sceneNamesProperty = serializedObject.FindProperty(SceneNamesProperty);
            sceneNamesProperty.isExpanded = EditorGUILayout.Foldout(sceneNamesProperty.isExpanded, ScenesLabel);

            if (!sceneNamesProperty.isExpanded) return;
            
            var sceneAssets = SceneLoaderSettings.GetAllSceneAssets().ToArray();
            
            if (CheckCanAddScenesIntoBuildSettings(sceneAssets)) {
                if (GUILayout.Button("Include scenes into build settings")) {
                    sceneLoaderSettings.IncludeScenesInBuildSettings();
                }
                
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
            
            EditorGUI.BeginDisabledGroup(true);
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                EditorGUILayout.ObjectField(sceneAssets[i], typeof(SceneAsset), false);
            }

            EditorGUI.EndDisabledGroup();
        }

        private bool CheckCanAddScenesIntoBuildSettings(SceneAsset[] sceneAssets) {
            _buildSceneAssetsCache.Clear();
            
            var scenesInBuild = EditorBuildSettings.scenes;
            for (int i = 0; i < scenesInBuild.Length; i++) {
                _buildSceneAssetsCache.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenesInBuild[i].path));
            }

            for (int i = 0; i < sceneAssets.Length; i++) {
                if (!_buildSceneAssetsCache.Contains(sceneAssets[i])) return true;
            }

            return false;
        }
    }
    
}