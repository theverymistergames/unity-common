using System;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class SoundOnTrigger : MonoBehaviour {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private PositionMode _positionMode;
        [SerializeField] private Collider[] _colliders;
        
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
        [SerializeField] [Min(0f)] private float _minSizeVolumeMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxSizeVolumeMul = 1f;
        
        [Header("Volume by Speed")]
        [SerializeField] [Min(0f)] private float _minSpeed;
        [SerializeField] [Min(0f)] private float _maxSpeed = 1f;
        [SerializeField] [Min(0f)] private float _minSpeedVolumeMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxSpeedVolumeMul = 1f;
        
        [Header("Sounds")]
        [SerializeField] private SoundData[] _enterSounds;
        [SerializeField] private SoundData[] _exitSounds;

        private enum PositionMode {
            ColliderPosition,
            ClosestPointOnBounds,
        }
        
        [Serializable]
        private struct SoundData {
            [Min(0f)] public float minColliderSize;
            [Min(0f)] public float minSpeed;
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
            if (collider == null || !_layerMask.Contains(collider.gameObject.layer)) return;
            
            float size = GetColliderSize(collider);
            float sqrSpeed = GetSqrSpeed(collider);
            
            int index = GetSoundDataIndex(size, sqrSpeed, _enterSounds);
            if (index < 0) return;
            
            ref var data = ref _enterSounds[index];
            float volumeMul = GetVolumeMul(size, sqrSpeed) * data.volumeMul;
            
            PlaySound(data.clips, GetPosition(collider), volumeMul);
        }

        private void TriggerExit(Collider collider) {
            if (collider == null || !_layerMask.Contains(collider.gameObject.layer)) return;
            
            float size = GetColliderSize(collider);
            float sqrSpeed = GetSqrSpeed(collider);
            
            int index = GetSoundDataIndex(size, sqrSpeed, _exitSounds);
            if (index < 0) return;
            
            ref var data = ref _exitSounds[index];
            float volumeMul = GetVolumeMul(size, sqrSpeed) * data.volumeMul;
            
            PlaySound(data.clips, GetPosition(collider), volumeMul);
        }

        private Vector3 GetPosition(Collider collider) {
            var colliderPos = collider.transform.position;
            switch (_positionMode) {
                case PositionMode.ColliderPosition:
                    return colliderPos;
                
                case PositionMode.ClosestPointOnBounds:
                    float minSqrDistance = float.MaxValue;
                    var point = colliderPos;
                    
                    for (int i = 0; i < _colliders.Length; i++) {
                        var c = _colliders[i];
                        var p = c.ClosestPoint(colliderPos);
                        
                        float sqrDistance = (p - colliderPos).sqrMagnitude;
                        
                        if (sqrDistance < minSqrDistance) continue;

                        point = p;
                        minSqrDistance = sqrDistance;
                    }

                    return point;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private static int GetSoundDataIndex(float colliderSize, float sqrSpeed, SoundData[] sounds) {
            for (int i = sounds.Length - 1; i >= 0; i--) {
                ref var data = ref sounds[i];
                
                if (colliderSize >= data.minColliderSize &&
                    (sqrSpeed < 0f || sqrSpeed >= data.minSpeed * data.minSpeed)) 
                {
                    return i;
                }
            }

            return -1;
        }

        private static float GetColliderSize(Collider collider) {
            var size = collider.bounds.size;
            return Mathf.Max(Mathf.Max(size.x, size.y), size.z);
        }
        
        private static float GetSqrSpeed(Collider collider) {
            if (collider.attachedRigidbody is not { } rb || rb.isKinematic) return -1f;
            
            return rb.linearVelocity.sqrMagnitude;
        }

        private float GetVolumeMul(float colliderSize, float sqrSpeed) {
            float tSize = _maxSize - _minSize > 0f ? (colliderSize - _minSize) / (_maxSize - _minSize) : 1f;
            float tSpeed = sqrSpeed >= 0f && _maxSpeed - _minSpeed > 0f ? (Mathf.Sqrt(sqrSpeed) - _minSpeed) / (_maxSpeed - _minSpeed) : 1f;
            
            return Mathf.Lerp(_minSizeVolumeMul, _maxSizeVolumeMul, tSize) * Mathf.Lerp(_minSpeedVolumeMul, _maxSpeedVolumeMul, tSpeed);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_maxSize < _minSize) _maxSize = _minSize;
            if (_maxSizeVolumeMul < _minSizeVolumeMul) _maxSizeVolumeMul = _minSizeVolumeMul;
            if (_maxSpeedVolumeMul < _minSpeedVolumeMul) _maxSpeedVolumeMul = _minSpeedVolumeMul;

            ValidateSoundData(_enterSounds);
            ValidateSoundData(_exitSounds);
        }

        private static void ValidateSoundData(SoundData[] sounds) {
            float size = 0f;
            
            for (int i = 0; i < sounds?.Length; i++) {
                ref var data = ref sounds[i];
                if (data.minColliderSize < size) data.minColliderSize = size;
                size = data.minColliderSize;
            }
        }
#endif
    }
    
}