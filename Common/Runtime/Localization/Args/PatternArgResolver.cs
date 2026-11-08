using System;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class PatternArgResolver : IArgumentResolver {

        public string pattern = "[{0}]";
        
        public void Resolve(LocalizationKey key, Locale locale, ref string value) {
            value = string.Format(pattern, value);
        }
    }
    
}