using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Tick.Core {

    [DefaultExecutionOrder(-10000)]
    public class TimeSourcesRunner : MonoBehaviour, ITimeSourceProvider {

        private readonly TimeSource _preUpdateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global);
        private readonly TimeSource _updateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global);
        private readonly TimeSource _unscaledUpdateTimeSource = new TimeSource(DeltaTimeProviders.Unscaled, TimeScaleProviders.Create());
        private readonly TimeSource _lateUpdateTimeSource = new TimeSource(DeltaTimeProviders.Main, TimeScaleProviders.Global);
        private readonly TimeSource _fixedUpdateTimeSource = new TimeSource(DeltaTimeProviders.Fixed, TimeScaleProviders.Global);

        private readonly List<TimeSource> _timeSources = new List<TimeSource>();

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
            TimeSources.InjectProvider(this);

            _timeSources.Add(_preUpdateTimeSource);
            _timeSources.Add(_updateTimeSource);
            _timeSources.Add(_unscaledUpdateTimeSource);
            _timeSources.Add(_lateUpdateTimeSource);
            _timeSources.Add(_fixedUpdateTimeSource);
        }

        private void OnDestroy() {
            for (int i = 0; i < _timeSources.Count; i++) {
                _timeSources[i].Reset();
            }
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
