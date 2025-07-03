using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Core {
    
    [CustomEditor(typeof(SceneLoaderSettings))]
    public sealed class SceneLoaderSettingsEditor : UnityEditor.Editor {

        private static readonly GUIContent ScenesLabel = new GUIContent("Scenes"); 
        private static readonly GUIContent ScenesProdLabel = new GUIContent("Production"); 
        private static readonly GUIContent ScenesDevLabel = new GUIContent("Development");
        private const string SceneNamesProperty = "_sceneNamesCache";
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            var sceneNamesProperty = serializedObject.FindProperty(SceneNamesProperty);
            sceneNamesProperty.isExpanded = EditorGUILayout.Foldout(sceneNamesProperty.isExpanded, ScenesLabel);

            if (!sceneNamesProperty.isExpanded) return;
            
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            
            GUILayout.Label(ScenesProdLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            
            var sceneAssets = SceneLoaderSettings.GetProductionBuildSceneAssets().ToArray();
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                EditorGUILayout.ObjectField(sceneAssets[i], typeof(SceneAsset), false);
            }

            EditorGUI.EndDisabledGroup();
            
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 4f);
            
            GUILayout.Label(ScenesDevLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            
            sceneAssets = SceneLoaderSettings.GetDevelopmentBuildSceneAssets().ToArray();
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                EditorGUILayout.ObjectField(sceneAssets[i], typeof(SceneAsset), false);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
    
}