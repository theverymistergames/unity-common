﻿using System.Runtime.CompilerServices;

namespace MisterGames.Common.Labels {
    
    public static class LabelValueExtensions {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this LabelValue labelValue)
        {
            return labelValue.library is null || !labelValue.library.ContainsLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this LabelValue<T> labelValue)
        {
            return labelValue.library is null || !labelValue.library.ContainsLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this LabelValue labelValue)
        {
            return labelValue.library?.ContainsLabel(labelValue.id) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this LabelValue<T> labelValue)
        {
            return labelValue.library?.ContainsLabel(labelValue.id) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue(this LabelValue labelValue)
        {
            return labelValue.library?.GetValue(labelValue.id) ?? 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValue<T>(this LabelValue<T> labelValue)
        {
            return labelValue.library?.GetValue(labelValue.id) ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(this LabelValue labelValue, out int value)
        {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                value = labelValue.library.GetValue(labelValue.id);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this LabelValue<T> labelValue, out int value)
        {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                value = labelValue.library.GetValue(labelValue.id);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel(this LabelValue labelValue)
        {
            return labelValue.library?.GetLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel<T>(this LabelValue<T> labelValue)
        {
            return labelValue.library?.GetLabel(labelValue.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLabel(this LabelValue labelValue, out string label) {
            if (labelValue.library?.ContainsLabel(labelValue.id) ?? false) {
                label = labelValue.library.GetLabel(labelValue.id);
                return true;
            }

            label = default;
            return false;
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
        public static T GetData<T>(this LabelValue<T> labelValue) {
            return labelValue.library?.TryGetData(labelValue.id, out var data) ?? false ? data : default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetData<T>(this LabelValue<T> labelValue, out T data) {
            if (labelValue.library?.TryGetData(labelValue.id, out data) ?? false) return true;
            
            data = default;
            return false;
        }
    }
    
}