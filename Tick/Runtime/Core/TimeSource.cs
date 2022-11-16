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
        private readonly HashSet<int> _unsubscribeIndexSet = new HashSet<int>();
        private ITimeProvider _timeProvider;

        private float _deltaTime;
        private float _timeScale = 1f;

        private bool _isPaused;
        private bool _isEnabled;
        private bool _isInitialized;

        public void Subscribe(IUpdate sub) {
            int index = _updateList.IndexOf(sub);
            if (index < 0) _updateList.Add(sub);
        }

        public void Unsubscribe(IUpdate sub) {
            int index = _updateList.IndexOf(sub);
            if (index < 0) return;

            if (IsNotUpdating()) {
                _updateList.RemoveAt(index);
                return;
            }

            _unsubscribeIndexSet.Add(index);
        }

        public void Run(IJob job) {
            job.Start();
            if (job.IsCompleted) return;

            if (job is IUpdate update) Subscribe(update);
        }

        public void Initialize(ITimeProvider timeProvider) {
            _timeProvider = timeProvider;

            _isInitialized = true;
            UpdateDeltaTime();
        }

        public void DeInitialize() {
            _isInitialized = false;
            UpdateDeltaTime();

            _timeProvider = null;

            _updateList.Clear();
            _unsubscribeIndexSet.Clear();
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
            if (IsNotUpdating()) return;
            UpdateDeltaTime();

            for (int i = _updateList.Count - 1; i >= 0; i--) {
                var update = _updateList[i];

                if (_unsubscribeIndexSet.Contains(i)) {
                    _updateList.RemoveAt(i);
                    continue;
                }

                update.OnUpdate(_deltaTime);

                if (update is IJob { IsCompleted: true }) {
                    _updateList.RemoveAt(i);
                }
            }

            _unsubscribeIndexSet.Clear();
        }

        public void UpdateDeltaTime() {
            _deltaTime = IsNotUpdating() ? 0f : _timeProvider.UnscaledDeltaTime * _timeScale;
        }

        private bool IsNotUpdating() {
            return _isPaused || !_isEnabled || !_isInitialized;
        }
    }

}
