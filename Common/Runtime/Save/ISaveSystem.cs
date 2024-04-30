using System.Collections.Generic;

namespace MisterGames.Common.Save {

    public interface ISaveSystem {

        string GetActiveSave();

        string GetLastWrittenSave();
        
        IReadOnlyList<SaveMeta> GetSaves();

        bool TryLoad(string saveId = null);

        void Save(string saveId = null);
        
        void DeleteAllSaves();

        void DeleteSave(string saveId);
        
        void Register(ISaveable saveable);
        
        void Register(ISaveable saveable, string id, out int hash);

        void Unregister(ISaveable saveable);

        string GetPropertyName(int hash);
        
        ISaveSystem CreateProperty(string id, out int hash);
        
        ISaveSystem CreateProperty(string id);
        
        bool TryGet<T>(int id, int index, out T data);
        
        T Get<T>(int id, int index);
        
        void Set<T>(int id, int index, T data);
        
        SaveBuilder Pop<T>(int id, out T data);
        
        SaveBuilder Pop<T>(int id, T def, out T data);
        
        SaveBuilder Push<T>(int id, T data);
    }
    
}