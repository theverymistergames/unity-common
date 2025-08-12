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
        
        void Register(ISaveable saveable, bool notifyLoad = true);

        void Unregister(ISaveable saveable);
        
        bool TryGet<T>(SaveStorage storage, string id, int index, out T data);
        
        T Get<T>(SaveStorage storage, string id, int index);
        
        void Set<T>(SaveStorage storage, string id, int index, T data);
        
        SaveBuilder Pop<T>(SaveStorage storage, string id, out T data);
        
        SaveBuilder Pop<T>(SaveStorage storage, string id, T def, out T data);
        
        SaveBuilder Push<T>(SaveStorage storage, string id, T data);
    }
    
}