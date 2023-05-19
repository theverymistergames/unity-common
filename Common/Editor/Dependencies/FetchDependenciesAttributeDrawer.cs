using MisterGames.Common.Dependencies;
using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(FetchDependenciesAttribute))]
    public class FetchDependenciesAttributeDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.GetValue() is not DependencyResolver resolver) return;

            var attr = (FetchDependenciesAttribute) attribute;

            string path = property.propertyPath;
            int lastDotIndex = path.LastIndexOf('.');

            path = lastDotIndex >= 0
                ? $"{path.Remove(lastDotIndex)}.{attr.dependencyPropertyName}"
                : attr.dependencyPropertyName;

            if (string.IsNullOrEmpty(path)) {
                if (property.serializedObject.targetObject is IDependency dep) {
                    resolver.Fetch(dep);
                }
                else {
                    resolver.Fetch((IDependency) null);
                }
            }
            else {
                var dependencyProperty = property.serializedObject.FindProperty(path);

                if (dependencyProperty.GetValue() is IDependency dep) {
                    resolver.Fetch(dep);
                }
                else if (dependencyProperty.GetValue() is IDependency[] deps) {
                    resolver.Fetch(deps);
                }
                else {
                    resolver.Fetch((IDependency) null);
                }
            }

            EditorGUI.BeginChangeCheck();

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            CustomPropertyGUI.PropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return fieldInfo.FieldType == typeof(DependencyResolver)
                ? CustomPropertyGUI.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true)
                : 0f;
        }
    }

}
