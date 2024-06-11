using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Transport {
    
    [RequireComponent(typeof(CarController))]
    public sealed class CarEffects : MonoBehaviour, IActorComponent {
        
        [Header("Lights")]
        [SerializeField] private InputActionKey _lightInput;
        [SerializeField] private bool _enableLightsOnEnter = true;
        [SerializeField] private LightData[] _lights;
        [SerializeReference] [SubclassSelector] private IActorAction _onLightsOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onLightsOff;

        [Header("Ignition")]
        [SerializeField] [Min(0f)] private float _ignitionLightIntensityMultiplier = 0.5f;
        [SerializeField] [Min(0f)] private float _ignitionBlinkFrequency = 6f;
        [SerializeField] [Min(0f)] private float _ignitionBlinkRange = 0.2f;
        [SerializeReference] [SubclassSelector] private IActorAction _onIgnitionStart;
        [SerializeReference] [SubclassSelector] private IActorAction _onIgnitionOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onIgnitionOff;

        [Header("Engine")]
        [SerializeField] private AudioSource _engineAudioSource;
        [SerializeField] private AudioClip _engineSound;
        [SerializeField] [Min(0f)] private float _startEngineAfterIgnitionStartedDelay;
        [SerializeField] [Min(0f)] private float _minPitch;
        [SerializeField] [Min(0f)] private float _maxPitch;
        [SerializeField] private float _rpmToPitch;
        
        [Header("Brakes")]
        [SerializeField] private TrailData[] _trails;
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOff;
        
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

        private CancellationTokenSource _enableCts;
        private IActor _actor;
        private CarController _carController;
        private bool _isLightEnabled;
        private bool _isBrakeEnabled;
        private float _ignitionDuration;
        private float _ignitionStartTime;
        private float _ignitionTurnOnTime;
        private float _ignitionTurnOffTime;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _carController = GetComponent<CarController>();
            
            _engineAudioSource.clip = _engineSound;
            _engineAudioSource.loop = true;
            
            InitializeRenderers();
            SetLightEnabled(false, false);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _lightInput.OnPress += OnLightInput;
            
            _carController.OnEnter += OnEnterCar;
            _carController.OnExit += OnExitCar;
            _carController.OnStartIgnition += OnStartIgnition;
            _carController.OnIgnition += OnIgnition;
            _carController.OnBrake += OnBrake;

            if (_carController.IsEntered) OnEnterCar();
            if (_carController.IsBrakeOn) OnBrake(true);
            if (_carController.IsIgnitionOn) OnIgnition(true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);

            _lightInput.OnPress -= OnLightInput;
            _carController.OnEnter -= OnEnterCar;
            _carController.OnExit -= OnExitCar;
            _carController.OnStartIgnition -= OnStartIgnition;
            _carController.OnIgnition -= OnIgnition;
            _carController.OnBrake -= OnBrake;
            
            SetLightEnabled(false, false);
        }

        private void LateUpdate() {
            UpdateEngineSound();
        }

        private void UpdateEngineSound() {
            float pitch = Mathf.Clamp(_rpmToPitch * _carController.Rpm, _minPitch, _maxPitch);
            _engineAudioSource.pitch = pitch;
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
            if (_enableLightsOnEnter) SetLightEnabled(true, _isBrakeEnabled);
        }

        private void OnExitCar() {
            SetLightEnabled(false, false);
        }
        
        private void OnStartIgnition(float duration) {
            _ignitionStartTime = Time.time;
            _ignitionDuration = duration;
            
            _onIgnitionStart?.Apply(_actor, _enableCts.Token).Forget();
            
            ApplyIgnitionLightBlink(_enableCts.Token).Forget();
            StartEngineSoundAfterDelay(_startEngineAfterIgnitionStartedDelay, _enableCts.Token).Forget();
        }

        private async UniTask ApplyIgnitionLightBlink(CancellationToken cancellationToken) {
            float timer = 0f;
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);
            
            while (!cancellationToken.IsCancellationRequested && 
                   Time.time < _ignitionStartTime + _ignitionDuration && 
                   _ignitionStartTime > _ignitionTurnOffTime
            ) {
                timer += timeSource.DeltaTime;
                float m = _ignitionLightIntensityMultiplier + Mathf.Sin(timer * _ignitionBlinkFrequency) * _ignitionBlinkRange;
                
                SetLightIntensityMultiplier(m);

                await UniTask.Yield();
            }
        }
        
        private async UniTask StartEngineSoundAfterDelay(float delay, CancellationToken cancellationToken) {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            
            if (cancellationToken.IsCancellationRequested || _ignitionTurnOffTime > _ignitionStartTime) return;

            if (!_engineAudioSource.isPlaying) _engineAudioSource.Play();
        }

        private void OnIgnition(bool on) {
            SetLightIntensityMultiplier(1f);

            if (on) {
                _ignitionTurnOnTime = Time.time;
                if (!_engineAudioSource.isPlaying) _engineAudioSource.Play();
                _onIgnitionOn?.Apply(_actor, _enableCts.Token).Forget();
                return;
            }
            
            _ignitionTurnOffTime = Time.time;
            _engineAudioSource.Stop();
            _onIgnitionOff?.Apply(_actor, _enableCts.Token).Forget();
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

        private void SetLightIntensityMultiplier(float multiplier) {
            for (int i = 0; i < _lights.Length; i++) {
                ref var data = ref _lights[i];
                
                for (int j = 0; j < data.lights.Length; j++) {
                    data.lights[j].intensity = data.intensity * multiplier;
                }
            }
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