using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.Transform {

    [InitializeOnLoad]
    internal static class TweenProgressActionScaleTransformContextMenuExtensions {

        static TweenProgressActionScaleTransformContextMenuExtensions() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Vector3) return;
            
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0) return;

            path = path.Remove(lastDot, path.Length - lastDot);

            if (property.serializedObject.FindProperty(path).GetValue() is not TweenProgressActionScaleTransform t ||
                t.transform == null
            ) {
                return;
            }

            var propertyCopy = property.Copy();

            menu.AddItem(new GUIContent("Write from Transform"), false, () => {
                Undo.RecordObject(property.serializedObject.targetObject, "TweenProgressActionScaleTransformContextMenuExtensions_WriteFromTransform");
                
                propertyCopy.vector3Value = t.transform.localScale;
                
                propertyCopy.serializedObject.ApplyModifiedProperties();
                propertyCopy.serializedObject.Update();
                
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            });
            
            menu.AddItem(new GUIContent("Set to Transform"), false, () => {
                Undo.RecordObject(t.transform, "TweenProgressActionScaleTransformContextMenuExtensions_SetToTransform");
                
                t.transform.localScale = propertyCopy.vector3Value;

                EditorUtility.SetDirty(t.transform);
            });
        }
    }
    
}
