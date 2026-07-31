using System;

namespace MisterGames.Character.Core {
    
    public interface ICharacterSettings {

        void AddValueChangeListener(int id, Action<int> listener);
        void RemoveValueChangeListener(int id, Action<int> listener);
        
        bool TryGet<T>(int key, out T value);
        T Get<T>(int key, T defaultValue);
        void Set<T>(int key, T value);
        bool Remove<T>(int key);
    }
    
}