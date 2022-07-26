using UnityEngine;

namespace MisterGames.Common.Color {

    public static class ColorUtils {

        public static UnityEngine.Color HexToColor(string hex, UnityEngine.Color defaultValue = default) {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : defaultValue;
        }

    }

}