using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Utils {
    
    public static class SerializedPropertyExtensions {
        
        private static readonly Regex ArrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private const string PROPERTY_PATH_ARRAY_ELEMENT_PREFIX = "data";
        private const string PROPERTY_PATH_ARRAY = "Array";

        public static FieldInfo GetPropertyFieldInfo(SerializedProperty property) {
            var serializedObject = property.serializedObject;
            var parentType = serializedObject.targetObject.GetType();
            var fieldInfo = parentType.GetField(property.propertyPath, BINDING_FLAGS);

            Debug.Log($"SerializedPropertyExtensions.GetPropertyFieldInfo: start {property.name}\n" +
                      $"property path {property.propertyPath}\n" +
                      $"parentType {parentType}\n" +
                      $"fieldInfo {fieldInfo}");

            var pathParts = SplitPropertyPath(property.propertyPath);
            SerializedProperty parentProperty = null;

            for (int i = 0; i < pathParts.Count; i++) {
                string pathPart = pathParts[i];
                fieldInfo = parentType?.GetField(pathPart, BINDING_FLAGS);

                Debug.Log($"SerializedPropertyExtensions.GetPropertyFieldInfo: start iter {i}\n" +
                          $"parentProperty path {parentProperty?.propertyPath}\n" +
                          $"parentType {parentType}\n" +
                          $"fieldInfo {fieldInfo}");

                if (fieldInfo == null) return null;

                var fieldType = fieldInfo.FieldType;

                if (fieldType.IsArray || fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
                    if (++i > pathParts.Count - 1) break;

                    pathPart = $"{pathPart}.{pathParts[i]}";

                    parentProperty = i == 0
                        ? serializedObject.FindProperty(pathPart)
                        : parentProperty?.FindPropertyRelative(pathPart);

                    parentType = fieldType.IsArray
                        ? fieldType.GetElementType()
                        : fieldType.GetGenericArguments()[0];

                    Debug.Log($"SerializedPropertyExtensions.GetPropertyFieldInfo: end iter {i - 1}\n" +
                              $"next parentProperty path {parentProperty?.propertyPath}\n" +
                              $"next parentType {parentType}\n" +
                              $"fieldType {fieldType}\n" +
                              $"fieldInfo {fieldInfo}\n" +
                              $"is array or list, so skip next iteration");

                    continue;
                }

                parentProperty = i == 0
                    ? serializedObject.FindProperty(pathPart)
                    : parentProperty?.FindPropertyRelative(pathPart);

                parentType = parentProperty?.GetValue()?.GetType() ?? fieldType;

                Debug.Log($"SerializedPropertyExtensions.GetPropertyFieldInfo: end iter {i}\n" +
                          $"next parentProperty path {parentProperty?.propertyPath}\n" +
                          $"next parentType {parentType}\n" +
                          $"fieldType {fieldType}\n" +
                          $"fieldInfo {fieldInfo}");
            }

            Debug.Log($"SerializedPropertyExtensions.GetPropertyFieldInfo: end {property.name}\n" +
                      $"property path {property.propertyPath}\n" +
                      $"parentType {parentType}\n" +
                      $"fieldInfo {fieldInfo}");

            return fieldInfo;
        }

        public static object GetValue(this SerializedProperty property) {
            var propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            var i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token)) {
                value = GetPathComponentValue(value, token);
            }
            return value;
        }
        
        public static void SetValue(this SerializedProperty property, object value) { 
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");
            
            SetValueNoRecord(property, value);
            
            EditorUtility.SetDirty(property.serializedObject.targetObject); 
            property.serializedObject.ApplyModifiedProperties(); 
        }

        private static List<string> SplitPropertyPath(string path) {
            var pathParts = new List<string>(path.Split('.'));

            for (int i = pathParts.Count - 1; i >= 0; i--) {
                string pathPart = pathParts[i];

                if (i == 0 ||
                    pathParts[i - 1] != PROPERTY_PATH_ARRAY ||
                    !pathPart.StartsWith(PROPERTY_PATH_ARRAY_ELEMENT_PREFIX)
                ) {
                    continue;
                }

                pathParts.RemoveAt(i);
                pathParts[i - 1] = $"{PROPERTY_PATH_ARRAY}.{pathPart}";
                i--;
            }

            return pathParts;
        }

        private static void SetValueNoRecord(this SerializedProperty property, object value) {
            var propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;

            var i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token)) {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }
            
            System.Diagnostics.Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary. Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
            SetPathComponentValue(container, deferredToken, value);
        }

        private static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component) {
            component = new PropertyPathComponent();
            if (index >= propertyPath.Length) return false;

            var arrayElementMatch = ArrayElementRegex.Match(propertyPath, index);
            if (arrayElementMatch.Success) {
                index += arrayElementMatch.Length + 1;
                component.elementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
                return true;
            }

            var dot = propertyPath.IndexOf('.', index);
            if (dot == -1) {
                component.propertyName = propertyPath.Substring(index);
                index = propertyPath.Length;
            }
            else {
                component.propertyName = propertyPath.Substring(index, dot - index);
                index = dot + 1;
            }

            return true;
        }
        
        private static object GetPathComponentValue(object container, PropertyPathComponent component) {
            return component.propertyName == null 
                ? ((IList) container)[component.elementIndex] 
                : GetMemberValue(container, component.propertyName);
        }
    
        private static void SetPathComponentValue(object container, PropertyPathComponent component, object value) {
            if (component.propertyName == null) ((IList) container)[component.elementIndex] = value;
            else SetMemberValue(container, component.propertyName, value);
        }
        
        private static object GetMemberValue(object container, string name) {
            if (container == null) return null;
            
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var info in members) {
                if (info is FieldInfo field) return field.GetValue(container);
                if (info is PropertyInfo property) return property.GetValue(container);
            }
            
            return null;
        }
        
        private static void SetMemberValue(object container, string name, object value) {
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var info in members) {
                if (info is FieldInfo field) {
                    field.SetValue(container, value);
                    return;
                }

                if (info is PropertyInfo property) {
                    property.SetValue(container, value);
                    return;
                }
            }
            
            System.Diagnostics.Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }
        
        private struct PropertyPathComponent {
            public string propertyName;
            public int elementIndex;
        }
        
    }

}
