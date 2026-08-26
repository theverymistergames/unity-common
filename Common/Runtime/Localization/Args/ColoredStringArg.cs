using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Colors;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class ColoredStringArg : IArgumentValue {

        [ColorUsage(showAlpha: true)] public Color color = Color.white;
        [SerializeReference] [SubclassSelector] public IArgumentValue coloredArg;
        
        public string GetValue(Locale locale) {
            return $"<color=#{color.ColorToHexRGBA()}>{coloredArg.GetValue(locale)}</color>";
        }
    }
    
}