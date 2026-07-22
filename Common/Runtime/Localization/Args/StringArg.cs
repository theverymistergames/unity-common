using System;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class StringArg : IArgumentValue {

        public string value;

        public string GetValue(Locale locale) {
            return value;
        }
    }
    
}