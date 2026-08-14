using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Tick {

    [DefaultExecutionOrder(-1_000_000)]
    internal sealed class TimeSourceRunner : MonoBehaviour {

        private readonly TimescaleSystem _timescaleSystem = new();
        private readonly TimeSource _timeSource = new();
        
        private void Awake() {
            TimeSources.InjectTimeSource(_timeSource);
            
            _timescaleSystem.Initialize();
            Services.Register<ITimescaleSystem>(_timescaleSystem);
        }

        private void OnDestroy() {
            _timescaleSystem.Dispose();
            Services.Unregister(_timescaleSystem);
        }

        private void OnApplicationPause(bool pauseStatus) {
            _timeSource.OnAppPause(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus) {
#if UNITY_EDITOR
            hasFocus = true;
#endif
            
            _timeSource.OnAppFocused(hasFocus);
        }

        private void Update() {
            _timeSource.TickUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        private void LateUpdate() {
            _timeSource.TickLateUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        private void FixedUpdate() {
            _timeSource.TickFixedUpdate(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
        }
    }
    
}
