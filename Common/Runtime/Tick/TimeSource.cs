using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Tick {

    internal sealed class TimeSource {

        public float DeltaTime { get; private set; }
        public float UnscaledDeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }
        public float FixedUnscaledDeltaTime { get; private set; }
        public float ScaledTime { get; private set; }
        public float UnscaledTime { get; private set; }
        public int FrameCount { get; private set; }
        public bool IsAppPaused { get; private set; }
        public bool IsAppFocused { get; private set; } = true;
        public int SubscribersCount => _indexMap.Count;

        private const int InitialCapacity = 32;
        
        private readonly Dictionary<IUpdate, Entry> _indexMap = new(InitialCapacity);
        private readonly List<IUpdate>[] _updateListMap = { new(), new(), new(), new(), new(), };
        private bool _discardNextFrameDt;

        private readonly struct Entry {
            public readonly int stage;
            public readonly int index;
            public Entry(int stage, int index) {
                this.stage = stage;
                this.index = index;
            }
        }
        
        public void Subscribe(IUpdate sub, PlayerLoopStage stage) {
            if (sub == null) {
                throw new NullReferenceException($"{nameof(TimeSource)}.Subscribe: f {Time.frameCount}, subscriber should not be null. Stage: {stage}");
            }
            
            int s = (int) stage;
            
            if (_indexMap.TryGetValue(sub, out var entry)) {
                if (entry.stage == s) return;
                
                _updateListMap[entry.stage][entry.index] = null;
                _indexMap.Remove(sub);
            }

            var list = _updateListMap[s];
            list.Add(sub);
            
            _indexMap[sub] = new Entry(s, list.Count - 1);
        }

        public void Unsubscribe(IUpdate sub) {
            if (sub == null) {
                throw new NullReferenceException($"{nameof(TimeSource)}.Unsubscribe: f {Time.frameCount}, subscriber should not be null");
            }
            
            if (!_indexMap.Remove(sub, out var entry)) return;

            _updateListMap[entry.stage][entry.index] = null;
        }

        public void OnAppPause(bool paused) {
            IsAppPaused = paused;
            _discardNextFrameDt |= !paused;
        }

        public void OnAppFocused(bool focused) {
            IsAppFocused = focused;
            _discardNextFrameDt |= focused;
        }

        public void TickUpdate(float dtScaled, float dtUnscaled) {
            UnscaledDeltaTime = IsAppPaused || !IsAppFocused || _discardNextFrameDt ? 0f : dtUnscaled;
            DeltaTime = dtScaled;

            UnscaledTime += UnscaledDeltaTime;
            ScaledTime += DeltaTime;

            _discardNextFrameDt = false;

            Tick((int) PlayerLoopStage.PreUpdate, DeltaTime);
            Tick((int) PlayerLoopStage.Update, DeltaTime);
            Tick((int) PlayerLoopStage.UnscaledUpdate, UnscaledDeltaTime);

            FrameCount++;
        }

        public void TickLateUpdate(float dtScaled, float dtUnscaled) {
            Tick((int) PlayerLoopStage.LateUpdate, DeltaTime);
        }

        public void TickFixedUpdate(float dtScaled, float dtUnscaled) {
            FixedDeltaTime = dtScaled;
            FixedUnscaledDeltaTime = dtUnscaled;

            Tick((int) PlayerLoopStage.FixedUpdate , FixedDeltaTime);
        }

        private void Tick(int stage, float dt) {
            var list =  _updateListMap[stage];
            int count = list.Count;
            int freeCount = 0;

            for (int i = 0; i < count; i++) {
                var updatable = list[i];

                if (updatable == null) {
                    freeCount++;
                    continue;
                }

                updatable.OnUpdate(dt);
            }

            if (freeCount > list.Count * 0.5f) {
                RemoveEmptySpaces(stage);
            }
        }
        
        private void RemoveEmptySpaces(int stage) {
            var list =  _updateListMap[stage];
            int count = list.Count;
            int validCount = count;
            
            for (int i = count - 1; i >= 0; i--) {
                if (list[i] is not null || list[--validCount] is not { } swap) continue;
                
                list[i] = swap;
                _indexMap[swap] = new Entry(stage, i);
            }
            
            list.RemoveRange(validCount, count - validCount);
        }
    }

}
