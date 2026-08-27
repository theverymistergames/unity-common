using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Weather {

    public sealed class LightningController : MonoBehaviour {

        [Header("Visual")]
        [SerializeField] private Light[] _lightningLights;
        [SerializeField] private Light[] _ambientLights;
        [SerializeField] private Volume _lightningVolume;

        [Header("Behaviour")]
        [SerializeField] private bool _waitLastOnDisable = true;
        
        [Header("Delay")]
        [SerializeField] [Min(0f)] private float _startDelayMin = 3f;
        [SerializeField] [Min(0f)] private float _startDelayMax = 7f;
        [SerializeField] [Min(0f)] private float _delayBetweenStrokesMin = 10f;
        [SerializeField] [Min(0f)] private float _delayBetweenStrokesMax = 30f;
        [SerializeField] [Range(0f, 1f)] private float _mean = 0.5f;

        [Header("Stroke")]
        [SerializeField] [Min(0f)] private float _strokeDurationMin = 0.25f;
        [SerializeField] [Min(0f)] private float _strokeDurationMax = 5f;
        [SerializeField] [Min(0f)] private float _noiseSpeedMin = 1f;
        [SerializeField] [Min(0f)] private float _noiseSpeedMax = 2f;
        [SerializeField] [Range(0f, 1f)] private float _noiseThreshold = 0.5f;
        [SerializeField] [Min(0f)] private float _intensityMin = 0.5f;
        [SerializeField] [Min(0f)] private float _intensityMax = 1f;
        [SerializeField] [Min(0f)] private float _intensityNoiseSpeed = 1f;
        [SerializeField] [Range(0f, 1f)] private float _forceEnableLightsOnStroke = 0.1f;
        [SerializeField] [Range(0f, 1f)] private float _forceEnableLightsIntensityMin = 0.5f;
        
        [Header("Thunder")]
        [SerializeField] private float _thunderDelayMin = 0.5f;
        [SerializeField] private float _thunderDelayMax = 3f;
        [SerializeField] private float _thunderNoiseSpeed = 0.5f;
        [SerializeField] [Range(0f, 2f)] private float _volume = 1f;
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new(0.9f, 1.1f);
        [SerializeField] [Range(0f, 1f)] private float _spatialBlend;
        [SerializeField] [Min(0f)] private float _fadeIn;
        [SerializeField] [Min(0f)] private float _fadeOut;
        [SerializeField] private AudioClip[] _thunderSounds;

        private const float IndexOffset = 1000f;
        private const float SeedRange = 100f;

        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _destroyCts;
        private float[] _originalIntensities;
        private byte _strokeId;

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
            
            _originalIntensities = new float[_lightningLights.Length + _ambientLights.Length];

            for (int i = 0; i < _lightningLights.Length; i++) {
                _originalIntensities[i] = _lightningLights[i].intensity;
            }

            for (int i = 0; i < _ambientLights.Length; i++) {
                _originalIntensities[i + _lightningLights.Length] = _ambientLights[i].intensity;
            }
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            StartLightningRoutine(_enableCts.Token, _destroyCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);

            for (int i = 0; i < _lightningLights?.Length; i++) {
                _lightningLights[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _ambientLights?.Length; i++) {
                _ambientLights[i].gameObject.SetActive(false);
            }

            if (_lightningVolume != null) _lightningVolume.weight = 0f;
        }

        private async UniTask StartLightningRoutine(CancellationToken enableCt, CancellationToken destroyCt) {
            float delay = RandomExtensions.NextGaussian(_startDelayMin, _startDelayMax, _mean);
            await AsyncExt.Delay(delay, cancellationToken: enableCt);

            while (!enableCt.IsCancellationRequested) {
                var ct = _waitLastOnDisable ? destroyCt : enableCt;
                
                PerformThunder(GetThunderDelay(), _thunderSounds, ct).Forget();
                await PerformStoke(ct);
                
                if (enableCt.IsCancellationRequested) break;

                delay = RandomExtensions.NextGaussian(_delayBetweenStrokesMin, _delayBetweenStrokesMax, _mean);
                await AsyncExt.Delay(delay, cancellationToken: enableCt);
            }
        }

        public UniTask PerformLightning(float thunderDelay, AudioClip[] thunderSounds, CancellationToken ct) {
            float delay = thunderDelay >= 0f ? thunderDelay : GetThunderDelay();
            var sounds = thunderSounds is { Length: > 0 } ? thunderSounds : _thunderSounds;
            PerformThunder(delay, sounds, ct).Forget();
            return PerformStoke(ct);
        }

        private async UniTask PerformThunder(float delay, AudioClip[] sounds, CancellationToken cancellationToken) {
            if (sounds is not { Length: > 0 }) return;
            
            if (delay > 0f) {
                await AsyncExt.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }

            var clip = AudioPool.Main.ShuffleClips(sounds);
            
            AudioPool.Main.Play(
                clip,
                GetThunderPosition(),
                volume: _volume,
                fadeIn: _fadeIn,
                fadeOut: _fadeOut,
                pitch: _pitch.GetRandomInRange(),
                spatialBlend: _spatialBlend,
                options: AudioOptions.AffectedByTimeScale | AudioOptions.AffectedByVolumes,
                cancellationToken: cancellationToken
            );
        }

        private float GetThunderDelay() {
            return Mathf.Lerp(_thunderDelayMin, _thunderDelayMax, Mathf.PerlinNoise1D(TimeSources.scaledTime * _thunderNoiseSpeed));
        }

        private float GetIntensity() {
            return Mathf.Lerp(_intensityMin, _intensityMax, Mathf.PerlinNoise1D(TimeSources.scaledTime * _intensityNoiseSpeed));
        }

        private Vector3 GetThunderPosition() {
            var thunderPos = _lightningLights.Length > 0 ? _lightningLights[0].transform.position : transform.position;

            for (int i = 0; i < _lightningLights.Length; i++) {
                var pos = _lightningLights[i].transform.position;
                thunderPos = Vector3.Lerp(thunderPos, pos, Random.value);
            }

            return thunderPos;
        }

        private async UniTask PerformStoke(CancellationToken cancellationToken) {
            byte id = _strokeId.IncrementUncheckedRef();

            float noiseSpeed = Random.Range(_noiseSpeedMin, _noiseSpeedMax);
            float strokeDuration = Random.Range(_strokeDurationMin, _strokeDurationMax);
            float speed = strokeDuration > 0f ? 1f / strokeDuration : float.MaxValue;
            float t = 0f;

            float seed = Random.Range(-SeedRange, SeedRange);

            while (t < 1f && id == _strokeId && !cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + Time.deltaTime * speed);

                int enableCount = 0;
                int lightningCount = _lightningLights?.Length ?? 0;
                int ambientCount = _ambientLights?.Length ?? 0;

                float intensity = GetIntensity();

                for (int i = 0; i < lightningCount; i++) {
                    bool enabled = t < _forceEnableLightsOnStroke || 
                                   ShouldLightBeEnabled(seed, t * noiseSpeed, i);

                    _lightningLights![i].intensity = _originalIntensities[i] * intensity;
                    _lightningLights[i].gameObject.SetActive(enabled);

                    if (enabled) enableCount++;
                }

                for (int i = 0; i < ambientCount; i++) {
                    _ambientLights![i].intensity = _originalIntensities[i + lightningCount] * intensity;
                    _ambientLights[i].gameObject.SetActive(enableCount > 0);
                }

                if (lightningCount > 0) intensity *= (float) enableCount / lightningCount;

                if (_lightningVolume != null) _lightningVolume.weight = intensity;

                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested || id != _strokeId) return;

            for (int i = 0; i < _lightningLights?.Length; i++) {
                _lightningLights[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _ambientLights?.Length; i++) {
                _ambientLights[i].gameObject.SetActive(false);
            }

            if (_lightningVolume != null) _lightningVolume.weight = 0f;
        }

        private bool ShouldLightBeEnabled(float seed, float t, int index) {
            return Mathf.PerlinNoise1D(seed + t + index * IndexOffset) <= _noiseThreshold;
        }
    }
    
}