using System;
using UnityEngine.AddressableAssets;

namespace MisterGames.Common.Audio {
    
    [Serializable]
    public sealed class AudioBankReference : AssetReferenceT<AudioBank> {
        public AudioBankReference(string guid) : base(guid) { }
    }
    
}