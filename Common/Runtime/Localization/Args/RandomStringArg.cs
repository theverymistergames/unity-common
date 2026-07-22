using System;
using MisterGames.Common.Lists;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class RandomStringArg : IArgumentValue {

        public string[] variants;

        public string GetValue(Locale locale) {
            return variants.GetRandom();
        }
    }
    
}