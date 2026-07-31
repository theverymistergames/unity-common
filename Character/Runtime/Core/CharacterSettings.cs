using System;
using MisterGames.Common.Data;
using MisterGames.Common.Save.Storages;

namespace MisterGames.Character.Core {
    
    public sealed class CharacterSettings : ICharacterSettings {

        private readonly SaveStorage<int> _storage = new();
        private readonly MultiValueDictionary<int, Action<int>> _listeners = new();

        public void AddValueChangeListener(int id, Action<int> listener) {
            if (!_listeners.ContainsValue(id, listener)) _listeners.AddValue(id, listener);
        }
        
        public void RemoveValueChangeListener(int id, Action<int> listener) {
            _listeners.RemoveValue(id, listener);
        }

        public bool TryGet<T>(int key, out T value) {
            value = default;
            return _storage.GetTable<T>()?.TryGetData(key, out value) ?? false;
        }

        public T Get<T>(int key, T defaultValue) {
            return _storage.GetTable<T>()?.TryGetData<T>(key, out var value) ?? false
                ? value
                : defaultValue;
        }

        public void Set<T>(int key, T value) {
            _storage.GetOrCreateTable<T>().SetData(key, value);
            NotifyValueChanged(key);
        }
        
        public bool Remove<T>(int key) {
            if (_storage.GetTable<T>()?.RemoveData(key) ?? false) {
                NotifyValueChanged(key);
                return true;
            }
            
            return false;
        }

        private void NotifyValueChanged(int key) {
            int count = _listeners.GetCount(key);
            for (int i = 0; i < count; i++) {
                _listeners.GetValueAt(key, i)?.Invoke(key);
            }
        }
    }
    
}