using System.Collections.Generic;
using MisterGames.Common.Maths;

namespace MisterGames.Tick.Core {

    public sealed class TimeSource : ITimeSource, ITimeSourceApi {

        public float DeltaTime => _deltaTime;

        public float TimeScale {
            get => _timeProvider.TimeScale;
            set => _timeProvider.TimeScale = value;
        }

        public bool IsPaused {
            get => _timeProvider.TimeScale.IsNearlyZero();
            set {
                if (IsPaused == value) return;
                if (value) {
                    _cachedTimeScale = _timeProvider.TimeScale;
                    _timeProvider.TimeScale = 0f;
                }
                else {
                    _timeProvider.TimeScale = _cachedTimeScale;
                }
            }
        }

        private readonly ITimeProvider _timeProvider;
        private readonly List<IUpdate> _updateList = new List<IUpdate>();

        private float _deltaTime;
        private float _cachedTimeScale = 1f;
        private bool _isPendingReset;
        private bool _isInUpdateLoop;

        public TimeSource(ITimeProvider timeProvider) {
            _timeProvider = timeProvider;
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

        public void Reset() {
            _isPendingReset = true;
            if (!_isInUpdateLoop) ResetImmediately();
        }

        public void Tick() {
            _deltaTime = _timeProvider.DeltaTime;
            _isInUpdateLoop = _deltaTime > 0f;

            if (_isInUpdateLoop) {
                for (int i = _updateList.Count - 1; i >= 0; i--) {
                    var update = _updateList[i];
                    if (CheckRemove(i, update)) continue;

                    update.OnUpdate(_deltaTime);

                    CheckRemove(i, update);
                }

                _isInUpdateLoop = false;
                _deltaTime = _timeProvider.DeltaTime;
            }

            if (_isPendingReset) Reset();
        }

        private void ResetImmediately() {
            _isPendingReset = false;
            _updateList.Clear();
        }

        private bool CheckRemove(int index, IUpdate update) {
            if (update is not null) return false;

            _updateList.RemoveAt(index);
            return true;
        }

        public override string ToString() {
            return $"{nameof(TimeSource)}(\n" +
                   $"timeProvider {_timeProvider}, \n" +
                   $"timeScale {TimeScale}, dt {DeltaTime}, \n" +
                   $"subscribers ({_updateList.Count}): [{(_updateList.Count == 0 ? "]\n" : $"\n- {string.Join("\n- ", _updateList)}\n]")}" +
                   ")";
        }
    }

}
