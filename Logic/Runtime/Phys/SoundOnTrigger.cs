using System;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class SoundOnTrigger : MonoBehaviour {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        
        [Header("Sound Settings")]
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _volume = new Vector2(0.8f, 1f);
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new Vector2(0.9f, 1.1f);
        [SerializeField] [Min(0f)] private float _fadeIn;
        [SerializeField] [Min(-1f)] private float _fadeOut = -1f;
        [SerializeField] [Range(0f, 1f)] private float _spatialBlend = 1f;
        [SerializeField] private AudioOptions _audioOptions;
        
        [Header("Volume by Size")]
        [SerializeField] [Min(0f)] private float _minSize;
        [SerializeField] [Min(0f)] private float _maxSize = 1f;
        [SerializeField] [Min(0f)] private float _minVolumeMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxVolumeMul = 1f;
        
        [Header("Sounds")]
        [SerializeField] private SoundData[] _enterSounds;
        [SerializeField] private SoundData[] _exitSounds;

        [Serializable]
        private struct SoundData {
            [Min(0f)] public float minSize;
            [Min(0f)] public float volumeMul;
            public AudioClip[] clips;
        }
        
        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEnter; 
            _triggerEmitter.TriggerExit += TriggerExit; 
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEnter; 
            _triggerEmitter.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider collider) {
            float size = GetColliderSize(collider);
            int index = GetSoundDataIndex(size, _enterSounds);
            if (index < 0) return;
            
            ref var data = ref _enterSounds[index];
            float volumeMul = GetVolumeMul(size) * data.volumeMul;
            
            PlaySound(data.clips, collider.transform.position, volumeMul);
        }

        private void TriggerExit(Collider collider) {
            float size = GetColliderSize(collider);
            int index = GetSoundDataIndex(size, _exitSounds);
            if (index < 0) return;
            
            ref var data = ref _exitSounds[index];
            float volumeMul = GetVolumeMul(size) * data.volumeMul;
            
            PlaySound(data.clips, collider.transform.position, volumeMul);
        }

        private void PlaySound(AudioClip[] clips, Vector3 point, float volumeMul) {
            AudioPool.Main.Play(
                AudioPool.Main.ShuffleClips(clips), 
                point, 
                volume: _volume.GetRandomInRange() * volumeMul,
                fadeIn: _fadeIn,
                fadeOut: _fadeOut,
                pitch: _pitch.GetRandomInRange(),
                spatialBlend: _spatialBlend,
                options: _audioOptions
            );
        }

        private static int GetSoundDataIndex(float colliderSize, SoundData[] sounds) {
            for (int i = 0; i < sounds.Length; i++) {
                ref var data = ref sounds[i];
                if (colliderSize > data.minSize) return i;
            }

            return -1;
        }

        private static float GetColliderSize(Collider collider) {
            var size = collider.bounds.size;
            return Mathf.Max(Mathf.Max(size.x, size.y), size.z);
        }

        private float GetVolumeMul(float colliderSize) {
            float t = _maxSize - _minSize > 0f ? (colliderSize - _minSize) / (_maxSize - _minSize) : 1f;
            return Mathf.Lerp(_minVolumeMul, _maxVolumeMul, t);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_maxSize < _minSize) _maxSize = _minSize;
            if (_maxVolumeMul < _minVolumeMul) _maxVolumeMul = _minVolumeMul;

            ValidateSoundData(_enterSounds);
            ValidateSoundData(_exitSounds);
        }

        private static void ValidateSoundData(SoundData[] sounds) {
            float size = 0f;
            
            for (int i = 0; i < sounds?.Length; i++) {
                ref var data = ref sounds[i];
                if (data.minSize < size) data.minSize = size;
                size = data.minSize;
            }
        }
#endif
    }
    
}