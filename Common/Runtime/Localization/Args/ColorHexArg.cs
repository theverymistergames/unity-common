using System;
using MisterGames.Common.Colors;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class ColorHexArg : IArgumentValue {

        [ColorUsage(showAlpha: true)]
        public Color color = Color.white;
        
        public string GetValue(Locale locale) {
            return $"#{color.ColorToHexRGBA()}";
        }
    }
    
}