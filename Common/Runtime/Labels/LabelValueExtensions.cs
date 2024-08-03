using System.Runtime.CompilerServices;

namespace MisterGames.Common.Labels {
    
    public static class LabelValueExtensions {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this LabelValue labelValue)
        {
            return labelValue.array < 0 || 
                   labelValue.library == null ||
                   !labelValue.library.ContainsLabel(labelValue.array, labelValue.value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this LabelValue labelValue)
        {
            return labelValue.array >= 0 && 
                   labelValue.library != null &&
                   labelValue.library.ContainsLabel(labelValue.array, labelValue.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue(this LabelValue labelValue, out int value)
        {
            value = labelValue.value;
            return labelValue.IsNotNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLabel(this LabelValue labelValue)
        {
            return labelValue.IsNull() 
                ? null 
                : labelValue.library.GetLabel(labelValue.array, labelValue.value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLabel(this LabelValue labelValue, out string label) {
            if (labelValue.IsNull()) {
                label = null;
                return false;
            }
            
            label = labelValue.library.GetLabel(labelValue.array, labelValue.value);
            return true;
        }    
    }
    
}