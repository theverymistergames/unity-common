using System;
using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Common.State {
    
    public sealed class EnableStateService : IEnableStateService {

        private readonly MultiValueDictionary<int, Action<bool>> _enableCallbacksMap = new();
        private readonly Dictionary<int, bool> _stateMap = new();
        
        public void NotifyEnable(int id, bool enable) {
            _stateMap[id] = enable;
            NotifyListeners(id, enable);
        }

        public void Subscribe(int id, Action<bool> callback, bool notifyOnSubscribe = true) {
            if (_enableCallbacksMap.ContainsValue(id, callback)) return;
            
            _enableCallbacksMap.AddValue(id, callback);

            if (notifyOnSubscribe && _stateMap.TryGetValue(id, out bool enabled)) {
                callback?.Invoke(enabled);
            }
        }

        public void Unsubscribe(int id, Action<bool> callback) {
            _enableCallbacksMap.RemoveValue(id, callback);
        }

        private void NotifyListeners(int id, bool state) {
            int count = _enableCallbacksMap.GetCount(id);
            for (int i = 0; i < count; i++) {
                _enableCallbacksMap.GetValueAt(id, i)?.Invoke(state);
            }
        }
    }
    
}