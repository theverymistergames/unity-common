using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Common.Volumes;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioSourceZone : MonoBehaviour, IUpdate {
        
        [Header("Zone")]
        [SerializeField] private Transform _root;
        [SerializeField] private PositionWeightProvider _positionWeightProvider;
        [SerializeField] private AnimationCurve _weightCurve = EasingType.Linear.ToAnimationCurve();
        
        [Header("Sound")]
        [SerializeField] private HashId _attachId;
        [SerializeField] [MinMaxSlider(0f, 3f)] private Vector2 _volumeRemap = new(1f, 1f);
        [SerializeField] [MinMaxSlider(0f, 3f)] private Vector2 _pitchRemap = new(1f, 1f);
        [SerializeField] [Range(0f, 1f)] private float _spatialBlend = 1f;
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _startTime;
        [SerializeField] [Min(0f)] private float _fadeIn;
        [SerializeField] [Min(-1f)] private float _fadeOut = -1f;
        [SerializeField] private AudioOptions _options = AudioOptions.Everything;
        [SerializeField] private AudioMixerGroup _mixerGroup;
        [SerializeField] private AudioClip[] _audioClipVariants;
        
        private AudioHandle _soundInstance;

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            ReleaseSound();
        }

        void IUpdate.OnUpdate(float dt) {
            if (!(AudioPool.Main?.TryGetListenerPosition(out var listenerPos) ?? false) || 
                _positionWeightProvider.GetWeight(listenerPos) is not { weight: > 0f } weightData) 
            {
                ReleaseSound();
                return;
            }

            var soundInstance = GetOrCreateSoundInstance();
            float t = _weightCurve.Evaluate(weightData.weight);
            
            soundInstance.Volume = Mathf.Lerp(_volumeRemap.x, _volumeRemap.y, t);
            soundInstance.PitchMul = Mathf.Lerp(_pitchRemap.x, _pitchRemap.y, t);
            soundInstance.SpatialBlend = _spatialBlend;
            soundInstance.Position = weightData.closestPoint;
        }

        private AudioHandle GetOrCreateSoundInstance() {
            if (_soundInstance.IsValid()) return _soundInstance;
            
            if (_audioClipVariants is not { Length: > 0 } || AudioPool.Main is not {} pool) return default;
            
            var clip = pool.ShuffleClips(_audioClipVariants);
            float resultStartTime = _startTime.GetRandomInRange();
            
            _soundInstance = pool.Play(clip, _root, localPosition: default, _attachId, volume: 0f, _fadeIn, _fadeOut, pitch: 1f, _spatialBlend, resultStartTime, _mixerGroup, _options);
            return _soundInstance;
        }
        
        private void ReleaseSound() {
            _soundInstance.Release();
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void Reset() {
            _root = transform;
            _positionWeightProvider = GetComponent<PositionWeightProvider>();
        }

        private void OnDrawGizmos() {
            if (_showDebugInfo && _soundInstance.IsValid()) {
                DebugExt.DrawLabel(_soundInstance.Position, $"[V = {_soundInstance.Volume}, clip = {_soundInstance}]");
            }
        }
#endif
    }
    
}