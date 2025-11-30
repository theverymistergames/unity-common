using System.Linq;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Character.Editor.Utils {
    
    public static class SelectCharacterMenu {
        
        [MenuItem("MisterGames/Tools/Select character %h")]
        private static void SelectCharacter() {
            if (!Application.isPlaying || IsCharacterInstanceSelected()) {
                PingCharacterPrefab();
                return;
            }
            
            var character = Object.FindFirstObjectByType<MainCharacter>();
            if (character == null) return;
            
            PingCharacterInstance(character).Forget();
        }
        
        private static bool IsCharacterInstanceSelected() {
            return Selection.activeGameObject != null && 
                   Selection.activeGameObject.TryGetComponent(out MainCharacter _) && 
                   Selection.activeGameObject.scene.name != null;
        }

        private static void PingCharacterPrefab() {
            var mainCharacterPrefab = AssetDatabase
                .FindAssets($"a:assets t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(g => g != null)
                .FirstOrDefault(g => g.TryGetComponent<MainCharacter>(out _));

            EditorGUIUtility.PingObject(mainCharacterPrefab);
            Selection.activeGameObject = mainCharacterPrefab;
        }

        private static async UniTask PingCharacterInstance(MainCharacter character) {
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