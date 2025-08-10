using System;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LocaleFilterAttribute : PropertyAttribute {
        
        public readonly LocaleFilter filter;

        public LocaleFilterAttribute(LocaleFilter filter) {
            this.filter = filter;
        }
    }
    
}