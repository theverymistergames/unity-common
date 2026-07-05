using System;

namespace MisterGames.Common.Save.Tables {
    
    public interface ISaveTable {

        Type GetKeyType();
        
        Type GetValueType();
        
        bool IsEmpty();
        
        void Clear();
    }
    
    public interface ISaveTable<in TKey> : ISaveTable where TKey : IEquatable<TKey> {

        bool TryGetData<V>(TKey key, out V data);
    
        bool SetData<V>(TKey key, V data);
        
        bool TryGetDataBoxed(TKey key, out object data);
    
        bool SetDataBoxed(TKey key, object data);
        
        bool RemoveData(TKey key);
        
        bool ContainsData(TKey key);
        
        string GetSerializedPropertyPath(TKey key);
    }
    
}