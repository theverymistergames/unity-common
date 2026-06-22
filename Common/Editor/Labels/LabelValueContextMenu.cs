using System.Collections.Generic;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Labels;
using MisterGames.Common.Labels.Base;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Labels {
    
    [InitializeOnLoad]
    internal static class LabelValueContextMenu {
        
        private const string LibraryPropertyPath = nameof(LabelValue.library);
        private const string IdPropertyPath = nameof(LabelValue.id);
        private const string DataNamePropertyPath = nameof(LabelLibrary.LabelData.name);
        private const string DataIdPropertyPath = nameof(LabelLibrary.LabelData.id);
        
        static LabelValueContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            CheckLabelValue(menu, property);
            CheckLabelData(menu, property);
            CheckLabelsInArray(menu, property);
            CheckLabelArray(menu, property);
        }

        private static void CheckLabelValue(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(LibraryPropertyPath) is not { objectReferenceValue: LabelLibraryBase labelLibrary } ||
                property.FindPropertyRelative(IdPropertyPath) is not { } idProperty
               ) {
                return;
            }

            menu.AddItem(new GUIContent("Select LabelLibrary"), false, () => {
                EditorGUIUtility.PingObject(labelLibrary);
            });

            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    LabelValueSearchWindow.SearchLabelValue(new LabelValue(labelLibrary, idProperty.intValue));
                });
            }
        }

        private static void CheckLabelData(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.String ||
                property.name != DataNamePropertyPath ||
                property.serializedObject.targetObject is not LabelLibraryBase labelLibrary) {
                return;
            }
            
            var parentProp = property.GetParentProperty();

            if (parentProp?.type != nameof(LabelLibrary.LabelData) ||
                parentProp.FindPropertyRelative(DataIdPropertyPath) is not { propertyType: SerializedPropertyType.Integer } idProperty) {
                return;
            }

            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    LabelValueSearchWindow.SearchLabelValue(new LabelValue(labelLibrary, idProperty.intValue));
                });
            }
        }

        private static void CheckLabelsInArray(GenericMenu menu, SerializedProperty property) {
            if (!property.isArray ||
                property.arrayElementType != nameof(LabelLibrary.LabelData) ||
                property.GetParentProperty()?.type != "LabelArray" ||
                property.serializedObject.targetObject is not LabelLibraryBase labelLibrary) {
                return;
            }

            menu.AddItem(new GUIContent("Search usages..."), false, () => {
                int arrayId = property.GetParentProperty().FindPropertyRelative("id").intValue;
                int arrayIndex = labelLibrary.GetArrayIndex(arrayId);
                int count = labelLibrary.GetArrayLabelsCount(arrayIndex);
                
                var labels = new List<LabelValue>();
                for (int i = 0; i < count; i++) {
                    labels.Add(new LabelValue(labelLibrary, labelLibrary.GetLabelId(arrayIndex, i)));
                }
                
                LabelValueSearchWindow.SearchLabelValues(labels.ToArray());
            });
        }
        
        private static void CheckLabelArray(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.String ||
                property.name != "name" ||
                property.serializedObject.targetObject is not LabelLibraryBase labelLibrary) {
                return;
            }
            
            var parentProp = property.GetParentProperty();

            if (parentProp?.type != "LabelArray" ||
                parentProp.FindPropertyRelative("id") is not { propertyType: SerializedPropertyType.Integer } idProperty) {
                return;
            }

            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    LabelValueSearchWindow.SearchLabelArray(new LabelArray(labelLibrary, idProperty.intValue));
                });
            }
        }
    }

}