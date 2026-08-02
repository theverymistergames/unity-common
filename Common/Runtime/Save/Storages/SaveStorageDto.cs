using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveStorageDto {
        
        [SerializeReference] private SerializedTypeMapByRef<ISaveTable> _tables;

        public IReadOnlyDictionary<Type, ISaveTable> Tables => _tables;
        
        public SaveStorageDto(IReadOnlyDictionary<Type, ISaveTable> tables) {
            _tables = new SerializedTypeMapByRef<ISaveTable>();
            foreach (var (type, table) in tables) {
                _tables[type] = table;
            }
        }
    }
    
}