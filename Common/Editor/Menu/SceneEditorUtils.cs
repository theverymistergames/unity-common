using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {
    
    public static class SceneEditorUtils {
        
        private static TransformData _transformData;
        
        private struct TransformData {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 localScale;
        }
        
        [MenuItem("Edit/Toggle Active &a", false, -101)]
        public static void ToggleActive() {
            foreach (var go in Selection.gameObjects) {
                go.SetActive(!go.activeSelf);
            }
        }
        
        [MenuItem("Edit/Copy Position &c", false, -101)]
        public static void CopyTransformValues() {
            if (Selection.gameObjects.Length == 0) return;
            
            var t = Selection.gameObjects[0].transform;
            _transformData = new TransformData { position = t.position, rotation = t.rotation, localScale = t.localScale };
        }

        [MenuItem("Edit/Paste Position &v", false, -101)]
        public static void PasteTransformValues() {
            foreach (var go in Selection.gameObjects) {
                var t = go.transform;
                Undo.RecordObject(t, "Paste Transform Values");
                t.position = _transformData.position;
                //t.rotation = _transformData.rotation;
                //t.localScale = _transformData.localScale;
            }
        }
    }
    
}