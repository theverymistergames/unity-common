﻿using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Utils {
    
    public static class SceneTools {
        
        private static TransformData _transformData;
        
        private struct TransformData {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 localScale;
        }
        
        [MenuItem("Edit/Copy Position &c", false, -101)]
        public static void CopyTransformValues() {
            if (Selection.gameObjects.Length == 0) return;
            
            var t = Selection.gameObjects[0].transform;
            _transformData = new TransformData { position = t.position, rotation = t.rotation, localScale = t.localScale };
        }

        [MenuItem("Edit/Paste Position &v", false, -101)]
        public static void PasteTransformPosition() {
            foreach (var go in Selection.gameObjects) {
                var t = go.transform;
                Undo.RecordObject(t, "Paste Transform Pos");
                t.position = _transformData.position;
            }
        }
        
        [MenuItem("Edit/Paste Rotation &r", false, -101)]
        public static void PasteTransformRotation() {
            foreach (var go in Selection.gameObjects) {
                var t = go.transform;
                Undo.RecordObject(t, "Paste Transform Rot");
                t.rotation = _transformData.rotation;
            }
        }
    }
    
}