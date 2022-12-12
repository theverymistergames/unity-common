﻿using System.Collections.Generic;
using MisterGames.Common.Maths;

namespace MisterGames.Tick.Core {

    public sealed class TimeSource : ITimeSource, ITimeSourceApi {

        public float DeltaTime => _deltaTime;
        public float TimeScale { get => _timeScaleProvider.TimeScale; set => _timeScaleProvider.TimeScale = value; }
        public bool IsPaused { get => _isPaused; set => _isPaused = value; }

        private readonly IDeltaTimeProvider _deltaTimeProvider;
        private readonly ITimeScaleProvider _timeScaleProvider;
        private readonly List<IUpdate> _updateList = new List<IUpdate>();

        private float _deltaTime;
        private bool _isPaused;
        private bool _isPendingReset;
        private bool _isInUpdateLoop;

        public TimeSource(IDeltaTimeProvider deltaTimeProvider, ITimeScaleProvider timeScaleProvider) {
            _deltaTimeProvider = deltaTimeProvider;
            _timeScaleProvider = timeScaleProvider;
        }

        public bool Subscribe(IUpdate sub) {
            int index = _updateList.IndexOf(sub);
            if (index >= 0) return false;

            _updateList.Add(sub);
            return true;
        }

        public bool Unsubscribe(IUpdate sub) {
            int index = _updateList.IndexOf(sub);
            if (index < 0) return false;

            if (_isInUpdateLoop) {
                _updateList[index] = null;
                return true;
            }

            _updateList.RemoveAt(index);
            return true;
        }

        public void Tick() {
            UpdateDeltaTime();
            _isInUpdateLoop = _deltaTime > 0f && !_isPaused;

            if (_isInUpdateLoop) {
                for (int i = _updateList.Count - 1; i >= 0; i--) {
                    var update = _updateList[i];

                    if (update is null) {
                        _updateList.RemoveAt(i);
                        continue;
                    }

                    update.OnUpdate(_deltaTime);
                }

                _isInUpdateLoop = false;
                UpdateDeltaTime();
            }

            if (_isPendingReset) ResetImmediately();
        }

        public void Reset() {
            _isPendingReset = true;
            if (!_isInUpdateLoop) ResetImmediately();
        }

        private void ResetImmediately() {
            _isPendingReset = false;

            _isPaused = false;
            _deltaTime = 0f;
            _updateList.Clear();
        }

        private void UpdateDeltaTime() {
            _deltaTime = _deltaTimeProvider.DeltaTime * _timeScaleProvider.TimeScale;
        }

        public override string ToString() {
            return $"{nameof(TimeSource)}(\n" +
                   $"timeProvider {_deltaTimeProvider}, \n" +
                   $"timeScale {TimeScale}, dt {DeltaTime}, \n" +
                   $"subscribers ({_updateList.Count}): [{(_updateList.Count == 0 ? "]\n" : $"\n- {string.Join("\n- ", _updateList)}\n]")}" +
                   ")";
        }
    }

}
