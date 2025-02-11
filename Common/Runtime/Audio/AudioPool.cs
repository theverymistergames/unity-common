using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-999)]
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

        private readonly struct AttachKey : IEquatable<AttachKey> {
            
            public static readonly AttachKey Invalid = new(0, 0);
            
            public readonly int instanceId;
            public readonly int hash;
            
            public AttachKey(int instanceId, int hash) {
                this.instanceId = instanceId;
                this.hash = hash;
            }
            
            public bool Equals(AttachKey other) => instanceId == other.instanceId && hash == other.hash;
            public override bool Equals(object obj) => obj is AttachKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(instanceId, hash);

            public static bool operator ==(AttachKey left, AttachKey right) => left.Equals(right);
            public static bool operator !=(AttachKey left, AttachKey right) => !left.Equals(right);
        }
        
        public static IAudioPool Main { get; private set; }

        private readonly Dictionary<int, IndexData> _lastIndexMap = new();
        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, AudioSource> _handleIdToSourceMap = new();
        private readonly List<int> _keysBuffer = new();
        
        private CancellationToken _cancellationToken;
        private int _lastHandleId;
        
        private void Awake() {
            Main = this;
            _cancellationToken = destroyCancellationToken;

            StartIndexStorageChecks(_cancellationToken).Forget();
        }

        private void OnDestroy() {
            _lastIndexMap.Clear();
            _attachKeyToHandleIdMap.Clear();
            _handleIdToSourceMap.Clear();
            _keysBuffer.Clear();
            
            Main = null;
        }
        
        public AudioHandle Play(
            AudioClip clip, 
            Vector3 position, 
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
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
            
            int id = GetNextHandleId();
            var source = GetAudioSourceAtWorldPosition(position);
            
            _handleIdToSourceMap[id] = source;
            
            RestartAudioSource(id, source, clip, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, cancellationToken).Forget();
            ReleaseDelayed(id, AttachKey.Invalid, source, clip.length, loop, fadeOut, cancellationToken).Forget();
            
            return new AudioHandle(id, source, this);
        }

        public AudioHandle Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            int attachId = 0,
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
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

            int id = GetNextHandleId();
            var source = GetAudioSourceAttached(attachTo, localPosition);
            var attachKey = new AttachKey(attachTo.GetInstanceID(), attachId);

            _handleIdToSourceMap[id] = source;

            if (attachId != 0) {
                _handleIdToSourceMap.Remove(_attachKeyToHandleIdMap.GetValueOrDefault(attachKey));
                _attachKeyToHandleIdMap[attachKey] = id;   
            }
            
            RestartAudioSource(id, source, clip, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, cancellationToken).Forget();
            ReleaseDelayed(id, attachKey, source, clip.length, loop, fadeOut, cancellationToken).Forget();

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

        public AudioHandle GetAudioHandle(Transform attachedTo, int hash) {
            return _attachKeyToHandleIdMap.TryGetValue(new AttachKey(attachedTo.GetInstanceID(), hash), out int id) && 
                   _handleIdToSourceMap.TryGetValue(id, out var source)
                ? new AudioHandle(id, source, this)
                : AudioHandle.Invalid;
        }

        public void ReleaseAudioHandle(int id) {
            _handleIdToSourceMap.Remove(id);
        }

        public bool IsValidAudioHandle(int id) {
            return _handleIdToSourceMap.ContainsKey(id);
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

        private async UniTask ReleaseDelayed(
            int id,
            AttachKey attachKey,
            AudioSource source,
            float delay,
            bool loop,
            float fadeOut,
            CancellationToken cancellationToken) 
        {
            float t = 0f;
            float speed = loop ? 0f : delay > 0f ? 1f / delay : float.MaxValue;
            
            while (t < 1f && _handleIdToSourceMap.ContainsKey(id) && 
                   !cancellationToken.IsCancellationRequested && !_cancellationToken.IsCancellationRequested) 
            {
                t += Time.unscaledDeltaTime * speed;
                await UniTask.Yield();
            }
            
            if (_cancellationToken.IsCancellationRequested) return;

            _handleIdToSourceMap.Remove(id);
            _attachKeyToHandleIdMap.Remove(attachKey);
            
            await FadeOut(source, fadeOut < 0f ? _fadeOut : fadeOut);
            
            if (_cancellationToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }

        private async UniTask FadeIn(int id, AudioSource source, float duration, float volume, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;

            while (t < 1f && _handleIdToSourceMap.ContainsKey(id) && 
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

        private int GetNextHandleId() {
            if (++_lastHandleId == 0) _lastHandleId++;
            return _lastHandleId;
        }
    }
    
}