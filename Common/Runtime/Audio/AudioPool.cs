using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioPool : MonoBehaviour, IAudioPool {

        [SerializeField] private AudioSource _prefab;

        public static IAudioPool Main { get; private set; } 
        
        private void Awake() {
            Main = this;
        }

        private void OnDestroy() {
            Main = null;
        }
        
        public void Play(AudioClip clip, Vector3 position, float volume = 1, float pitch = 1, float spatialBlend = 1f, bool loop = false) {
            var source = GetAudioSourceAtWorldPosition(position);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, loop);
            if (!loop) ReleaseDelayed(source, clip.length, destroyCancellationToken).Forget();
        }

        public void Play(AudioClip clip, Transform attachTo, Vector3 localPosition = default, float volume = 1, float pitch = 1, float spatialBlend = 1f, bool loop = false) {
            var source = GetAudioSourceAttached(attachTo, localPosition);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, loop);
            if (!loop) ReleaseDelayed(source, clip.length, destroyCancellationToken).Forget();
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

        private static async UniTask ReleaseDelayed(AudioSource source, float delay, CancellationToken cancellationToken) {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (cancellationToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }
    }
    
}