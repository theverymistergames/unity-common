using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Tick;

namespace MisterGames.Common.Data {
    
    public sealed class CancelableSet<K> : IUpdate {

        public event Action OnUpdate = delegate { };
        public int Count => _map.Count;
        
        private readonly Dictionary<K, CancellationToken> _map = new();
        private readonly List<K> _keys = new();
        
        public void Add(K key, CancellationToken cancellationToken = default) {
            if (cancellationToken.IsCancellationRequested) {
                Remove(key);
                return;
            }
            
            bool hadKey = _map.ContainsKey(key);
            _map[key] = cancellationToken;

            if (hadKey) return;
            
            _keys.Add(key);
            OnUpdate.Invoke();
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        public void Remove(K key) {
            if (!_map.Remove(key)) return;
            
            OnUpdate.Invoke();
            
            if (_map.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }

        public bool Contains(K key) {
            return _map.TryGetValue(key, out var token) && !token.IsCancellationRequested;
        }

        public void Clear() {
            int mapCount = _map.Count;
            
            _map.Clear();
            _keys.Clear();
            
            if (mapCount > 0) OnUpdate.Invoke();
            
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            int mapCount = _map.Count;
            int count = _keys.Count;

            for (int i = count - 1; i >= 0; i--) {
                var k = _keys[i];
                if (_map.TryGetValue(k, out var token) && !token.IsCancellationRequested) continue;
                
                int lastValid = --count;
                _keys[i] = _keys[lastValid];
                _keys[lastValid] = k;

                _map.Remove(k);
            }
            
            _keys.RemoveRange(count, _keys.Count - count);

            int newMapCount = _map.Count;
            if (newMapCount != mapCount) OnUpdate.Invoke();
            
            if (newMapCount == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }
    }
    
}