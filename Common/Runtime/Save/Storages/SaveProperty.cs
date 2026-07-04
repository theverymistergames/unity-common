using System;
using MisterGames.Common.Types;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public struct SaveProperty {
        public string name;
        public SerializedType type;
    }
    
}