using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-999)]
    public sealed class AudioPool : MonoBehaviour, IAudioPool, IUpdate {

        [Header("Audio Element")]
        [SerializeField] private AudioElement _prefab;
        [SerializeField] private AudioMixerGroup _defaultMixerGroup;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        
        [Header("Shuffle")]
        [SerializeField] [Min(0f)] private float _lastIndexStoreLifetime = 60f;

        [Header("Occlusion Detection")]
        [SerializeField] private bool _applyOcclusion = true;
        [SerializeField] [Min(0f)] private float _minDistance = 0.1f;
        [SerializeField] [Min(0f)] private float _maxDistance = 100f;
        [SerializeField] [Min(1)] private int _rays = 3;
        [SerializeField] [Min(0f)] private float _rayOffset0 = 1f;
        [SerializeField] [Min(0f)] private float _rayOffset1 = 5f;
        [SerializeField] [Min(1)] private int _maxHits = 1;
        [SerializeField] private LayerMask _layerMask;

        [Header("Occlusion Profile")]
        [SerializeField] [Min(0f)] private float _occlusionSmoothing = 0f;
        
        [Space]
        [SerializeField] [Range(0f, 1f)] private float _distanceWeightLow = 1f;
        [SerializeField] [Range(0f, 1f)] private float _collisionWeightLow = 0.3f;
        [SerializeField] [Range(1f, 10f)] private float _qLow = 1f;
        [SerializeField] [Range(10f, 22000f)] private float _cutoffLow = 2000f;
        [SerializeField] private AnimationCurve _distanceCurveLow = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _cutoffLowCurve = EasingType.Linear.ToAnimationCurve();
        
        [Space]
        [SerializeField] [Range(0f, 1f)] private float _distanceWeightHigh = 1f;
        [SerializeField] [Range(0f, 1f)] private float _collisionWeightHigh = 0.3f;
        [SerializeField] [Range(1f, 10f)] private float _qHigh = 1f;
        [SerializeField] [Range(10f, 22000f)] private float _cutoffHigh = 500f;
        [SerializeField] private AnimationCurve _distanceCurveHigh = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _cutoffHighCurve = EasingType.Linear.ToAnimationCurve();

        public static IAudioPool Main { get; private set; }

        private const float DistanceThreshold = 0.001f;
        private const float HpCutoffLowerBound = 10f;
        private const float LpCutoffUpperBound = 22000f;
        
        private static readonly float3 Up = Vector3.up;
        private static readonly float3 Forward = Vector3.forward;
        private static readonly float3 Right = Vector3.right;
        
        private readonly Dictionary<int, IndexData> _clipsHashToLastIndexMap = new();
        private readonly List<int> _clipsHashBuffer = new();

        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, IAudioElement> _handleIdToAudioElementMap = new();
        private int _lastHandleId;
        
        private readonly Dictionary<AudioListener, ListenerData> _audioListenersMap = new();
        private Transform _listenerTransform;
        private Transform _listenerUp;

        private readonly HashSet<IAudioVolume> _volumes = new();
        
        private NativeHashMap<int, float> _listenerClusterToWeightMap;
        private NativeArray<VolumeValueData> _volumePitchArray;
        private NativeArray<VolumeValueData> _volumeOcclusionArray;
        private NativeArray<VolumeValueData> _volumeLpArray;
        private NativeArray<VolumeValueData> _volumeHpArray;
        private NativeArray<VolumeValueData> _resultVolumePitchArray;
        private NativeArray<VolumeValueData> _resultVolumeOcclusionArray;
        private NativeArray<VolumeValueData> _resultVolumeLpArray;
        private NativeArray<VolumeValueData> _resultVolumeHpArray;
        
        private NativeArray<OcclusionSearchData> _occlusionSearchArray;
        private NativeArray<OcclusionData> _occlusionArray;
        private float _globalOcclusionWeight = 1f;
        
        private Transform _transform;
        private CancellationToken _destroyToken;
        private float _lastTimeScale;
        
        private void Awake() {
            Main = this;
            
            _transform = transform;
            _destroyToken = destroyCancellationToken;
            
            StartLastClipIndexUpdates(_destroyToken).Forget();
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDestroy() {
            _clipsHashToLastIndexMap.Clear();
            _clipsHashBuffer.Clear();
            
            _attachKeyToHandleIdMap.Clear();
            _handleIdToAudioElementMap.Clear();
            
            CleanupVolumeData();
            CleanupOcclusionData();

            Main = null;
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        public void RegisterListener(AudioListener listener, Transform up, int priority) {
            _audioListenersMap[listener] = new ListenerData(priority, up);
            UpdateListeners();
        }

        public void UnregisterListener(AudioListener listener) {
            _audioListenersMap.Remove(listener);
            UpdateListeners();
        }

        public void RegisterVolume(IAudioVolume volume) {
            _volumes.Add(volume);
        }

        public void UnregisterVolume(IAudioVolume volume) {
            _volumes.Remove(volume);
        }

        public void SetGlobalOcclusionWeightNextFrame(float weight) {
            _globalOcclusionWeight = weight;
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

            InitializeAudioElement(audioElement, id, pitch, options);
            
            _handleIdToAudioElementMap[id] = audioElement;

            RestartAudioSource(
                id, audioElement.Source, clip, mixerGroup, 
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f), 
                spatialBlend, normalizedTime, loop, 
                cancellationToken
            ).Forget();
            
            WaitAndRelease(
                id, AttachKey.Invalid, audioElement.Source, 
                loop, fadeOut, 
                cancellationToken
            ).Forget();
            
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
            
            InitializeAudioElement(audioElement, id, pitch, options);
            
            _handleIdToAudioElementMap[id] = audioElement;
            
            RestartAudioSource(
                id, audioElement.Source, clip, mixerGroup, 
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f), 
                spatialBlend, normalizedTime, loop, 
                cancellationToken
            ).Forget();
            
            WaitAndRelease(
                id, attachKey, audioElement.Source, 
                loop, fadeOut, 
                cancellationToken
            ).Forget();
            
            return new AudioHandle(this, id);
        }

        public AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips) {
            int count = clips?.Count ?? 0;
            
            switch (count) {
                case 0:
                    return default;
                
                case 1:
                    return clips![0];
            }
            
            int hash = 0;
            for (int i = 0; i < count; i++) {
                unchecked {
                    hash += clips![i].GetHashCode();   
                }
            }

            return clips![NextClipIndex(hash, count)];
        }
        
        public AudioHandle GetAudioHandle(Transform attachedTo, int hash) {
            return _attachKeyToHandleIdMap.TryGetValue(new AttachKey(attachedTo.GetInstanceID(), hash), out int id) && 
                   _handleIdToAudioElementMap.ContainsKey(id)
                ? new AudioHandle(this, id)
                : AudioHandle.Invalid;
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
            CancellationToken cancellationToken) 
        {
            source.Stop();

            source.clip = clip;
            source.time = normalizedTime * clip.length;
            source.volume = fadeIn > 0f ? 0f : volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.outputAudioMixerGroup = mixerGroup == null ? _defaultMixerGroup : mixerGroup;
            
            source.Play();
            
            return fadeIn > 0f ? FadeIn(id, source, fadeIn, volume, cancellationToken) : default;
        }

        private async UniTask WaitAndRelease(
            int id,
            AttachKey attachKey,
            AudioSource source,
            bool loop,
            float fadeOut,
            CancellationToken cancellationToken) 
        {
            float maxTime = source.time;
            float clipLength = source.clip.length;
            
            while (!cancellationToken.IsCancellationRequested && 
                   !_destroyToken.IsCancellationRequested && 
                   _handleIdToAudioElementMap.ContainsKey(id) && 
                   (loop || source.time is var time && time < clipLength && time >= maxTime)) 
            {
                maxTime = source.time;
                await UniTask.Yield();
            }
            
            if (_destroyToken.IsCancellationRequested) return;
            
            _handleIdToAudioElementMap.Remove(id);
            _attachKeyToHandleIdMap.Remove(attachKey);
            
            if (source == null) return;
            
            await FadeOut(source, fadeOut < 0f ? _fadeOut : fadeOut);
            
            if (_destroyToken.IsCancellationRequested) return;
            
            PrefabPool.Main.Release(source);
        }

        private async UniTask FadeIn(int id, AudioSource source, float duration, float volume, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;

            while (t < 1f && _handleIdToAudioElementMap.ContainsKey(id) && 
                   !cancellationToken.IsCancellationRequested && !_destroyToken.IsCancellationRequested) 
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
            
            while (t < 1f && !_destroyToken.IsCancellationRequested) {
                t += Time.unscaledDeltaTime * speed;
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                
                await UniTask.Yield();
            }
        }
        
        void IAudioPool.ReleaseAudioHandle(int handleId) {
            _handleIdToAudioElementMap.Remove(handleId);
        }

        void IAudioPool.SetAudioHandlePitch(int handleId, float pitch) {
            if (!_handleIdToAudioElementMap.TryGetValue(handleId, out var audioElement)) return;
            
            bool applyTimescale = (audioElement.AudioOptions & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;
            
            audioElement.Pitch = pitch;
            audioElement.Source.pitch = pitch * (applyTimescale ? Time.timeScale : 1f);
        }

        bool IAudioPool.TryGetAudioElement(int handleId, out IAudioElement audioElement) {
            return _handleIdToAudioElementMap.TryGetValue(handleId, out audioElement);
        }
        
        private int GetNextHandleId() {
            if (++_lastHandleId == 0) _lastHandleId++;
            return _lastHandleId;
        }

        private void InitializeAudioElement(IAudioElement audioElement, int id, float pitch, AudioOptions options) {
            audioElement.Id = id;
            audioElement.AudioOptions = options;
            audioElement.Pitch = pitch;
            audioElement.OcclusionFlag = 0;
            
            audioElement.LowPass.lowpassResonanceQ = _qLow;
            audioElement.HighPass.highpassResonanceQ = _qHigh;
            
            audioElement.LowPass.cutoffFrequency = LpCutoffUpperBound;
            audioElement.HighPass.cutoffFrequency = HpCutoffLowerBound;
        }
        
        private IAudioElement GetAudioElementAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity, _transform);
        }

        private IAudioElement GetAudioElementAttached(Transform parent, Vector3 localPosition = default) {
            return PrefabPool.Main.Get(_prefab, parent.TransformPoint(localPosition), Quaternion.identity, parent);
        }

        private void UpdateListeners() {
            if (TryGetCurrentListener(out var currentListener, out var transformUp)) {
                _listenerTransform = currentListener.transform;
                _listenerUp = transformUp;
                
                foreach (var l in _audioListenersMap.Keys) {
                    l.enabled = l == currentListener;
                }
                
                return;
            }
            
            _listenerTransform = null;
            _listenerUp = null;
        }
        
        private bool TryGetCurrentListener(out AudioListener listener, out Transform transformUp) {
            listener = null;
            transformUp = null;
            int priority = 0;
            
            foreach (var (audioListener, data) in _audioListenersMap) {
                if (data.priority < priority && listener != null) continue;
                
                priority = data.priority;
                listener = audioListener;
                transformUp = data.transformUp;
            }
            
            return listener != null;
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
        
        void IUpdate.OnUpdate(float dt) {
            bool hasListener = _audioListenersMap.Count > 0;
            var listenerPos = hasListener ? _listenerTransform.position : default;
            var listenerUp = hasListener ? _listenerUp.up : Vector3.up;
            float listenerOcclusionWeight = 1f;
            
            float timeScale = Time.timeScale;
            int occlusionCount = 0;
            
            int count = _handleIdToAudioElementMap.Count;
            
            float minSqr = _minDistance * _minDistance;
            float maxSqr = _maxDistance * _maxDistance;
            
            PrepareVolumeData(out int volumeCount);
            PrepareOcclusionData(count);
            
            if (hasListener) ProcessVolumesForListener(volumeCount, listenerPos, ref listenerOcclusionWeight);
            
            foreach (var audioElement in _handleIdToAudioElementMap.Values) {
                var position = audioElement.Transform.position;
                var options = audioElement.AudioOptions;

                float pitch = audioElement.Pitch;
                float occlusionWeight = 1f;
                float lpCutoffBound = LpCutoffUpperBound;
                float hpCutoffBound = HpCutoffLowerBound;
                
                if (hasListener && (options & AudioOptions.AffectedByVolumes) == AudioOptions.AffectedByVolumes) {
                    ProcessVolumesForSound(volumeCount, position, ref pitch, ref occlusionWeight, ref lpCutoffBound, ref hpCutoffBound);
                }
                
                if ((options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale) {
                    pitch *= timeScale;
                }

                occlusionWeight *= listenerOcclusionWeight;
                audioElement.Source.pitch = pitch;

                if (!hasListener || !_applyOcclusion || 
                    (options & AudioOptions.ApplyOcclusion) != AudioOptions.ApplyOcclusion) 
                {
                    ApplyOcclusion(audioElement, distanceWeight: 0f, collisionWeight: 0f, lpCutoffBound, hpCutoffBound, dt);
                    continue;
                }
                
                float sqr = (listenerPos - position).sqrMagnitude;

                if (sqr <= minSqr) {
                    ApplyOcclusion(audioElement, distanceWeight: 0f, collisionWeight: 0f, lpCutoffBound, hpCutoffBound, dt);
                    continue;
                }
                
                float spatial = audioElement.Source.spatialBlend;
                
                if (sqr > minSqr && sqr < maxSqr) {
                    float distance = (listenerPos - position).magnitude;
                    
                    _occlusionSearchArray[occlusionCount] = new OcclusionSearchData(
                        position,
                        direction: (listenerPos - position).normalized,
                        distance
                    );
                    
                    _occlusionArray[occlusionCount++] = new OcclusionData(
                        audioElement.Id,
                        distance,
                        weight: spatial * occlusionWeight,
                        lpCutoffBound,
                        hpCutoffBound
                    );
                    continue;
                }
                
                ApplyOcclusion(audioElement, distanceWeight: spatial * occlusionWeight, collisionWeight: occlusionWeight, lpCutoffBound, hpCutoffBound, dt);
            }
            
            if (hasListener) ProcessOcclusion(listenerUp, occlusionCount, dt);

            _globalOcclusionWeight = 1f;
            
            CleanupVolumeData();
            CleanupOcclusionData();
        }

        private void PrepareVolumeData(out int count) {
            count = _volumes.Count;
            
            _listenerClusterToWeightMap = new NativeHashMap<int, float>(count, Allocator.TempJob);
            
            _volumePitchArray = new NativeArray<VolumeValueData>(count, Allocator.TempJob);
            _volumeOcclusionArray = new NativeArray<VolumeValueData>(count, Allocator.TempJob);
            _volumeLpArray = new NativeArray<VolumeValueData>(count, Allocator.TempJob);
            _volumeHpArray = new NativeArray<VolumeValueData>(count, Allocator.TempJob);
            
            _resultVolumePitchArray = new NativeArray<VolumeValueData>(2, Allocator.TempJob);
            _resultVolumeOcclusionArray = new NativeArray<VolumeValueData>(2, Allocator.TempJob);
            _resultVolumeLpArray = new NativeArray<VolumeValueData>(2, Allocator.TempJob);
            _resultVolumeHpArray = new NativeArray<VolumeValueData>(2, Allocator.TempJob);
        }

        private void CleanupVolumeData() {
            if (_listenerClusterToWeightMap.IsCreated) _listenerClusterToWeightMap.Dispose();
            
            if (_volumePitchArray.IsCreated) _volumePitchArray.Dispose();
            if (_volumeOcclusionArray.IsCreated) _volumeOcclusionArray.Dispose();
            if (_volumeLpArray.IsCreated) _volumeLpArray.Dispose();
            if (_volumeHpArray.IsCreated) _volumeHpArray.Dispose();
            
            if (_resultVolumePitchArray.IsCreated) _resultVolumePitchArray.Dispose();
            if (_resultVolumeOcclusionArray.IsCreated) _resultVolumeOcclusionArray.Dispose();
            if (_resultVolumeLpArray.IsCreated) _resultVolumeLpArray.Dispose();
            if (_resultVolumeHpArray.IsCreated) _resultVolumeHpArray.Dispose();
        }

        private void ProcessVolumesForListener(int count, Vector3 position, ref float occlusionWeight) {
            if (count == 0) return;
            
            int realCount = 0;
            int topPriority = int.MinValue;

            var volumeListenerOcclusionArray = new NativeArray<VolumeListenerData>(count, Allocator.TempJob);
            var resultVolumeListenerOcclusionArray = new NativeArray<VolumeListenerData>(2, Allocator.TempJob);
            
            foreach (var volume in _volumes) {
                int priority = volume.Priority;
                if (priority < topPriority || volume.GetWeight(position, out int cluster) is var weight && weight <= 0f) continue;
                
                topPriority = Mathf.Max(topPriority, priority);
                float occlusionWeightLocal = occlusionWeight;

                float w = volume.ModifyOcclusionWeightForListener(ref occlusionWeightLocal) ? 1f : 0f;
                
                volumeListenerOcclusionArray[realCount++] = new VolumeListenerData {
                    priority = priority,
                    cluster = cluster,
                    weight = weight,
                    valueAndWeight = new float2(Mathf.Lerp(occlusionWeight, occlusionWeightLocal, weight * w), weight * w),
                };  
            }

            resultVolumeListenerOcclusionArray[0] = default;
            
            var job = new CalculateVolumeForListenerJob {
                count = realCount,
                topPriority = topPriority,
                volumeDataArray = volumeListenerOcclusionArray,
                result = resultVolumeListenerOcclusionArray,
                listenerClusterToWeightMap = _listenerClusterToWeightMap,
            };
            
            job.Schedule().Complete();
            
            var result = resultVolumeListenerOcclusionArray[0];

            volumeListenerOcclusionArray.Dispose();
            resultVolumeListenerOcclusionArray.Dispose();
            
            occlusionWeight = result.valueAndWeight.y > 0f ? result.valueAndWeight.x : occlusionWeight;
        }

        private void ProcessVolumesForSound(
            int count,
            Vector3 position,
            ref float pitch,
            ref float occlusionWeight,
            ref float lpCutoffBound,
            ref float hpCutoffBound) 
        {
            if (count == 0) return;
            
            int realCountPitch = 0;
            int realCountOcclusion = 0;
            int realCountLp = 0;
            int realCountHp = 0;
            
            int topPriorityPitch = int.MinValue;
            int topPriorityOcclusion = int.MinValue;
            int topPriorityLp = int.MinValue;
            int topPriorityHp = int.MinValue;

            foreach (var volume in _volumes) {
                float weight = volume.GetWeight(position, out int cluster);
                if (weight <= 0f) continue;
                
                int priority = volume.Priority;
                float listenerPresence = volume.ListenerPresence;
                
                float pitchLocal = pitch;
                float occlusionWeightLocal = occlusionWeight;
                float lpCutoffBoundLocal = lpCutoffBound;
                float hpCutoffBoundLocal = hpCutoffBound;

                if (volume.ModifyPitch(ref pitchLocal)) {
                    topPriorityPitch = Mathf.Max(topPriorityPitch, priority);

                    _volumePitchArray[realCountPitch++] = new VolumeValueData {
                        priority = priority,
                        cluster = cluster,
                        weight = weight,
                        listenerPresence = listenerPresence,
                        value = Mathf.Lerp(pitch, pitchLocal, weight)
                    };
                }

                if (volume.ModifyOcclusionWeightForSound(ref occlusionWeightLocal)) {
                    topPriorityOcclusion = Mathf.Max(topPriorityOcclusion, priority);
                    
                    _volumeOcclusionArray[realCountOcclusion++] = new VolumeValueData {
                        priority = priority,
                        cluster = cluster,
                        weight = weight,
                        listenerPresence = listenerPresence,
                        value = Mathf.Lerp(occlusionWeight, occlusionWeightLocal, weight)
                    };
                }

                if (volume.ModifyLowPassFilter(ref lpCutoffBoundLocal)) {
                    topPriorityLp = Mathf.Max(topPriorityLp, priority);
                    
                    _volumeLpArray[realCountLp++] = new VolumeValueData {
                        priority = priority,
                        cluster = cluster,
                        weight = weight,
                        listenerPresence = listenerPresence,
                        value = Mathf.Lerp(lpCutoffBound, lpCutoffBoundLocal, weight)
                    };
                }

                if (volume.ModifyHighPassFilter(ref hpCutoffBoundLocal)) {
                    topPriorityHp = Mathf.Max(topPriorityHp, priority);
                    
                    _volumeHpArray[realCountHp++] = new VolumeValueData {
                        priority = priority,
                        cluster = cluster,
                        weight = weight,
                        listenerPresence = listenerPresence,
                        value = Mathf.Lerp(hpCutoffBound, hpCutoffBoundLocal, weight)
                    };
                }
            }

            _resultVolumePitchArray[0] = default;
            _resultVolumeOcclusionArray[0] = default;
            _resultVolumeLpArray[0] = default;
            _resultVolumeHpArray[0] = default;

            var calculatePitchJob = new CalculateVolumeValueJob {
                count = realCountPitch,
                topPriority = topPriorityPitch,
                volumeDataArray = _volumePitchArray,
                listenerClusterToWeightMap = _listenerClusterToWeightMap,
                result = _resultVolumePitchArray,
            };
            
            var calculateOcclusionWeightJob = new CalculateVolumeValueJob {
                count = realCountOcclusion,
                topPriority = topPriorityOcclusion,
                volumeDataArray = _volumeOcclusionArray,
                listenerClusterToWeightMap = _listenerClusterToWeightMap,
                result = _resultVolumeOcclusionArray,
            };
            
            var calculateLpJob = new CalculateVolumeValueJob {
                count = realCountLp,
                topPriority = topPriorityLp,
                volumeDataArray = _volumeLpArray,
                listenerClusterToWeightMap = _listenerClusterToWeightMap,
                result = _resultVolumeLpArray,
            };
            
            var calculateHpJob = new CalculateVolumeValueJob {
                count = realCountHp,
                topPriority = topPriorityHp,
                volumeDataArray = _volumeHpArray,
                listenerClusterToWeightMap = _listenerClusterToWeightMap,
                result = _resultVolumeHpArray,
            };
            
            calculatePitchJob.Schedule().Complete();
            calculateOcclusionWeightJob.Schedule().Complete();
            calculateLpJob.Schedule().Complete();
            calculateHpJob.Schedule().Complete();
            
            var resultPitch = _resultVolumePitchArray[0];
            var resultOcclusion = _resultVolumeOcclusionArray[0];
            var resultLp = _resultVolumeLpArray[0];
            var resultHp = _resultVolumeHpArray[0];
            
            pitch = resultPitch.weight > 0f ? resultPitch.value : pitch;
            occlusionWeight = resultOcclusion.weight > 0f ? resultOcclusion.value : occlusionWeight;
            lpCutoffBound = resultLp.weight > 0f ? resultLp.value : lpCutoffBound;
            hpCutoffBound = resultHp.weight > 0f ? resultHp.value : hpCutoffBound;
        }

        private void PrepareOcclusionData(int count) {
            _occlusionSearchArray = new NativeArray<OcclusionSearchData>(count, Allocator.TempJob);
            _occlusionArray = new NativeArray<OcclusionData>(count, Allocator.TempJob);
        }

        private void CleanupOcclusionData() {
            if (_occlusionSearchArray.IsCreated) _occlusionSearchArray.Dispose();
            if (_occlusionArray.IsCreated) _occlusionArray.Dispose();
        }
        
        private void ProcessOcclusion(Vector3 up, int count, float dt) {
            var raycastCommands = new NativeArray<RaycastCommand>(count * _rays, Allocator.TempJob);
            var hits = new NativeArray<RaycastHit>(count * _rays * _maxHits, Allocator.TempJob);
            var occlusionWeightArray = new NativeArray<OcclusionWeightData>(count, Allocator.TempJob);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DrawOcclusionRays(count, up);
#endif

            var prepareRaycastCommandsJob = new PrepareRaycastCommandsJob {
                occlusionSearchArray = _occlusionSearchArray,
                up = up,
                raySector = 360f / _rays,
                rayOffset0 = _rayOffset0,
                rayOffset1 = _rayOffset1,
                minDistance = _minDistance,
                maxDistance = _maxDistance,
                layerMask = _layerMask,
                raycastCommands = raycastCommands,
            };

            var calculateOcclusionWeightsJob = new CalculateOcclusionWeightsJob {
                hitsArray = hits,
                occlusionArray = _occlusionArray,
                maxHits = _maxHits,
                maxDistance = _maxDistance,
                minDistance = _minDistance,
                rays = _rays,
                occlusionWeightArray = occlusionWeightArray
            };
            
            int commandsPerJob = Mathf.Max(count / JobsUtility.JobWorkerCount, 1);

            var prepareJobHandle = prepareRaycastCommandsJob.ScheduleBatch(count * _rays, _rays);
            var raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, commandsPerJob, _maxHits, prepareJobHandle);
            var calculateWeightsJobHandle = calculateOcclusionWeightsJob.Schedule(count, innerloopBatchCount: 256, raycastJobHandle);
                
            calculateWeightsJobHandle.Complete();
            
            for (int i = 0; i < count; i++) {
                var data = occlusionWeightArray[i];
                
                ApplyOcclusion(_handleIdToAudioElementMap[data.id], data.distanceWeight, data.collisionWeight, data.lpCutoffBound, data.hpCutoffBound, dt);
            }

            hits.Dispose();
            raycastCommands.Dispose();
            occlusionWeightArray.Dispose();
        }

        private void ApplyOcclusion(
            IAudioElement audioElement,
            float distanceWeight,
            float collisionWeight,
            float lpCutoffBound,
            float hpCutoffBound,
            float dt) 
        {
            float wLow = Mathf.Clamp01((_distanceCurveLow.Evaluate(distanceWeight) * _distanceWeightLow + collisionWeight * _collisionWeightLow) * _globalOcclusionWeight);
            float wHigh = Mathf.Clamp01((_distanceCurveHigh.Evaluate(distanceWeight) * _distanceWeightHigh + collisionWeight * _collisionWeightHigh) * _globalOcclusionWeight);
            
            float cutoffLow = Mathf.Min(lpCutoffBound, Mathf.Lerp(LpCutoffUpperBound, _cutoffLow, _cutoffLowCurve.Evaluate(wLow)));
            float cutoffHigh = Mathf.Max(hpCutoffBound, Mathf.Lerp(HpCutoffLowerBound, _cutoffHigh, _cutoffHighCurve.Evaluate(wHigh)));

            var lp = audioElement.LowPass;
            var hp = audioElement.HighPass;

            // Apply occlusion instantly if flag is 0 after audio element was just initialized
            float smoothing = audioElement.OcclusionFlag * _occlusionSmoothing;
            
            lp.cutoffFrequency = lp.cutoffFrequency.SmoothExpNonZero(cutoffLow, smoothing, dt);
            hp.cutoffFrequency = hp.cutoffFrequency.SmoothExpNonZero(cutoffHigh, smoothing, dt);
            
            audioElement.OcclusionFlag = 1;
        }
        
        private static float3 GetOcclusionOffset(int i, int count, float sector) {
            return count switch {
                2 => (2 * i - 1) * Right,
                3 => (i - 1) * Right,
                _ => i == 0 ? default : math.mul(quaternion.AxisAngle(Forward, i * sector), Up),
            };
        }

        private static float GetRelativeDistance(float distance, float min, float max) {
            return math.clamp((distance - min) / (max - min + DistanceThreshold), 0f, 1f);
        }
        
        private int NextClipIndex(int hash, int count) {
            var data = _clipsHashToLastIndexMap.GetValueOrDefault(hash);
            
            int mask = data.indicesMask;
            int startIndex = data.startIndex;
            int index = GetRandomIndex(ref mask, ref startIndex, data.lastIndex, count);
            
            _clipsHashToLastIndexMap[hash] = new IndexData(mask, startIndex, index, Time.time);
            
            return index;
        }

        private static int GetRandomIndex(ref int indicesMask, ref int startIndex, int lastIndex, int count) {
            switch (count) {
                case 2:
                    return 1 - lastIndex;
                
                case 3:
                    return (int) Mathf.Repeat(lastIndex + Mathf.Sign(Random.value - 0.5f), 3);
            }
            
            const int bits = 32;
            
            int max = Mathf.Min(bits, count - startIndex);
            int freeCount = max;
            int r;
            
            for (int i = 0; i < max; i++) {
                if ((indicesMask & (1 << i)) != 0) freeCount--;
            }

            if (freeCount <= 0) {
                startIndex += bits;
                if (startIndex > count - 1) startIndex = 0;

                if (count > bits) {
                    r = Random.Range(0, Mathf.Min(bits, count - startIndex));
                    indicesMask = 1 << r;
                    return r + startIndex;
                }
                
                indicesMask = 0;
                max = Mathf.Min(bits, count - startIndex);
                freeCount = max - 1;
            }
            
            r = Random.Range(0, freeCount);
            
            if (freeCount >= max) {
                indicesMask |= 1 << r;
                return r + startIndex;
            }
            
            freeCount = 0;
            for (int i = 0; i < max; i++) {
                if ((indicesMask & (1 << i)) != 0 || i + startIndex == lastIndex || freeCount++ != r) {
                    continue;
                }
                
                indicesMask |= 1 << i;
                return i + startIndex;
            }

            return Random.Range(0, count);
        }

        private readonly struct ListenerData {
            
            public readonly int priority;
            public readonly Transform transformUp;
            
            public ListenerData(int priority, Transform transformUp) {
                this.priority = priority;
                this.transformUp = transformUp;
            }
        }
        
        private readonly struct IndexData {
            
            public readonly int indicesMask;
            public readonly int startIndex;
            public readonly int lastIndex;
            public readonly float time;
            
            public IndexData(int indicesMask, int startIndex, int lastIndex, float time) {
                this.indicesMask = indicesMask;
                this.startIndex = startIndex;
                this.lastIndex = lastIndex;
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
        
        private readonly struct OcclusionSearchData {

            public readonly float3 position;
            public readonly float3 direction;
            public readonly float distance;
            
            public OcclusionSearchData(float3 position, float3 direction, float distance) {
                this.position = position;
                this.direction = direction;
                this.distance = distance;
            }
        }
        
        private readonly struct OcclusionData {

            public readonly int id;
            public readonly float distance;
            public readonly float weight;
            public readonly float lpCutoffBound;
            public readonly float hpCutoffBound;
            
            public OcclusionData(int id, float distance, float weight, float lpCutoffBound, float hpCutoffBound) {
                this.id = id;
                this.distance = distance;
                this.weight = weight;
                this.lpCutoffBound = lpCutoffBound;
                this.hpCutoffBound = hpCutoffBound;
            }
        }
        
        private readonly struct OcclusionWeightData {

            public readonly int id;
            public readonly float distanceWeight;
            public readonly float collisionWeight;
            public readonly float lpCutoffBound;
            public readonly float hpCutoffBound;
            
            public OcclusionWeightData(int id, float distanceWeight, float collisionWeight, float lpCutoffBound, float hpCutoffBound) {
                this.id = id;
                this.distanceWeight = distanceWeight;
                this.collisionWeight = collisionWeight;
                this.lpCutoffBound = lpCutoffBound;
                this.hpCutoffBound = hpCutoffBound;
            }
        }

        private struct VolumeListenerData {
            public int priority;
            public int cluster;
            public float weight;
            public float2 valueAndWeight;
        }
        
        private struct VolumeValueData {
            public int priority;
            public int cluster;
            public float weight;
            public float listenerPresence;
            public float value;
        }
        
        [BurstCompile]
        private struct CalculateVolumeForListenerJob : IJob {
            
            [ReadOnly] public int count;
            [ReadOnly] public int topPriority;
            [ReadOnly] public NativeArray<VolumeListenerData> volumeDataArray;

            public NativeHashMap<int, float> listenerClusterToWeightMap;
            public NativeArray<VolumeListenerData> result;
            
            public void Execute() {
                var resultData = result[0];

                for (int i = 0; i < count; i++) {
                    var data = volumeDataArray[i];
                    if (data.priority < topPriority) continue;
                    
                    resultData.valueAndWeight += new float2(data.valueAndWeight.x * data.valueAndWeight.y, data.valueAndWeight.y);
                    
                    listenerClusterToWeightMap[data.cluster] = 
                        math.max(data.weight, listenerClusterToWeightMap.TryGetValue(data.cluster, out float w) ? w : 0f);
                }
                
                if (resultData.valueAndWeight.y <= 0f) return;
                
                resultData.valueAndWeight = new float2(resultData.valueAndWeight.x / resultData.valueAndWeight.y, resultData.valueAndWeight.y);
                result[0] = resultData;
            }
        }
        
        [BurstCompile]
        private struct CalculateVolumeValueJob : IJob {
            
            [ReadOnly] public int count;
            [ReadOnly] public int topPriority;
            [ReadOnly] public NativeArray<VolumeValueData> volumeDataArray;
            [ReadOnly] public NativeHashMap<int, float> listenerClusterToWeightMap;
            
            public NativeArray<VolumeValueData> result;
            
            public void Execute() {
                var resultData = result[0];
                
                for (int i = 0; i < count; i++) {
                    var data = volumeDataArray[i];
                    if (data.priority < topPriority) continue;

                    float listenerWeight = listenerClusterToWeightMap.TryGetValue(data.cluster, out float w) ? w : 0f;
                    data.weight *= math.lerp(1f, listenerWeight, data.listenerPresence);
                    
                    resultData.weight += data.weight;
                    resultData.value += data.value * data.weight;
                }

                if (resultData.weight > 0f) resultData.value /= resultData.weight;

                result[0] = resultData;
            }
        }
        
        [BurstCompile]
        private struct PrepareRaycastCommandsJob : IJobParallelForBatch {
            
            [ReadOnly] public NativeArray<OcclusionSearchData> occlusionSearchArray;
            [ReadOnly] public float3 up;
            [ReadOnly] public float raySector;
            [ReadOnly] public float rayOffset0;
            [ReadOnly] public float rayOffset1;
            [ReadOnly] public float minDistance;
            [ReadOnly] public float maxDistance;
            [ReadOnly] public int layerMask;
            
            public NativeArray<RaycastCommand> raycastCommands;

            public void Execute(int startIndex, int count) {
                var data = occlusionSearchArray[startIndex / count];
                
                var rot = quaternion.LookRotation(data.direction, up);
                float offset = math.lerp(rayOffset0, rayOffset1, GetRelativeDistance(data.distance, minDistance, maxDistance));
                
                for (int j = startIndex; j < startIndex + count; j++) {
                    raycastCommands[j] = new RaycastCommand(
                        from: data.position + math.mul(rot, offset * GetOcclusionOffset(j, count, raySector)),
                        data.direction,
                        new QueryParameters(layerMask, hitMultipleFaces: false, hitTriggers: QueryTriggerInteraction.Ignore, hitBackfaces: false),
                        data.distance
                    );
                }
            }
        }
        
        [BurstCompile]
        private struct CalculateOcclusionWeightsJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<RaycastHit> hitsArray;
            [ReadOnly] public NativeArray<OcclusionData> occlusionArray;
            [ReadOnly] public int rays;
            [ReadOnly] public int maxHits;
            [ReadOnly] public float minDistance;
            [ReadOnly] public float maxDistance;
            
            public NativeArray<OcclusionWeightData> occlusionWeightArray;

            public void Execute(int startIndex) {
                var data = occlusionArray[startIndex];
                float weightSum = 0f;
                
                for (int j = 0; j < rays; j++) {
                    int collisions = 0;
                    
                    for (int r = 0; r < maxHits; r++) {
                        var hit = hitsArray[startIndex * rays + j * maxHits + r];
                        if (hit.colliderInstanceID == 0) break;
                        
                        collisions++;
                    }

                    weightSum += (float) collisions / maxHits;
                }

                float distanceWeight = data.weight * GetRelativeDistance(data.distance, minDistance, maxDistance);
                float collisionWeight = data.weight * math.clamp(weightSum / rays, 0f, 1f);
                
                occlusionWeightArray[startIndex] = new OcclusionWeightData(
                    data.id,
                    distanceWeight,
                    collisionWeight,
                    data.lpCutoffBound,
                    data.hpCutoffBound
                );
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [SerializeField] private bool _showOcclusionInfo;
        
        internal bool ShowDebugInfo => _showDebugInfo;
        
        private void OnValidate() {
            if (_maxDistance < _minDistance) _maxDistance = _minDistance;
        }

        private void DrawOcclusionRays(int count, Vector3 up) {
            for (int i = 0; i < count; i++) {
                var data = _occlusionSearchArray[i];
                var rot = quaternion.LookRotation(data.direction, up);

                DebugExt.DrawSphere(data.position, 0.01f, Color.magenta);

                float off = Mathf.Lerp(_rayOffset0, _rayOffset1, GetRelativeDistance(data.distance, _minDistance, _maxDistance));
                
                for (int j = 0; j < _rays; j++) {
                    var offset = math.mul(rot, GetOcclusionOffset(j, _rays, 360f / _rays) * off);
                    DebugExt.DrawRay(data.position, offset, Color.magenta);
                    DebugExt.DrawRay(data.position + offset, data.direction * data.distance, Color.magenta);
                }
            }
        }
#endif
    }
    
}