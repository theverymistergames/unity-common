using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class CombineArgResolver : IArgumentResolver {

        public string pattern = "{0} {1}";
        [SerializeReference] [SubclassSelector] public IArgumentValue value2;
        
        public void Resolve(LocalizationKey key, Locale locale, ref string value) {
            value = string.Format(pattern, value, value2.GetValue(locale));
        }
    }
    
}