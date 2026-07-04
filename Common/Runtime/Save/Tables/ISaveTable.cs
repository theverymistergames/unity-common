using System;

namespace MisterGames.Common.Save.Tables {
    
    public interface ISaveTable {

        Type GetElementType();
        
        bool TryGetData<T>(long id, out T data);
    
        bool SetData<T>(long id, T data);
        
        bool TryGetDataBoxed(long id, out object data);
    
        bool SetDataBoxed(long id, object data);
        
        bool RemoveData(long id);
        
        bool ContainsData(long id);

        bool IsEmpty();
        
        void Clear();
        
        string GetSerializedPropertyPath(long id);
    }
    
}