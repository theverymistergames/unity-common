using System;
using System.Runtime.CompilerServices;

namespace MisterGames.Common.Labels {
    
    public static class LabelValueExtensions {
        
        // Null checks
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this LabelValue labelValue) {
            return labelValue.library is null || !labelValue.library.ContainsLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this LabelValue<T> labelValue) {
            return labelValue.library is null || !labelValue.library.ContainsLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this LabelValue labelValue) {
            return labelValue.library?.ContainsLabel(labelValue.id) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.ContainsLabel(labelValue.id) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this LabelArray labelArray) {
            return (labelArray.library?.GetArrayIndex(labelArray.id) ?? -1) < 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this LabelArray<T> labelArray) {
            return (labelArray.library?.GetArrayIndex(labelArray.id) ?? -1) < 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this LabelArray labelArray) {
            return (labelArray.library?.GetArrayIndex(labelArray.id) ?? -1) >= 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this LabelArray<T> labelArray) {
            return (labelArray.library?.GetArrayIndex(labelArray.id) ?? -1) >= 0;
        }
        
        // Value
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue(this LabelValue labelValue) {
            return labelValue.library?.GetValue(labelValue.id) ?? 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.GetValue(labelValue.id) ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(this LabelValue labelValue, out int value) {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                value = labelValue.library.GetValue(labelValue.id);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this LabelValue<T> labelValue, out int value) {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                value = labelValue.library.GetValue(labelValue.id);
                return true;
            }

            value = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValuesCount(this LabelArray labelArray) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1; 
            return arrayIndex >= 0 ? labelArray.library!.GetArrayLabelsCount(arrayIndex) : 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValuesCount<T>(this LabelArray<T> labelArray) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1; 
            return arrayIndex >= 0 ? labelArray.library!.GetArrayLabelsCount(arrayIndex) : 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue(this LabelArray labelArray, int labelIndex) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1;
            return arrayIndex < 0 
                ? 0 
                : labelArray.library!.GetValue(labelArray.library.GetLabelId(arrayIndex, labelIndex));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue<T>(this LabelArray<T> labelArray, int labelIndex) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1;
            return arrayIndex < 0 
                ? 0 
                : labelArray.library!.GetValue(labelArray.library.GetLabelId(arrayIndex, labelIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(this LabelArray labelArray, int labelIndex, out int value) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1;
            int labelId = labelArray.library?.GetLabelId(arrayIndex, labelIndex) ?? 0;
            
            if (arrayIndex >= 0 && labelArray.library!.ContainsLabel(labelId)) {
                value = labelArray.library.GetValue(labelId);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this LabelArray<T> labelArray, int labelIndex, out int value) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1;
            int labelId = labelArray.library?.GetLabelId(arrayIndex, labelIndex) ?? 0;
            
            if (arrayIndex >= 0 && labelArray.library!.ContainsLabel(labelId)) {
                value = labelArray.library.GetValue(labelId);
                return true;
            }

            value = default;
            return false;
        }

        // Label
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel(this LabelValue labelValue) {
            return labelValue.library?.GetLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.GetLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLabel(this LabelValue labelValue, out string label) {
            label = labelValue.library?.GetLabel(labelValue.id);
            return label != null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLabel<T>(this LabelValue<T> labelValue, out string label) {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                label = labelValue.library.GetLabel(labelValue.id);
                return true;
            }

            label = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel(this LabelArray labelArray) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1; 
            return arrayIndex >= 0 ? labelArray.library!.GetArrayName(arrayIndex) : null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel<T>(this LabelArray<T> labelArray) {
            int arrayIndex = labelArray.library?.GetArrayIndex(labelArray.id) ?? -1; 
            return arrayIndex >= 0 ? labelArray.library!.GetArrayName(arrayIndex) : null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLabel(this LabelArray labelArray, out string label) {
            label = GetLabel(labelArray);
            return label != null;
        }
        
        // Data
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetData<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.TryGetData(labelValue.id, out var data) ?? false ? data : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetData<T>(this LabelValue<T> labelValue, out T data) {
            if (labelValue.library?.TryGetData(labelValue.id, out data) ?? false) return true;
            
            data = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetData<T>(this LabelValue<T> labelValue, T data) {
            return labelValue.library?.TrySetData(labelValue.id, data) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClearData<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.ClearData(labelValue.id) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SubscribeChanges<T>(this LabelValue<T> labelValue, Action<T> listener, bool notifyOnSubscribe = true) {
            if (!LabelLibrariesRunner.EventSystem.Subscribe(labelValue, listener)) return false;

            if (notifyOnSubscribe && labelValue.TryGetData(out var data)) {
                listener?.Invoke(data);
            }
            
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UnsubscribeChanges<T>(this LabelValue<T> labelValue, Action<T> listener) {
            return LabelLibrariesRunner.EventSystem.Unsubscribe(labelValue, listener);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SubscribeChanges<T>(this LabelValue<T> labelValue, ILabelValueListener<T> listener, bool notifyOnSubscribe = true) {
            if (!LabelLibrariesRunner.EventSystem.Subscribe(labelValue, listener)) return false;

            if (notifyOnSubscribe && labelValue.TryGetData(out var data)) {
                listener?.OnDataChanged(labelValue, data);
            }
            
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UnsubscribeChanges<T>(this LabelValue<T> labelValue, ILabelValueListener<T> listener) {
            return LabelLibrariesRunner.EventSystem.Unsubscribe(labelValue, listener);
        }
    }
    
}