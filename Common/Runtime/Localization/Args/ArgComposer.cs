using System;
using System.Text;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class ArgComposer : IArgumentValue {
        
        [SerializeReference] [SubclassSelector] private IArgumentValue[] values;
        
        public string GetValue(Locale locale) {
            var sb = new StringBuilder();

            for (int i = 0; i < values.Length; i++) {
                sb.Append(values[i].GetValue(locale));
            }

            return sb.ToString();
        }
    }
    
}