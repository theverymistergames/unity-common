using MisterGames.Character.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Character.Editor.Utils {
    
    public static class SelectCharacterMenu {
        
        [MenuItem("MisterGames/Tools/Select character %h")]
        private static void SelectCharacter() {
            var character = Object.FindFirstObjectByType<MainCharacter>();
            if (character == null) return;
            
            Selection.activeGameObject = character.gameObject;
            
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var window = EditorWindow.GetWindow(type);
            var method = type.GetMethod("SetExpandedRecursive");
            
            method!.Invoke(window, new object[] { character.gameObject.GetInstanceID(), true });

            int childCount = character.transform.childCount;
            for (int i = 0; i < childCount; i++) {
                method!.Invoke(window, new object[] { character.transform.GetChild(i).gameObject.GetInstanceID(), false });   
            }
            
            EditorGUIUtility.PingObject(character.gameObject);
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
        }
    }
    
}