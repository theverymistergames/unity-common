using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Character.Transport {
    
    [RequireComponent(typeof(CarController))]
    public sealed class CarEffects : MonoBehaviour, IActorComponent, IUpdate {
        
        [Header("Lights")]
        [SerializeField] private InputActionRef _lightInput;
        [SerializeField] private bool _enableLightsOnEnter = true;
        [SerializeField] private float _intensityMultiplier = 1f;
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

        [Header("Engine Sound")]
        [SerializeField] private AudioSource _engineAudioSource;
        [SerializeField] private AudioClip _engineSound;
        [SerializeField] [Min(0f)] private float _minPitchEngine = 0.9f;
        [SerializeField] [Min(0f)] private float _maxPitchEngine = 2f;
        [SerializeField] private float _rpmToPitch = 1f;
        [SerializeField] [Min(0f)] private float _minVolumeEngine = 0.3f;
        [SerializeField] [Min(0f)] private float _maxVolumeEngine = 1f;
        [SerializeField] [Min(0f)] private float _rpmToVolume = 1f;
        
        [Header("Brakes Sound")]
        [SerializeField] private AudioSource _brakesAudioSource;
        [SerializeField] private AudioClip _brakesSound;
        [SerializeField] [Min(0f)] private float _minPitchBrakes = 0.9f;
        [SerializeField] [Min(0f)] private float _maxPitchBrakes = 2f;
        [SerializeField] [Min(0f)] private float _brakeForceToPitch = 1f;
        [SerializeField] [Min(0f)] private float _minVolumeBrakes = 0.3f;
        [SerializeField] [Min(0f)] private float _maxVolumeBrakes = 1f;
        [SerializeField] [Min(0f)] private float _brakeForceToVolume = 1f;
        [SerializeField] [Min(0f)] private float _minSpeedToPlayBrakesSound = 1f;
        
        [Header("Brakes Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOn;
        [SerializeReference] [SubclassSelector] private IActorAction _onBrakeOff;

        [Header("Brakes Trails")]
        [SerializeField] private float _trailGroundOffset = 0.05f;
        [SerializeField] private TrailData[] _trails;
        
        [Serializable]
        private struct LightData {
            public float intensity;
            public float emissionIntensity;
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
        private float _ignitionTurnOffTime;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _carController = GetComponent<CarController>();
            
            _engineAudioSource.clip = _engineSound;
            _engineAudioSource.loop = true;

            _brakesAudioSource.clip = _brakesSound;
            _brakesAudioSource.loop = true;
            _brakesAudioSource.volume = 0f; 
            
            InitializeRenderers();
            SetLightEnabled(false, false);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _lightInput.Get().performed += OnLightInput;
            
            _carController.OnEnter += OnEnterCar;
            _carController.OnExit += OnExitCar;
            _carController.OnStartIgnition += OnStartIgnition;
            _carController.OnIgnition += OnIgnition;
            _carController.OnBrake += OnBrake;

            if (_carController.IsEntered) OnEnterCar();
            if (_carController.IsBrakeOn) OnBrake(true);
            if (_carController.IsIgnitionOn) OnIgnition(true);
            
            DisableTrails();
            
            _brakesAudioSource.Play();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);

            _lightInput.Get().performed -= OnLightInput;
            
            _carController.OnEnter -= OnEnterCar;
            _carController.OnExit -= OnExitCar;
            _carController.OnStartIgnition -= OnStartIgnition;
            _carController.OnIgnition -= OnIgnition;
            _carController.OnBrake -= OnBrake;
            
            SetLightEnabled(false, false);
            DisableTrails();
            
            _engineAudioSource.Stop();
            _brakesAudioSource.Stop();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateEngineSound();
            UpdateBrakesSound();
            UpdateTrails();
        }

        private void UpdateEngineSound() {
            float pitch = Mathf.Clamp(_rpmToPitch * _carController.Rpm, _minPitchEngine, _maxPitchEngine);
            float volume = Mathf.Clamp(_rpmToVolume * _carController.Rpm, _minVolumeEngine, _maxVolumeEngine);

            _engineAudioSource.volume = volume;
            _engineAudioSource.pitch = pitch;
        }

        private void UpdateBrakesSound() {
            float brakeRatio = _carController.BrakeForce * _carController.Speed;
            float pitch = Mathf.Clamp(_brakeForceToPitch * brakeRatio, _minPitchBrakes, _maxPitchBrakes);
            float volume = Mathf.Clamp(_brakeForceToVolume * brakeRatio, _minVolumeBrakes, _maxVolumeBrakes);

            _brakesAudioSource.pitch = pitch;
            _brakesAudioSource.volume = volume *
                                        _carController.IsBrakeOn.AsFloat() *
                                        _carController.AreBrakeWheelsGrounded.AsFloat() *
                                        (_carController.Speed >= _minSpeedToPlayBrakesSound).AsFloat();
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
        }

        private void OnIgnition(bool on) {
            SetLightIntensityMultiplier(1f);
            
            if (on) {
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
            (isBrakeActive ? _onBrakeOn : _onBrakeOff)?.Apply(_actor, destroyCancellationToken).Forget();
        }

        private async UniTask ApplyIgnitionLightBlink(CancellationToken cancellationToken) {
            float timer = 0f;
            
            while (!cancellationToken.IsCancellationRequested && 
                   Time.time < _ignitionStartTime + _ignitionDuration && 
                   _ignitionStartTime > _ignitionTurnOffTime
            ) {
                timer += Time.deltaTime;
                float m = _ignitionLightIntensityMultiplier + Mathf.Sin(timer * _ignitionBlinkFrequency) * _ignitionBlinkRange;
                
                SetLightIntensityMultiplier(m);

                await UniTask.Yield();
            }
        }

        private void UpdateTrails() {
            var up = _carController.Root.up;
            
            for (int i = 0; i < _trails.Length; i++) {
                ref var trail = ref _trails[i];
                trail.wheelCollider.GetWorldPose(out var pos, out _);
                
                trail.trailRenderer.transform.position = pos + up * (_trailGroundOffset - trail.wheelCollider.radius);
                trail.trailRenderer.emitting = trail.wheelCollider.isGrounded && _carController.IsWheelBrakeActive(trail.wheelCollider);
            }
        }
        
        private void DisableTrails() {
            for (int i = 0; i < _trails.Length; i++) {
                ref var trail = ref _trails[i];
                trail.trailRenderer.emitting = false;
            }
        }

        private void OnLightInput(InputAction.CallbackContext callbackContext) {
            SetLightEnabled(!_isLightEnabled, _isBrakeEnabled);
        }

        private void SetLightIntensityMultiplier(float multiplier) {
            for (int i = 0; i < _lights.Length; i++) {
                ref var data = ref _lights[i];
                
                var color = data.isBrake && _isBrakeEnabled
                    ? data.colorOnBrake 
                    : enabled ? data.colorOn : data.colorOff;
                
                SetLightIntensity(ref data, _intensityMultiplier * multiplier);
                SetLightEmission(ref data, color * data.emissionIntensity * _intensityMultiplier * multiplier);
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
                
                var color = data.isBrake && _isBrakeEnabled
                        ? data.colorOnBrake 
                        : enabled ? data.colorOn : data.colorOff;

                for (int j = 0; j < data.lights.Length; j++) {
                    data.lights[j].enabled = data.isBrake && isBrakeEnabled || enabled;
                }
                
                SetLightIntensity(ref data, _intensityMultiplier);
                SetLightEmission(ref data, color * data.emissionIntensity * _intensityMultiplier);
            }
        }

        private void SetLightIntensity(ref LightData data, float intensity) {
            for (int j = 0; j < data.lights.Length; j++) {
                var light = data.lights[j];
                if (light == null) continue;
                
                var color = data.isBrake && _isBrakeEnabled
                    ? data.colorOnBrake 
                    : enabled ? data.colorOn : data.colorOff;

                light.color = color;
                light.intensity = data.intensity * intensity;
            }
        }

        private void SetLightEmission(ref LightData data, Color color) {
            for (int j = 0; j < data.renderers.Length; j++) {
#if UNITY_EDITOR
                if (data.renderers[j] == null) continue;
                
                if (!Application.isPlaying) {
                    if (data.renderers[j].sharedMaterial == null) continue;
                        
                    data.renderers[j].sharedMaterial.SetColor(EmissiveColor, color);
                    EditorUtility.SetDirty(data.renderers[j].sharedMaterial);
                        
                    continue;
                }

                if (PrefabUtility.IsPartOfRegularPrefab(data.renderers[j])) continue;
#endif

                data.renderers[j].material.SetColor(EmissiveColor, color);   
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