using System;
using System.Collections.Generic;
using MisterGames.Common.Save.Tables;

namespace MisterGames.Common.Save.Storages {
    
    public interface ISaveStorage {

        IEnumerable<ISaveTable> Tables { get; }
        
        ISaveTable GetTable<T>();

        ISaveTable GetTable(Type elementType);
        
        void SetTable<T>(ISaveTable value);

        void SetTable(Type elementType, ISaveTable value);

        ISaveTable GetOrCreateTable<T>();

        ISaveTable GetOrCreateTable(Type elementType);

        bool RemoveTable<T>();
        
        bool RemoveTable(Type elementType);

        void Clear();
        
        string GetSerializedPropertyPath(Type elementType);
    }
    
}