using System;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Save {
    
    public interface IGameplaySaveService {

        event Action<string> OnDataChanged;
        event Action<string> OnProfileChanged;

        bool HasUnsavedChanges { get; }
        
        string GetCurrentProfileKey();
        string GetProfileKey(int index);
        UniTask LoadOrCreateProfile(string profileKey);
        UniTask SaveProfile(string profileKey);
        void DeleteProfile(string profileKey);
        bool HasSavedProfile(string profileKey);
        
        bool TryGet<T>(string key, int index, out T data);
        bool Set<T>(string key, int index, T setting);
        bool Remove<T>(string key, int index);
    }
    
}