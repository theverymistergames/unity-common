using System.Collections.Generic;
using MisterGames.Common.Labels;
using MisterGames.Common.Labels.Base;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(LabelValueMap<>))]
    [CustomPropertyDrawer(typeof(LabelValueMap<,>))]
    public sealed class LabelValueMapPropertyDrawer : PropertyDrawer {
        
        private const string LabelArrayPropertyPath = "labelArray";
        private const string ValuesPropertyPath = "values";
        private const string LabelPropertyPath = "label";
        
        private readonly Dictionary<int, int> _labelsBuffer = new();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            
            var labelArrayProperty = property.FindPropertyRelative(LabelArrayPropertyPath);
            int arrayId = labelArrayProperty.FindPropertyRelative(nameof(LabelArray.id)).intValue;
            var lib = labelArrayProperty.FindPropertyRelative(nameof(LabelArray.library)).objectReferenceValue as LabelLibraryBase;
            
            var valuesProperty = property.FindPropertyRelative(ValuesPropertyPath);
            int entriesCount = valuesProperty.arraySize;
            int validEntriesCount = entriesCount;
            bool changed = false;
            
            _labelsBuffer.Clear();
            
            for (int i = entriesCount - 1; i >= 0; i--) {
                var labelProperty = valuesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(LabelPropertyPath);
                var labelIdProperty = labelProperty.FindPropertyRelative(nameof(LabelValue.id));
                var labelLibProperty = labelProperty.FindPropertyRelative(nameof(LabelValue.library));
                
                int labelId = labelIdProperty.intValue;
                var labelLib = labelLibProperty.objectReferenceValue as LabelLibraryBase;

                if (labelId == 0 || 
                    labelLib == null || labelLib != lib || 
                    labelLib.GetArrayId(labelLib.GetLabelArrayIndex(labelId)) != arrayId || 
                    !_labelsBuffer.TryAdd(labelId, i)) 
                {
                    var swapLabelProperty = valuesProperty.GetArrayElementAtIndex(--validEntriesCount)?.FindPropertyRelative(LabelPropertyPath);
                    if (swapLabelProperty == null) continue;
                    
                    var swapIdProperty = swapLabelProperty.FindPropertyRelative(nameof(LabelValue.id));
                    var swapLibProperty = swapLabelProperty.FindPropertyRelative(nameof(LabelValue.library));
                    
                    if (swapIdProperty.intValue == 0 || swapLibProperty.objectReferenceValue == null) {
                        continue;
                    }

                    valuesProperty.MoveArrayElement(--validEntriesCount, i);
                    changed = true;
                }
            }
            
            int arrayIndex = lib == null ? -1 : lib.GetArrayIndex(arrayId);
            int labelsInArrayCount = arrayIndex < 0 || lib == null ? 0 : lib.GetArrayLabelsCount(arrayIndex);
            
            if (entriesCount != labelsInArrayCount) {
                valuesProperty.arraySize = labelsInArrayCount;
                changed = true;
            }

            for (int i = 0; i < labelsInArrayCount; i++) {
                int labelId = lib!.GetLabelId(arrayIndex, i);
                
                int currentIndexInValues = _labelsBuffer.GetValueOrDefault(labelId, -1);
                if (currentIndexInValues == i) continue;

                changed = true;
                
                if (currentIndexInValues >= 0) {
                    valuesProperty.MoveArrayElement(currentIndexInValues, i);
                    continue;
                }
                
                var labelProperty = valuesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(LabelPropertyPath);
                var labelIdProperty = labelProperty.FindPropertyRelative(nameof(LabelValue.id));
                var labelLibProperty = labelProperty.FindPropertyRelative(nameof(LabelValue.library));
                
                labelIdProperty.intValue = labelId;
                labelLibProperty.objectReferenceValue = lib;
            }
            
            if (changed) {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUI.PropertyField(position, property, label, includeChildren: true);
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
    
}