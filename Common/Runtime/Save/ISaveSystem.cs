using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Save.Storages;
using MisterGames.Common.Save.Tables;

namespace MisterGames.Common.Save {

    public interface ISaveSystem {

        void Register(ISaveable saveable, bool notifyLoad = true);
        void Unregister(ISaveable saveable);

        bool TryGet<T>(string storageId, string dataId, int index, out T data);
        T Get<T>(string storageId, string dataId, int index);
        bool Set<T>(string storageId, string dataId, int index, T data);
        bool Remove<T>(string storageId, string dataId, int index);
   
        ISaveStorage<SaveKey> GetStorage(string storageId);
        
        IReadOnlyList<StorageData> GetStorageFiles();
        UniTask SaveIntoFile(string storageId);
        UniTask SaveIntoFile(string storageId, ISaveStorage source);
        UniTask LoadFromFile(string storageId);
        UniTask LoadFromFile(string storageId, ISaveStorage dest);
        void DeleteFile(string storageId);

        void SaveAllFiles();
        void LoadAllFiles();
        void DeleteAllFiles();
    }
    
}