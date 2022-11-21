using System.Collections.Generic;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.TimeProviders;

namespace MisterGames.Tick.Core {

    public sealed class TimeSource : ITimeSource, ITimeSourceApi {

        public float DeltaTime => _deltaTime;

        public float TimeScale {
            get => _timeScale;
            set => _timeScale = value;
        }

        public bool IsPaused {
            get => _isPaused;
            set => _isPaused = value;
        }

        private readonly List<IUpdate> _updateList = new List<IUpdate>();
        private ITimeProvider _timeProvider;

        private float _timeScale = 1f;
        private float _deltaTime;

        private bool _isPaused;
        private bool _isEnabled;
        private bool _isInitialized;
        private bool _isInUpdateLoop;

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

        public void Initialize(ITimeProvider timeProvider) {
            _timeProvider = timeProvider;
            _isInitialized = true;
            UpdateDeltaTime();
        }

        public void DeInitialize() {
            _isInitialized = false;
            if (!_isInUpdateLoop) Reset();
        }

        public void Enable() {
            _isEnabled = true;
            UpdateDeltaTime();
        }

        public void Disable() {
            _isEnabled = false;
            UpdateDeltaTime();
        }

        public void Tick() {
            UpdateDeltaTime();

            _isInUpdateLoop = CanTick();
            if (!_isInUpdateLoop) return;

            for (int i = _updateList.Count - 1; i >= 0; i--) {
                var update = _updateList[i];

                if (update == null) {
                    _updateList.RemoveAt(i);
                    continue;
                }

                update.OnUpdate(_deltaTime);

                if (update is null or IJob { IsCompleted: true }) {
                    _updateList.RemoveAt(i);
                }
            }

            _isInUpdateLoop = false;

            UpdateDeltaTime();
            if (!_isInitialized && _timeProvider != null) Reset();
        }

        public void UpdateDeltaTime() {
            if (_isInUpdateLoop) return;
            _deltaTime = CanTick() ? _timeProvider.UnscaledDeltaTime * _timeScale : 0f;
        }

        private bool CanTick() {
            return !_isPaused && _isEnabled && _isInitialized;
        }

        private void Reset() {
            _deltaTime = 0f;
            _timeScale = 1f;

            _isEnabled = false;
            _isPaused = false;

            _timeProvider = null;
            _updateList.Clear();
        }

        public override string ToString() {
            return $"{nameof(TimeSource)}(\n" +
                   $"timeProvider {_timeProvider}, \n" +
                   $"enabled {_isEnabled}, paused {_isPaused}, \n" +
                   $"timeScale {_timeScale}, dt {_deltaTime}, \n" +
                   $"subscribers ({_updateList.Count}): [{(_updateList.Count == 0 ? "]\n" : $"\n- {string.Join("\n- ", _updateList)}\n]")}" +
                   ")";
        }
    }

}
