﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
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
        
        private static readonly Vector3 Up = Vector3.up;
        private static readonly Vector3 Forward = Vector3.forward;
        private static readonly Vector3 Right = Vector3.right;
        
        private readonly Dictionary<int, IndexData> _clipsHashToLastIndexMap = new();
        private readonly List<int> _clipsHashBuffer = new();

        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, IAudioElement> _handleIdToAudioElementMap = new();
        private int _lastHandleId;
        
        private readonly List<OcclusionSearchData> _occlusionSearchList = new();
        private readonly List<OcclusionData> _occlusionList = new();
        private float _globalOcclusionWeight = 1f;
        
        private readonly Dictionary<AudioListener, ListenerData> _audioListenersMap = new();
        private Transform _listenerTransform;
        private Transform _listenerUp;

        private readonly HashSet<IAudioVolume> _volumes = new();
        private NativeArray<VolumeData> _volumeDataArray;
        private NativeArray<VolumeData> _resultVolumeDataBuffer;
        
        private Transform _transform;
        private CancellationToken _destroyToken;
        private float _lastTimeScale;
        
        private void Awake() {
            Main = this;
            
            _transform = transform;
            _destroyToken = destroyCancellationToken;

            _resultVolumeDataBuffer = new NativeArray<VolumeData>(1, Allocator.Persistent);
            
            StartLastClipIndexUpdates(_destroyToken).Forget();
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDestroy() {
            _clipsHashToLastIndexMap.Clear();
            _clipsHashBuffer.Clear();
            
            _attachKeyToHandleIdMap.Clear();
            _handleIdToAudioElementMap.Clear();
            
            _occlusionSearchList.Clear();
            _occlusionList.Clear();

            _volumeDataArray.Dispose();
            _resultVolumeDataBuffer.Dispose();
            
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
            
            _occlusionSearchList.Add(default);
            _occlusionList.Add(default);

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
            
            _occlusionSearchList.Add(default);
            _occlusionList.Add(default);
            
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
                hash += clips![i].GetHashCode();
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
            
            float timeScale = Time.timeScale;
            int occlusionCount = 0;
            
            int count = _handleIdToAudioElementMap.Count;
            
            _occlusionSearchList.RemoveRange(count, _occlusionSearchList.Count - count);
            _occlusionList.RemoveRange(count, _occlusionList.Count - count);

            float minSqr = _minDistance * _minDistance;
            float maxSqr = _maxDistance * _maxDistance;
            
            foreach (var audioElement in _handleIdToAudioElementMap.Values) {
                var position = audioElement.Transform.position;
                var options = audioElement.AudioOptions;

                float pitch = audioElement.Pitch;
                float occlusionWeight = 1f;
                float lpCutoffBound = LpCutoffUpperBound;
                float hpCutoffBound = HpCutoffLowerBound;

                if ((options & AudioOptions.AffectedByVolumes) == AudioOptions.AffectedByVolumes) {
                    ProcessVolumes(position, ref pitch, ref occlusionWeight, ref lpCutoffBound, ref hpCutoffBound);
                }
                
                if ((options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale) {
                    pitch *= timeScale;
                }

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
                    
                    _occlusionSearchList[occlusionCount] = new OcclusionSearchData(
                        position,
                        direction: (listenerPos - position).normalized,
                        distance
                    );
                    
                    _occlusionList[occlusionCount++] = new OcclusionData(
                        audioElement,
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
        }

        private void ProcessVolumes(
            Vector3 position,
            ref float pitch,
            ref float occlusionWeight,
            ref float lpCutoffBound,
            ref float hpCutoffBound) 
        {
            int count = _volumes.Count;
            if (count == 0) return;
            
            int realCount = 0;
            int topPriority = int.MinValue;

            if (_volumeDataArray.Length < count) {
                _volumeDataArray.Dispose();
                _volumeDataArray = new NativeArray<VolumeData>(Mathf.NextPowerOfTwo(count), Allocator.Persistent);
            }
            
            foreach (var volume in _volumes) {
                int priority = volume.Priority;
                if (priority < topPriority || volume.GetWeight(position) is var weight && weight <= 0f) continue;
                
                topPriority = Mathf.Max(topPriority, priority);
                
                float pitchLocal = pitch;
                float occlusionWeightLocal = occlusionWeight;
                float lpCutoffBoundLocal = lpCutoffBound;
                float hpCutoffBoundLocal = hpCutoffBound;
                
                volume.ModifyPitch(ref pitchLocal);
                volume.ModifyOcclusionWeight(ref occlusionWeightLocal);
                volume.ModifyLowHighPassFilters(ref lpCutoffBoundLocal, ref hpCutoffBoundLocal);
                
                _volumeDataArray[realCount++] = new VolumeData {
                    priority = priority,
                    weight = weight,
                    pitch = Mathf.Lerp(pitch, pitchLocal, weight),
                    occlusionWeight = Mathf.Lerp(occlusionWeight, occlusionWeightLocal, weight),
                    lpCutoffBound = Mathf.Lerp(lpCutoffBound, lpCutoffBoundLocal, weight),
                    hpCutoffBound = Mathf.Lerp(hpCutoffBound, hpCutoffBoundLocal, weight),
                };
            }

            _resultVolumeDataBuffer[0] = default;
            
            var job = new CalculateVolumeJob {
                count = realCount,
                topPriority = topPriority,
                volumeDataArray = _volumeDataArray,
                result = _resultVolumeDataBuffer,
            };
            
            job.Schedule().Complete();
            
            var result = job.result[0];
            if (result.weight <= 0f) return;
            
            pitch = result.pitch;
            occlusionWeight = result.occlusionWeight;
            lpCutoffBound = result.lpCutoffBound;
            hpCutoffBound = result.hpCutoffBound;
        }
        
        private void ProcessOcclusion(Vector3 up, int count, float dt) {
            var raycastCommands = new NativeArray<RaycastCommand>(count * _rays, Allocator.TempJob);
            var hits = new NativeArray<RaycastHit>(count * _rays * _maxHits, Allocator.TempJob);
            
            float sector = 360f / _rays;
            
            for (int i = 0; i < count; i++) {
                var data = _occlusionSearchList[i];
                var rot = Quaternion.LookRotation(data.direction, up);

#if UNITY_EDITOR
                if (_showOcclusionInfo) DebugExt.DrawSphere(data.position, 0.01f, Color.magenta);
#endif
                
                for (int j = 0; j < _rays; j++) {
                    var offset = rot * 
                                 GetOcclusionOffset(j, _rays, sector) * 
                                 Mathf.Lerp(_rayOffset0, _rayOffset1, GetRelativeDistance(data.distance));
                    
                    raycastCommands[i * _rays + j] = new RaycastCommand(
                        from: data.position + offset,
                        data.direction,
                        new QueryParameters(_layerMask, hitMultipleFaces: false, hitTriggers: QueryTriggerInteraction.Ignore, hitBackfaces: false),
                        data.distance
                    );
                    
#if UNITY_EDITOR
                    if (_showOcclusionInfo) DebugExt.DrawRay(data.position, offset, Color.magenta);
                    if (_showOcclusionInfo) DebugExt.DrawRay(data.position + offset, data.direction * data.distance, Color.magenta);
#endif
                }
            }

            int commandsPerJob = Mathf.Max(count / JobsUtility.JobWorkerCount, 1);
            RaycastCommand.ScheduleBatch(raycastCommands, hits, commandsPerJob, _maxHits).Complete();

            for (int i = 0; i < count; i++) {
                var data = _occlusionList[i];
                float weightSum = 0f;
                
                for (int j = 0; j < _rays; j++) {
                    int collisions = 0;
                    
                    for (int r = 0; r < _maxHits; r++) {
                        var hit = hits[i * _rays + j * _maxHits + r];
                        if (hit.collider == null) break;
                        
                        collisions++;
                    }

                    weightSum += (float) collisions / _maxHits;
                }

                float distanceWeight = data.weight * Mathf.Clamp01(GetRelativeDistance(data.distance));
                float collisionWeight = data.weight * Mathf.Clamp01(weightSum / _rays);
                
                ApplyOcclusion(data.audioElement, distanceWeight, collisionWeight, data.lpCutoffBound, data.hpCutoffBound, dt);
            }

            hits.Dispose();
            raycastCommands.Dispose();
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
        
        private static Vector3 GetOcclusionOffset(int i, int count, float sector) {
            return count switch {
                2 => (2 * i - 1) * Right,
                3 => (i - 1) * Right,
                _ => i == 0 ? default : Quaternion.AngleAxis(i * sector, Forward) * Up,
            };
        }

        private float GetRelativeDistance(float distance) {
            return Mathf.Clamp01((distance - _minDistance) / (_maxDistance - _minDistance + DistanceThreshold));
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
        
        private readonly struct OcclusionSearchData {

            public readonly Vector3 position;
            public readonly Vector3 direction;
            public readonly float distance;
            
            public OcclusionSearchData(Vector3 position, Vector3 direction, float distance) {
                this.position = position;
                this.direction = direction;
                this.distance = distance;
            }
        }
        
        private readonly struct OcclusionData {

            public readonly IAudioElement audioElement;
            public readonly float distance;
            public readonly float weight;
            public readonly float lpCutoffBound;
            public readonly float hpCutoffBound;
            
            public OcclusionData(IAudioElement audioElement, float distance, float weight, float lpCutoffBound, float hpCutoffBound) {
                this.audioElement = audioElement;
                this.distance = distance;
                this.weight = weight;
                this.lpCutoffBound = lpCutoffBound;
                this.hpCutoffBound = hpCutoffBound;
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

        private struct VolumeData {
            public int priority;
            public float weight;
            public float pitch;
            public float occlusionWeight;
            public float lpCutoffBound;
            public float hpCutoffBound;
        }
        
        private struct CalculateVolumeJob : IJob {
            
            [ReadOnly] public int count;
            [ReadOnly] public int topPriority;
            [ReadOnly] public NativeArray<VolumeData> volumeDataArray;
            public NativeArray<VolumeData> result;
            
            public void Execute() {
                var resultData = result[0];

                resultData.pitch = 0f;
                resultData.occlusionWeight = 0f;
                resultData.lpCutoffBound = 0f;
                resultData.hpCutoffBound = 0f;
                
                for (int i = 0; i < count; i++) {
                    var data = volumeDataArray[i];
                    if (data.priority < topPriority) continue;
                    
                    resultData.weight += data.weight;
                    resultData.pitch += data.pitch * data.weight;
                    resultData.occlusionWeight += data.occlusionWeight * data.weight;
                    resultData.lpCutoffBound += data.lpCutoffBound * data.weight;
                    resultData.hpCutoffBound += data.hpCutoffBound * data.weight;
                }

                if (resultData.weight <= 0f) return;
                
                resultData.pitch /= resultData.weight;
                resultData.occlusionWeight /= resultData.weight;
                resultData.lpCutoffBound /= resultData.weight;
                resultData.hpCutoffBound /= resultData.weight;

                result[0] = resultData;
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
#endif
    }
    
}