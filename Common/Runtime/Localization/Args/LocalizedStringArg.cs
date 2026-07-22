using System;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class LocalizedStringArg : IArgumentValue {

        public LocalizationKey value;

        public string GetValue(Locale locale) {
            return value.GetValue(locale);
        }
    }
    
}