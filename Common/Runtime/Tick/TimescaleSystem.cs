using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Maths;
using Unity.Collections;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public sealed class TimescaleSystem : ITimescaleSystem, IDisposable {

        private readonly Dictionary<int, (byte priority, float timescale)> _timescaleMap = new();
        private readonly Dictionary<int, byte> _sourceToChangeIdMap = new();

        private float _baseFixedDt;
        
        public void Initialize() {
            _baseFixedDt = Time.fixedDeltaTime;
        }
        
        public void Dispose() {
            _timescaleMap.Clear();
            _sourceToChangeIdMap.Clear();
        }

        public float GetTimescale() {
            return Time.timeScale;
        }

        public float GetTimescale(TimescalePriority priority) {
            return GetTimescale((byte) priority);
        }

        public float GetTimescale(byte priority) {
            return CalculateTimescale(priority);
        }

        public void SetTimescale(object source, TimescalePriority priority, float timescale) {
            SetTimescale(source, (byte) priority, timescale);
        }

        public void SetTimescale(object source, byte priority, float timescale) {
            int hash = source.GetHashCode();
            _timescaleMap[hash] = (priority, timescale);
            
            byte id = _sourceToChangeIdMap.GetValueOrDefault(hash);
            _sourceToChangeIdMap[hash] = id.IncrementUncheckedRef();
            
            ApplySystemTimescale(CalculateTimescale(0));
        }

        public void RemoveTimescale(object source) {
            int hash = source.GetHashCode();
            _sourceToChangeIdMap.Remove(hash);
            
            if (!_timescaleMap.Remove(hash)) return;
            
            ApplySystemTimescale(CalculateTimescale(0));
        }

        public UniTask ChangeTimescale(
            object source,
            TimescalePriority priority,
            float timescale,
            float duration,
            bool removeOnFinish = false,
            AnimationCurve curve = null,
            CancellationToken cancellationToken = default) 
        {
            return ChangeTimescale(source, (byte) priority, timescale, duration, removeOnFinish, curve, cancellationToken);
        }

        public async UniTask ChangeTimescale(
            object source,
            byte priority,
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
            float startTimescale = _timescaleMap.GetValueOrDefault(hash, (priority, timescale: 1f)).timescale;
            
            _timescaleMap[hash] = (priority, startTimescale);
            
            while (t < 1f && 
                   !cancellationToken.IsCancellationRequested && 
                   _sourceToChangeIdMap.TryGetValue(hash, out currentId) && id == currentId) 
            {
                t = Mathf.Clamp01(t + TimeSources.unscaledDeltaTime * speed);
                float ts = Mathf.Lerp(startTimescale, timescale, curve?.Evaluate(t) ?? t);
                _timescaleMap[hash] = (priority, ts);
                
                ApplySystemTimescale(CalculateTimescale(0));
                
                await UniTask.Yield();
            }

            if (removeOnFinish && _sourceToChangeIdMap.TryGetValue(hash, out currentId) && id == currentId) {
                RemoveTimescale(source);
            }
        }

        private void ApplySystemTimescale(float timescale) {
            Time.timeScale = timescale;
            Time.fixedDeltaTime = _baseFixedDt * timescale;
        }
        
        private float CalculateTimescale(byte minPriority) {
            if (_timescaleMap.Count <= 0) return 1f;
            
            var priorityTimescaleMap = new NativeHashMap<byte, float>(2, Allocator.Temp);
            
            foreach ((byte prior, float timescale) in _timescaleMap.Values) {
                if (prior < minPriority) continue;

                if (priorityTimescaleMap.TryGetValue(prior, out float existingTimescale)) {
                    priorityTimescaleMap[prior] = Mathf.Min(existingTimescale, timescale);
                }
                else {
                    priorityTimescaleMap[prior] = timescale;
                }
            }

            float result = 1f;
            
            foreach (var kv in priorityTimescaleMap) {
                result *= kv.Value;
            }
            
            priorityTimescaleMap.Dispose();
            
            return result;
        }
    }
    
}