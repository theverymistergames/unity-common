using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {
    
    public static class PasteComponentNoRefsMenu {
        
        private const string MenuPath = "CONTEXT/Behaviour/Paste Component Values (no refs)";
        private const string UndoName = "Paste Component Values (no refs)";

        [MenuItem(MenuPath)]
        private static void Paste(MenuCommand command) {
            if (command.context is not Behaviour target) return;

            var previousRefs = CollectObjectReferences(target);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            Undo.RecordObject(target, UndoName);

            if (!ComponentUtility.PasteComponentValues(target)) {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var iterator = serializedObject.GetIterator();

            while (iterator.Next(true)) {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;

                var pasted = iterator.objectReferenceValue;
                previousRefs.TryGetValue(iterator.propertyPath, out var previous);

                bool isMissing = pasted == null && iterator.objectReferenceEntityIdValue != default;
                bool isHierarchyRef = isMissing ||
                                      IsHierarchyObject(pasted, target) ||
                                      (pasted == null && IsHierarchyObject(previous, target));

                if (!isHierarchyRef || (!isMissing && pasted == previous)) continue;

                iterator.objectReferenceValue = previous;
            }

            serializedObject.ApplyModifiedProperties();
            Undo.CollapseUndoOperations(undoGroup);
        }

        private static Dictionary<string, Object> CollectObjectReferences(Object target) {
            var refs = new Dictionary<string, Object>();
            var iterator = new SerializedObject(target).GetIterator();

            while (iterator.Next(true)) {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference) {
                    refs[iterator.propertyPath] = iterator.objectReferenceValue;
                }
            }

            return refs;
        }

        private static bool IsHierarchyObject(Object obj, Object owner) {
            if (obj == null || obj is not (GameObject or Component)) return false;
            if (!EditorUtility.IsPersistent(obj)) return true;

            return EditorUtility.IsPersistent(owner) && AssetDatabase.GetAssetPath(obj) == AssetDatabase.GetAssetPath(owner);
        }
    }
    
}