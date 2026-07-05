using System;
using System.Collections.Generic;
using MisterGames.Common.Save.Tables;

namespace MisterGames.Common.Save.Storages {

    public interface ISaveStorage {
        
        IEnumerable<ISaveTable> Tables { get; }
        
        ISaveTable GetTable(Type valueType);
        
        ISaveTable GetOrCreateTable(Type valueType);
        
        void SetTable(Type valueType, ISaveTable value);
        
        bool RemoveTable(Type valueType);
        
        void Clear();
        
        string GetSerializedPropertyPath(Type valueType);
    }
    
    public interface ISaveStorage<TKey> : ISaveStorage where TKey : IEquatable<TKey> {
        
        ISaveTable<TKey> GetTable<T>();
        
        void SetTable<T>(ISaveTable<TKey> value);
        
        ISaveTable<TKey> GetOrCreateTable<T>();
        
        bool RemoveTable<T>();
    }
    
}