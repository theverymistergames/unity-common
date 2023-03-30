using UnityEngine;

namespace MisterGames.Common.Colors {

    public static class ColorUtils {

        public static Color HexToColor(string hex, Color defaultValue = default) {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : defaultValue;
        }
    }

}
