﻿using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.Transform {

    [InitializeOnLoad]
    internal static class TweenProgressActionMoveTransformContextMenuExtensions {

        static TweenProgressActionMoveTransformContextMenuExtensions() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Vector3) return;

            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0) return;

            path = path.Remove(lastDot, path.Length - lastDot);

            if (property.serializedObject.FindProperty(path)?.GetValue() is not TweenProgressActionMoveTransform t ||
                t.transform == null
            ) {
                return;
            }

            var propertyCopy = property.Copy();

            menu.AddItem(new GUIContent("Write from Transform"), false, () => {
                Undo.RecordObject(property.serializedObject.targetObject, "TweenProgressActionMoveTransformContextMenuExtensions_WriteFromTransform");
                
                propertyCopy.vector3Value = t.useLocal ? t.transform.localPosition : t.transform.position;

                propertyCopy.serializedObject.ApplyModifiedProperties();
                propertyCopy.serializedObject.Update();
            });

            menu.AddItem(new GUIContent("Set to Transform"), false, () => {
                Undo.RecordObject(t.transform, "TweenProgressActionMoveTransformContextMenuExtensions_SetToTransform");
                
                if (t.useLocal) t.transform.localPosition = propertyCopy.vector3Value;
                else t.transform.position = propertyCopy.vector3Value;

                EditorUtility.SetDirty(t.transform);
            });
        }
    }

}
