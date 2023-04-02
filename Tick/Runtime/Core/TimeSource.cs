using System.Collections.Generic;

namespace MisterGames.Tick.Core {

    internal sealed class TimeSource : ITimeSource, ITimeSourceApi {

        public float DeltaTime => _deltaTime;
        public float TimeScale { get => _timeScaleProvider.TimeScale; set => _timeScaleProvider.TimeScale = value; }
        public bool IsPaused { get => _isPaused; set => _isPaused = value; }

        public int SubscribersCount => _updateList.Count;

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
            _isInUpdateLoop = _deltaTime > 0f;

            if (_isInUpdateLoop) {
                int count = _updateList.Count;
                for (int i = 0; i < count; i++) {
                    var update = _updateList[i];

                    if (update is null) {
                        _updateList.RemoveAt(i--);
                        count--;
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
            _deltaTime = _isPaused ? 0f : _deltaTimeProvider.DeltaTime * _timeScaleProvider.TimeScale;
        }
    }

}
