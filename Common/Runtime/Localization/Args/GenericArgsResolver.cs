using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class GenericArgsResolver : IArgumentResolver {

        [SerializeReference] [SubclassSelector] public IArgumentValue[] args;
        
        public void Resolve(Locale locale, ref string value) {
            ArgumentResolveExtensions.ResolveArgs((args, locale), args.Length, ref value, (a, i) => a.args[i].GetValue(a.locale));
        }
    }
    
}