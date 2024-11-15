using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioPool : MonoBehaviour, IAudioPool {

        [SerializeField] private AudioSource _prefab;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;

        public static IAudioPool Main { get; private set; }

        private CancellationToken _cancellationToken;
        
        private void Awake() {
            Main = this;
            _cancellationToken = destroyCancellationToken;
        }

        private void OnDestroy() {
            Main = null;
        }
        
        public void Play(AudioClip clip, Vector3 position, float volume = 1, float pitch = 1, float spatialBlend = 1f, bool loop = false, CancellationToken cancellationToken = default) {
            var source = GetAudioSourceAtWorldPosition(position);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, loop);
            ReleaseDelayed(source, clip.length, loop, cancellationToken).Forget();
        }

        public void Play(AudioClip clip, Transform attachTo, Vector3 localPosition = default, float volume = 1, float pitch = 1, float spatialBlend = 1f, bool loop = false, CancellationToken cancellationToken = default) {
            var source = GetAudioSourceAttached(attachTo, localPosition);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, loop);
            ReleaseDelayed(source, clip.length, loop, cancellationToken).Forget();
        }

        private AudioSource GetAudioSourceAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity);
        }

        private AudioSource GetAudioSourceAttached(Transform parent, Vector3 localPosition = default) {
            var audioSource = PrefabPool.Main.Get(_prefab, parent);
            audioSource.transform.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            return audioSource;
        }

        private static void RestartAudioSource(AudioSource source, AudioClip clip, float volume = 1, float pitch = 1, float spatialBlend = 1f, bool loop = false) {
            source.Stop();
            
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            
            source.Play();
        }

        private async UniTask ReleaseDelayed(AudioSource source, float delay, bool loop, CancellationToken cancellationToken) {
            if (loop) {
                while (!cancellationToken.IsCancellationRequested && !_cancellationToken.IsCancellationRequested) {
                    await UniTask.Yield();
                }   
            }
            else {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            if (_cancellationToken.IsCancellationRequested) return;

            float t = 0f;
            float speed = _fadeOut > 0f ? 1f / _fadeOut : float.MaxValue;
            float startVolume = source.volume;
            
            while (!_cancellationToken.IsCancellationRequested && t < 1f) {
                t += Time.unscaledDeltaTime * speed;
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                
                await UniTask.Yield();
            }
            
            if (_cancellationToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }
    }
    
}