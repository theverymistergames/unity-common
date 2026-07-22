using System;
using MisterGames.Common.Lists;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class RandomLocalizedStringArg : IArgumentValue {

        public LocalizationKey[] variants;

        public string GetValue(Locale locale) {
            return variants.GetRandom().GetValue(locale);
        }
    }
    
}