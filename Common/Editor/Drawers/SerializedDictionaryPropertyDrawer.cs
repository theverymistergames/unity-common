using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SerializedDictionaryBase<,,>), useForChildren: true)]
    public sealed class SerializedDictionaryPropertyDrawer : PropertyDrawer {
        
        private const string key = "key";
        private readonly Dictionary<long, ReorderableList> _listMap = new();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.x -= 3f;
            rect.width += 3f;
            rect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true, EditorStyles.foldoutHeader);

            if (property.isExpanded) {
                rect = position;
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
                var list = GetList(property);
                list.DoList(rect);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded) {
                height += GetList(property).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        private ReorderableList GetList(SerializedProperty property)
        {
            long key = GetPropertyKey(property);
            var items = property.FindPropertyRelative("_entries");
            var newEntry = property.FindPropertyRelative("_newEntry");

            bool displayAdd = newEntry.intValue == 0 || 
                              IsUniqueNewItem(items.GetArrayElementAtIndex(items.arraySize - 1), items);
            
            if (_listMap.TryGetValue(key, out var existing)) {
                existing.displayAdd = displayAdd;
                existing.draggable = newEntry.intValue == 0;
                existing.multiSelect = displayAdd;
                if (newEntry.intValue > 0) {
                    existing.index = items.arraySize - 1;
                } 
                return existing;
            }
            
            var list = new ReorderableList(
                property.serializedObject,
                items,
                draggable: newEntry.intValue == 0,
                displayHeader: false,
                displayAddButton: displayAdd,
                displayRemoveButton: true
            ) {
                multiSelect = displayAdd,
                
                elementHeightCallback = index =>
                {
                    var element = items.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element, true) + 4f;
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = items.GetArrayElementAtIndex(index);
                    rect.y += 1f;
                    rect.x += 8f;
                    rect.width -= 8f;
                    EditorGUIUtility.labelWidth -= 21f;
                    
                    rect.height = EditorGUI.GetPropertyHeight(element, true);
                    bool isNew = newEntry.intValue == 1 && index == items.arraySize - 1;
                    string header = isNew ? "New Element" : $"Element {index}";

                    EditorGUI.BeginDisabledGroup(!isNew && newEntry.intValue == 1);
                    CustomPropertyGUI.PropertyField(rect, element, new GUIContent(header), element.GetFieldInfo(), includeChildren: true);
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUIUtility.labelWidth += 21f;
                },
                drawElementBackgroundCallback = (rect, index, isActive, isFocused) => {
                    bool isNew = newEntry.intValue == 1 && index == items.arraySize - 1;

                    rect.x += 2f;
                    rect.width -= 4f;
                    rect.y += 1f;
                    rect.height -= 2f;

                    bool isValid = isNew && 
                                   IsUniqueNewItem(items.GetArrayElementAtIndex(items.arraySize - 1), items);
                    
                    EditorGUI.DrawRect(rect, GetElementColor(isNew, isValid, isActive, isFocused));
                },
                onAddCallback = l => {
                    if (newEntry.intValue > 0) {
                        newEntry.intValue = 0;
                        property.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    
                    newEntry.intValue = 1;
                    int index = items.arraySize;
                    items.InsertArrayElementAtIndex(index);
                    var item = items.GetArrayElementAtIndex(index);
                    item.isExpanded = true;
                    property.serializedObject.ApplyModifiedProperties();

                    l.index = index;
                },
                onRemoveCallback = l => {
                    if (newEntry.intValue > 0) {
                        newEntry.intValue = 0;
                        items.DeleteArrayElementAtIndex(items.arraySize - 1);
                        property.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    
                    items.DeleteArrayElementAtIndex(l.index);
                    property.serializedObject.ApplyModifiedProperties();
                }
            };

            if (newEntry.intValue > 0) {
                list.index = items.arraySize - 1;
            } 
            
            _listMap[key] = list;
            return list;
        }
        
        private static Color GetElementColor(
            bool isNew,
            bool isValid,
            bool isActive,
            bool isFocused)
        {
            if (isNew) {
                return isValid 
                    ? new Color(44f / 256f, 93f / 256f,  44f/ 256f, 1f)
                    : new Color(135f / 256f, 93f / 256f,  44f/ 256f, 1f);
            }
            
            if (isActive) {
                return new Color(44f / 256f, 93f / 256f, 135f / 256f, 1f);
            }

            if (isFocused) {
                return new Color(77f / 256f, 77f / 256f, 77f / 256f, 1f);
            }
            
            return new Color(65f / 256f, 65f / 256f, 65f / 256f, 1f);
        }
        
        private static bool IsUniqueNewItem(SerializedProperty item, SerializedProperty items) {
            int count = items.arraySize;
            var itemKey = item.FindPropertyRelative(key);
            
            for (int i = 0; i < count - 1; i++) {
                if (SerializedProperty.DataEquals(items.GetArrayElementAtIndex(i).FindPropertyRelative(key), itemKey)) {
                    return false;
                }
            }

            if (!SerializedPropertyExtensions.CanBeNullable(itemKey.propertyType)) {
                return true;
            }

            return itemKey.propertyType switch {
                SerializedPropertyType.Generic => itemKey.GetValue() != null,
                SerializedPropertyType.String => !string.IsNullOrEmpty(itemKey.stringValue),
                SerializedPropertyType.ObjectReference => itemKey.objectReferenceValue != null,
                SerializedPropertyType.AnimationCurve => itemKey.animationCurveValue != null,
                SerializedPropertyType.ExposedReference => itemKey.exposedReferenceValue != null,
                SerializedPropertyType.ManagedReference => itemKey.managedReferenceValue != null,
                _ => true
            };
        }

        private static long GetPropertyKey(SerializedProperty property) {
            return NumberExtensions.TwoIntsAsLong(
                property.serializedObject.targetObject.GetInstanceID(),
                Animator.StringToHash(property.propertyPath)
            );
        }
    }

}
