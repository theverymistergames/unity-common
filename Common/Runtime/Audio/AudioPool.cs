using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioPool : MonoBehaviour, IAudioPool {

        [SerializeField] private AudioSource _prefab;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        [SerializeField] [Min(0f)] private float _lastIndexStoreLifetime = 60f;

        private readonly struct IndexData {
            
            public static readonly IndexData Invalid = new(-1, 0f); 
            
            public readonly int index;
            public readonly float time;
            
            public IndexData(int index, float time) {
                this.index = index;
                this.time = time;
            }
        }
        
        public static IAudioPool Main { get; private set; }

        private readonly HashSet<int> _validHandles = new();
        private int _handleId;
        
        private readonly Dictionary<int, IndexData> _lastIndexMap = new();
        private readonly List<int> _keysBuffer = new();
        private CancellationToken _cancellationToken;
        
        private void Awake() {
            Main = this;
            _cancellationToken = destroyCancellationToken;

            StartIndexStorageChecks(_cancellationToken).Forget();
        }

        private void OnDestroy() {
            _validHandles.Clear();
            Main = null;
        }
        
        public AudioHandle Play(
            AudioClip clip, 
            Vector3 position, 
            float fadeIn = 0f,
            float volume = 1f, 
            float pitch = 1f, 
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            bool loop = false, 
            CancellationToken cancellationToken = default) 
        {
            if (clip == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: trying to play clip that is null");          
#endif
                return AudioHandle.Invalid;
            }
            
            int id = ++_handleId;
            var source = GetAudioSourceAtWorldPosition(position);
            
            _validHandles.Add(id);
            
            RestartAudioSource(id, source, clip, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, cancellationToken).Forget();
            ReleaseDelayed(id, source, clip.length, loop, cancellationToken).Forget();
            
            return new AudioHandle(id, source, this);
        }

        public AudioHandle Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            float fadeIn = 0f,
            float volume = 1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            bool loop = false,
            CancellationToken cancellationToken = default) 
        {
            if (clip == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: trying to play clip that is null");          
#endif
                return AudioHandle.Invalid;
            }
            
            int id = ++_handleId;
            var source = GetAudioSourceAttached(attachTo, localPosition);
            
            _validHandles.Add(id);
            
            RestartAudioSource(id, source, clip, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, cancellationToken).Forget();
            ReleaseDelayed(id, source, clip.length, loop, cancellationToken).Forget();

            return new AudioHandle(id, source, this);
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
        
        public void ReleaseAudioHandle(int id) {
            _validHandles.Remove(id);
        }

        public bool IsValidAudioHandle(int id) {
            return _validHandles.Contains(id);
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

        private UniTask RestartAudioSource(
            int id,
            AudioSource source,
            AudioClip clip,
            float fadeIn,
            float volume, 
            float pitch, 
            float spatialBlend,
            float normalizedTime, 
            bool loop, 
            CancellationToken cancellationToken) 
        {
            source.Stop();

            source.clip = clip;
            source.time = normalizedTime * clip.length;
            source.volume = fadeIn > 0f ? 0f : volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            
            source.Play();
            
            return fadeIn > 0f ? FadeIn(id, source, fadeIn, volume, cancellationToken) : default;
        }

        private async UniTask ReleaseDelayed(int id, AudioSource source, float delay, bool loop, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = loop ? 0f : delay > 0f ? 1f / delay : float.MaxValue;
            
            while (t < 1f && _validHandles.Contains(id) && 
                   !cancellationToken.IsCancellationRequested && !_cancellationToken.IsCancellationRequested) 
            {
                t += Time.unscaledDeltaTime * speed;
                await UniTask.Yield();
            }
            
            if (_cancellationToken.IsCancellationRequested) return;

            _validHandles.Remove(id);

            await FadeOut(source, _fadeOut);
            
            if (_cancellationToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }

        private async UniTask FadeIn(int id, AudioSource source, float duration, float volume, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;

            while (t < 1f && _validHandles.Contains(id) && 
                   !cancellationToken.IsCancellationRequested && !_cancellationToken.IsCancellationRequested) 
            {
                t += Time.unscaledDeltaTime * speed;
                source.volume = Mathf.Lerp(0f, volume, t);
                
                await UniTask.Yield();
            }
        }
        
        private async UniTask FadeOut(AudioSource source, float duration) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float startVolume = source.volume;
            
            while (t < 1f && !_cancellationToken.IsCancellationRequested) {
                t += Time.unscaledDeltaTime * speed;
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                
                await UniTask.Yield();
            }
        }
    }
    
}