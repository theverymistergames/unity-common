using System;
using UnityEngine.AddressableAssets;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public sealed class LocalizationTableReference : AssetReferenceT<LocalizationTableStorage> {
        
        public LocalizationTableReference(string guid) : base(guid) { }
        
    }
    
}