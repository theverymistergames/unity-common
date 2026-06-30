using System;
using System.Collections.Generic;

namespace MisterGames.Common.Save.Tables {
    
    public interface ISaveStorage {

        IEnumerable<ISaveTable> Tables { get; }
        
        ISaveTable Get<T>();

        ISaveTable Get(Type elementType);
        
        void Set<T>(ISaveTable value);

        void Set(Type elementType, ISaveTable value);

        ISaveTable GetOrCreateTable<T>();

        ISaveTable GetOrCreateTable(Type elementType);

        void PrewarmTables();

        void Clear();
    }
    
}