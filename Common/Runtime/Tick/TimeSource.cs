﻿using System.Collections.Generic;

namespace MisterGames.Common.Tick {

    internal sealed class TimeSource : ITimeSource, ITimeSourceApi {

        public float DeltaTime => _deltaTime;
        public float TimeScale { get => _timeScaleProvider.TimeScale; set => _timeScaleProvider.TimeScale = value; }
        public bool IsPaused { get => _isPaused; set => _isPaused = value; }

        public int SubscribersCount => _updateList.Count;

        private readonly List<IUpdate> _updateList = new();
        private readonly Dictionary<int, int> _indexMap = new();
        
        private readonly IDeltaTimeProvider _deltaTimeProvider;
        private readonly ITimeScaleProvider _timeScaleProvider;

        private float _deltaTime;
        private bool _isPaused;

        public TimeSource(IDeltaTimeProvider deltaTimeProvider, ITimeScaleProvider timeScaleProvider) {
            _deltaTimeProvider = deltaTimeProvider;
            _timeScaleProvider = timeScaleProvider;
        }

        public bool Subscribe(IUpdate sub) {
            if (!_indexMap.TryAdd(sub.GetHashCode(), _updateList.Count)) return false;

            _updateList.Add(sub);
            return true;
        }

        public bool Unsubscribe(IUpdate sub) {
            if (!_indexMap.Remove(sub.GetHashCode(), out int index)) return false;

            _updateList[index] = null;
            return true;
        }

        public void Tick() {
            UpdateDeltaTime();

            int count = _updateList.Count;
            int validCount = count;
                
            for (int i = count - 1; i >= 0; i--) {
                if (_updateList[i] is { } update) {
                    update.OnUpdate(_deltaTime);
                    continue;
                }

                if (_updateList[--validCount] is { } swap) {
                    _updateList[i] = swap;
                    _indexMap[swap.GetHashCode()] = i;
                }
            }

            _updateList.RemoveRange(validCount, count - validCount);
        }

        public void Reset() {
            ResetImmediately();
        }

        private void ResetImmediately() {
            _isPaused = false;
            _deltaTime = 0f;
            _updateList.Clear();
            _indexMap.Clear();
        }

        private void UpdateDeltaTime() {
            _deltaTime = _isPaused ? 0f : _deltaTimeProvider.DeltaTime * _timeScaleProvider.TimeScale;
        }
    }

}
