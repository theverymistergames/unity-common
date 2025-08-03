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
            var windows = Resources.FindObjectsOfTypeAll(type);

            for (int i = 0; i < windows.Length; i++) {
                var window = windows[i];
                
                var method = type.GetMethod("SetExpandedRecursive");
            
                method!.Invoke(window, new object[] { character.gameObject.GetInstanceID(), true });

                int childCount = character.transform.childCount;
                for (int j = 0; j < childCount; j++) {
                    method!.Invoke(window, new object[] { character.transform.GetChild(j).gameObject.GetInstanceID(), false });   
                }
            }
            
            EditorGUIUtility.PingObject(character.gameObject);
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
        }
    }
    
}