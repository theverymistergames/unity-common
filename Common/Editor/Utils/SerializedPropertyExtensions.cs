using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace MisterGames.Common.Editor.Utils {
    
    public static class SerializedPropertyExtensions {
        
        private static readonly Regex ArrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);
        
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

        private static void SetValueNoRecord(this SerializedProperty property, object value) {
            var propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;

            var i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token)) {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }
            
            Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary. Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
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
            
            Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }
        
        private struct PropertyPathComponent {
            public string propertyName;
            public int elementIndex;
        }
        
    }

}