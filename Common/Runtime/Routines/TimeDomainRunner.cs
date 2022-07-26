using UnityEngine;

namespace MisterGames.Common.Routines {
    
    public class TimeDomainRunner : MonoBehaviour {

        [SerializeField] private TimeDomain[] _timeDomains;

        private void Start() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].Start();
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].Terminate();
            }
        }

        private void OnEnable() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].Activate();
            }
        }
        
        private void OnDisable() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].Deactivate();
            }
        }

        private void Update() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].DoUpdate();
            }
        }
        
        private void LateUpdate() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].LateUpdate();
            }
        }
        
        private void FixedUpdate() {
            for (int i = 0; i < _timeDomains.Length; i++) {
                _timeDomains[i].FixedUpdate();
            }
        }
    }
    
}