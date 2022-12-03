using UnityEngine;

namespace MisterGames.Tick.Core {
    
    public class TimeSourcesRunner : MonoBehaviour, ITimeSourceProvider {

        ITimeSource ITimeSourceProvider.MainUpdate => _mainUpdateTimeSource;
        ITimeSource ITimeSourceProvider.LateUpdate => _lateUpdateTimeSource;
        ITimeSource ITimeSourceProvider.FixedUpdate => _fixedUpdateTimeSource;
        ITimeSource ITimeSourceProvider.UnscaledUpdate => _unscaledUpdateTimeSource;

        private readonly TimeSource _mainUpdateTimeSource = new TimeSource(TimeProviders.Main);
        private readonly TimeSource _lateUpdateTimeSource = new TimeSource(TimeProviders.Main);
        private readonly TimeSource _fixedUpdateTimeSource = new TimeSource(TimeProviders.Fixed);
        private readonly TimeSource _unscaledUpdateTimeSource = new TimeSource(TimeProviders.Unscaled);

        private void Awake() {
            TimeSources.InjectProvider(this);
        }

        private void OnEnable() {
            _mainUpdateTimeSource.IsPaused = false;
            _lateUpdateTimeSource.IsPaused = false;
            _fixedUpdateTimeSource.IsPaused = false;
            _unscaledUpdateTimeSource.IsPaused = false;
        }

        private void OnDisable() {
            _mainUpdateTimeSource.IsPaused = true;
            _lateUpdateTimeSource.IsPaused = true;
            _fixedUpdateTimeSource.IsPaused = true;
            _unscaledUpdateTimeSource.IsPaused = true;
        }

        private void OnDestroy() {
            _mainUpdateTimeSource.Reset();
            _lateUpdateTimeSource.Reset();
            _fixedUpdateTimeSource.Reset();
            _unscaledUpdateTimeSource.Reset();
        }

        private void Start() {
            _mainUpdateTimeSource.Tick();
            _lateUpdateTimeSource.Tick();
            _fixedUpdateTimeSource.Tick();
            _unscaledUpdateTimeSource.Tick();
        }

        private void Update() {
            _mainUpdateTimeSource.Tick();
            _unscaledUpdateTimeSource.Tick();
        }
        
        private void LateUpdate() {
            _lateUpdateTimeSource.Tick();
        }
        
        private void FixedUpdate() {
            _fixedUpdateTimeSource.Tick();
        }
    }
    
}
