using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-999)]
    public sealed class AudioPool : MonoBehaviour, IAudioPool, IUpdate {

        [SerializeField] private AudioElement _prefab;
        [SerializeField] private AudioMixerGroup _defaultMixerGroup;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        [SerializeField] [Min(0f)] private float _lastIndexStoreLifetime = 60f;
        
        public static IAudioPool Main { get; private set; }

        private readonly Dictionary<int, IndexData> _clipsHashToLastIndexMap = new();
        private readonly List<int> _clipsHashBuffer = new();

        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, IAudioElement> _handleIdToAudioElementMap = new();
        private readonly List<IAudioElement> _audioElements = new();

        private Transform _currentListener;
        private CancellationToken _cancellationToken;
        private int _lastHandleId;

        private float _lastTimeScale;
        
        private void Awake() {
            Main = this;
            _cancellationToken = destroyCancellationToken;
            
            StartLastClipIndexUpdates(_cancellationToken).Forget();
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDestroy() {
            _clipsHashToLastIndexMap.Clear();
            _clipsHashBuffer.Clear();
            
            _attachKeyToHandleIdMap.Clear();
            _handleIdToAudioElementMap.Clear();
            _audioElements.Clear();
            
            Main = null;
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        public void RegisterListener(AudioListener listener) {
            _currentListener = listener.transform;
        }

        public void UnregisterListener(AudioListener listener) {
            if (listener.transform != _currentListener) return;
            
            _currentListener = null;
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
            AudioMixerGroup mixerGroup = null,
            AudioOptions options = default,
            CancellationToken cancellationToken = default) 
        {
            if (clip == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: trying to play clip that is null");          
#endif
                return AudioHandle.Invalid;
            }
            
            int id = GetNextHandleId();
            var audioElement = GetAudioElementAtWorldPosition(position);

            bool loop = (options & AudioOptions.Loop) == AudioOptions.Loop;
            bool affectedByTimeScale = (options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;
            normalizedTime = Mathf.Clamp01(normalizedTime);

            audioElement.Id = id;
            audioElement.AffectedByTimeScale = affectedByTimeScale;
            audioElement.Pitch = pitch;
            
            _handleIdToAudioElementMap[id] = audioElement;
            _audioElements.Add(audioElement);
            
            RestartAudioSource(id, audioElement.Source, clip, mixerGroup, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, affectedByTimeScale, cancellationToken).Forget();
            ReleaseDelayed(id, AttachKey.Invalid, audioElement.Source, (1f - normalizedTime) * clip.length, loop, fadeOut, cancellationToken).Forget();
            
            return new AudioHandle(this, id);
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
            AudioMixerGroup mixerGroup = null,
            AudioOptions options = default,
            CancellationToken cancellationToken = default) 
        {
            if (clip == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: trying to play clip that is null");          
#endif
                return AudioHandle.Invalid;
            }

            int id = GetNextHandleId();
            var audioElement = GetAudioElementAttached(attachTo, localPosition);
            var attachKey = new AttachKey(attachTo.GetInstanceID(), attachId);

            bool loop = (options & AudioOptions.Loop) == AudioOptions.Loop;
            bool affectedByTimeScale = (options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;
            normalizedTime = Mathf.Clamp01(normalizedTime);
            
            if (attachId != 0) {
                _handleIdToAudioElementMap.Remove(_attachKeyToHandleIdMap.GetValueOrDefault(attachKey));
                _attachKeyToHandleIdMap[attachKey] = id;   
            }
            
            audioElement.Id = id;
            audioElement.AffectedByTimeScale = affectedByTimeScale;
            audioElement.Pitch = pitch;
            
            _handleIdToAudioElementMap[id] = audioElement;
            _audioElements.Add(audioElement);
            
            RestartAudioSource(id, audioElement.Source, clip, mixerGroup, fadeIn, volume, pitch, spatialBlend, normalizedTime, loop, affectedByTimeScale, cancellationToken).Forget();
            ReleaseDelayed(id, attachKey, audioElement.Source, (1f - normalizedTime) * clip.length, loop, fadeOut, cancellationToken).Forget();

            return new AudioHandle(this, id);
        }

        public AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips) {
            int count = clips?.Count ?? 0;
            if (count == 0) return default;

            int s = 0;

            for (int i = 0; i < count; i++) {
                s += clips![i].GetHashCode();
            }

            var lastData = _clipsHashToLastIndexMap.GetValueOrDefault(s, IndexData.Invalid);
            int index = ArrayExtensions.GetRandom(0, count, tryExclude: lastData.index);

            _clipsHashToLastIndexMap[s] = new IndexData(index, Time.time);
            
            return clips![index];
        }

        public AudioHandle GetAudioHandle(Transform attachedTo, int hash) {
            return _attachKeyToHandleIdMap.TryGetValue(new AttachKey(attachedTo.GetInstanceID(), hash), out int id) && 
                   _handleIdToAudioElementMap.ContainsKey(id)
                ? new AudioHandle(this, id)
                : AudioHandle.Invalid;
        }

        void IAudioPool.ReleaseAudioHandle(int handleId) {
            _handleIdToAudioElementMap.Remove(handleId);
        }

        void IAudioPool.SetAudioHandlePitch(int handleId, float pitch) {
            if (!_handleIdToAudioElementMap.TryGetValue(handleId, out var audioElement)) return;
            
            audioElement.Pitch = pitch;
            audioElement.Source.pitch = pitch * (audioElement.AffectedByTimeScale ? Time.timeScale : 1f);
        }

        bool IAudioPool.TryGetAudioElement(int handleId, out IAudioElement audioElement) {
            return _handleIdToAudioElementMap.TryGetValue(handleId, out audioElement);
        }

        void IUpdate.OnUpdate(float dt) {
            float timeScale = Time.timeScale;
            int count = _audioElements.Count;
            int validCount = count;
            
            
            
            for (int i = count - 1; i >= 0; i--) {
                var audioElement = _audioElements[i];

                if (audioElement == null || !_handleIdToAudioElementMap.ContainsKey(audioElement.Id)) {
                    _audioElements[i] = _audioElements[--validCount];
                    continue;
                }

                if (audioElement.AffectedByTimeScale) audioElement.Source.pitch = audioElement.Pitch * timeScale;
                
                
            }
            
            _audioElements.RemoveRange(validCount, count - validCount);
        }
        
        private IAudioElement GetAudioElementAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity);
        }

        private IAudioElement GetAudioElementAttached(Transform parent, Vector3 localPosition = default) {
            var audioSource = PrefabPool.Main.Get(_prefab, parent);
            audioSource.transform.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            return audioSource;
        }

        private UniTask RestartAudioSource(
            int id,
            AudioSource source,
            AudioClip clip,
            AudioMixerGroup mixerGroup,
            float fadeIn,
            float volume, 
            float pitch, 
            float spatialBlend,
            float normalizedTime, 
            bool loop, 
            bool affectedByTimeScale, 
            CancellationToken cancellationToken) 
        {
            source.Stop();

            source.clip = clip;
            source.time = normalizedTime * clip.length;
            source.volume = fadeIn > 0f ? 0f : volume;
            source.pitch = pitch * (affectedByTimeScale ? Time.timeScale : 1f);
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.outputAudioMixerGroup = mixerGroup == null ? _defaultMixerGroup : mixerGroup;
            
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
            while (!cancellationToken.IsCancellationRequested && 
                   !_cancellationToken.IsCancellationRequested && 
                   _handleIdToAudioElementMap.ContainsKey(id) && 
                   (loop || source.time < delay))
            {
                await UniTask.Yield();
            }
            
            if (_cancellationToken.IsCancellationRequested) return;

            _handleIdToAudioElementMap.Remove(id);
            _attachKeyToHandleIdMap.Remove(attachKey);
            
            if (source == null) return;
            
            await FadeOut(source, fadeOut < 0f ? _fadeOut : fadeOut);
            
            if (_cancellationToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }

        private async UniTask FadeIn(int id, AudioSource source, float duration, float volume, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;

            while (t < 1f && _handleIdToAudioElementMap.ContainsKey(id) && 
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

        private async UniTask StartLastClipIndexUpdates(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float time = Time.time;
            
                _clipsHashBuffer.Clear();
                _clipsHashBuffer.AddRange(_clipsHashToLastIndexMap.Keys);

                for (int i = 0; i < _clipsHashBuffer.Count; i++) {
                    int hash = _clipsHashBuffer[i];
                    if (time - _clipsHashToLastIndexMap[hash].time > _lastIndexStoreLifetime) {
                        _clipsHashToLastIndexMap.Remove(hash);
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_lastIndexStoreLifetime), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow(); 
            }
        }

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

            private readonly int _instanceId;
            private readonly int _hash;
            
            public AttachKey(int instanceId, int hash) {
                _instanceId = instanceId;
                _hash = hash;
            }
            
            public bool Equals(AttachKey other) => _instanceId == other._instanceId && _hash == other._hash;
            public override bool Equals(object obj) => obj is AttachKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_instanceId, _hash);

            public static bool operator ==(AttachKey left, AttachKey right) => left.Equals(right);
            public static bool operator !=(AttachKey left, AttachKey right) => !left.Equals(right);
        }
    }
    
}