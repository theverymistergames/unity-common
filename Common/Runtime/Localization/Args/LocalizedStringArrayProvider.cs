using System;
using System.Collections.Generic;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class LocalizedStringArrayProvider : ILocalizedStringProvider {

        public LocalizationKey[] keys;
        
        public void GetValues(List<LocalizationKey> buffer) {
            buffer.AddRange(keys);
        }
    }
    
}