using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {
    
    public static class MoveScaleToChildrenMenu {

        private const string UndoOperation = "MoveScaleToChildrenMenu_MoveScale";
        
        [MenuItem("CONTEXT/Transform/Move scale to children")]
        public static void MoveScale(MenuCommand command) {
            if (command.context is not Transform root) return;
            
            var go = new GameObject();
            
            go.transform.SetParent(root);
            go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            go.transform.SetAsLastSibling();
            
            int childCount = root.childCount;
            
            for (int i = childCount - 2; i >= 0; i--) {
                var child = root.GetChild(i);
                Undo.RecordObject(child, UndoOperation);
                
                child.SetParent(go.transform, worldPositionStays: true);
            }

            var rootScale = root.localScale;
            
            Undo.RecordObject(root, UndoOperation);
            root.localScale = Vector3.one;
            
            go.transform.localScale = rootScale;

            childCount = go.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--) {
                var child = go.transform.GetChild(i);
                child.SetParent(root, worldPositionStays: true);
                
                EditorUtility.SetDirty(child); 
            }
            
            Object.DestroyImmediate(go);
            
            EditorUtility.SetDirty(root);
        }
    }
    
}