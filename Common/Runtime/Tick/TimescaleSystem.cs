using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public sealed class TimescaleSystem : ITimescaleSystem, IDisposable {
        
        private readonly PriorityMap<int, float> _priorityMap = new();
        private readonly Dictionary<int, byte> _sourceToChangeIdMap = new();
        
        public void Dispose() {
            _priorityMap.Clear();
            _sourceToChangeIdMap.Clear();
        }

        public void SetTimeScale(object source, int priority, float timeScale) {
            int hash = source.GetHashCode();
            
            _priorityMap.Set(hash, timeScale, priority);
            
            byte id = _sourceToChangeIdMap.GetValueOrDefault(hash);
            _sourceToChangeIdMap[hash] = id.IncrementUncheckedRef();
            
            Time.timeScale = CalculateTimeScale(_priorityMap.GetFirstOrder());
        }

        public void RemoveTimeScale(object source) {
            int hash = source.GetHashCode();
            _sourceToChangeIdMap.Remove(hash);
            
            if (!_priorityMap.Remove(hash)) return;
            
            Time.timeScale = CalculateTimeScale(_priorityMap.GetFirstOrder());
        }

        public async UniTask ChangeTimeScale(
            object source,
            int priority,
            float timescale,
            float duration,
            bool removeOnFinish = false,
            AnimationCurve curve = null,
            CancellationToken cancellationToken = default) 
        {
            int hash = source.GetHashCode();

            byte currentId;
            byte id = _sourceToChangeIdMap.GetValueOrDefault(hash);
            _sourceToChangeIdMap[hash] = id.IncrementUncheckedRef();
            
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float startTimescale = _priorityMap.GetValueOrDefault(hash, 1f);
            
            _priorityMap.Set(hash, startTimescale, priority);
            
            while (t < 1f && 
                   !cancellationToken.IsCancellationRequested && 
                   _sourceToChangeIdMap.TryGetValue(hash, out currentId) && id == currentId) 
            {
                t = Mathf.Clamp01(t + Time.unscaledDeltaTime * speed);
                
                _priorityMap.Set(hash, Mathf.Lerp(startTimescale, timescale, curve?.Evaluate(t) ?? t), priority);
                Time.timeScale = CalculateTimeScale(_priorityMap.GetFirstOrder());

                await UniTask.Yield();
            }

            if (removeOnFinish && _sourceToChangeIdMap.TryGetValue(hash, out currentId) && id == currentId) {
                RemoveTimeScale(source);
            }
        }

        public float GetTimeScale() {
            return Time.timeScale;
        }

        public float GetTimeScale(int order) {
            return CalculateTimeScale(order);
        }

        private float CalculateTimeScale(int maxOrder) {
            float lowestTimescale = 1f;
            int firstOrder = _priorityMap.GetFirstOrder();
            
            if (firstOrder > maxOrder) return lowestTimescale;
            
            maxOrder = Mathf.Min(firstOrder, maxOrder);
            var keys = _priorityMap.SortedKeys;
            
            for (int i = 0; i < keys.Count; i++) {
                int key = keys[i];
                int order = _priorityMap.GetOrder(key);
                
                if (order > maxOrder) break;
                
                float timescale = _priorityMap[key];
                if (timescale < lowestTimescale) lowestTimescale = timescale;
            }
            
            return lowestTimescale;
        }
    }
    
}