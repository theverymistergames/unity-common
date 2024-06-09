using System;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Transport {
    
    [RequireComponent(typeof(CarController))]
    public sealed class CarVfx : MonoBehaviour, IActorComponent {
        
        [SerializeField] private InputActionKey _lightInput;
        [SerializeField] private bool _enableLightsOnEnter = true;
        [SerializeField] private LightData[] _lights;
        [SerializeField] private TrailData[] _trails;
        
        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOff;
        [SerializeReference] [SubclassSelector] private IActorAction _onLightsOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onLightsOff;

        [Serializable]
        private struct LightData {
            public float intensity;
            public bool isBrake;
            [ColorUsage(showAlpha: true, hdr: true)] public Color colorOff;
            [ColorUsage(showAlpha: true, hdr: true)] public Color colorOn;
            [VisibleIf(nameof(isBrake))]
            [ColorUsage(showAlpha: true, hdr: true)] public Color colorOnBrake;
            public Light[] lights;
            public Renderer[] renderers;
        }

        [Serializable]
        private struct TrailData {
            public TrailRenderer trailRenderer;
            public WheelCollider wheelCollider;
        }
        
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        
        private IActor _actor;
        private CarController _carController;
        private bool _isLightEnabled;
        private bool _isBrakeEnabled;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _carController = GetComponent<CarController>();
            
            InitializeRenderers();
            SetLightEnabled(false, false);
        }

        private void OnEnable() {
            _lightInput.OnPress += OnLightInput;
            
            _carController.OnEnter += OnEnterCar;
            _carController.OnExit += OnExitCar;
            
            _carController.OnBrake += OnBrake;

            if (_carController.IsEntered) OnEnterCar();
        }

        private void OnDisable() {
            _lightInput.OnPress -= OnLightInput;
            _carController.OnEnter -= OnEnterCar;
            _carController.OnExit -= OnExitCar;
            _carController.OnBrake -= OnBrake;
            
            OnExitCar();
        }

        private void InitializeRenderers() {
            for (int i = 0; i < _lights.Length; i++) {
                ref var data = ref _lights[i];
                
                for (int j = 0; j < data.renderers.Length; j++) {
                    var renderer = data.renderers[j];
                    
                    if (renderer.material == renderer.sharedMaterial) {
                        renderer.material = new Material(renderer.sharedMaterial);
                    }
                }
            }
        }
        
        private void OnEnterCar() {
            if (!_enableLightsOnEnter) return;
            
            SetLightEnabled(true, false);
        }

        private void OnExitCar() {
            SetLightEnabled(false, false);
        }
        
        private void OnBrake(bool isBrakeActive) {
            SetLightEnabled(_isLightEnabled, isBrakeActive);
            ApplyTrails(isBrakeActive);

            (isBrakeActive ? _onBrakeOn : _onBrakeOff)?.Apply(_actor, destroyCancellationToken).Forget();
        }

        private void ApplyTrails(bool isBrakeActive) {
            for (int i = 0; i < _trails.Length; i++) {
                ref var trail = ref _trails[i];
                trail.trailRenderer.emitting = isBrakeActive && trail.wheelCollider.isGrounded;
            }
        }

        private void OnLightInput() {
            SetLightEnabled(!_isLightEnabled, _isBrakeEnabled);
        }
        
        private void SetLightEnabled(bool enabled, bool isBrakeEnabled) {
            bool wasEnabled = _isLightEnabled;
            
            _isLightEnabled = enabled;
            _isBrakeEnabled = isBrakeEnabled;

            if (wasEnabled != _isLightEnabled) {
                (_isLightEnabled ? _onLightsOn : _onLightsOff)?.Apply(_actor, destroyCancellationToken).Forget();
            }
            
            for (int i = 0; i < _lights.Length; i++) {
                ref var data = ref _lights[i];
                
                var color = data.isBrake && isBrakeEnabled
                        ? data.colorOnBrake 
                        : enabled ? data.colorOn : data.colorOff;

                for (int j = 0; j < data.lights.Length; j++) {
                    var light = data.lights[j];
                    if (light == null) continue;

                    light.enabled = data.isBrake && isBrakeEnabled || enabled;
                    light.color = color;
                    light.intensity = data.intensity;
                }

                for (int j = 0; j < data.renderers.Length; j++) {
                    if (data.renderers[j] == null) continue;
                    
#if UNITY_EDITOR
                    if (!Application.isPlaying) {
                        if (data.renderers[j].sharedMaterial == null) continue;
                        
                        data.renderers[j].sharedMaterial.SetColor(EmissiveColor, color);
                        UnityEditor.EditorUtility.SetDirty(data.renderers[j].sharedMaterial);
                        
                        continue;
                    }
#endif
                    
                    data.renderers[j].material.SetColor(EmissiveColor, color);   
                }
            }
        }

#if UNITY_EDITOR
        [Button]
        private void ToggleLights() {
            SetLightEnabled(!_isLightEnabled, _isBrakeEnabled);
        }
        
        [Button]
        private void ToggleBrakes() {
            SetLightEnabled(_isLightEnabled, !_isBrakeEnabled);
        }

        private void OnValidate() {
            SetLightEnabled(_isLightEnabled, _isBrakeEnabled);
        }
#endif
    }
    
}