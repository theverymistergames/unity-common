using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class SingleArgResolver : IArgumentResolver {
    
        [SerializeReference] [SubclassSelector] public IArgumentValue arg;
        
        public void Resolve(Locale locale, ref string value) {
            ArgumentResolveExtensions.ResolveArgs((arg, locale), 1, ref value, (a, _) => a.arg.GetValue(a.locale));
        }
    }
}