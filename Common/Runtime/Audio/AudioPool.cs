using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Common.Pooling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioPool : MonoBehaviour, IAudioPool {

        [SerializeField] private AudioSource _prefab;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        [SerializeField] [Min(0f)] private float _lastIndexStoreLifetime = 60f;

        public static IAudioPool Main { get; private set; }

        private readonly Dictionary<int, IndexData> _lastIndexMap = new();
        private readonly List<int> _keysBuffer = new();
        private CancellationToken _cancellationToken;

        private readonly struct IndexData {
            
            public static readonly IndexData Invalid = new(-1, 0f); 
            
            public readonly int index;
            public readonly float time;
            
            public IndexData(int index, float time) {
                this.index = index;
                this.time = time;
            }
        }
        
        private void Awake() {
            Main = this;
            _cancellationToken = destroyCancellationToken;

            StartIndexStorageChecks(_cancellationToken).Forget();
        }

        private void OnDestroy() {
            Main = null;
        }
        
        public void Play(
            AudioClip clip, 
            Vector3 position, 
            float volume = 1f, 
            float pitch = 1f, 
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            bool loop = false, 
            CancellationToken cancellationToken = default) 
        {
            var source = GetAudioSourceAtWorldPosition(position);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, normalizedTime, loop);
            ReleaseDelayed(source, clip.length, loop, cancellationToken).Forget();
        }

        public void Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            float volume = 1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            bool loop = false,
            CancellationToken cancellationToken = default) 
        {
            var source = GetAudioSourceAttached(attachTo, localPosition);
            RestartAudioSource(source, clip, volume, pitch, spatialBlend, normalizedTime, loop);
            ReleaseDelayed(source, clip.length, loop, cancellationToken).Forget();
        }

        public AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips) {
            int count = clips?.Count ?? 0;
            if (count == 0) return default;

            int s = 0;

            for (int i = 0; i < count; i++) {
                s += clips![i].GetHashCode();
            }

            var lastData = _lastIndexMap.GetValueOrDefault(s, IndexData.Invalid);
            int index = ArrayExtensions.GetRandom(0, count, tryExclude: lastData.index);

            _lastIndexMap[s] = new IndexData(index, Time.time);
            
            return clips![index];
        }

        private async UniTask StartIndexStorageChecks(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float time = Time.time;
            
                _keysBuffer.Clear();
                _keysBuffer.AddRange(_lastIndexMap.Keys);

                for (int i = 0; i < _keysBuffer.Count; i++) {
                    int k = _keysBuffer[i];
                    var data = _lastIndexMap[k];
                    if (time - data.time < _lastIndexStoreLifetime) continue;

                    _lastIndexMap.Remove(k);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_lastIndexStoreLifetime), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow(); 
            }
        }
        
        private AudioSource GetAudioSourceAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity);
        }

        private AudioSource GetAudioSourceAttached(Transform parent, Vector3 localPosition = default) {
            var audioSource = PrefabPool.Main.Get(_prefab, parent);
            audioSource.transform.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            return audioSource;
        }

        private static void RestartAudioSource(
            AudioSource source,
            AudioClip clip,
            float volume = 1f, 
            float pitch = 1f, 
            float spatialBlend = 1f,
            float normalizedTime = 0f, 
            bool loop = false) 
        {
            source.Stop();

            source.clip = clip;
            source.time = normalizedTime * clip.length;
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