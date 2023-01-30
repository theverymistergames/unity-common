using MisterGames.Common.Editor;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer {

        private bool _isPendingToWriteSelectedScene;
        private string _selectedScene;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var sceneProperty = property.FindPropertyRelative("scene");
            string sceneName = sceneProperty.stringValue;

            if (string.IsNullOrEmpty(sceneName)) {
                sceneName = SceneManager.GetActiveScene().name;
                sceneProperty.stringValue = sceneName;
            }

            EditorGUI.LabelField(position, label);

            var dropdownPosition = new Rect(position);
            dropdownPosition.width -= EditorGUIUtility.labelWidth;
            dropdownPosition.x += EditorGUIUtility.labelWidth;
            dropdownPosition.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.DropdownButton(dropdownPosition, new GUIContent(sceneProperty.stringValue), FocusType.Keyboard)) {
                var scenesDropdown = new AdvancedDropdown<SceneAsset>(
                    "Select scene",
                    ScenesStorage.Instance.GetAllSceneAssets(),
                    sceneAsset => ScenesMenu.RemoveSceneAssetFileFormat(AssetDatabase.GetAssetPath(sceneAsset)),
                    sceneAsset => {
                        _isPendingToWriteSelectedScene = true;
                        _selectedScene = sceneAsset.name;
                    });

                scenesDropdown.Show(dropdownPosition);
            }

            if (_isPendingToWriteSelectedScene) {
                _isPendingToWriteSelectedScene = false;
                sceneProperty.stringValue = _selectedScene;
            }

            EditorGUI.EndProperty();
        }
    }

}
