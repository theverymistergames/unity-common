using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MisterGames.Common.Tick {

    internal sealed class TimeSource : ITimeSource, ITimeSourceApi {
        
        public bool IsPaused { get; set; }
        public float DeltaTime { get; private set; }
        public float TimeScale { get => _timeScaleProvider.TimeScale; set => _timeScaleProvider.TimeScale = value; }
        public float ScaledTime { get; private set; }
        public int FrameCount { get; private set; }

        public int SubscribersCount => _updateList.Count;

        private readonly string _name;
        
        private readonly Dictionary<IUpdate, int> _indexMap = new();
        private readonly List<IUpdate> _updateList = new();
        
        private readonly IDeltaTimeProvider _deltaTimeProvider;
        private readonly ITimeScaleProvider _timeScaleProvider;
        
        public TimeSource(IDeltaTimeProvider deltaTimeProvider, ITimeScaleProvider timeScaleProvider, string name = null) {
            _deltaTimeProvider = deltaTimeProvider;
            _timeScaleProvider = timeScaleProvider;
            _name = name;
        }

        public bool Subscribe(IUpdate sub) {
            if (!_indexMap.TryAdd(sub, _updateList.Count)) return false;

            _updateList.Add(sub);

#if UNITY_EDITOR
            if (TimeSources.ShowDebugInfo) Log($"subscribed {sub}");
#endif
            
            return true;
        }

        public bool Unsubscribe(IUpdate sub) {
            if (!_indexMap.Remove(sub, out int index)) return false;

            _updateList[index] = null;
            
#if UNITY_EDITOR
            if (TimeSources.ShowDebugInfo) Log($"unsubscribed {sub}");
#endif
            
            return true;
        }

        public void Tick() {
            DeltaTime = IsPaused ? 0f : _deltaTimeProvider.DeltaTime * _timeScaleProvider.TimeScale;
            ScaledTime += DeltaTime;
            
            int count = _updateList.Count;
            for (int i = 0; i < count; i++) {
                if (_updateList[i] is { } update) {
                    update.OnUpdate(DeltaTime);
                }
            }
            
            count = _updateList.Count;
            int validCount = count;
            
            for (int i = count - 1; i >= 0; i--) {
                if (_updateList[i] is null && _updateList[--validCount] is { } swap) {
                    _updateList[i] = swap;
                    _indexMap[swap] = i;
                }
            }
            
            _updateList.RemoveRange(validCount, count - validCount);

            FrameCount++;
                
#if UNITY_EDITOR
            if (count != validCount && TimeSources.ShowDebugInfo) Log($"cleaned {count - validCount} null subscribers");
#endif
        }

        public void Reset() {
            IsPaused = false;
            
            _updateList.Clear();
            _indexMap.Clear();
        }

#if UNITY_EDITOR
        private void Log(string message) {
            Debug.Log($"TimeSource[{_name}]: f {Time.frameCount}, {message}, state:\n{GetStateString()}");
        }
        
        private string GetStateString() {
            var sb = new StringBuilder($"Subscribers ({_updateList.Count}):\n");
            for (int i = 0; i < _updateList.Count; i++) {
                var sub = _updateList[i];
                sb.AppendLine(sub == null 
                    ? $"[{i}] null" 
                    : $"[{i}] {sub} [index in map {_indexMap.GetValueOrDefault(sub, -1)}]"
                );
            }
            return sb.ToString();
        }  
#endif
    }

}
