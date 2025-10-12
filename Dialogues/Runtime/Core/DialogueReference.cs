using System;
using MisterGames.Dialogues.Storage;
using UnityEngine.AddressableAssets;

namespace MisterGames.Dialogues.Core {
    
    [Serializable]
    public sealed class DialogueReference : AssetReferenceT<DialogueTableStorage> {
        public DialogueReference(string guid) : base(guid) { }
    }
    
}