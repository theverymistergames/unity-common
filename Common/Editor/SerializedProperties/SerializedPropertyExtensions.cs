using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace MisterGames.Common.Editor.SerializedProperties {
    
    public static class SerializedPropertyExtensions {
        
        private static readonly Regex ArrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private const string PROPERTY_PATH_ARRAY_ELEMENT_PREFIX = "data";
        private const string PROPERTY_PATH_ARRAY = "Array";

        private struct PropertyPathComponent {
            public string propertyName;
            public int elementIndex;
        }
        
        public static string GetNeighbourPropertyPath(SerializedProperty property, string propertyName) {
            string path = property.propertyPath;
            int dotIndex = path.LastIndexOf('.');
            
            if (dotIndex < 0) return propertyName;

            if (path.Contains('[')) {
                var parts = SplitPropertyPath(path);
                if (parts.Count <= 1) return propertyName;

                if (ArrayElementRegex.Match(parts[^1], 0).Success) {
                    if (parts.Count == 2) return propertyName;
                    
                    var sb = new StringBuilder();
                    
                    for (int i = 0; i < parts.Count - 2; i++) {
                        sb.Append(parts[i]);
                        if (i < parts.Count - 3) sb.Append('.');
                    }

                    return $"{sb}.{propertyName}";
                }
            }

            return $"{path.Remove(dotIndex)}.{propertyName}";
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property) {
            var serializedObject = property.serializedObject;
            var parentType = serializedObject.targetObject.GetType();
            var fieldInfo = parentType.GetField(property.propertyPath, BINDING_FLAGS);

            var pathParts = SplitPropertyPath(property.propertyPath);
            SerializedProperty parentProperty = null;

            for (int i = 0; i < pathParts.Count; i++) {
                string pathPart = pathParts[i];
                fieldInfo = parentType?.GetField(pathPart, BINDING_FLAGS);

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

                    continue;
                }

                parentProperty = i == 0
                    ? serializedObject.FindProperty(pathPart)
                    : parentProperty?.FindPropertyRelative(pathPart);

                parentType = parentProperty is { propertyType: SerializedPropertyType.ManagedReference }
                    ? parentProperty.managedReferenceValue?.GetType() ?? fieldType
                    : fieldType;
            }

            return fieldInfo;
        }

        public static object GetValue(this SerializedProperty property) {
            string propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            int i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token)) {
                value = GetPathComponentValue(value, token);
            }
            return value;
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
                ? (container as IList)?[component.elementIndex] 
                : GetMemberValue(container, component.propertyName);
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
    }

}
