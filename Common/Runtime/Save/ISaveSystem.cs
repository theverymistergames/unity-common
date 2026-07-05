using System.Collections.Generic;

namespace MisterGames.Common.Save {

    public interface ISaveSystem {

        void Register(ISaveable saveable, bool notifyLoad = true);
        void Unregister(ISaveable saveable);

        bool TryGet<T>(string storageId, string dataId, int index, out T data);
        T Get<T>(string storageId, string dataId, int index);
        bool Set<T>(string storageId, string dataId, int index, T data);
        SaveBuilder Pop<T>(string storageId, string dataId, out T data);
        SaveBuilder Pop<T>(string storageId, string dataId, T def, out T data);
        SaveBuilder Push<T>(string storageId, string dataId, T data);

        IReadOnlyList<StorageData> GetStorageFiles();
        
        void SaveIntoFile(string storageId);
        void LoadFromFile(string storageId);
        void DeleteFile(string storageId);

        void SaveAllFiles();
        void LoadAllFiles();
        void DeleteAllFiles();
    }
    
}