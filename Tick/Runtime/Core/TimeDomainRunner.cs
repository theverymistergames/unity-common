using MisterGames.Tick.TimeProviders;
using UnityEngine;

namespace MisterGames.Tick.Core {
    
    public class TimeDomainRunner : MonoBehaviour {

        [SerializeField] private TimeDomain[] _timeDomains;

        private readonly ITimeProviderFactory _timeProviderFactory = new DefaultTimeProviderFactory();

        private void Awake() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeDomain = _timeDomains[i];
                var timeProvider = _timeProviderFactory.Create(timeDomain.TimerProviderType);

                timeDomain.SourceApi.Initialize(timeProvider);
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].SourceApi.DeInitialize();
            }
        }

        private void OnEnable() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].SourceApi.Enable();
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].SourceApi.Disable();
            }
        }

        private void Start() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].SourceApi.UpdateDeltaTime();
            }
        }

        private void Update() {
            UpdateTickerGroup(TimerProviderType.MainUpdate);
        }
        
        private void LateUpdate() {
            UpdateTickerGroup(TimerProviderType.LateUpdate);
        }
        
        private void FixedUpdate() {
            UpdateTickerGroup(TimerProviderType.FixedUpdate);
        }

        private void UpdateTickerGroup(TimerProviderType timerProviderType) {
            for (int i = 0; i < _timeDomains.Length; i++) {
                var timeDomain = _timeDomains[i];
                if (timeDomain.TimerProviderType == timerProviderType) timeDomain.SourceApi.Tick();
            }
        }
    }
    
}
