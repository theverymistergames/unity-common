using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Trees;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Core {

    [CustomPropertyDrawer(typeof(SceneReference))]
    public sealed class SceneReferencePropertyDrawer : PropertyDrawer {
        
        private const string NullPath = "<null>";
        private static readonly GUIContent NullContent = new GUIContent(NullPath);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var sceneProperty = property.FindPropertyRelative("scene").Copy();
            property = property.Copy();

            string sceneName = sceneProperty.stringValue;
            if (string.IsNullOrEmpty(sceneName)) {
                sceneProperty.stringValue = null;
                sceneName = null;
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            var guiContent = sceneName == null ? NullContent : new GUIContent(sceneName); 
            
            var dropdownPosition = new Rect(position);

            if (label.text != sceneName) {
                EditorGUI.LabelField(position, label);

                dropdownPosition.width -= EditorGUIUtility.labelWidth;
                dropdownPosition.x += EditorGUIUtility.labelWidth;
                dropdownPosition.height = EditorGUIUtility.singleLineHeight;
            }

            if (EditorGUI.DropdownButton(dropdownPosition, guiContent, FocusType.Keyboard)) {
                string nullPath = GetNullPath();
                
                var scenesDropdown = new AdvancedDropdown<SceneAsset>(
                    "Select scene",
                    SceneLoaderSettings.GetAllSceneAssets().Prepend(null),
                    sceneAsset => GetItemPath(sceneAsset, nullPath),
                    (sceneAsset, _) => {
                        sceneProperty.stringValue = sceneAsset == null ? null : sceneAsset.name;

                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data == null)
                        .ThenBy(n => n.data.name)
                );

                scenesDropdown.Show(dropdownPosition);
            }

            EditorGUI.EndProperty();
        }

        private static string GetItemPath(SceneAsset sceneAsset, string nullPath) {
            return sceneAsset == null 
                ? nullPath 
                : SceneUtils.RemoveSceneAssetFileFormat(AssetDatabase.GetAssetPath(sceneAsset));
        }

        private static string GetNullPath() {
            var folders = new List<string>();
            
            folders.AddRange(SceneLoaderSettings.Instance.searchScenesInFolders);
            folders.AddRange(SceneLoaderSettings.Instance.searchDevScenesInFolders);
                
            return $"{PathTree.CreateTree(folders, p => p).data.name}/{NullPath}";
        }
    }

}
