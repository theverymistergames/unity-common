using System;
using System.Collections.Generic;
using MisterGames.Common.Save.Tables;

namespace MisterGames.Common.Save.Storages {

    public interface ISaveStorage {
        
        IReadOnlyDictionary<Type, ISaveTable> Tables { get; }
        
        ISaveTable GetTable(Type valueType);
        
        ISaveTable GetOrCreateTable(Type valueType);
        
        void SetTable(Type valueType, ISaveTable table);
        
        bool RemoveTable(Type valueType);
        
        void Clear();
        
        string GetSerializedPropertyPath(Type valueType);
    }
    
    public interface ISaveStorage<TKey> : ISaveStorage where TKey : IEquatable<TKey> {
        
        ISaveTable<TKey> GetTable<T>();
        
        ISaveTable<TKey> GetOrCreateTable<T>();
        
        void SetTable<T>(ISaveTable<TKey> table);
        
        bool RemoveTable<T>();
        
        void CopyTo(ISaveStorage<TKey> dest);
    }
    
}