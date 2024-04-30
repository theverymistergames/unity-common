using System;

namespace MisterGames.Common.Save.Tables {
    
    public interface ISaveTable {

        Type GetElementType();
        
        bool TryGetData<T>(long id, out T data);
    
        void SetData<T>(long id, T data);
        
        void PrepareRecord(ISaveSystem saveSystem, long id);

        void FetchRecords(ISaveSystem saveSystem);
        
        void RemoveData(long id);
        
        void Clear();
    }
    
}