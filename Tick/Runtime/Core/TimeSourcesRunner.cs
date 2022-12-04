using System;
using UnityEngine;

namespace MisterGames.Tick.Core {
    
    public class TimeSourcesRunner : MonoBehaviour, ITimeSourceProvider {

        private readonly TimeSource _preUpdateTimeSource = new TimeSource(TimeProviders.Main);
        private readonly TimeSource _updateTimeSource = new TimeSource(TimeProviders.Main);
        private readonly TimeSource _unscaledUpdateTimeSource = new TimeSource(TimeProviders.Unscaled);
        private readonly TimeSource _lateUpdateTimeSource = new TimeSource(TimeProviders.Main);
        private readonly TimeSource _fixedUpdateTimeSource = new TimeSource(TimeProviders.Fixed);

        public ITimeSource Get(PlayerLoopStage stage) {
            return stage switch {
                PlayerLoopStage.PreUpdate => _preUpdateTimeSource,
                PlayerLoopStage.Update => _updateTimeSource,
                PlayerLoopStage.UnscaledUpdate => _unscaledUpdateTimeSource,
                PlayerLoopStage.LateUpdate => _lateUpdateTimeSource,
                PlayerLoopStage.FixedUpdate => _fixedUpdateTimeSource,
                _ => throw new NotImplementedException($"There is no initialized TimeSource for player loop stage {stage}")
            };
        }

        private void Awake() {
            TimeSources.InjectProvider(this);
        }

        private void OnEnable() {
            _preUpdateTimeSource.IsPaused = false;
            _updateTimeSource.IsPaused = false;
            _unscaledUpdateTimeSource.IsPaused = false;
            _lateUpdateTimeSource.IsPaused = false;
            _fixedUpdateTimeSource.IsPaused = false;
        }

        private void OnDisable() {
            _preUpdateTimeSource.IsPaused = true;
            _updateTimeSource.IsPaused = true;
            _unscaledUpdateTimeSource.IsPaused = true;
            _lateUpdateTimeSource.IsPaused = true;
            _fixedUpdateTimeSource.IsPaused = true;
        }

        private void OnDestroy() {
            _preUpdateTimeSource.Reset();
            _updateTimeSource.Reset();
            _unscaledUpdateTimeSource.Reset();
            _lateUpdateTimeSource.Reset();
            _fixedUpdateTimeSource.Reset();
        }

        private void Start() {
            _preUpdateTimeSource.Tick();
            _updateTimeSource.Tick();
            _unscaledUpdateTimeSource.Tick();
            _lateUpdateTimeSource.Tick();
            _fixedUpdateTimeSource.Tick();
        }

        private void Update() {
            _preUpdateTimeSource.Tick();
            _updateTimeSource.Tick();
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
