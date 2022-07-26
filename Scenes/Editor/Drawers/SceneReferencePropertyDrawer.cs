using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var sceneProperty = property.FindPropertyRelative("scene");
            string sceneName = sceneProperty.stringValue;
            var sceneNames = ScenesStorage.Instance.SceneNames;

            int sceneIndex = 0;
            
            for (int i = 0; i < sceneNames.Length; i++) {
                string scene = sceneNames[i];
                if (sceneName != scene) continue;

                sceneIndex = i;
                break;
            }
            
            EditorGUI.BeginProperty(position, label, property);
            sceneIndex = EditorGUI.Popup(position, label.text, sceneIndex, sceneNames);
            EditorGUI.EndProperty();

            sceneProperty.stringValue = sceneNames[sceneIndex];
        }
        
    }

}