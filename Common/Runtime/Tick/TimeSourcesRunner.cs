using System;
using System.Collections.Generic;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Tick {

    [DefaultExecutionOrder(-1_000_000)]
    public sealed class TimeSourcesRunner : MonoBehaviour, ITimeSourceProvider {

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        public ITimeSource UpdateStage() {
            return _updateTimeSource;
        }

        public ITimeSource FixedUpdateStage() {
            return _fixedUpdateTimeSource;
        }

        public bool ShowDebugInfo => _showDebugInfo;
        
        private readonly TimeSource _preUpdateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global, "pre update");
        private readonly TimeSource _updateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global, "update");
        private readonly TimeSource _unscaledUpdateTimeSource = new TimeSource(DeltaTimeProviders.Unscaled, TimeScaleProviders.Create(), "unscaled update");
        private readonly TimeSource _lateUpdateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global, "late update");
        private readonly TimeSource _fixedUpdateTimeSource = new TimeSource(DeltaTimeProviders.Fixed, TimeScaleProviders.Global, "fixed update");

        private readonly List<TimeSource> _timeSources = new List<TimeSource>();

        private readonly TimescaleSystem _timescaleSystem = new();
        
        public ITimeSource Get(PlayerLoopStage stage) {
            return stage switch {
                PlayerLoopStage.PreUpdate => _preUpdateTimeSource,
                PlayerLoopStage.Update => _updateTimeSource,
                PlayerLoopStage.UnscaledUpdate => _unscaledUpdateTimeSource,
                PlayerLoopStage.LateUpdate => _lateUpdateTimeSource,
                PlayerLoopStage.FixedUpdate => _fixedUpdateTimeSource,
                _ => throw new NotImplementedException($"No initialized {nameof(ITimeSource)} found for {nameof(PlayerLoopStage)} {stage}")
            };
        }

        private void Awake() {
            _timeSources.Add(_preUpdateTimeSource);
            _timeSources.Add(_updateTimeSource);
            _timeSources.Add(_unscaledUpdateTimeSource);
            _timeSources.Add(_lateUpdateTimeSource);
            _timeSources.Add(_fixedUpdateTimeSource);

            Services.Register<ITimescaleSystem>(_timescaleSystem);
            
            TimeSources.InjectProvider(this);
        }

        private void OnDestroy() {
            for (int i = 0; i < _timeSources.Count; i++) {
                _timeSources[i].Reset();
            }
            
            _timescaleSystem.Dispose();
            
            Services.Unregister(_timescaleSystem);
        }

        private void Start() {
            for (int i = 0; i < _timeSources.Count; i++) {
                _timeSources[i].Tick();
            }
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
