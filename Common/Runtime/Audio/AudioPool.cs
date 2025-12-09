using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MisterGames.Common.Easing;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-999)]
    public sealed class AudioPool : MonoBehaviour, IAudioPool, IUpdate {

        [Header("Audio Element")]
        [SerializeField] private AudioElement _prefab;
        [SerializeField] private AudioMixerGroup _defaultMixerGroup;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        [SerializeField] [Min(0f)] private float _attenuationDistance = 50f;
        [SerializeField] [Min(0f)] private float _audioParametersSmoothing = 3f;
        
        [Header("Shuffle")]
        [SerializeField] [Min(0f)] private float _lastIndexStoreLifetime = 60f;

        [Header("Audio Volumes")]
        [SerializeField] private bool _enableVolumes = true;
        [SerializeField] private bool _includeDefaultMixerGroupsForVolumes = true;
        [SerializeField] private AudioMixerGroup[] _includeMixerGroupsForVolumes;
        
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
        [SerializeField] [Range(0f, 1f)] private float _distanceWeightLow = 1f;
        [SerializeField] [Range(0f, 1f)] private float _collisionWeightLow = 0.3f;
        [SerializeField] [Range(1f, 10f)] private float _qLow = 1f;
        [SerializeField] [Range(10f, 22000f)] private float _cutoffLow = 2000f;
        [SerializeField] private EasingType _distanceLowFreqEasing = EasingType.EaseOutQuad;
        [SerializeField] private EasingType _cutoffLowFreqEasing = EasingType.EaseOutCubic;
        
        [Space]
        [SerializeField] [Range(0f, 1f)] private float _distanceWeightHigh = 1f;
        [SerializeField] [Range(0f, 1f)] private float _collisionWeightHigh = 0.3f;
        [SerializeField] [Range(1f, 10f)] private float _qHigh = 1f;
        [SerializeField] [Range(10f, 22000f)] private float _cutoffHigh = 500f;
        [SerializeField] private EasingType _distanceHighFreqEasing = EasingType.EaseInSine;
        [SerializeField] private EasingType _cutoffHighFreqEasing = EasingType.EaseOutSine;
        
        public static IAudioPool Main { get; private set; }

        private const float DistanceThreshold = 0.001f;
        private const float HpCutoffLowerBound = 10f;
        private const float LpCutoffUpperBound = 22000f;
        
        private static readonly float3 Up = Vector3.up;
        private static readonly float3 Forward = Vector3.forward;
        private static readonly float3 Right = Vector3.right;
        
        private readonly Dictionary<int, IndexData> _clipsHashToLastIndexMap = new();
        private float _lastClipShufflesCheckTime;
        
        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, AttachKey> _handleIdToAttachKeyMap = new();
        private readonly Dictionary<int, IAudioElement> _handleIdToAudioElementMap = new();
        private int _lastHandleId;
        
        private readonly Dictionary<int, FadeData> _fadeInDataMap = new();
        private readonly Dictionary<int, FadeData> _fadeOutDataMap = new();
        private readonly Dictionary<int, AudioSource> _releaseSourcesMap = new();
        
        private readonly Dictionary<AudioListener, ListenerData> _audioListenersMap = new();
        private Transform _listenerTransform;
        private Transform _listenerUp;

        private readonly HashSet<IAudioVolume> _volumes = new();
        private readonly HashSet<int> _includeMixerGroupsForVolumesSet = new();
        
        private Transform _transform;
        private float _lastTimeScale;
        private float _globalOcclusionWeight = 1f;
        
        private void Awake() {
            Main = this;
            
            _transform = transform;
            
            FetchIncludeMixerGroupsFromVolumes();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDestroy() {
            _clipsHashToLastIndexMap.Clear();
            
            _attachKeyToHandleIdMap.Clear();
            _handleIdToAttachKeyMap.Clear();
            _handleIdToAudioElementMap.Clear();

            _fadeInDataMap.Clear();
            _fadeOutDataMap.Clear();
            _releaseSourcesMap.Clear();
            
            _audioListenersMap.Clear();
            _volumes.Clear();
            _includeMixerGroupsForVolumesSet.Clear();
            
            Main = null;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }
        
        private void FetchIncludeMixerGroupsFromVolumes() {
            _includeMixerGroupsForVolumesSet.Clear();
            
            for (int i = 0; i < _includeMixerGroupsForVolumes.Length; i++) {
                _includeMixerGroupsForVolumesSet.Add(_includeMixerGroupsForVolumes[i].GetInstanceID());
            }

            if (_includeDefaultMixerGroupsForVolumes && _defaultMixerGroup != null) {
                _includeMixerGroupsForVolumesSet.Add(_defaultMixerGroup.GetInstanceID());
            }
        }
        
        public void RegisterListener(AudioListener listener, Transform up, int priority) {
            _audioListenersMap[listener] = new ListenerData(priority, up);
            UpdateListeners();
        }

        public void UnregisterListener(AudioListener listener) {
            _audioListenersMap.Remove(listener);
            UpdateListeners();
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

#if UNITY_EDITOR
            CreateDebugColor(id, clip.name);
#endif
            
            bool loop = (options & AudioOptions.Loop) == AudioOptions.Loop;
            bool affectedByTimeScale = (options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;
            normalizedTime = Mathf.Clamp01(normalizedTime);
            mixerGroup = mixerGroup == null ? _defaultMixerGroup : mixerGroup;

            float clipLength = clip.length;
            float clipTime = normalizedTime * clipLength;
            
            InitializeAudioElement(
                audioElement,
                id,
                pitch,
                fadeOut,
                clipLength,
                clipTime,
                options,
                mixerGroup,
                cancellationToken
            );
            
            ProcessSound(audioElement);

            if (fadeIn > 0f) {
                _fadeInDataMap[id] = new FadeData(
                    affectedByTimeScale ? TimeSources.scaledTime : Time.realtimeSinceStartup, 
                    fadeIn, 
                    volume,
                    affectedByTimeScale
                );   
            }

            _handleIdToAudioElementMap[id] = audioElement;
            
            RestartAudioSource(
                audioElement.Source, clip, mixerGroup, 
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f), 
                spatialBlend, clipTime, loop
            );
            
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
            var attachKey = new AttachKey(attachTo.GetHashCode(), attachId);

#if UNITY_EDITOR
            CreateDebugColor(id, clip.name);
#endif
            
            bool loop = (options & AudioOptions.Loop) == AudioOptions.Loop;
            bool affectedByTimeScale = (options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;
            normalizedTime = Mathf.Clamp01(normalizedTime);
            mixerGroup = mixerGroup == null ? _defaultMixerGroup : mixerGroup;
            
            float clipLength = clip.length;
            float clipTime = normalizedTime * clipLength;
            
            if (attachId != 0) {
                if (_attachKeyToHandleIdMap.TryGetValue(attachKey, out int oldId)) {
                    ReleaseSound(oldId, immediate: false);
                }
                
                _attachKeyToHandleIdMap[attachKey] = id;
            }
            
            InitializeAudioElement(
                audioElement,
                id,
                pitch,
                fadeOut,
                clipLength,
                clipTime,
                options,
                mixerGroup,
                cancellationToken
            );
            
            ProcessSound(audioElement);
            
            _handleIdToAudioElementMap[id] = audioElement;
            
            if (fadeIn > 0f) {
                _fadeInDataMap[id] = new FadeData(
                    affectedByTimeScale ? TimeSources.scaledTime : Time.realtimeSinceStartup, 
                    fadeIn, 
                    volume,
                    affectedByTimeScale
                );   
            }

            RestartAudioSource(
                audioElement.Source, clip, mixerGroup, 
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f), 
                spatialBlend, clipTime, loop
            );

            return new AudioHandle(this, id);
        }
        
        private int GetNextHandleId() {
            unchecked {
                if (++_lastHandleId == 0) _lastHandleId++;   
            }
            
            return _lastHandleId;
        }

        private void InitializeAudioElement(
            IAudioElement audioElement,
            int id,
            float pitch,
            float fadeOut,
            float clipLength,
            float clipTime,
            AudioOptions options,
            AudioMixerGroup mixerGroup,
            CancellationToken cancellationToken) 
        {
            audioElement.Id = id;
            audioElement.MixerGroupId = mixerGroup == null ? 0 : mixerGroup.GetInstanceID();
            audioElement.AudioPool = this;
            
            audioElement.AudioOptions = options;
            audioElement.PitchMul = pitch;
            audioElement.AttenuationMul = 1f;

            audioElement.ClipLength = clipLength;
            audioElement.ClipTime = clipTime;
            audioElement.FadeOut = fadeOut < 0f ? _fadeOut : fadeOut;
            audioElement.OcclusionFlag = 0;

            audioElement.LowPass.lowpassResonanceQ = _qLow;
            audioElement.HighPass.highpassResonanceQ = _qHigh;
            audioElement.Source.maxDistance = _attenuationDistance;

            audioElement.CancellationToken = cancellationToken;
        }
        
        private IAudioElement GetAudioElementAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity, _transform);
        }

        private IAudioElement GetAudioElementAttached(Transform parent, Vector3 localPosition = default) {
            return PrefabPool.Main.Get(_prefab, parent.TransformPoint(localPosition), Quaternion.identity, parent);
        }
        
        private static void RestartAudioSource(
            AudioSource source,
            AudioClip clip,
            AudioMixerGroup mixerGroup,
            float fadeIn,
            float volume, 
            float pitch, 
            float spatialBlend,
            float clipTime,
            bool loop) 
        {
            source.Stop();

            source.clip = clip;
            source.time = clipTime;
            source.volume = fadeIn > 0f ? 0f : volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.outputAudioMixerGroup = mixerGroup;
            
            source.Play();
        }

        public AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips) {
            int count = clips?.Count ?? 0;
            
            switch (count) {
                case 0:
                    return null;
                
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
        
        private int NextClipIndex(int hash, int count) {
            var data = _clipsHashToLastIndexMap.GetValueOrDefault(hash);
            
            int mask = data.indicesMask;
            int startIndex = data.startIndex;
            int index = AudioExtensions.GetRandomIndex(ref mask, ref startIndex, data.lastIndex, count);
            
            _clipsHashToLastIndexMap[hash] = new IndexData(mask, startIndex, index, Time.time);
            
            return index;
        }
        
        public AudioHandle GetAudioHandle(Transform attachedTo, int hash) {
            return _attachKeyToHandleIdMap.TryGetValue(new AttachKey(attachedTo.GetInstanceID(), hash), out int id) && 
                   _handleIdToAudioElementMap.ContainsKey(id)
                ? new AudioHandle(this, id)
                : AudioHandle.Invalid;
        }
        
        void IAudioPool.ReleaseAudioHandle(int handleId, bool immediate) {
            ReleaseSound(handleId, immediate);
        }

        private void ReleaseSound(int handleId, bool immediate) {
            if (!_handleIdToAudioElementMap.Remove(handleId, out var e)) return;

            if (_handleIdToAttachKeyMap.Remove(handleId, out var attachKey)) {
                _attachKeyToHandleIdMap.Remove(attachKey);
            }

            _fadeInDataMap.Remove(handleId);

            bool isNull = e.Source == null;
            
            if (immediate || isNull) {
                if (!isNull) PrefabPool.Main?.Release(e.Source);
            }
            else {
                bool affectedByTimescale = (e.AudioOptions & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale;

                _fadeOutDataMap[handleId] = new FadeData(
                    affectedByTimescale ? TimeSources.scaledTime : Time.realtimeSinceStartup,
                    e.FadeOut,
                    e.Source.volume,
                    affectedByTimescale
                );

                _releaseSourcesMap[handleId] = e.Source;
            }

#if UNITY_EDITOR
            RemoveDebugColor(handleId);
#endif
        }

        void IAudioPool.SetAudioHandleVolume(int handleId, float volume) { 
            if (!_handleIdToAudioElementMap.TryGetValue(handleId, out var e)) return;

            // Stop fade in
            _fadeInDataMap.Remove(handleId);
            e.Source.volume = volume;
        }

        bool IAudioPool.TryGetAudioElement(int handleId, out IAudioElement audioElement) {
            return _handleIdToAudioElementMap.TryGetValue(handleId, out audioElement);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessClipShuffles();
            ProcessFadeIn();
            ProcessFadeOutAndRelease();
            ProcessSounds(dt);
        }

        private void ProcessClipShuffles() {
            float time = Time.realtimeSinceStartup;
            if (time < _lastClipShufflesCheckTime + _lastIndexStoreLifetime) return;

            _lastClipShufflesCheckTime = time;
            
            int count = _clipsHashToLastIndexMap.Count;
            var buffer = new NativeArray<IndexCheckData>(count, Allocator.Temp);
            int index = 0;
                
            foreach ((int hash, var data) in _clipsHashToLastIndexMap) {
                buffer[index++] = new IndexCheckData(hash, data.time);
            }

            for (int i = 0; i < count; i++) {
                var data = buffer[i];
                    
                if (time - data.time > _lastIndexStoreLifetime) {
                    _clipsHashToLastIndexMap.Remove(data.hash);
                }
            }
                
            buffer.Dispose();
        }

        private void ProcessFadeIn() {
            float scaledTime = TimeSources.scaledTime;
            float time = Time.realtimeSinceStartup;

            int fadeInCount = _fadeInDataMap.Count;
            var fadeInCalculateArray = new NativeArray<FadeCalculateData>(fadeInCount, Allocator.TempJob);
            var fadeInResultArray = new NativeArray<FadeResultData>(fadeInCount, Allocator.TempJob);
            
            int index = 0;
            
            foreach ((int id, var data) in _fadeInDataMap) {
                fadeInCalculateArray[index++] = new FadeCalculateData(id, data.affectedByTimescale, data.startTime, data.fade, 0f, data.volume);
            }

            var calculateFadeInJob = new CalculateFadeJob {
                fadeCalculateDataArray = fadeInCalculateArray,
                scaledTime = scaledTime,
                unscaledTime = time,
                fadeResultArray = fadeInResultArray
            };
            
            calculateFadeInJob.Schedule(fadeInCount, JobExt.BatchFor(fadeInCount)).Complete();

            for (int i = 0; i < fadeInCount; i++) {
                var result = fadeInResultArray[i];
                var e = _handleIdToAudioElementMap[result.id];
                
                e.Source.volume = result.volume;

                if (result.progress < 1f) continue;
                
                _fadeInDataMap.Remove(result.id);
            }
            
            fadeInCalculateArray.Dispose();
            fadeInResultArray.Dispose();
        }

        private void ProcessFadeOutAndRelease() {
            float scaledTime = TimeSources.scaledTime;
            float time = Time.realtimeSinceStartup;

            int fadeOutCount = _fadeOutDataMap.Count;
            var fadeOutCalculateArray = new NativeArray<FadeCalculateData>(fadeOutCount, Allocator.TempJob);
            var fadeOutResultArray = new NativeArray<FadeResultData>(fadeOutCount, Allocator.TempJob);
            
            int index = 0;
            
            foreach ((int id, var data) in _fadeOutDataMap) {
                fadeOutCalculateArray[index++] = new FadeCalculateData(id, data.affectedByTimescale, data.startTime, data.fade, data.volume, 0f);
            }

            var calculateFadeOutJob = new CalculateFadeJob {
                fadeCalculateDataArray = fadeOutCalculateArray,
                scaledTime = scaledTime,
                unscaledTime = time,
                fadeResultArray = fadeOutResultArray
            };
            
            calculateFadeOutJob.Schedule(fadeOutCount, JobExt.BatchFor(fadeOutCount)).Complete();

            var pool = PrefabPool.Main;
            
            for (int i = 0; i < fadeOutCount; i++) {
                var result = fadeOutResultArray[i];
                var source = _releaseSourcesMap[result.id];
                
                source.volume = result.volume;
                
                if (result.progress < 1f) continue;
                
                _fadeOutDataMap.Remove(result.id);
                _releaseSourcesMap.Remove(result.id);
                pool.Release(source);
            }
            
            fadeOutCalculateArray.Dispose();
            fadeOutResultArray.Dispose();
        }

        private void ProcessSounds(float dt) {
            if (_audioListenersMap.Count == 0) {
                foreach (var audioElement in _handleIdToAudioElementMap.Values) {
                    // To reset smoothed values
                    audioElement.OcclusionFlag = 0;
                }
                return;
            }
            
            var listenerPos = _listenerTransform.position;
            var listenerUp = _listenerUp.up;
            
            int soundCount = _handleIdToAudioElementMap.Count;

            var soundDataArray = new NativeArray<SoundData>(soundCount, Allocator.TempJob);
            var soundOptionsArray = new NativeArray<AudioOptions>(soundCount, Allocator.TempJob);
            var listenerAndSoundsPositionArray = new NativeArray<float3>(soundCount + 1, Allocator.TempJob);
            
            listenerAndSoundsPositionArray[0] = listenerPos;
            
            int index = 0;
            foreach (var e in _handleIdToAudioElementMap.Values) {
                var options = e.AudioOptions;
                int mixerGroupId = e.MixerGroupId;

                if (mixerGroupId != 0 && !_includeMixerGroupsForVolumesSet.Contains(mixerGroupId)) {
                    options &= ~AudioOptions.AffectedByVolumes;
                }
                
                soundDataArray[index] = new SoundData(
                    e.Id, e.OcclusionFlag,
                    e.Source.spatialBlend, e.PitchMul, e.AttenuationMul, e.LowPass.cutoffFrequency, e.HighPass.cutoffFrequency
                );
                
                soundOptionsArray[index] = options;
                listenerAndSoundsPositionArray[1 + index++] = e.Transform.position;
            }

            var volumeResultArray = CalculateVolumes(listenerAndSoundsPositionArray, soundOptionsArray, soundCount);
            
            var occlusionResultArray = CalculateOcclusion(listenerAndSoundsPositionArray, soundOptionsArray, soundCount, listenerUp
#if UNITY_EDITOR
                , soundDataArray
#endif
            );
            
            var resultSoundArray = CalculateResult(soundDataArray, soundOptionsArray, volumeResultArray, occlusionResultArray, soundCount, dt);

            for (int i = 0; i < soundCount; i++) {
                var options = soundOptionsArray[i];
                var soundData = soundDataArray[i];
                
                var e = _handleIdToAudioElementMap[soundData.id];
                
                if (e.CancellationToken.IsCancellationRequested || 
                    (options & AudioOptions.Loop) == 0 && 
                    (e.ClipTime = math.max(e.ClipTime, e.Source.time)) >= e.ClipLength) 
                {
                    ReleaseSound(soundData.id, immediate: false);
                    continue;
                }
                
                var resultData = resultSoundArray[i];
                
#if UNITY_EDITOR
                if (_showSoundsDebugInfo) {
                    bool show = true;

                    if (_showSoundsNameFilters?.Length > 0) {
                        show = false;
                        string clipName = e.Source.clip.name;

                        for (int f = 0; f < _showSoundsNameFilters?.Length; f++) {
                            string filter = _showSoundsNameFilters[f];
                            if (string.IsNullOrWhiteSpace(filter) || !clipName.Contains(filter)) continue;
                            
                            show = true;
                            break;
                        }   
                    }

                    if (show) {
                        var occ = occlusionResultArray[i];
                        var vol = volumeResultArray[i];
                        Debug.Log($"AudioPool.ProcessSounds: f {Time.frameCount}, clip {e.Source.clip.name}, " +
                                  $"w low {occ.weightLowFreq}, w high {occ.weightHighFreq}, lp {resultData.lpCutoff}, hp {resultData.hpCutoff}, " +
                                  $"dist {occ.distance}, collisions {occ.collisions}, dist w {occ.distanceWeight}, coll w {occ.collisionWeight}, " +
                                  $"vol occ {vol.occlusion}, vol pitch {vol.pitch}, vol atten {vol.attenuation}, vol lp {vol.lpCutoff}, vol hp {vol.hpCutoff}");
                    }
                }
#endif
                
                e.Source.pitch = resultData.pitch;
                e.Source.maxDistance = resultData.attenuationDistance;
                
                e.LowPass.cutoffFrequency = resultData.lpCutoff;
                e.HighPass.cutoffFrequency = resultData.hpCutoff;

                e.OcclusionFlag = 1;
            }

            soundDataArray.Dispose();
            soundOptionsArray.Dispose();
            listenerAndSoundsPositionArray.Dispose();
            
            volumeResultArray.Dispose();
            occlusionResultArray.Dispose();
            resultSoundArray.Dispose();
            
            _globalOcclusionWeight = 1f;
        }
        
        private void ProcessSound(IAudioElement e) {
            if (_audioListenersMap.Count == 0) {
                // To reset smoothed values
                e.OcclusionFlag = 0;
                return;
            }
            
            var listenerPos = _listenerTransform.position;
            var listenerUp = _listenerUp.up;

            var soundDataArray = new NativeArray<SoundData>(2, Allocator.TempJob);
            var soundOptionsArray = new NativeArray<AudioOptions>(2, Allocator.TempJob);
            var listenerAndSoundsPositionArray = new NativeArray<float3>(2, Allocator.TempJob);
            
            listenerAndSoundsPositionArray[0] = listenerPos;
            
            var options = e.AudioOptions;
            int mixerGroupId = e.MixerGroupId;

            if (mixerGroupId != 0 && !_includeMixerGroupsForVolumesSet.Contains(mixerGroupId)) {
                options &= ~AudioOptions.AffectedByVolumes;
            }
                
            soundDataArray[0] = new SoundData(
                e.Id, e.OcclusionFlag,
                e.Source.spatialBlend, e.PitchMul, e.AttenuationMul, e.LowPass.cutoffFrequency, e.HighPass.cutoffFrequency
            );
            
            soundOptionsArray[0] = options;
            listenerAndSoundsPositionArray[1] = e.Transform.position;

            var volumeResultArray = CalculateVolumes(listenerAndSoundsPositionArray, soundOptionsArray, 1);
            
            var occlusionResultArray = CalculateOcclusion(listenerAndSoundsPositionArray, soundOptionsArray, 1, listenerUp
#if UNITY_EDITOR
                , soundDataArray
#endif
            );
            
            var resultSoundArray = CalculateResult(soundDataArray, soundOptionsArray, volumeResultArray, occlusionResultArray, 1, 0f);

            var resultData = resultSoundArray[0];
                
            e.Source.pitch = resultData.pitch;
            e.Source.maxDistance = resultData.attenuationDistance;
                
            e.LowPass.cutoffFrequency = resultData.lpCutoff;
            e.HighPass.cutoffFrequency = resultData.hpCutoff;

            e.OcclusionFlag = 1;

            soundDataArray.Dispose();
            soundOptionsArray.Dispose();
            listenerAndSoundsPositionArray.Dispose();
            
            volumeResultArray.Dispose();
            occlusionResultArray.Dispose();
            resultSoundArray.Dispose();
        }
        
        private NativeArray<VolumeResultData> CalculateVolumes(
            NativeArray<float3> listenerAndSoundPositionArray,
            NativeArray<AudioOptions> soundOptionsArray,
            int soundCount)
        {
            NativeArray<VolumeResultData> resultArray;
            
            if (!_enableVolumes) {
                resultArray = new NativeArray<VolumeResultData>(soundCount, Allocator.TempJob);
                
                var writeDefaultVolumeResultJob = new WriteDefaultVolumeResultDataJob {
                    attenuationDefault = _attenuationDistance,
                    resultArray = resultArray,
                };

                writeDefaultVolumeResultJob.Schedule(soundCount, JobExt.BatchFor(soundCount)).Complete();
                
                return resultArray;
            }
            
            int volumeCount = _volumes.Count;
            int volumeIndex = 0;
            
            var volumeWeightDataArray = new NativeArray<VolumeWeightData>((soundCount + 1) * volumeCount, Allocator.TempJob);
            var volumeProcessDataArray = new NativeArray<VolumeProcessData>(volumeCount, Allocator.TempJob);
            
            foreach (var volume in _volumes) {
                int priority = volume.Priority;
                float listenerPresence = volume.ListenerPresence;

                float occlusionListener = 1f;
                float occlusionSound = 1f;
                float pitch = 1f;
                float attenuation = _attenuationDistance;
                float lpCutoff = LpCutoffUpperBound;
                float hpCutoff = HpCutoffLowerBound;

                int mask = 0;
                
                if (volume.ModifyOcclusionWeightForListener(ref occlusionListener)) AudioParameter.ListenerOcclusion.WriteToMask(ref mask);
                if (volume.ModifyOcclusionWeightForSound(ref occlusionSound)) AudioParameter.SoundOcclusion.WriteToMask(ref mask);
                if (volume.ModifyPitch(ref pitch)) AudioParameter.Pitch.WriteToMask(ref mask);
                if (volume.ModifyAttenuationDistance(ref attenuation)) AudioParameter.Attenuation.WriteToMask(ref mask);
                if (volume.ModifyLowPassFilter(ref lpCutoff)) AudioParameter.LpCutoff.WriteToMask(ref mask);
                if (volume.ModifyHighPassFilter(ref hpCutoff)) AudioParameter.HpCutoff.WriteToMask(ref mask);
                
                volumeProcessDataArray[volumeIndex] = new VolumeProcessData(
                    mask, listenerPresence,
                    occlusionListener, occlusionSound, pitch, attenuation, lpCutoff, hpCutoff
                );
                
                var weightArray = new NativeArray<WeightData>(soundCount + 1, Allocator.TempJob);
                volume.GetWeight(listenerAndSoundPositionArray, weightArray, soundCount + 1);

#if UNITY_EDITOR
                if (_showVolumeInfo) {
                    var sb = new StringBuilder();
                    
                    if (AudioParameter.ListenerOcclusion.InMask(mask)) sb.Append($"{AudioParameter.ListenerOcclusion} ");
                    if (AudioParameter.SoundOcclusion.InMask(mask)) sb.Append($"{AudioParameter.SoundOcclusion} ");
                    if (AudioParameter.Pitch.InMask(mask)) sb.Append($"{AudioParameter.Pitch} ");
                    if (AudioParameter.Attenuation.InMask(mask)) sb.Append($"{AudioParameter.Attenuation} ");
                    if (AudioParameter.LpCutoff.InMask(mask)) sb.Append($"{AudioParameter.LpCutoff} ");
                    if (AudioParameter.HpCutoff.InMask(mask)) sb.Append($"{AudioParameter.HpCutoff}");
                    
                    Debug.Log($"AudioPool.CalculateVolumes: f {Time.frameCount}, vol #{volumeIndex} {volume}, " +
                              $"list presence {listenerPresence}, " +
                              $"occ lis {occlusionListener}, occ sound {occlusionSound}, pitch {pitch}, atten {attenuation}, lp {lpCutoff}, hp {hpCutoff}, " +
                              $"changed [{sb}]");
                }
#endif
                
                var writeWeightJob = new WriteVolumeWeightDataJob {
                    weightArray = weightArray,
                    priority = priority,
                    volumeCount = volumeCount,
                    volumeIndex = volumeIndex,
                    volumeWeightDataArray = volumeWeightDataArray, 
                };
                
                writeWeightJob.Schedule(soundCount + 1, JobExt.BatchFor(soundCount + 1)).Complete();

                weightArray.Dispose();
                
                volumeIndex++;
            }

            var listenerVolumeIdToWeightMap = new NativeHashMap<int, float>(volumeCount, Allocator.TempJob);
            var occlusionListenerResultArray = new NativeArray<float>(2, Allocator.TempJob);
            resultArray = new NativeArray<VolumeResultData>(soundCount, Allocator.TempJob);
            
            var calculateListenerVolumeJob = new CalculateListenerVolumeJob {
                volumeWeightDataArray = volumeWeightDataArray,
                volumeProcessDataArray = volumeProcessDataArray,
                volumeCount = volumeCount,
                listenerVolumeIdToWeightMap = listenerVolumeIdToWeightMap,
                occlusionListenerResultArray = occlusionListenerResultArray,
            };
            
            var calculateVolumeResultJob = new CalculateVolumeResultDataJob {
                volumeWeightDataArray = volumeWeightDataArray,
                volumeProcessDataArray = volumeProcessDataArray,
                soundOptionsArray = soundOptionsArray,
                occlusionListenerResultArray = occlusionListenerResultArray,
                listenerVolumeIdToWeightMap = listenerVolumeIdToWeightMap,
                volumeCount = volumeCount,
                attenuationDefault = _attenuationDistance,
                resultArray = resultArray,
            };

            var listenerJobHandle = calculateListenerVolumeJob.Schedule();
            var resultJobHandle = calculateVolumeResultJob.Schedule(soundCount, JobExt.BatchFor(soundCount), listenerJobHandle);
            
            resultJobHandle.Complete();
            
            volumeWeightDataArray.Dispose();
            volumeProcessDataArray.Dispose();
            listenerVolumeIdToWeightMap.Dispose();
            occlusionListenerResultArray.Dispose();

            return resultArray;
        }

        private NativeArray<OcclusionResultData> CalculateOcclusion(
            NativeArray<float3> listenerAndSoundPositionArray,
            NativeArray<AudioOptions> soundOptionsArray,
            int soundCount, 
            Vector3 up
#if UNITY_EDITOR
            , NativeArray<SoundData> soundDataArray
#endif
            ) 
        {
            if (!_applyOcclusion) {
                return new NativeArray<OcclusionResultData>(soundCount, Allocator.TempJob);
            }
            
            var raycastCommands = new NativeArray<RaycastCommand>(soundCount * _rays, Allocator.TempJob);
            var hits = new NativeArray<RaycastHit>(soundCount * _rays * _maxHits, Allocator.TempJob);
            var resultArray = new NativeArray<OcclusionResultData>(soundCount, Allocator.TempJob);
         
            var prepareRaycastCommandsJob = new PrepareRaycastCommandsJob {
                listenerAndSoundsPositionArray = listenerAndSoundPositionArray,
                soundOptionsArray = soundOptionsArray,
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
                listenerAndSoundsPositionArray = listenerAndSoundPositionArray,
                soundOptionsArray = soundOptionsArray,
                hitsArray = hits,
                maxHits = _maxHits,
                maxDistance = _maxDistance,
                minDistance = _minDistance,
                rays = _rays,
                globalOcclusionWeight = _globalOcclusionWeight,
                distanceLowFreqWeight = _distanceWeightLow,
                distanceHighFreqWeight = _distanceWeightHigh,
                collisionLowFreqWeight = _collisionWeightLow,
                collisionHighFreqWeight = _collisionWeightHigh,
                distanceLowFreqEasing = _distanceLowFreqEasing,
                distanceHighFreqEasing = _distanceHighFreqEasing,
                resultArray = resultArray
            };

            var prepareJobHandle = prepareRaycastCommandsJob.ScheduleBatch(soundCount * _rays, _rays);
            var raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, JobExt.BatchFor(soundCount * _rays), _maxHits, prepareJobHandle);
            var calculateWeightsJobHandle = calculateOcclusionWeightsJob.Schedule(soundCount, JobExt.BatchFor(soundCount), raycastJobHandle);
                
            calculateWeightsJobHandle.Complete();

#if UNITY_EDITOR
            if (_showOcclusionInfo) {
                for (int i = 0; i < soundCount; i++) {
                    int id = soundDataArray[i].id;
                    
                    if (_showOcclusionNameFilters?.Length > 0) {
                        string clipName = _handleIdToAudioElementMap[id].Source.clip.name;
                        bool show = false;
                        
                        for (int f = 0; f < _showOcclusionNameFilters.Length; f++) {
                            string filter = _showOcclusionNameFilters[f];
                            if (string.IsNullOrWhiteSpace(filter) || !clipName.Contains(filter)) continue;
                            
                            show = true;
                            break;
                        }
                        
                        if (!show) continue;
                    }
                    
                    var pos = listenerAndSoundPositionArray[i + 1];
                    var color = GetDebugColor(id);
                    
                    for (int j = 0; j < _rays; j++) {
                        var com = raycastCommands[i * _rays + j];
                        DebugExt.DrawLine(pos, com.from, color);
                        DebugExt.DrawRay(com.from, com.direction * com.distance, color);
                    }
                }
            }
#endif
            
            hits.Dispose();
            raycastCommands.Dispose();

            return resultArray;
        }

        private NativeArray<SoundResultData> CalculateResult(
            NativeArray<SoundData> soundDataArray, 
            NativeArray<AudioOptions> soundOptionsArray, 
            NativeArray<VolumeResultData> volumeResultDataArray, 
            NativeArray<OcclusionResultData> occlusionResultDataArray,
            int soundCount,
            float dt) 
        {
            var resultArray = new NativeArray<SoundResultData>(soundCount, Allocator.TempJob);
            
            var calculateResultJob = new CalculateResultSoundJob {
                soundDataArray = soundDataArray,
                soundOptionsArray = soundOptionsArray,
                volumeResultDataArray = volumeResultDataArray,
                occlusionResultDataArray = occlusionResultDataArray,
                timescale = Time.timeScale,
                dt = dt,
                smoothing = _audioParametersSmoothing,
                lpCutoff = _cutoffLow,
                hpCutoff = _cutoffHigh,
                lpCutoffEasing = _cutoffLowFreqEasing,
                hpCutoffEasing = _cutoffHighFreqEasing,
                resultArray = resultArray,
            };

            calculateResultJob.Schedule(soundCount, JobExt.BatchFor(soundCount)).Complete();
            
            return resultArray;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetOcclusionOffset(int i, int count, float sector) {
            return count switch {
                2 => (2 * i - 1) * Right,
                3 => (i - 1) * Right,
                _ => i == 0 ? default : math.mul(quaternion.AxisAngle(Forward, i * sector), Up),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetRelativeDistance(float distance, float min, float max) {
            return math.clamp((distance - min) / (max - min + DistanceThreshold), 0f, 1f);
        }
        
        #region DATA TYPES
        
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
        
        private readonly struct IndexCheckData {

            public readonly int hash;
            public readonly float time;
            
            public IndexCheckData(int hash, float time) {
                this.hash = hash;
                this.time = time;
            }
        }

        private readonly struct AttachKey : IEquatable<AttachKey> {
            
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
        
        private readonly struct FadeData {
            
            public readonly float startTime;
            public readonly float fade;
            public readonly float volume;
            public readonly bool affectedByTimescale;
            
            public FadeData(float startTime, float fade, float volume, bool affectedByTimescale) {
                this.startTime = startTime;
                this.fade = fade;
                this.volume = volume;
                this.affectedByTimescale = affectedByTimescale;
            }
        }
        
        private readonly struct FadeCalculateData {
            
            public readonly int id;
            public readonly bool affectedByTimescale;
            public readonly float startTime;
            public readonly float fade;
            public readonly float volume0;
            public readonly float volume1;
            
            public FadeCalculateData(int id, bool affectedByTimescale, float startTime, float fade, float volume0, float volume1) {
                this.id = id;
                this.affectedByTimescale = affectedByTimescale;
                this.startTime = startTime;
                this.fade = fade;
                this.volume0 = volume0;
                this.volume1 = volume1;
            }
        }
        
        private readonly struct FadeResultData {
            
            public readonly int id;
            public readonly float volume;
            public readonly float progress;
            
            public FadeResultData(int id, float volume, float progress) {
                this.id = id;
                this.volume = volume;
                this.progress = progress;
            }
        }
        
        private readonly struct SoundData {
            
            public readonly int id;
            public readonly int occlusionFlag;
            public readonly float spatialBlend;
            public readonly float pitchMul;
            public readonly float attenuationMul;
            public readonly float lpCutoff;
            public readonly float hpCutoff;
            
            public SoundData(int id, int occlusionFlag,
                float spatialBlend, float pitchMul, float attenuationMul, float lpCutoff, float hpCutoff) 
            {
                this.id = id;
                this.occlusionFlag = occlusionFlag;
                this.spatialBlend = spatialBlend;
                this.pitchMul = pitchMul;
                this.attenuationMul = attenuationMul;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        private readonly struct VolumeWeightData {
            
            public readonly int volumeId;
            public readonly int priority;
            public readonly float weight;
            
            public VolumeWeightData(int volumeId, int priority, float weight) {
                this.volumeId = volumeId;
                this.priority = priority;
                this.weight = weight;
            }
        }
        
        private readonly struct VolumeProcessData {
            
            public readonly int mask;
            public readonly float listenerPresence;
            public readonly float occlusionListener;
            public readonly float occlusionSound;
            public readonly float pitch;
            public readonly float attenuation;
            public readonly float lpCutoff;
            public readonly float hpCutoff;
            
            public VolumeProcessData(
                int mask, float listenerPresence,
                float occlusionListener, float occlusionSound, float pitch, float attenuation, float lpCutoff, float hpCutoff) 
            {
                this.mask = mask;
                this.listenerPresence = listenerPresence;
                
                this.occlusionListener = occlusionListener;
                this.occlusionSound = occlusionSound;
                this.pitch = pitch;
                this.attenuation = attenuation;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        private readonly struct VolumeResultData {
            
            public readonly float occlusion;
            public readonly float pitch;
            public readonly float attenuation;
            public readonly float lpCutoff;
            public readonly float hpCutoff;
            
            public VolumeResultData(float occlusion, float pitch, float attenuation, float lpCutoff, float hpCutoff) {
                this.occlusion = occlusion;
                this.pitch = pitch;
                this.attenuation = attenuation;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        private readonly struct OcclusionResultData {

            public readonly float weightLowFreq;
            public readonly float weightHighFreq;
#if UNITY_EDITOR
            public readonly float distance;
            public readonly int collisions;
            public readonly float distanceWeight;
            public readonly float collisionWeight;
#endif
            
            public OcclusionResultData(float weightLowFreq, float weightHighFreq
#if UNITY_EDITOR
                , float distance
                , int collisions
                , float distanceWeight
                , float collisionWeight
#endif
            ) {
                this.weightLowFreq = weightLowFreq;
                this.weightHighFreq = weightHighFreq;
#if UNITY_EDITOR
                this.distance = distance;
                this.collisions = collisions;
                this.distanceWeight = distanceWeight;
                this.collisionWeight = collisionWeight;          
#endif
            }
        }
        
        private readonly struct SoundResultData {
            
            public readonly float pitch;
            public readonly float attenuationDistance;
            public readonly float lpCutoff;
            public readonly float hpCutoff;
            
            public SoundResultData(float pitch, float attenuationDistance, float lpCutoff, float hpCutoff) {
                this.pitch = pitch;
                this.attenuationDistance = attenuationDistance;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        #endregion DATA TYPES
        
        #region JOBS
        
        [BurstCompile]
        private struct CalculateFadeJob : IJobParallelFor {

            [ReadOnly] public NativeArray<FadeCalculateData> fadeCalculateDataArray;
            [ReadOnly] public float scaledTime;
            [ReadOnly] public float unscaledTime;
            
            [WriteOnly] public NativeArray<FadeResultData> fadeResultArray;
            
            public void Execute(int index) {
                var data = fadeCalculateDataArray[index];
                
                float currentTime = data.affectedByTimescale ? scaledTime : unscaledTime;
                float t = data.fade > 0f 
                    ? math.clamp((currentTime - data.startTime) / data.fade, 0f, 1f)
                    : 1f;

                fadeResultArray[index] = new FadeResultData(data.id, math.lerp(data.volume0, data.volume1, t), t);
            }
        }
        
        [BurstCompile]
        private struct WriteVolumeWeightDataJob : IJobParallelFor {

            [ReadOnly] public NativeArray<WeightData> weightArray;
            [ReadOnly] public int priority;
            [ReadOnly] public int volumeCount;
            [ReadOnly] public int volumeIndex;
            
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<VolumeWeightData> volumeWeightDataArray;

            public void Execute(int index) {
                var data = weightArray[index];
                volumeWeightDataArray[volumeIndex + volumeCount * index] = new VolumeWeightData(data.volumeId, priority, data.weight);
            }
        }
        
        [BurstCompile]
        private struct CalculateListenerVolumeJob : IJob {

            [ReadOnly] public NativeArray<VolumeWeightData> volumeWeightDataArray;
            [ReadOnly] public NativeArray<VolumeProcessData> volumeProcessDataArray;
            [ReadOnly] public int volumeCount;

            public NativeHashMap<int, float> listenerVolumeIdToWeightMap;
            public NativeArray<float> occlusionListenerResultArray;

            public void Execute() {
                int topPriority = int.MinValue;
                
                for (int i = 0; i < volumeCount; i++) {
                    var weightData = volumeWeightDataArray[i];
                    if (weightData.weight <= 0f || !AudioParameter.ListenerOcclusion.InMask(volumeProcessDataArray[i].mask)) continue;

                    topPriority = math.max(topPriority, weightData.priority);
                }
                
                float weightSum = 0f;
                float occlusionMul = 1f;
                
                for (int i = 0; i < volumeCount; i++) {
                    var weightData = volumeWeightDataArray[i];
                    var processData = volumeProcessDataArray[i];

                    listenerVolumeIdToWeightMap[weightData.volumeId] = 
                        math.max(weightData.weight, listenerVolumeIdToWeightMap.TryGetValue(weightData.volumeId, out float w) ? w : 0f);
                    
                    if (weightData.weight <= 0f || weightData.priority < topPriority ||
                        !AudioParameter.ListenerOcclusion.InMask(processData.mask)) 
                    {
                        continue;
                    }

                    weightSum += weightData.weight;
                    occlusionMul += weightData.weight * processData.occlusionListener;
                }
                
                occlusionMul = weightSum > 0f ? occlusionMul / weightSum : 1f;
                occlusionListenerResultArray[0] = math.lerp(1f, occlusionMul, math.clamp(weightSum, 0f, 1f));
            }
        }

        [BurstCompile]
        private struct CalculateVolumeResultDataJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<VolumeWeightData> volumeWeightDataArray;
            [ReadOnly] public NativeArray<VolumeProcessData> volumeProcessDataArray;
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray;
            [ReadOnly] public NativeArray<float> occlusionListenerResultArray;
            [ReadOnly] public NativeHashMap<int, float> listenerVolumeIdToWeightMap;
            [ReadOnly] public int volumeCount;
            [ReadOnly] public float attenuationDefault;
            
            [WriteOnly] public NativeArray<VolumeResultData> resultArray;

            public void Execute(int index) {
                if ((soundOptionsArray[index] & AudioOptions.AffectedByVolumes) == 0) {
                    resultArray[index] = new VolumeResultData(1f, 1f, attenuationDefault, LpCutoffUpperBound, HpCutoffLowerBound);
                    return;
                }
                
                int topPriorityOcclusionSound = int.MinValue;
                int topPriorityPitch = int.MinValue;
                int topPriorityAttenuation = int.MinValue;
                int topPriorityLp = int.MinValue;
                int topPriorityHp = int.MinValue;
                
                for (int i = 0; i < volumeCount; i++) {
                    var weightData = volumeWeightDataArray[(index + 1) * volumeCount + i];
                    if (weightData.weight <= 0f) continue;

                    int mask = volumeProcessDataArray[i].mask;
                    
                    if (AudioParameter.SoundOcclusion.InMask(mask)) topPriorityOcclusionSound = math.max(topPriorityOcclusionSound, weightData.priority);
                    if (AudioParameter.Pitch.InMask(mask)) topPriorityPitch = math.max(topPriorityPitch, weightData.priority);
                    if (AudioParameter.Attenuation.InMask(mask)) topPriorityAttenuation = math.max(topPriorityAttenuation, weightData.priority);
                    if (AudioParameter.LpCutoff.InMask(mask)) topPriorityLp = math.max(topPriorityLp, weightData.priority);
                    if (AudioParameter.HpCutoff.InMask(mask)) topPriorityHp = math.max(topPriorityHp, weightData.priority);
                }
                
                float occlusionSound = 0f;
                float pitch = 0f;
                float attenuation = 0f;
                float lpCutoff = 0f;
                float hpCutoff = 0f;
                
                float occlusionSoundWeightSum = 0f;
                float pitchWeightSum = 0f;
                float attenuationWeightSum = 0f;
                float lpCutoffWeightSum = 0f;
                float hpCutoffWeightSum = 0f;
                
                for (int i = 0; i < volumeCount; i++) {
                    var weightData = volumeWeightDataArray[(index + 1) * volumeCount + i];
                    if (weightData.weight <= 0f) continue;
                    
                    var processData = volumeProcessDataArray[i];
                    int mask = processData.mask;

                    float listenerWeight = listenerVolumeIdToWeightMap.TryGetValue(weightData.volumeId, out float lw) ? lw : 0f;
                    float w = weightData.weight * math.lerp(1f, listenerWeight, processData.listenerPresence);
                    
                    if (weightData.priority >= topPriorityOcclusionSound && AudioParameter.SoundOcclusion.InMask(mask)) {
                        occlusionSound += w * processData.occlusionSound;
                        occlusionSoundWeightSum += w;
                    }
                    
                    if (weightData.priority >= topPriorityPitch && AudioParameter.Pitch.InMask(mask)) {
                        pitch += w * processData.pitch;
                        pitchWeightSum += w;
                    }
                    
                    if (weightData.priority >= topPriorityAttenuation && AudioParameter.Attenuation.InMask(mask)) {
                        attenuation += w * processData.attenuation;
                        attenuationWeightSum += w;
                    }
                    
                    if (weightData.priority >= topPriorityLp && AudioParameter.LpCutoff.InMask(mask)) {
                        lpCutoff += w * processData.lpCutoff;
                        lpCutoffWeightSum += w;
                    }
                    
                    if (weightData.priority >= topPriorityHp && AudioParameter.HpCutoff.InMask(mask)) {
                        hpCutoff += w * processData.hpCutoff;
                        hpCutoffWeightSum += w;
                    }
                }

                occlusionSound = occlusionSoundWeightSum > 0f 
                    ? math.lerp(1f, occlusionSound / occlusionSoundWeightSum, math.clamp(occlusionSoundWeightSum, 0f, 1f))
                    : 1f;

                pitch = pitchWeightSum > 0f
                    ? math.lerp(1f, pitch / pitchWeightSum, math.clamp(pitchWeightSum, 0f, 1f))
                    : 1f;
                
                attenuation = attenuationWeightSum > 0f
                    ? math.lerp(attenuationDefault, attenuation / attenuationWeightSum, math.clamp(attenuationWeightSum, 0f, 1f))
                    : attenuationDefault;
                
                lpCutoff = lpCutoffWeightSum > 0f
                    ? math.lerp(LpCutoffUpperBound, lpCutoff / lpCutoffWeightSum, math.clamp(lpCutoffWeightSum, 0f, 1f))
                    : LpCutoffUpperBound;
                
                hpCutoff = hpCutoffWeightSum > 0f
                    ? math.lerp(HpCutoffLowerBound, hpCutoff / hpCutoffWeightSum, math.clamp(hpCutoffWeightSum, 0f, 1f))
                    : HpCutoffLowerBound;
                
                resultArray[index] = new VolumeResultData(
                    occlusionSound * occlusionListenerResultArray[0],
                    pitch, 
                    attenuation, 
                    lpCutoff, 
                    hpCutoff
                );
            }
        }
        
        [BurstCompile]
        private struct WriteDefaultVolumeResultDataJob : IJobParallelFor {
            
            [ReadOnly] public float attenuationDefault;
            [WriteOnly] public NativeArray<VolumeResultData> resultArray;
            
            public void Execute(int index) {
                resultArray[index] = new VolumeResultData(
                    1f,
                    1f, 
                    attenuationDefault, 
                    LpCutoffUpperBound, 
                    HpCutoffLowerBound
                );
            }
        }
        
        [BurstCompile]
        private struct PrepareRaycastCommandsJob : IJobParallelForBatch {
            
            [ReadOnly] public NativeArray<float3> listenerAndSoundsPositionArray;
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray;
            [ReadOnly] public float3 up;
            [ReadOnly] public float raySector;
            [ReadOnly] public float rayOffset0;
            [ReadOnly] public float rayOffset1;
            [ReadOnly] public float minDistance;
            [ReadOnly] public float maxDistance;
            [ReadOnly] public int layerMask;
            
            [WriteOnly] public NativeArray<RaycastCommand> raycastCommands;

            public void Execute(int startIndex, int count) {
                int soundIndex = startIndex / count;
                
                if ((soundOptionsArray[soundIndex] & AudioOptions.ApplyOcclusion) == 0) return;
                
                var listenerPos = listenerAndSoundsPositionArray[0];
                var soundPos = listenerAndSoundsPositionArray[soundIndex + 1];
                
                float distanceSqr = math.lengthsq(listenerPos - soundPos);
                if (distanceSqr < minDistance * minDistance || distanceSqr > maxDistance * maxDistance) return;
                
                var dir = math.normalize(listenerPos - soundPos);
                float distance = math.length(listenerPos - soundPos);
                var rot = quaternion.LookRotation(dir, up);
                float offset = math.lerp(rayOffset0, rayOffset1, GetRelativeDistance(distance, minDistance, maxDistance));
                
                for (int i = 0; i < count; i++) {
                    raycastCommands[startIndex + i] = new RaycastCommand(
                        from: soundPos + math.mul(rot, offset * GetOcclusionOffset(i, count, raySector)),
                        dir,
                        new QueryParameters(layerMask, hitMultipleFaces: false, hitTriggers: QueryTriggerInteraction.Ignore, hitBackfaces: false),
                        distance
                    );
                }
            }
        }
        
        [BurstCompile]
        private struct CalculateOcclusionWeightsJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<float3> listenerAndSoundsPositionArray;
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray;
            [ReadOnly] public NativeArray<RaycastHit> hitsArray;
            [ReadOnly] public int rays;
            [ReadOnly] public int maxHits;
            [ReadOnly] public float minDistance;
            [ReadOnly] public float maxDistance;
            [ReadOnly] public float globalOcclusionWeight;
            [ReadOnly] public float distanceLowFreqWeight;
            [ReadOnly] public float distanceHighFreqWeight;
            [ReadOnly] public float collisionLowFreqWeight;
            [ReadOnly] public float collisionHighFreqWeight;
            [ReadOnly] public EasingType distanceLowFreqEasing;
            [ReadOnly] public EasingType distanceHighFreqEasing;
            
            public NativeArray<OcclusionResultData> resultArray;

            public void Execute(int index) {
                if ((soundOptionsArray[index] & AudioOptions.ApplyOcclusion) == 0) return;
                
                var listenerPos = listenerAndSoundsPositionArray[0];
                var soundPos = listenerAndSoundsPositionArray[index + 1];
                
                float distanceSqr = math.lengthsq(listenerPos - soundPos);
                if (distanceSqr < minDistance * minDistance) return;

                if (distanceSqr > maxDistance * maxDistance) {
                    resultArray[index] = new OcclusionResultData(1f, 1f
#if UNITY_EDITOR
                    , math.length(listenerPos - soundPos)
                    , 0
                    , 1f
                    , 1f
#endif
                        );
                    return;
                }
                
                float weightSum = 0f;
#if UNITY_EDITOR
                int totalCollisions = 0;
#endif
                
                for (int j = 0; j < rays; j++) {
                    int collisions = 0;
                    
                    for (int r = 0; r < maxHits; r++) {
                        var hit = hitsArray[index * rays + j * maxHits + r];
                        if (hit.colliderInstanceID == 0) break;
                        
                        collisions++;

#if UNITY_EDITOR
                        totalCollisions++;
#endif
                    }

                    weightSum += (float) collisions / maxHits;
                }

                float distance = math.length(listenerPos - soundPos);
                
                float distanceWeight = GetRelativeDistance(distance, minDistance, maxDistance);
                float collisionWeight = math.clamp(weightSum / rays, 0f, 1f);
                
                float wLow = math.clamp((distanceLowFreqEasing.Evaluate(distanceWeight) * distanceLowFreqWeight + collisionWeight * collisionLowFreqWeight) * globalOcclusionWeight, 0f, 1f);
                float wHigh = math.clamp((distanceHighFreqEasing.Evaluate(distanceWeight) * distanceHighFreqWeight + collisionWeight * collisionHighFreqWeight) * globalOcclusionWeight, 0f, 1f);
                
                resultArray[index] = new OcclusionResultData(wLow, wHigh
#if UNITY_EDITOR
                    , distance
                    , totalCollisions
                    , distanceWeight
                    , collisionWeight
#endif
                );
            }
        }
        
        [BurstCompile]
        private struct CalculateResultSoundJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<SoundData> soundDataArray; 
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray; 
            [ReadOnly] public NativeArray<VolumeResultData> volumeResultDataArray; 
            [ReadOnly] public NativeArray<OcclusionResultData> occlusionResultDataArray;
            [ReadOnly] public float timescale;
            [ReadOnly] public float dt;
            [ReadOnly] public float smoothing;
            [ReadOnly] public float lpCutoff;
            [ReadOnly] public float hpCutoff;
            [ReadOnly] public EasingType lpCutoffEasing;
            [ReadOnly] public EasingType hpCutoffEasing;
            
            [WriteOnly] public NativeArray<SoundResultData> resultArray;
            
            public void Execute(int index) {
                var soundData = soundDataArray[index];
                var options = soundOptionsArray[index];
                var volumeData = volumeResultDataArray[index];
                var occlusionData = occlusionResultDataArray[index];

                float pitch = volumeData.pitch * soundData.pitchMul;
                if ((options & AudioOptions.AffectedByTimeScale) == AudioOptions.AffectedByTimeScale) {
                    pitch *= timescale;
                }

                float attenuationDistance = soundData.attenuationMul * volumeData.attenuation;

                float lpCutoffT = lpCutoffEasing.Evaluate(occlusionData.weightLowFreq * volumeData.occlusion * soundData.spatialBlend);
                float hpCutoffT = hpCutoffEasing.Evaluate(occlusionData.weightHighFreq * volumeData.occlusion * soundData.spatialBlend);
                
                float lpCutoffBound = math.min(volumeData.lpCutoff, math.lerp(LpCutoffUpperBound, lpCutoff, lpCutoffT));
                float hpCutoffBound = math.max(volumeData.hpCutoff, math.lerp(HpCutoffLowerBound, hpCutoff, hpCutoffT));
                
                lpCutoffBound = dt > 0f ? soundData.lpCutoff.SmoothExpNonZero(lpCutoffBound, soundData.occlusionFlag * smoothing, dt) : lpCutoffBound;
                hpCutoffBound = dt > 0f ? soundData.hpCutoff.SmoothExpNonZero(hpCutoffBound, soundData.occlusionFlag * smoothing, dt) : hpCutoffBound;
                
                resultArray[index] = new SoundResultData(pitch, attenuationDistance, lpCutoffBound, hpCutoffBound);
            }
        }
        
        #endregion JOBS
        
        #region EDITOR
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showSoundsGizmo;
        [SerializeField] private bool _showSoundsDebugInfo;
        [SerializeField] private bool _showVolumeInfo;
        [SerializeField] private bool _showOcclusionInfo;
        [SerializeField] private string[] _showSoundsNameFilters;
        [SerializeField] private string[] _showOcclusionNameFilters;

        bool IAudioPool.ShowGizmo => _showSoundsGizmo;
        
        private readonly Dictionary<int, Color> _debugColors = new();

        private void OnValidate() {
            if (_maxDistance < _minDistance) _maxDistance = _minDistance;
            
            if (Application.isPlaying) FetchIncludeMixerGroupsFromVolumes();
        }

        private void CreateDebugColor(int id, string clipName) {
            int hash = clipName.GetHashCode();
            var rand = new System.Random(hash);
            var color = new Color(rand.Next(256) / 256f, rand.Next(256) / 256f, rand.Next(256) / 256f);
            
            _debugColors[id] = color;
        }

        private Color GetDebugColor(int id) {
            return _debugColors.TryGetValue(id, out var color) ? color : Color.magenta;
        }

        private void RemoveDebugColor(int id) {
            _debugColors.Remove(id);
        }
#endif
        
        #endregion EDITOR
    }
    
}