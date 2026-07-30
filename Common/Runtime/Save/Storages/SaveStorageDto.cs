using System;
using System.Collections.Generic;
using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveStorageDto {
        
        [SerializeReference] private ISaveTable[] _tables;

        public IReadOnlyList<ISaveTable> Tables => _tables;
        
        public SaveStorageDto(ISaveTable[] tables) {
            _tables = tables;
        }
    }
    
}