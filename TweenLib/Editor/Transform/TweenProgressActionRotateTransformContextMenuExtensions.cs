using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.Transform {

    [InitializeOnLoad]
    internal static class TweenProgressActionRotateTransformContextMenuExtensions {

        static TweenProgressActionRotateTransformContextMenuExtensions() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Vector3) return;
            
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot < 0) return;

            path = path.Remove(lastDot, path.Length - lastDot);

            if (property.serializedObject.FindProperty(path).GetValue() is not TweenProgressActionRotateTransform t ||
                t.transform == null
            ) {
                return;
            }

            var propertyCopy = property.Copy();

            menu.AddItem(new GUIContent("Write from Transform"), false, () => {
                propertyCopy.vector3Value = t.useLocal ? t.transform.localEulerAngles : t.transform.eulerAngles;
                
                propertyCopy.serializedObject.ApplyModifiedProperties();
                propertyCopy.serializedObject.Update();
            });
            
            menu.AddItem(new GUIContent("Set to Transform"), false, () => {
                if (t.useLocal) t.transform.localEulerAngles = propertyCopy.vector3Value;
                else t.transform.eulerAngles = propertyCopy.vector3Value;
                
                EditorUtility.SetDirty(t.transform);
            });
        }
    }
    
}
