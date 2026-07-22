using System;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class FloatArg : IArgumentValue {

        public float from;
        public float to;
        public string format = "0.000";
        public string prefix;
        public string postfix;
        
        public string GetValue(Locale locale) {
            return $"{prefix}{UnityEngine.Random.Range(from, to).ToString(format)}{postfix}";
        }
    }
}