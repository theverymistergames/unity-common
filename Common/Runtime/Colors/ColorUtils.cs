using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Colors {

    public static class ColorUtils {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(this Color color, float alpha) {
            color.a = alpha;
            return color;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ColorToHexRGBA(this Color color) {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ColorToHexRGB(this Color color) {
            return ColorUtility.ToHtmlStringRGB(color);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color HexToColor(string hex, Color defaultValue = default) {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : defaultValue;
        }
        
        public static Color ColorFromHash(int hashCode, float saturation = 1f, float value = 1f) {
            uint x = (uint) hashCode;
            x ^= x >> 16;
            x *= 0x7feb352d;
            x ^= x >> 15;
            x *= 0x846ca68b;
            x ^= x >> 16;

            var prevState = Random.state;

            Random.InitState(unchecked((int) x));

            float h = Random.value; 
            float s = Mathf.Clamp01(saturation * (0.7f + 0.3f * Random.value));
            float v = Mathf.Clamp01(value * (0.7f + 0.3f * Random.value));

            Random.state = prevState;

            return Color.HSVToRGB(h, s, v);
        }
    }

}
