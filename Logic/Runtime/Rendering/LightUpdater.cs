using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Rendering {
    
    public sealed class LightUpdater : MonoBehaviour, IUpdate {

        [SerializeField] private HDAdditionalLightData[] _lights;
        [SerializeField] private UpdateMode _updateMode = UpdateMode.Period;
        [VisibleIf(nameof(_updateMode), 1)]
        [SerializeField] [Min(0f)] private float _period = 0.5f;

        private enum UpdateMode {
            OnEnable,
            Period,
        }

        private float _timer;
        
        private void Awake() {
            for (int i = 0; i < _lights.Length; i++) {
                _lights[i].shadowUpdateMode = ShadowUpdateMode.OnDemand;
            }
        }

        private void OnEnable() {
            UpdateLights();
            SetupUpdates();
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        private void SetupUpdates() {
            _timer = Random.value * _period;

            switch (_updateMode) {
                case UpdateMode.OnEnable:
                    PlayerLoopStage.LateUpdate.Unsubscribe(this);
                    break;
                
                case UpdateMode.Period:
                    PlayerLoopStage.LateUpdate.Subscribe(this);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void IUpdate.OnUpdate(float dt) {
            _timer += dt;
            
            if (_timer < _period) return;

            _timer -= _period;
            UpdateLights();
        }
        
        private void UpdateLights() {
            for (int i = 0; i < _lights.Length; i++) {
                _lights[i].RequestShadowMapRendering();
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _lights = GetComponentsInChildren<HDAdditionalLightData>();
        }

        private void OnValidate() {
            if (Application.isPlaying && enabled) SetupUpdates();
        }
#endif
    }
    
}