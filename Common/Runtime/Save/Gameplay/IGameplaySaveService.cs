using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Save {
    
    public interface IGameplaySaveService {

        public delegate void ProfileCallback(string profileKey);
        public delegate void DataCallback(string profileKey, string dataKey);

        event ProfileCallback OnProfileUpdated;
        event ProfileCallback OnCurrentProfileChanged;
        event DataCallback OnDataChanged;

        bool HasUnsavedChanges { get; }
        
        string GetCurrentProfileKey();
        string GetProfileKey(int index);
        UniTask LoadOrCreateProfile(string profileKey, bool makeCurrent);
        UniTask SaveProfile(string profileKey);
        void DeleteProfile(string profileKey);
        bool HasSavedProfile(string profileKey);
        bool IsProfileLoaded(string profileKey);
        
        bool TryGet<T>(string profileKey, string key, int index, out T data);
        bool Set<T>(string profileKey, string key, int index, T data);
        bool Remove<T>(string profileKey, string key, int index);
        
        bool TryGet<T>(string key, int index, out T data);
        bool Set<T>(string key, int index, T data);
        bool Remove<T>(string key, int index);
    }
    
}