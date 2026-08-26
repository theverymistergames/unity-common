using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MisterGames.Common.Colors;
using MisterGames.Common.Easing;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Jobs;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-999)]
    public sealed class AudioPool : MonoBehaviour, IAudioPool, IUpdate {

        [Header("Audio Element")]
        [SerializeField] private AudioElement _prefab;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;
        [SerializeField] [Min(0f)] private float _attenuationDistance = 50f;
        [SerializeField] [Min(0f)] private float _audioParametersSmoothing = 3f;
        [SerializeField] [Min(0f)] private float _shuffleClipsOrderLifetime = 60f;
        
        [Header("Mixers")]
        [SerializeField] private AudioMixerGroup _defaultMixerGroup;
        [SerializeField] private AudioMixerGroup[] _includeMixerGroupsForVolumes;
        [SerializeField] private AudioMixerGroup[] _ignoreZeroTimescaleForMixerGroups;
        [SerializeField] private AudioMixer[] _reverbMixers;

        [Header("Audio Volumes")]
        [SerializeField] private bool _enableVolumes = true;
        [SerializeField] private bool _includeDefaultMixerGroupsForVolumes = true;
        
        [Header("Reverb Volumes")]
        [SerializeField] private bool _enableReverbVolumes = true;
        [SerializeField] [Min(0f)] private float _reverbParamsSmoothing = 3f;
        [SerializeField] private string _reverbParamPrefix = "Reverb_";
        
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
        private const int MinBatch = 16;
        private const float HpCutoffLowerBound = 10f;
        private const float LpCutoffUpperBound = 22000f;
        
        private const string ReverbParamRoom = "Room";
        private const string ReverbParamRoomHf = "RoomHF";
        private const string ReverbParamRoomLf = "RoomLF";
        private const string ReverbParamDecayTime = "DecayTime";
        private const string ReverbParamDecayHfRatio = "DecayHFRatio";
        private const string ReverbParamReflectionsLevel = "Reflections";
        private const string ReverbParamReflectionsDelay = "ReflectDelay";
        private const string ReverbParamReverbLevel = "Reverb";
        private const string ReverbParamReverbDelay = "ReverbDelay";
        private const string ReverbParamHfReference = "HFReference";
        private const string ReverbParamLfReference = "LFReference";
        private const string ReverbParamDiffusion = "Diffusion";
        private const string ReverbParamDensity = "Density";
        
        private static readonly float3 Up = Vector3.up;
        private static readonly float3 Forward = Vector3.forward;
        private static readonly float3 Right = Vector3.right;
        
        private readonly Dictionary<int, IndexData> _clipsHashToLastIndexMap = new();
        private float _lastClipShufflesCheckTime;
        
        private readonly Dictionary<AttachKey, int> _attachKeyToHandleIdMap = new();
        private readonly Dictionary<int, AttachKey> _handleIdToAttachKeyMap = new();
        private readonly Dictionary<int, IAudioElement> _handleIdToAudioElementMap = new();
        private int _lastHandleId;
        
        private readonly List<IAudioElement> _elements = new();
        private readonly Dictionary<int, int> _handleIdToIndexMap = new();
        private TransformAccessArray _transformAccessArray;
        private IAudioElement[] _elementsBuffer = Array.Empty<IAudioElement>();
        
        private readonly Dictionary<int, FadeData> _fadeInDataMap = new();
        private readonly Dictionary<int, FadeData> _fadeOutDataMap = new();
        private readonly Dictionary<int, IAudioElement> _releaseElementsMap = new();
        
        private readonly Dictionary<AudioListener, ListenerData> _audioListenersMap = new();
        private Transform _listenerTransform;
        private Transform _listenerUp;

        private readonly Dictionary<EntityId, IAudioVolume> _audioVolumes = new();
        private IAudioVolume[] _volumesBuffer = Array.Empty<IAudioVolume>();

        private readonly List<ImmediateVolumeData> _immediateVolumes = new();
        private readonly Dictionary<int, float> _immediateListenerWeights = new();
        private float _immediateListenerOcclusion = 1f;
        private int _immediateVolumesFrame = -1;
        private RaycastHit[] _immediateHitsBuffer = Array.Empty<RaycastHit>();
        private readonly HashSet<EntityId> _includeMixerGroupsForVolumesSet = new();
        private readonly HashSet<EntityId> _ignoreZeroTimescaleForMixerGroupsSet = new();
        
        private readonly Dictionary<EntityId, IReverbVolume> _reverbVolumes = new();
        private string[] _reverbParamNames;
        private ReverbSettingsData _smoothedReverbSettings;
        
        private Transform _transform;
        private float _lastTimeScale;
        private bool _checkPause;
        private bool _resumeSoundsAfterFocus;
        private float _globalOcclusionWeight = 1f;
        
        // 0 - scaled time, 1 - unscaled above ts = 0, 2 - unscaled focused
        private readonly float[] _internalTime = new float[3];
        
        private void Awake() {
            Main = this;

            _transform = transform;
            _lastTimeScale = Time.timeScale;
            _transformAccessArray = new TransformAccessArray(64);
            
            CreateReverbParamNames();
            FetchIncludeMixerGroupsFromVolumes();
            FetchIgnoreZeroTimescaleMixerGroups();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnApplicationFocus(bool hasFocus) {
#if UNITY_EDITOR
            hasFocus = true;
#endif

            if (hasFocus) _resumeSoundsAfterFocus = true;
            else PauseSoundsOnFocusLost();
        }

        private void OnApplicationPause(bool pauseStatus) {
            // Not every platform reports going into background as a focus change.
            OnApplicationFocus(!pauseStatus);
        }

        private void OnDestroy() {
            _clipsHashToLastIndexMap.Clear();
            
            _attachKeyToHandleIdMap.Clear();
            _handleIdToAttachKeyMap.Clear();
            _handleIdToAudioElementMap.Clear();
            
            _elements.Clear();
            _handleIdToIndexMap.Clear();
            _immediateVolumes.Clear();
            _immediateListenerWeights.Clear();
            _elementsBuffer = Array.Empty<IAudioElement>();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

            _fadeInDataMap.Clear();
            _fadeOutDataMap.Clear();
            _releaseElementsMap.Clear();
            
            _audioListenersMap.Clear();
            _audioVolumes.Clear();
            _includeMixerGroupsForVolumesSet.Clear();
            
            Main = null;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        private void CreateReverbParamNames() {
            _reverbParamNames ??= new string[13];
            
            _reverbParamNames[0] = $"{_reverbParamPrefix}{ReverbParamRoom}"; 
            _reverbParamNames[1] = $"{_reverbParamPrefix}{ReverbParamRoomHf}"; 
            _reverbParamNames[2] = $"{_reverbParamPrefix}{ReverbParamRoomLf}"; 
            _reverbParamNames[3] = $"{_reverbParamPrefix}{ReverbParamDecayTime}"; 
            _reverbParamNames[4] = $"{_reverbParamPrefix}{ReverbParamDecayHfRatio}"; 
            _reverbParamNames[5] = $"{_reverbParamPrefix}{ReverbParamReflectionsLevel}"; 
            _reverbParamNames[6] = $"{_reverbParamPrefix}{ReverbParamReflectionsDelay}"; 
            _reverbParamNames[7] = $"{_reverbParamPrefix}{ReverbParamReverbLevel}"; 
            _reverbParamNames[8] = $"{_reverbParamPrefix}{ReverbParamReverbDelay}"; 
            _reverbParamNames[9] = $"{_reverbParamPrefix}{ReverbParamHfReference}"; 
            _reverbParamNames[10] = $"{_reverbParamPrefix}{ReverbParamLfReference}"; 
            _reverbParamNames[11] = $"{_reverbParamPrefix}{ReverbParamDiffusion}"; 
            _reverbParamNames[12] = $"{_reverbParamPrefix}{ReverbParamDensity}"; 
        }
        
        private void FetchIncludeMixerGroupsFromVolumes() {
            _includeMixerGroupsForVolumesSet.Clear();
            
            for (int i = 0; i < _includeMixerGroupsForVolumes.Length; i++) {
                _includeMixerGroupsForVolumesSet.Add(_includeMixerGroupsForVolumes[i].GetEntityId());
            }

            if (_includeDefaultMixerGroupsForVolumes && _defaultMixerGroup != null) {
                _includeMixerGroupsForVolumesSet.Add(_defaultMixerGroup.GetEntityId());
            }
        }
        
        private void FetchIgnoreZeroTimescaleMixerGroups() {
            _ignoreZeroTimescaleForMixerGroupsSet.Clear();
            
            for (int i = 0; i < _ignoreZeroTimescaleForMixerGroups.Length; i++) {
                _ignoreZeroTimescaleForMixerGroupsSet.Add(_ignoreZeroTimescaleForMixerGroups[i].GetEntityId());
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

        public bool TryGetListenerPosition(out Vector3 position) {
            if (_listenerTransform == null) {
                position = default;
                return false;
            }

            position = _listenerTransform.position;
            return true;
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

        public void RegisterAudioVolume(IAudioVolume volume) {
            _audioVolumes[volume.Id] = volume;
        }

        public void UnregisterAudioVolume(IAudioVolume volume) {
            _audioVolumes.Remove(volume.Id);
        }

        public void RegisterReverbVolume(IReverbVolume volume) {
            _reverbVolumes[volume.Id] = volume;
        }

        public void UnregisterReverbVolume(IReverbVolume volume) {
            _reverbVolumes.Remove(volume.Id);
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
            float attenuationMul = 1f,
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
            
            if (audioElement == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: cannot create audio element, prefab pool is disposed.");          
#endif          
                return AudioHandle.Invalid;
            }

#if UNITY_EDITOR
            CreateDebugColor(id, clip);
            LogPlaySound(id, clip, options);
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
                attenuationMul,
                options,
                mixerGroup,
                cancellationToken
            );
            
            audioElement.SpatialBlend = spatialBlend;

            if (fadeIn > 0f) {
                int timeSource = affectedByTimeScale ? 0 : audioElement.IgnoreZeroTimescale ? 2 : 1;
                
                _fadeInDataMap[id] = new FadeData(
                    _internalTime[timeSource], 
                    fadeIn, 
                    volume,
                    timeSource
                );   
            }

            AddAudioElement(id, audioElement);

            RestartAudioSource(
                audioElement.Source, clip, mixerGroup,
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f),
                spatialBlend, clipTime, loop
            );

            // After the source restart, so that computed pitch is not overwritten by the raw one.
            ProcessSoundImmediate(audioElement);

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
            float attenuationMul = 1f,
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

            if (audioElement == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"{nameof(AudioPool)}: cannot create audio element, prefab pool is disposed.");          
#endif          
                return AudioHandle.Invalid;
            }
            
            var attachKey = new AttachKey(attachTo.GetEntityId(), attachId);

#if UNITY_EDITOR
            CreateDebugColor(id, clip);
            LogPlaySound(id, clip, options);
#endif
            
            normalizedTime = Mathf.Clamp01(normalizedTime);
            mixerGroup = mixerGroup == null ? _defaultMixerGroup : mixerGroup;
            
            float clipLength = clip.length;
            float clipTime = normalizedTime * clipLength;
            
            if (attachId != 0) {
                if (_attachKeyToHandleIdMap.TryGetValue(attachKey, out int oldId)) {
                    ReleaseSound(oldId, immediate: false);
                }
                
                _attachKeyToHandleIdMap[attachKey] = id;
                _handleIdToAttachKeyMap[id] = attachKey;
            }
            
            InitializeAudioElement(
                audioElement,
                id,
                pitch,
                fadeOut,
                clipLength,
                clipTime,
                attenuationMul,
                options,
                mixerGroup,
                cancellationToken
            );
            
            audioElement.SpatialBlend = spatialBlend;
            
            AddAudioElement(id, audioElement);
            
            bool loop = (options & AudioOptions.Loop) != 0;
            bool affectedByTimeScale = (options & AudioOptions.AffectedByTimeScale) != 0;
            
            if (fadeIn > 0f) {
                int timeSource = affectedByTimeScale ? 0 : audioElement.IgnoreZeroTimescale ? 2 : 1;
                
                _fadeInDataMap[id] = new FadeData(
                    _internalTime[timeSource], 
                    fadeIn, 
                    volume,
                    timeSource
                );   
            }

            RestartAudioSource(
                audioElement.Source, clip, mixerGroup,
                fadeIn, volume, pitch * (affectedByTimeScale ? Time.timeScale : 1f),
                spatialBlend, clipTime, loop
            );

            // After the source restart, so that computed pitch is not overwritten by the raw one.
            ProcessSoundImmediate(audioElement);

            return new AudioHandle(this, id);
        }
        
        private void AddAudioElement(int id, IAudioElement e) {
            _handleIdToAudioElementMap[id] = e;
            _handleIdToIndexMap[id] = _elements.Count;
            _elements.Add(e);
            _transformAccessArray.Add(e.Transform);
        }

        private bool RemoveAudioElement(int id, out IAudioElement e) {
            if (!_handleIdToAudioElementMap.Remove(id, out e)) return false;
            if (!_handleIdToIndexMap.Remove(id, out int index)) return true;

            int last = _elements.Count - 1;
            
            if (index != last) {
                var moved = _elements[last];
                _elements[index] = moved;
                _handleIdToIndexMap[moved.Id] = index;
            }
            
            _elements.RemoveAt(last);
            _transformAccessArray.RemoveAtSwapBack(index);
            
            return true;
        }

        private void WriteMixerGroupFlags(IAudioElement e) {
            var mixerGroupId = e.MixerGroupId;
            
            e.MixerGroupAffectedByVolumes = mixerGroupId == EntityId.None || 
                                            _includeMixerGroupsForVolumesSet.Contains(mixerGroupId);
            
            e.IgnoreZeroTimescale = _ignoreZeroTimescaleForMixerGroupsSet.Contains(mixerGroupId);
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
            float attenuationMul,
            AudioOptions options,
            AudioMixerGroup mixerGroup,
            CancellationToken cancellationToken) 
        {
            audioElement.Id = id;
            audioElement.MixerGroupId = mixerGroup == null ? EntityId.None : mixerGroup.GetEntityId();
            audioElement.AudioPool = this;

            audioElement.IsPaused = false;
            audioElement.IsPausedByFocus = false;
            audioElement.AudioOptions = options;
            audioElement.PitchMul = pitch;
            audioElement.AttenuationMul = attenuationMul;

            audioElement.ClipLength = clipLength;
            audioElement.ClipTime = clipTime;
            audioElement.FadeOut = fadeOut < 0f ? _fadeOut : fadeOut;
            audioElement.OcclusionFlag = 0;

            audioElement.LowPass.lowpassResonanceQ = _qLow;
            audioElement.HighPass.highpassResonanceQ = _qHigh;
            audioElement.Source.maxDistance = _attenuationDistance;

            audioElement.LpCutoff = LpCutoffUpperBound;
            audioElement.HpCutoff = HpCutoffLowerBound;
            audioElement.LowPass.cutoffFrequency = LpCutoffUpperBound;
            audioElement.HighPass.cutoffFrequency = HpCutoffLowerBound;
            
            WriteMixerGroupFlags(audioElement);

            audioElement.CancellationToken = cancellationToken;
        }
        
        private IAudioElement GetAudioElementAtWorldPosition(Vector3 position) {
            return PrefabPool.Main.Get(_prefab, position, Quaternion.identity, _transform);
        }

        private IAudioElement GetAudioElementAttached(Transform parent, Vector3 localPosition = default) {
            return PrefabPool.Main?.Get(_prefab, parent.TransformPoint(localPosition), Quaternion.identity, parent);
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
            
            _clipsHashToLastIndexMap[hash] = new IndexData(mask, startIndex, index, _internalTime[2]);
            
            return index;
        }
        
        public AudioHandle GetAudioHandle(Transform attachedTo, int hash) {
            return _attachKeyToHandleIdMap.TryGetValue(new AttachKey(attachedTo.GetEntityId(), hash), out int id) && 
                   _handleIdToAudioElementMap.ContainsKey(id)
                ? new AudioHandle(this, id)
                : AudioHandle.Invalid;
        }
        
        void IAudioPool.ReleaseAudioHandle(int handleId, bool immediate) {
            ReleaseSound(handleId, immediate);
        }

        void IAudioPool.ReleaseAll(bool immediate) {
            ReleaseSounds(timescaledOnly: false, immediate);
        }

        void IAudioPool.ReleaseTimescaled(bool immediate) {
            ReleaseSounds(timescaledOnly: true, immediate);
        }

        private void ReleaseSounds(bool timescaledOnly, bool immediate) {
            int count = _handleIdToAudioElementMap.Count + (immediate ? _fadeOutDataMap.Count : 0);
            if (count == 0) return;

            var handleIds = new NativeArray<int>(count, Allocator.Temp);
            int handleCount = 0;

            foreach ((int handleId, var e) in _handleIdToAudioElementMap) {
                if (!timescaledOnly || GetTimeSource(e) < 2) handleIds[handleCount++] = handleId;
            }

            if (immediate) {
                foreach ((int handleId, var data) in _fadeOutDataMap) {
                    if (!timescaledOnly || data.timeSource < 2) handleIds[handleCount++] = handleId;
                }
            }

            for (int i = 0; i < handleCount; i++) {
                ReleaseSound(handleIds[i], immediate);
            }

            handleIds.Dispose();
        }

        private static int GetTimeSource(IAudioElement e) {
            return (e.AudioOptions & AudioOptions.AffectedByTimeScale) != 0 ? 0 : e.IgnoreZeroTimescale ? 2 : 1;
        }

        private void ReleaseSound(int handleId, bool immediate) {
            if (!RemoveAudioElement(handleId, out var e)) {
                if (immediate) StopFadeOutAndRelease(handleId);
                return;
            }

            if (_handleIdToAttachKeyMap.Remove(handleId, out var attachKey)) {
                _attachKeyToHandleIdMap.Remove(attachKey);
            }

            _fadeInDataMap.Remove(handleId);

            bool isNull = e.Source == null;

#if UNITY_EDITOR
            LogReleaseSound(handleId, e, immediate);
#endif
            
            if (immediate || isNull) {
                if (!isNull) PrefabPool.Main?.Release(e.Source);
            }
            else {
                int timeSource = GetTimeSource(e);

                _fadeOutDataMap[handleId] = new FadeData(
                    _internalTime[timeSource],
                    e.FadeOut,
                    e.Source.volume,
                    timeSource
                );

                _releaseElementsMap[handleId] = e;
            }

#if UNITY_EDITOR
            RemoveDebugColor(handleId);
#endif
        }

        private void StopFadeOutAndRelease(int handleId) {
            _fadeOutDataMap.Remove(handleId);

            if (_releaseElementsMap.Remove(handleId, out var e)) PrefabPool.Main?.Release(e.Source);
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
            ProcessInternalTime(out float dtScaled, out float dtUnscaled);
            ResumeSoundsAfterFocus();
            ProcessClipShuffles();
            ProcessFadeIn();
            ProcessFadeOutAndRelease();
            ProcessSounds(dtScaled, dtUnscaled);
            ProcessReverb(dtScaled);
        }

        private void ProcessInternalTime(out float dtScaled, out float dtUnscaled) {
            dtScaled = TimeSources.deltaTime;
            dtUnscaled = TimeSources.unscaledDeltaTime;
            float ts = Time.timeScale;

            // Audio sources only need pause state checks while timescale is zero
            // and on the frame it becomes non zero again.
            _checkPause = ts <= 0f || _lastTimeScale <= 0f;
            _lastTimeScale = ts;

            // scaled
            _internalTime[0] += dtScaled;

            // unscaled above ts = 0
            if (ts > 0f) _internalTime[1] += dtUnscaled;
            
            // unscaled focused
            _internalTime[2] += dtUnscaled;
        }
        
        private void ProcessClipShuffles() {
            float time = _internalTime[2];
            if (time < _lastClipShufflesCheckTime + _shuffleClipsOrderLifetime) return;

            _lastClipShufflesCheckTime = time;
            
            int count = _clipsHashToLastIndexMap.Count;
            var buffer = new NativeArray<IndexCheckData>(count, Allocator.Temp);
            int index = 0;
                
            foreach ((int hash, var data) in _clipsHashToLastIndexMap) {
                buffer[index++] = new IndexCheckData(hash, data.time);
            }

            for (int i = 0; i < count; i++) {
                var data = buffer[i];
                    
                if (time - data.time > _shuffleClipsOrderLifetime) {
                    _clipsHashToLastIndexMap.Remove(data.hash);
                }
            }
                
            buffer.Dispose();
        }

        private void ProcessFadeIn() {
            int fadeInCount = _fadeInDataMap.Count;
            if (fadeInCount == 0) return;

            // Fade math is a few flops per element and the volume write below is main thread bound anyway,
            // so scheduling a job here costs more than the work it does.
            var ids = ArrayPool<int>.Shared.Rent(fadeInCount);
            var datas = ArrayPool<FadeData>.Shared.Rent(fadeInCount);
            int index = 0;

            foreach ((int id, var data) in _fadeInDataMap) {
                ids[index] = id;
                datas[index++] = data;
            }

            for (int i = 0; i < fadeInCount; i++) {
                int id = ids[i];
                var data = datas[i];

                if (!_handleIdToAudioElementMap.TryGetValue(id, out var e)) {
                    _fadeInDataMap.Remove(id);
                    continue;
                }

                float t = GetFadeProgress(data);
                e.Source.volume = math.lerp(0f, data.volume, t);

                if (t >= 1f) _fadeInDataMap.Remove(id);
            }

            ArrayPool<int>.Shared.Return(ids);
            ArrayPool<FadeData>.Shared.Return(datas);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetFadeProgress(FadeData data) {
            return data.fade > 0f
                ? math.clamp((_internalTime[data.timeSource] - data.startTime) / data.fade, 0f, 1f)
                : 1f;
        }

        private void ProcessFadeOutAndRelease() {
            int fadeOutCount = _fadeOutDataMap.Count;
            if (fadeOutCount == 0) return;

            var ids = ArrayPool<int>.Shared.Rent(fadeOutCount);
            var datas = ArrayPool<FadeData>.Shared.Rent(fadeOutCount);
            int index = 0;

            foreach ((int id, var data) in _fadeOutDataMap) {
                ids[index] = id;
                datas[index++] = data;
            }

            var pool = PrefabPool.Main;
            float timescale = Time.timeScale;

            for (int i = 0; i < fadeOutCount; i++) {
                int id = ids[i];
                var data = datas[i];

                if (!_releaseElementsMap.TryGetValue(id, out var e) || e.Source == null) {
                    _fadeOutDataMap.Remove(id);
                    _releaseElementsMap.Remove(id);
                    continue;
                }

                float t = GetFadeProgress(data);
                e.Source.volume = math.lerp(data.volume, 0f, t);

                if (_checkPause && data.timeSource < 2) {
                    CheckPause(e, timescale);
                }

                if (t < 1f) continue;

                _fadeOutDataMap.Remove(id);
                _releaseElementsMap.Remove(id);

                pool?.Release(e.Transform);
            }

            ArrayPool<int>.Shared.Return(ids);
            ArrayPool<FadeData>.Shared.Return(datas);
        }

        private void ProcessSounds(float dtScaled, float dtUnscaled) {
            if (_audioListenersMap.Count == 0) {
                ReleaseFinishedSounds();
                return;
            }

            int soundCount = _elements.Count;

            if (soundCount == 0) {
                _globalOcclusionWeight = 1f;
                return;
            }

            float3 listenerPos = _listenerTransform.position;
            var listenerUp = _listenerUp.up;

            var soundDataArray = new NativeArray<SoundData>(soundCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var soundOptionsArray = new NativeArray<AudioOptions>(soundCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var positionArray = new NativeArray<float3>(soundCount + 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            positionArray[0] = listenerPos;

            // Sound positions are read in a job, so the fill loop below costs no native transform access.
            var readPositionsHandle = new ReadSoundPositionsJob { positions = positionArray }.Schedule(_transformAccessArray);
            JobHandle.ScheduleBatchedJobs();

            if (_elementsBuffer.Length < soundCount) _elementsBuffer = new IAudioElement[math.ceilpow2(soundCount)];
            var elements = _elementsBuffer;

            bool anyVolumeSound = false;

            for (int i = 0; i < soundCount; i++) {
                var e = _elements[i];
                elements[i] = e;

                var options = e.AudioOptions;
                if (!e.MixerGroupAffectedByVolumes) options &= ~AudioOptions.AffectedByVolumes;

                anyVolumeSound |= (options & AudioOptions.AffectedByVolumes) != 0;

                soundDataArray[i] = new SoundData(
                    e.Id, e.OcclusionFlag,
                    e.SpatialBlend, e.PitchMul, e.AttenuationMul, e.LpCutoff, e.HpCutoff
                );

                soundOptionsArray[i] = options;
            }

            readPositionsHandle.Complete();

            var occlusionResultArray = new NativeArray<OcclusionResultData>(soundCount, Allocator.TempJob);
            var occlusionCandidates = new NativeArray<OcclusionCandidate>(_applyOcclusion ? soundCount : 0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            int occlusionCandidateCount = _applyOcclusion
                ? CollectOcclusionCandidates(positionArray, soundOptionsArray, soundDataArray, soundCount, occlusionCandidates, occlusionResultArray)
                : 0;

            // Raycasts are the long pole here, so they are queued first and volume setup below overlaps them.
            var occlusionHandle = ScheduleOcclusion(
                positionArray, occlusionCandidates, occlusionCandidateCount, listenerUp, occlusionResultArray,
                out var raycastCommands, out var hits
            );

            JobHandle.ScheduleBatchedJobs();

            var volumesHandle = ScheduleAudioVolumes(
                positionArray, soundOptionsArray, soundCount, anyVolumeSound,
                out var volumeResultArray, out var volumeResources
            );

            var resultSoundArray = new NativeArray<SoundResultData>(soundCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var calculateResultJob = new CalculateResultSoundJob {
                soundDataArray = soundDataArray,
                soundOptionsArray = soundOptionsArray,
                volumeResultDataArray = volumeResultArray,
                volumesEnabled = volumeResources.created ? 1 : 0,
                occlusionResultDataArray = occlusionResultArray,
                timescale = Time.timeScale,
                dtScaled = dtScaled,
                dtUnscaled = dtUnscaled,
                smoothing = _audioParametersSmoothing,
                attenuationDefault = _attenuationDistance,
                lpCutoff = _cutoffLow,
                hpCutoff = _cutoffHigh,
                lpCutoffEasing = _cutoffLowFreqEasing,
                hpCutoffEasing = _cutoffHighFreqEasing,
                resultArray = resultSoundArray,
            };

            // Volumes and occlusion are independent, so the whole frame has a single sync point.
            calculateResultJob
                .Schedule(soundCount, JobExt.BatchFor(soundCount, MinBatch), JobHandle.CombineDependencies(volumesHandle, occlusionHandle))
                .Complete();

#if UNITY_EDITOR
            DrawOcclusionDebug(positionArray, soundDataArray, occlusionCandidates, occlusionCandidateCount, raycastCommands);
#endif

            float timescale = Time.timeScale;

            for (int i = 0; i < soundCount; i++) {
                var e = elements[i];
                elements[i] = null;

                var options = soundOptionsArray[i];

                if (e.CancellationToken.IsCancellationRequested || IsSoundFinished(e, options)) {
                    ReleaseSound(e.Id, immediate: false);
                    continue;
                }

                var resultData = resultSoundArray[i];

                if (_checkPause && !e.IgnoreZeroTimescale) {
                    CheckPause(e, timescale);
                }

#if UNITY_EDITOR
                LogSoundDebugInfo(e, occlusionResultArray[i], volumeResources.created ? volumeResultArray[i] : default, resultData);
#endif

                e.Source.pitch = resultData.pitch;
                e.Source.maxDistance = resultData.attenuationDistance;

                e.LowPass.cutoffFrequency = resultData.lpCutoff;
                e.HighPass.cutoffFrequency = resultData.hpCutoff;

                e.LpCutoff = resultData.lpCutoff;
                e.HpCutoff = resultData.hpCutoff;

                e.OcclusionFlag = 1;
            }

            soundDataArray.Dispose();
            soundOptionsArray.Dispose();
            positionArray.Dispose();

            occlusionResultArray.Dispose();
            occlusionCandidates.Dispose();
            if (raycastCommands.IsCreated) raycastCommands.Dispose();
            if (hits.IsCreated) hits.Dispose();

            volumeResultArray.Dispose();
            volumeResources.Dispose();
            resultSoundArray.Dispose();

            _globalOcclusionWeight = 1f;
        }

        /// <summary>
        /// Picks sounds that actually need raycasts, so that array sizes and the physics batch
        /// depend on the number of occluded sounds instead of the total sound count.
        /// </summary>
        private int CollectOcclusionCandidates(
            NativeArray<float3> positionArray,
            NativeArray<AudioOptions> soundOptionsArray,
            NativeArray<SoundData> soundDataArray,
            int soundCount,
            NativeArray<OcclusionCandidate> candidates,
            NativeArray<OcclusionResultData> occlusionResultArray)
        {
            var listenerPos = positionArray[0];
            float minDistanceSqr = _minDistance * _minDistance;
            float maxDistanceSqr = _maxDistance * _maxDistance;
            int count = 0;

            for (int i = 0; i < soundCount; i++) {
                if ((soundOptionsArray[i] & AudioOptions.ApplyOcclusion) == 0) continue;

                // Occlusion weights are multiplied by spatial blend down the pipeline,
                // so a 2d sound can never be affected by them and does not need to be traced.
                if (soundDataArray[i].spatialBlend <= 0f) continue;

                float distanceSqr = math.lengthsq(listenerPos - positionArray[i + 1]);
                if (distanceSqr <= 0f || distanceSqr < minDistanceSqr) continue;

                if (distanceSqr > maxDistanceSqr) {
                    occlusionResultArray[i] = new OcclusionResultData(1f, 1f
#if UNITY_EDITOR
                        , math.sqrt(distanceSqr)
                        , 0
                        , 1f
                        , 1f
#endif
                    );
                    continue;
                }

                candidates[count++] = new OcclusionCandidate(i, math.sqrt(distanceSqr));
            }

            return count;
        }

        /// <summary>
        /// Computes occlusion, volumes and filter cutoffs for a single sound right away,
        /// so that it starts with final values instead of waiting for the next pool update.
        /// Runs on the main thread without jobs: for one sound the scheduling overhead
        /// is far bigger than the math itself.
        /// </summary>
        private void ProcessSoundImmediate(IAudioElement e) {
            if (_audioListenersMap.Count == 0) {
                // To reset smoothed values
                e.OcclusionFlag = 0;
                return;
            }

            float3 listenerPos = _listenerTransform.position;
            float3 soundPos = e.Transform.position;

            var options = e.AudioOptions;
            if (!e.MixerGroupAffectedByVolumes) options &= ~AudioOptions.AffectedByVolumes;

            var volumeResult = CalculateVolumeResultImmediate(listenerPos, soundPos, options);
            var occlusionResult = CalculateOcclusionImmediate(listenerPos, soundPos, e.SpatialBlend, options);

            var soundData = new SoundData(
                e.Id, e.OcclusionFlag,
                e.SpatialBlend, e.PitchMul, e.AttenuationMul, e.LpCutoff, e.HpCutoff
            );

            // Zero delta time: no smoothing, the sound starts at its final values.
            var resultData = CalculateSoundResult(
                soundData, options, volumeResult, occlusionResult,
                Time.timeScale, dt: 0f, _audioParametersSmoothing,
                _cutoffLow, _cutoffHigh, _cutoffLowFreqEasing, _cutoffHighFreqEasing
            );

            e.Source.pitch = resultData.pitch;
            e.Source.maxDistance = resultData.attenuationDistance;

            e.LowPass.cutoffFrequency = resultData.lpCutoff;
            e.HighPass.cutoffFrequency = resultData.hpCutoff;

            e.LpCutoff = resultData.lpCutoff;
            e.HpCutoff = resultData.hpCutoff;

            e.OcclusionFlag = 1;
        }

        private OcclusionResultData CalculateOcclusionImmediate(float3 listenerPos, float3 soundPos, float spatialBlend, AudioOptions options) {
            if (!_applyOcclusion || (options & AudioOptions.ApplyOcclusion) == 0 || spatialBlend <= 0f) {
                return default;
            }

            float distanceSqr = math.lengthsq(listenerPos - soundPos);
            if (distanceSqr <= 0f || distanceSqr < _minDistance * _minDistance) return default;

            var profile = GetOcclusionProfile();
            float distance = math.sqrt(distanceSqr);

            if (distanceSqr > _maxDistance * _maxDistance) {
                return new OcclusionResultData(1f, 1f
#if UNITY_EDITOR
                    , distance
                    , 0
                    , 1f
                    , 1f
#endif
                );
            }

            var dir = (listenerPos - soundPos) / distance;
            var rot = quaternion.LookRotation(dir, _listenerUp.up);
            float offset = math.lerp(_rayOffset0, _rayOffset1, GetRelativeDistance(distance, _minDistance, _maxDistance));
            float raySector = 360f / _rays;

            if (_immediateHitsBuffer.Length < _maxHits) _immediateHitsBuffer = new RaycastHit[math.ceilpow2(_maxHits)];

            int collisions = 0;

            for (int i = 0; i < _rays; i++) {
                float3 from = soundPos + math.mul(rot, offset * GetOcclusionOffset(i, _rays, raySector));

                int hitCount = Physics.RaycastNonAlloc(
                    from, dir, _immediateHitsBuffer, distance, _layerMask, QueryTriggerInteraction.Ignore
                );

                collisions += math.min(hitCount, _maxHits);
            }

            return CalculateOcclusionResult(distance, collisions, profile);
        }

        private AudioVolumeResultData CalculateVolumeResultImmediate(float3 listenerPos, float3 soundPos, AudioOptions options) {
            var defaultResult = new AudioVolumeResultData(1f, 1f, _attenuationDistance, LpCutoffUpperBound, HpCutoffLowerBound);

            if (!_enableVolumes || (options & AudioOptions.AffectedByVolumes) == 0) return defaultResult;

            UpdateImmediateVolumeData(listenerPos);

            int volumeCount = _immediateVolumes.Count;
            if (volumeCount == 0) return defaultResult;

            var occlusionSound = VolumeParamAccumulator.New();
            var pitch = VolumeParamAccumulator.New();
            var attenuation = VolumeParamAccumulator.New();
            var lpCutoff = VolumeParamAccumulator.New();
            var hpCutoff = VolumeParamAccumulator.New();

            for (int i = 0; i < volumeCount; i++) {
                var data = _immediateVolumes[i];

                if (data.volume.GetWeight(soundPos) is not { weight: > 0f } weightData) continue;

                int mask = data.mask;
                int priority = data.priority;
                float w = weightData.weight;

                if (data.listenerPresence > 0f) {
                    w *= math.lerp(1f, _immediateListenerWeights.GetValueOrDefault(weightData.volumeId), data.listenerPresence);
                }

                if (AudioParameter.SoundOcclusion.InMask(mask)) occlusionSound.Add(priority, w, data.occlusionSound);
                if (AudioParameter.Pitch.InMask(mask)) pitch.Add(priority, w, data.pitch);
                if (AudioParameter.Attenuation.InMask(mask)) attenuation.Add(priority, w, data.attenuation);
                if (AudioParameter.LpCutoff.InMask(mask)) lpCutoff.Add(priority, w, data.lpCutoff);
                if (AudioParameter.HpCutoff.InMask(mask)) hpCutoff.Add(priority, w, data.hpCutoff);
            }

            return new AudioVolumeResultData(
                occlusionSound.Resolve(1f) * _immediateListenerOcclusion,
                pitch.Resolve(1f),
                attenuation.Resolve(_attenuationDistance),
                lpCutoff.Resolve(LpCutoffUpperBound),
                hpCutoff.Resolve(HpCutoffLowerBound)
            );
        }

        private void UpdateImmediateVolumeData(float3 listenerPos) {
            int frame = Time.frameCount;
            if (_immediateVolumesFrame == frame) return;

            _immediateVolumesFrame = frame;
            _immediateVolumes.Clear();
            _immediateListenerWeights.Clear();

            int topPriority = int.MinValue;
            float weightSum = 0f;
            float occlusionMul = 1f;

            foreach (var volume in _audioVolumes.Values) {
                if (volume.Weight <= 0f) continue;

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

                if (mask == 0) continue;

                var weightData = volume.GetWeight(listenerPos);
                int priority = volume.Priority;

                _immediateVolumes.Add(new ImmediateVolumeData(
                    volume, mask, priority, volume.ListenerPresence,
                    occlusionSound, pitch, attenuation, lpCutoff, hpCutoff
                ));

                _immediateListenerWeights[weightData.volumeId] =
                    math.max(weightData.weight, _immediateListenerWeights.GetValueOrDefault(weightData.volumeId));

                if (weightData.weight <= 0f || priority < topPriority ||
                    !AudioParameter.ListenerOcclusion.InMask(mask))
                {
                    continue;
                }

                if (priority > topPriority) {
                    topPriority = priority;
                    weightSum = 0f;
                    occlusionMul = 1f;
                }

                weightSum += weightData.weight;
                occlusionMul += weightData.weight * occlusionListener;
            }

            occlusionMul = weightSum > 0f ? occlusionMul / weightSum : 1f;
            _immediateListenerOcclusion = math.lerp(1f, occlusionMul, math.clamp(weightSum, 0f, 1f));
        }

        private void PauseSoundsOnFocusLost() {
            for (int i = 0; i < _elements.Count; i++) {
                PauseOnFocusLost(_elements[i]);
            }

            foreach (var e in _releaseElementsMap.Values) {
                PauseOnFocusLost(e);
            }
        }

        private static void PauseOnFocusLost(IAudioElement e) {
            if (e.Source == null || !e.Source.isPlaying) return;

            e.IsPausedByFocus = true;
            e.Source.Pause();
        }

        private void ResumeSoundsAfterFocus() {
            if (!_resumeSoundsAfterFocus) return;

            _resumeSoundsAfterFocus = false;

            for (int i = 0; i < _elements.Count; i++) {
                ResumeAfterFocus(_elements[i]);
            }

            foreach (var e in _releaseElementsMap.Values) {
                ResumeAfterFocus(e);
            }
        }

        private static void ResumeAfterFocus(IAudioElement e) {
            if (!e.IsPausedByFocus) return;

            e.IsPausedByFocus = false;

            if (e.Source == null || e.IsPaused) return;

            e.Source.UnPause();
            if (e.Source.isPlaying) return;

            if ((e.AudioOptions & AudioOptions.Loop) != 0) {
                e.Source.Play();
                return;
            }

            if (e.ClipTime >= e.ClipLength) return;

            e.Source.time = e.ClipTime;
            e.Source.Play();
        }

        private static void CheckPause(IAudioElement e, float timescale) {
            if (timescale <= 0f && e.Source.isPlaying) {
                e.IsPaused = true;
                e.Source.Pause();
                return;
            }

            if (timescale > 0f && !e.Source.isPlaying) {
                e.Source.UnPause();
                e.IsPaused = false;
            }
        }
        
        private void ProcessReverb(float dt) {
            if (!_enableReverbVolumes || 
                _audioListenersMap.Count == 0 || 
                _reverbMixers is not { Length: > 0 }) 
            {
                return;
            }
            
            var listenerPos = _listenerTransform.position;
            var resultReverb = CalculateReverbVolumes(listenerPos);
            
            if (UpdateSmoothedReverbSettings(ref _smoothedReverbSettings, ref resultReverb, dt)) {
                ApplyReverbSettings(ref _smoothedReverbSettings);   
            }
        }

        private bool UpdateSmoothedReverbSettings(ref ReverbSettingsData dest, ref ReverbSettingsData target, float dt) {
            var old = dest;
            
            if (_reverbParamsSmoothing <= 0f) {
                dest = target;
            }
            else {
                float t = _reverbParamsSmoothing * dt;

                dest.room = dest.room.SmoothExp(target.room, t);
                dest.roomHf = dest.roomHf.SmoothExp(target.roomHf, t);
                dest.roomLf = dest.roomLf.SmoothExp(target.roomLf, t);
                dest.decayTime = dest.decayTime.SmoothExp(target.decayTime, t);
                dest.decayHfRatio = dest.decayHfRatio.SmoothExp(target.decayHfRatio, t);
                dest.reflectionsLevel = dest.reflectionsLevel.SmoothExp(target.reflectionsLevel, t);
                dest.reflectionsDelay = dest.reflectionsDelay.SmoothExp(target.reflectionsDelay, t);
                dest.reverbLevel = dest.reverbLevel.SmoothExp(target.reverbLevel, t);
                dest.reverbDelay = dest.reverbDelay.SmoothExp(target.reverbDelay, t);
                dest.hfReference = dest.hfReference.SmoothExp(target.hfReference, t);
                dest.lfReference = dest.lfReference.SmoothExp(target.lfReference, t);
                dest.diffusion = dest.diffusion.SmoothExp(target.diffusion, t);
                dest.density = dest.density.SmoothExp(target.density, t);   
            }

            return !old.Equals(dest);
        }
        
        private void ApplyReverbSettings(ref ReverbSettingsData reverbSettings) {
            for (int i = 0; i < _reverbMixers.Length; i++) {
                var mixer = _reverbMixers[i]; 
                mixer.SetFloat(_reverbParamNames[0], reverbSettings.room);
                mixer.SetFloat(_reverbParamNames[1], reverbSettings.roomHf);
                mixer.SetFloat(_reverbParamNames[2], reverbSettings.roomLf);
                mixer.SetFloat(_reverbParamNames[3], reverbSettings.decayTime);
                mixer.SetFloat(_reverbParamNames[4], reverbSettings.decayHfRatio);
                mixer.SetFloat(_reverbParamNames[5], reverbSettings.reflectionsLevel);
                mixer.SetFloat(_reverbParamNames[6], reverbSettings.reflectionsDelay);
                mixer.SetFloat(_reverbParamNames[7], reverbSettings.reverbLevel);
                mixer.SetFloat(_reverbParamNames[8], reverbSettings.reverbDelay);
                mixer.SetFloat(_reverbParamNames[9], reverbSettings.hfReference);
                mixer.SetFloat(_reverbParamNames[10], reverbSettings.lfReference);
                mixer.SetFloat(_reverbParamNames[11], reverbSettings.diffusion);
                mixer.SetFloat(_reverbParamNames[12], reverbSettings.density);
            }
        }
        
        private void ReleaseFinishedSounds() {
            var releaseCandidateIdsBuffer = new NativeList<int>(Allocator.Temp);
            
            foreach ((int id, var e) in _handleIdToAudioElementMap) {
                // To reset smoothed values
                e.OcclusionFlag = 0;

                if (e.CancellationToken.IsCancellationRequested || IsSoundFinished(e, e.AudioOptions)) {
                    releaseCandidateIdsBuffer.Add(id);
                }
            }

            for (int i = 0; i < releaseCandidateIdsBuffer.Length; i++) {
                ReleaseSound(releaseCandidateIdsBuffer[i], immediate: false);
            }

            releaseCandidateIdsBuffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSoundFinished(IAudioElement e, AudioOptions options) {
            if ((options & AudioOptions.Loop) != 0) return false;

            e.ClipTime = math.max(e.ClipTime, e.Source.time);
            return !e.IsPaused && !e.IsPausedByFocus && !e.Source.isPlaying || e.ClipTime >= e.ClipLength;
        }

        private JobHandle ScheduleAudioVolumes(
            NativeArray<float3> positionArray,
            NativeArray<AudioOptions> soundOptionsArray,
            int soundCount,
            bool anyVolumeSound,
            out NativeArray<AudioVolumeResultData> resultArray,
            out VolumeJobResources resources)
        {
            resources = default;

            if (!_enableVolumes || !anyVolumeSound || _audioVolumes.Count == 0) {
                // Placeholder: the result job falls back to default values on its own.
                resultArray = new NativeArray<AudioVolumeResultData>(1, Allocator.TempJob);
                return default;
            }

            int stride = soundCount + 1;
            int maxVolumeCount = _audioVolumes.Count;

            if (_volumesBuffer.Length < maxVolumeCount) _volumesBuffer = new IAudioVolume[math.ceilpow2(maxVolumeCount)];
            var volumes = _volumesBuffer;

            var volumeProcessDataArray = new NativeArray<AudioVolumeProcessData>(maxVolumeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            int volumeCount = 0;

            foreach (var volume in _audioVolumes.Values) {
                // A volume with zero weight or without a single modified parameter cannot affect anything,
                // but would still cost a weight job and a whole column of the per sound volume loop.
                if (volume.Weight <= 0f) continue;

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

                if (mask == 0) continue;

#if UNITY_EDITOR
                LogVolumeDebugInfo(volume, volumeCount, mask, occlusionListener, occlusionSound, pitch, attenuation, lpCutoff, hpCutoff);
#endif

                volumes[volumeCount] = volume;

                volumeProcessDataArray[volumeCount++] = new AudioVolumeProcessData(
                    mask, volume.Priority, volume.ListenerPresence,
                    occlusionListener, occlusionSound, pitch, attenuation, lpCutoff, hpCutoff
                );
            }

            if (volumeCount == 0) {
                volumeProcessDataArray.Dispose();
                resultArray = new NativeArray<AudioVolumeResultData>(1, Allocator.TempJob);
                return default;
            }

            // One buffer for all volumes: each volume fills its own contiguous range in parallel,
            // instead of a schedule and a sync point per volume.
            var weightArray = new NativeArray<WeightSample>(stride * volumeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var weightHandles = new NativeArray<JobHandle>(volumeCount, Allocator.Temp);

            for (int i = 0; i < volumeCount; i++) {
                var volume = volumes[i];
                volumes[i] = null;

                weightHandles[i] = volume.GetWeight(positionArray, weightArray.GetSubArray(i * stride, stride), stride);
            }

            var weightsHandle = JobHandle.CombineDependencies(weightHandles);
            weightHandles.Dispose();

            var listenerVolumeIdToWeightMap = new NativeHashMap<int, float>(volumeCount, Allocator.TempJob);
            var occlusionListenerResultArray = new NativeArray<float>(1, Allocator.TempJob);
            resultArray = new NativeArray<AudioVolumeResultData>(soundCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            resources = new VolumeJobResources {
                created = true,
                weightArray = weightArray,
                processDataArray = volumeProcessDataArray,
                listenerVolumeIdToWeightMap = listenerVolumeIdToWeightMap,
                occlusionListenerResultArray = occlusionListenerResultArray,
            };

            var calculateListenerVolumeJob = new CalculateListenerVolumeJob {
                weightArray = weightArray,
                volumeProcessDataArray = volumeProcessDataArray,
                volumeCount = volumeCount,
                stride = stride,
                listenerVolumeIdToWeightMap = listenerVolumeIdToWeightMap,
                occlusionListenerResultArray = occlusionListenerResultArray,
            };

            var calculateVolumeResultJob = new CalculateVolumeResultDataJob {
                weightArray = weightArray,
                volumeProcessDataArray = volumeProcessDataArray,
                soundOptionsArray = soundOptionsArray,
                occlusionListenerResultArray = occlusionListenerResultArray,
                listenerVolumeIdToWeightMap = listenerVolumeIdToWeightMap,
                volumeCount = volumeCount,
                stride = stride,
                attenuationDefault = _attenuationDistance,
                resultArray = resultArray,
            };

            var listenerJobHandle = calculateListenerVolumeJob.Schedule(weightsHandle);

            return calculateVolumeResultJob.Schedule(soundCount, JobExt.BatchFor(soundCount, MinBatch), listenerJobHandle);
        }
        
        private ReverbSettingsData CalculateReverbVolumes(float3 listenerPosition)
        {
            int volumeCount = _reverbVolumes.Count;
            var volumeIdArray = new NativeArray<EntityId>(volumeCount, Allocator.Temp);
            var volumeWeightDataArray = new NativeArray<VolumeWeightData>(volumeCount, Allocator.TempJob);
            volumeCount = 0;
            
            foreach (var (id, volume) in _reverbVolumes) {
                var settings = volume.GetReverbSettings();
                if (settings == null || volume.GetWeight(listenerPosition) is not { weight: > 0f } weightData) continue;

                int i = volumeCount++;
                volumeIdArray[i] = id;
                volumeWeightDataArray[i] = new VolumeWeightData(volume.Priority, weightData.weight);
            }
            
            var reverbSettingsArray = new NativeArray<ReverbSettingsData>(volumeCount, Allocator.TempJob);

            for (int i = 0; i < volumeCount; i++) {
                var id = volumeIdArray[i];
                var volume = _reverbVolumes[id];
                reverbSettingsArray[i] = new ReverbSettingsData(volume.Level, volume.GetReverbSettings());
            }
            
            var resultReverbSettingsArray = new NativeArray<ReverbSettingsData>(2, Allocator.TempJob);

            var job = new CalculateReverbJob {
                volumeWeightDataArray = volumeWeightDataArray,
                reverbSettingsArray = reverbSettingsArray,
                volumeCount = volumeCount,
                resultReverbSettingsArray = resultReverbSettingsArray,
            };
            
            job.Schedule().Complete();

            var result = resultReverbSettingsArray[0];
            
            volumeIdArray.Dispose();
            volumeWeightDataArray.Dispose();
            reverbSettingsArray.Dispose();
            resultReverbSettingsArray.Dispose();
            
            return result;
        }

        private JobHandle ScheduleOcclusion(
            NativeArray<float3> positionArray,
            NativeArray<OcclusionCandidate> candidates,
            int candidateCount,
            Vector3 up,
            NativeArray<OcclusionResultData> resultArray,
            out NativeArray<RaycastCommand> raycastCommands,
            out NativeArray<RaycastHit> hits)
        {
            if (candidateCount <= 0) {
                raycastCommands = default;
                hits = default;
                return default;
            }

            int commandCount = candidateCount * _rays;

            // Every command is fully written by the prepare job, so it does not need to be cleared.
            raycastCommands = new NativeArray<RaycastCommand>(commandCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            hits = new NativeArray<RaycastHit>(commandCount * _maxHits, Allocator.TempJob);

            var prepareRaycastCommandsJob = new PrepareRaycastCommandsJob {
                listenerAndSoundsPositionArray = positionArray,
                candidates = candidates,
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
                candidates = candidates,
                hitsArray = hits,
                profile = GetOcclusionProfile(),
                resultArray = resultArray
            };

            var prepareJobHandle = prepareRaycastCommandsJob.ScheduleBatch(commandCount, _rays);
            var raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, JobExt.BatchFor(commandCount, MinBatch), _maxHits, prepareJobHandle);

            return calculateOcclusionWeightsJob.Schedule(candidateCount, JobExt.BatchFor(candidateCount, MinBatch), raycastJobHandle);
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

        private OcclusionProfile GetOcclusionProfile() {
            return new OcclusionProfile(
                _rays, _maxHits, _minDistance, _maxDistance, _globalOcclusionWeight,
                _distanceWeightLow, _distanceWeightHigh, _collisionWeightLow, _collisionWeightHigh,
                _distanceLowFreqEasing, _distanceHighFreqEasing
            );
        }

        /// <summary>
        /// Shared by the batched job and by the immediate path taken on play,
        /// so that a sound gets exactly the same weights either way.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static OcclusionResultData CalculateOcclusionResult(float distance, int collisions, in OcclusionProfile profile) {
            float distanceWeight = GetRelativeDistance(distance, profile.minDistance, profile.maxDistance);
            float collisionWeight = math.saturate((float) collisions / (profile.rays * profile.maxHits));

            float wLow = math.clamp((profile.distanceLowFreqEasing.Evaluate(distanceWeight) * profile.distanceLowFreqWeight +
                                     collisionWeight * profile.collisionLowFreqWeight) * profile.globalOcclusionWeight, 0f, 1f);

            float wHigh = math.clamp((profile.distanceHighFreqEasing.Evaluate(distanceWeight) * profile.distanceHighFreqWeight +
                                      collisionWeight * profile.collisionHighFreqWeight) * profile.globalOcclusionWeight, 0f, 1f);

            return new OcclusionResultData(wLow, wHigh
#if UNITY_EDITOR
                , distance
                , collisions
                , distanceWeight
                , collisionWeight
#endif
            );
        }

        /// <summary>
        /// Shared by the batched job and by the immediate path taken on play.
        /// Zero <paramref name="dt"/> means no smoothing, so the sound starts with final values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SoundResultData CalculateSoundResult(
            in SoundData soundData,
            AudioOptions options,
            in AudioVolumeResultData volumeData,
            in OcclusionResultData occlusionData,
            float timescale,
            float dt,
            float smoothing,
            float lpCutoff,
            float hpCutoff,
            EasingType lpCutoffEasing,
            EasingType hpCutoffEasing)
        {
            float pitch = volumeData.pitch * soundData.pitchMul;
            if ((options & AudioOptions.AffectedByTimeScale) != 0) {
                pitch *= timescale;
            }

            float attenuationDistance = soundData.attenuationMul * volumeData.attenuation;

            float lpCutoffT = lpCutoffEasing.Evaluate(occlusionData.weightLowFreq * volumeData.occlusion * soundData.spatialBlend);
            float hpCutoffT = hpCutoffEasing.Evaluate(occlusionData.weightHighFreq * volumeData.occlusion * soundData.spatialBlend);

            float lpCutoffBound = math.min(volumeData.lpCutoff, math.lerp(LpCutoffUpperBound, lpCutoff, lpCutoffT));
            float hpCutoffBound = math.max(volumeData.hpCutoff, math.lerp(HpCutoffLowerBound, hpCutoff, hpCutoffT));

            lpCutoffBound = dt > 0f ? soundData.lpCutoff.SmoothExpNonZero(lpCutoffBound, soundData.occlusionFlag * smoothing, dt) : lpCutoffBound;
            hpCutoffBound = dt > 0f ? soundData.hpCutoff.SmoothExpNonZero(hpCutoffBound, soundData.occlusionFlag * smoothing, dt) : hpCutoffBound;

            return new SoundResultData(pitch, attenuationDistance, lpCutoffBound, hpCutoffBound);
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
            
            private readonly EntityId _entityId;
            private readonly int _hash;
            
            public AttachKey(EntityId entityId, int hash) {
                _entityId = entityId;
                _hash = hash;
            }
            
            public bool Equals(AttachKey other) => _entityId == other._entityId && _hash == other._hash;
            public override bool Equals(object obj) => obj is AttachKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_entityId, _hash);
            public static bool operator ==(AttachKey left, AttachKey right) => left.Equals(right);
            public static bool operator !=(AttachKey left, AttachKey right) => !left.Equals(right);
        }
        
        private readonly struct FadeData {
            
            public readonly float startTime;
            public readonly float fade;
            public readonly float volume;
            public readonly int timeSource;
            
            public FadeData(float startTime, float fade, float volume, int timeSource) {
                this.startTime = startTime;
                this.fade = fade;
                this.volume = volume;
                this.timeSource = timeSource;
            }
        }
        
        private readonly struct OcclusionProfile {

            public readonly int rays;
            public readonly int maxHits;
            public readonly float minDistance;
            public readonly float maxDistance;
            public readonly float globalOcclusionWeight;
            public readonly float distanceLowFreqWeight;
            public readonly float distanceHighFreqWeight;
            public readonly float collisionLowFreqWeight;
            public readonly float collisionHighFreqWeight;
            public readonly EasingType distanceLowFreqEasing;
            public readonly EasingType distanceHighFreqEasing;

            public OcclusionProfile(
                int rays, int maxHits, float minDistance, float maxDistance, float globalOcclusionWeight,
                float distanceLowFreqWeight, float distanceHighFreqWeight,
                float collisionLowFreqWeight, float collisionHighFreqWeight,
                EasingType distanceLowFreqEasing, EasingType distanceHighFreqEasing)
            {
                this.rays = rays;
                this.maxHits = maxHits;
                this.minDistance = minDistance;
                this.maxDistance = maxDistance;
                this.globalOcclusionWeight = globalOcclusionWeight;
                this.distanceLowFreqWeight = distanceLowFreqWeight;
                this.distanceHighFreqWeight = distanceHighFreqWeight;
                this.collisionLowFreqWeight = collisionLowFreqWeight;
                this.collisionHighFreqWeight = collisionHighFreqWeight;
                this.distanceLowFreqEasing = distanceLowFreqEasing;
                this.distanceHighFreqEasing = distanceHighFreqEasing;
            }
        }

        /// <summary>
        /// Accumulates one audio volume parameter across volumes in a single pass:
        /// a volume with a higher priority discards everything gathered so far.
        /// </summary>
        private struct VolumeParamAccumulator {

            private int _topPriority;
            private float _sum;
            private float _weightSum;

            public static VolumeParamAccumulator New() {
                var accumulator = default(VolumeParamAccumulator);
                accumulator._topPriority = int.MinValue;
                return accumulator;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(int priority, float weight, float value) {
                if (priority < _topPriority) return;

                if (priority > _topPriority) {
                    _topPriority = priority;
                    _sum = 0f;
                    _weightSum = 0f;
                }

                _sum += weight * value;
                _weightSum += weight;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly float Resolve(float defaultValue) {
                return _weightSum > 0f
                    ? math.lerp(defaultValue, _sum / _weightSum, math.clamp(_weightSum, 0f, 1f))
                    : defaultValue;
            }
        }

        private readonly struct ImmediateVolumeData {

            public readonly IAudioVolume volume;
            public readonly int mask;
            public readonly int priority;
            public readonly float listenerPresence;
            public readonly float occlusionSound;
            public readonly float pitch;
            public readonly float attenuation;
            public readonly float lpCutoff;
            public readonly float hpCutoff;

            public ImmediateVolumeData(
                IAudioVolume volume, int mask, int priority, float listenerPresence,
                float occlusionSound, float pitch, float attenuation, float lpCutoff, float hpCutoff)
            {
                this.volume = volume;
                this.mask = mask;
                this.priority = priority;
                this.listenerPresence = listenerPresence;
                this.occlusionSound = occlusionSound;
                this.pitch = pitch;
                this.attenuation = attenuation;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }

        private readonly struct OcclusionCandidate {

            public readonly int soundIndex;
            public readonly float distance;

            public OcclusionCandidate(int soundIndex, float distance) {
                this.soundIndex = soundIndex;
                this.distance = distance;
            }
        }

        private struct VolumeJobResources {

            public bool created;
            public NativeArray<WeightSample> weightArray;
            public NativeArray<AudioVolumeProcessData> processDataArray;
            public NativeHashMap<int, float> listenerVolumeIdToWeightMap;
            public NativeArray<float> occlusionListenerResultArray;

            public void Dispose() {
                if (weightArray.IsCreated) weightArray.Dispose();
                if (processDataArray.IsCreated) processDataArray.Dispose();
                if (listenerVolumeIdToWeightMap.IsCreated) listenerVolumeIdToWeightMap.Dispose();
                if (occlusionListenerResultArray.IsCreated) occlusionListenerResultArray.Dispose();
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
            
            public readonly int priority;
            public readonly float weight;
            
            public VolumeWeightData(int priority, float weight) {
                this.priority = priority;
                this.weight = weight;
            }
        }
        
        private readonly struct AudioVolumeProcessData {

            public readonly int mask;
            public readonly int priority;
            public readonly float listenerPresence;
            public readonly float occlusionListener;
            public readonly float occlusionSound;
            public readonly float pitch;
            public readonly float attenuation;
            public readonly float lpCutoff;
            public readonly float hpCutoff;

            public AudioVolumeProcessData(
                int mask, int priority, float listenerPresence,
                float occlusionListener, float occlusionSound, float pitch, float attenuation, float lpCutoff, float hpCutoff)
            {
                this.mask = mask;
                this.priority = priority;
                this.listenerPresence = listenerPresence;

                this.occlusionListener = occlusionListener;
                this.occlusionSound = occlusionSound;
                this.pitch = pitch;
                this.attenuation = attenuation;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        private readonly struct AudioVolumeResultData {
            
            public readonly float occlusion;
            public readonly float pitch;
            public readonly float attenuation;
            public readonly float lpCutoff;
            public readonly float hpCutoff;
            
            public AudioVolumeResultData(float occlusion, float pitch, float attenuation, float lpCutoff, float hpCutoff) {
                this.occlusion = occlusion;
                this.pitch = pitch;
                this.attenuation = attenuation;
                this.lpCutoff = lpCutoff;
                this.hpCutoff = hpCutoff;
            }
        }
        
        private struct ReverbSettingsData {

            public float room;
            public float roomHf;
            public float roomLf;
            public float decayTime;
            public float decayHfRatio;
            public float reflectionsLevel;
            public float reflectionsDelay;
            public float reverbLevel;
            public float reverbDelay;
            public float hfReference;
            public float lfReference;
            public float diffusion;
            public float density;

            public ReverbSettingsData(float room, IReverbSettings settings) : this(
                room, settings.RoomHf, settings.RoomLf,
                settings.DecayTime, settings.DecayHfRatio, 
                settings.ReflectionsLevel, settings.ReflectionsDelay,
                settings.ReverbLevel, settings.ReverbDelay,
                settings.HfReference, settings.LfReference,
                settings.Diffusion, settings.Density) 
            {
                
            }
            
            public ReverbSettingsData(
                float room, float roomHf, float roomLf,
                float decayTime, float decayHfRatio,
                float reflectionsLevel, float reflectionsDelay,
                float reverbLevel, float reverbDelay,
                float hfReference, float lfReference,
                float diffusion, float density) 
            {
                this.room = room;
                this.roomHf = roomHf;
                this.roomLf = roomLf;
                this.decayTime = decayTime;
                this.decayHfRatio = decayHfRatio;
                this.reflectionsLevel = reflectionsLevel;
                this.reflectionsDelay = reflectionsDelay;
                this.reverbLevel = reverbLevel;
                this.reverbDelay = reverbDelay;
                this.hfReference = hfReference;
                this.lfReference = lfReference;
                this.diffusion = diffusion;
                this.density = density;
            }
            
            public bool Equals(ReverbSettingsData other) {
                return room.IsNearlyEqual(other.room) &&
                       roomHf.IsNearlyEqual(other.roomHf) &&
                       roomLf.IsNearlyEqual(other.roomLf) &&
                       decayTime.IsNearlyEqual(other.decayTime) &&
                       decayHfRatio.IsNearlyEqual(other.decayHfRatio) &&
                       reflectionsLevel.IsNearlyEqual(other.reflectionsLevel) &&
                       reflectionsDelay.IsNearlyEqual(other.reflectionsDelay) &&
                       reverbLevel.IsNearlyEqual(other.reverbLevel) &&
                       reverbDelay.IsNearlyEqual(other.reverbDelay) &&
                       hfReference.IsNearlyEqual(other.hfReference) &&
                       lfReference.IsNearlyEqual(other.lfReference) &&
                       diffusion.IsNearlyEqual(other.diffusion) &&
                       density.IsNearlyEqual(other.density);
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
        private struct ReadSoundPositionsJob : IJobParallelForTransform {

            // Index zero is the listener position, written on the main thread.
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<float3> positions;

            public void Execute(int index, TransformAccess transform) {
                positions[index + 1] = transform.position;
            }
        }
        
        [BurstCompile]
        private struct CalculateReverbJob : IJob {

            [ReadOnly] public NativeArray<VolumeWeightData> volumeWeightDataArray;
            public NativeArray<ReverbSettingsData> reverbSettingsArray;
            [ReadOnly] public int volumeCount;
            public NativeArray<ReverbSettingsData> resultReverbSettingsArray;
            
            public void Execute() {
                int topPriority = int.MinValue;
                float weightSum = 0f;
                var result = new ReverbSettingsData();

                // Single pass: a volume with a higher priority discards everything accumulated so far.
                for (int i = 0; i < volumeCount; i++) {
                    var weightData = volumeWeightDataArray[i];
                    if (weightData.weight <= 0f || weightData.priority < topPriority) continue;

                    if (weightData.priority > topPriority) {
                        topPriority = weightData.priority;
                        weightSum = 0f;
                        result = new ReverbSettingsData();
                    }

                    ref var reverbSettings = ref reverbSettingsArray.GetRef(i);

                    weightSum += weightData.weight;

                    result.room += reverbSettings.room * weightData.weight;
                    result.roomHf += reverbSettings.roomHf * weightData.weight;
                    result.roomLf += reverbSettings.roomLf * weightData.weight;
                    result.decayTime += reverbSettings.decayTime * weightData.weight;
                    result.decayHfRatio += reverbSettings.decayHfRatio * weightData.weight;
                    result.reflectionsLevel += reverbSettings.reflectionsLevel * weightData.weight;
                    result.reflectionsDelay += reverbSettings.reflectionsDelay * weightData.weight;
                    result.reverbLevel += reverbSettings.reverbLevel * weightData.weight;
                    result.reverbDelay += reverbSettings.reverbDelay * weightData.weight;
                    result.hfReference += reverbSettings.hfReference * weightData.weight;
                    result.lfReference += reverbSettings.lfReference * weightData.weight;
                    result.diffusion += reverbSettings.diffusion * weightData.weight;
                    result.density += reverbSettings.density * weightData.weight;
                }

                if (weightSum > 0f) {
                    float invW = 1f / weightSum;
                    
                    result.room *= invW;
                    result.roomHf *= invW;
                    result.roomLf *= invW;
                    result.decayTime *= invW;
                    result.decayHfRatio *= invW;
                    result.reflectionsLevel *= invW;
                    result.reflectionsDelay *= invW;
                    result.reverbLevel *= invW;
                    result.reverbDelay *= invW;
                    result.hfReference *= invW;
                    result.lfReference *= invW;
                    result.diffusion *= invW;
                    result.density *= invW;
                }

                resultReverbSettingsArray[0] = result;
            }
        }
        
        [BurstCompile]
        private struct CalculateListenerVolumeJob : IJob {

            // Weights of all volumes for all positions: volume v, position i is at v * stride + i,
            // where position zero is the listener.
            [ReadOnly] public NativeArray<WeightSample> weightArray;
            [ReadOnly] public NativeArray<AudioVolumeProcessData> volumeProcessDataArray;
            [ReadOnly] public int volumeCount;
            [ReadOnly] public int stride;

            public NativeHashMap<int, float> listenerVolumeIdToWeightMap;
            [WriteOnly] public NativeArray<float> occlusionListenerResultArray;

            public void Execute() {
                int topPriority = int.MinValue;
                float weightSum = 0f;
                float occlusionMul = 1f;

                // Single pass: a volume with a higher priority discards everything accumulated so far.
                for (int v = 0; v < volumeCount; v++) {
                    var sample = weightArray[v * stride];
                    var processData = volumeProcessDataArray[v];

                    listenerVolumeIdToWeightMap[sample.volumeId] =
                        math.max(sample.weight, listenerVolumeIdToWeightMap.TryGetValue(sample.volumeId, out float w) ? w : 0f);

                    if (sample.weight <= 0f || processData.priority < topPriority ||
                        !AudioParameter.ListenerOcclusion.InMask(processData.mask))
                    {
                        continue;
                    }

                    if (processData.priority > topPriority) {
                        topPriority = processData.priority;
                        weightSum = 0f;
                        occlusionMul = 1f;
                    }

                    weightSum += sample.weight;
                    occlusionMul += sample.weight * processData.occlusionListener;
                }

                occlusionMul = weightSum > 0f ? occlusionMul / weightSum : 1f;
                occlusionListenerResultArray[0] = math.lerp(1f, occlusionMul, math.clamp(weightSum, 0f, 1f));
            }
        }

        [BurstCompile]
        private struct CalculateVolumeResultDataJob : IJobParallelFor {

            // Weights of all volumes for all positions: volume v, position i is at v * stride + i,
            // where position zero is the listener and sound at index is at index + 1.
            [ReadOnly] public NativeArray<WeightSample> weightArray;
            [ReadOnly] public NativeArray<AudioVolumeProcessData> volumeProcessDataArray;
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray;
            [ReadOnly] public NativeArray<float> occlusionListenerResultArray;
            [ReadOnly] public NativeHashMap<int, float> listenerVolumeIdToWeightMap;
            [ReadOnly] public int volumeCount;
            [ReadOnly] public int stride;
            [ReadOnly] public float attenuationDefault;

            [WriteOnly] public NativeArray<AudioVolumeResultData> resultArray;

            public void Execute(int index) {
                if ((soundOptionsArray[index] & AudioOptions.AffectedByVolumes) == 0) {
                    resultArray[index] = new AudioVolumeResultData(1f, 1f, attenuationDefault, LpCutoffUpperBound, HpCutoffLowerBound);
                    return;
                }

                var occlusionSound = VolumeParamAccumulator.New();
                var pitch = VolumeParamAccumulator.New();
                var attenuation = VolumeParamAccumulator.New();
                var lpCutoff = VolumeParamAccumulator.New();
                var hpCutoff = VolumeParamAccumulator.New();

                int row = index + 1;

                for (int v = 0; v < volumeCount; v++) {
                    var sample = weightArray[v * stride + row];
                    if (sample.weight <= 0f) continue;

                    var processData = volumeProcessDataArray[v];
                    int mask = processData.mask;
                    int priority = processData.priority;

                    float w = sample.weight;

                    // Listener weight is only needed when the volume actually blends by listener presence.
                    if (processData.listenerPresence > 0f) {
                        float listenerWeight = listenerVolumeIdToWeightMap.TryGetValue(sample.volumeId, out float lw) ? lw : 0f;
                        w *= math.lerp(1f, listenerWeight, processData.listenerPresence);
                    }

                    if (AudioParameter.SoundOcclusion.InMask(mask)) occlusionSound.Add(priority, w, processData.occlusionSound);
                    if (AudioParameter.Pitch.InMask(mask)) pitch.Add(priority, w, processData.pitch);
                    if (AudioParameter.Attenuation.InMask(mask)) attenuation.Add(priority, w, processData.attenuation);
                    if (AudioParameter.LpCutoff.InMask(mask)) lpCutoff.Add(priority, w, processData.lpCutoff);
                    if (AudioParameter.HpCutoff.InMask(mask)) hpCutoff.Add(priority, w, processData.hpCutoff);
                }

                resultArray[index] = new AudioVolumeResultData(
                    occlusionSound.Resolve(1f) * occlusionListenerResultArray[0],
                    pitch.Resolve(1f),
                    attenuation.Resolve(attenuationDefault),
                    lpCutoff.Resolve(LpCutoffUpperBound),
                    hpCutoff.Resolve(HpCutoffLowerBound)
                );
            }
        }
        
        [BurstCompile]
        private struct PrepareRaycastCommandsJob : IJobParallelForBatch {

            [ReadOnly] public NativeArray<float3> listenerAndSoundsPositionArray;
            [ReadOnly] public NativeArray<OcclusionCandidate> candidates;
            [ReadOnly] public float3 up;
            [ReadOnly] public float raySector;
            [ReadOnly] public float rayOffset0;
            [ReadOnly] public float rayOffset1;
            [ReadOnly] public float minDistance;
            [ReadOnly] public float maxDistance;
            [ReadOnly] public int layerMask;

            [WriteOnly] public NativeArray<RaycastCommand> raycastCommands;

            public void Execute(int startIndex, int count) {
                // Candidates are already filtered by flags and distance on the main thread,
                // so every command written here is a real raycast.
                var candidate = candidates[startIndex / count];

                var listenerPos = listenerAndSoundsPositionArray[0];
                var soundPos = listenerAndSoundsPositionArray[candidate.soundIndex + 1];

                float distance = candidate.distance;
                var dir = (listenerPos - soundPos) / distance;
                var rot = quaternion.LookRotation(dir, up);
                float offset = math.lerp(rayOffset0, rayOffset1, GetRelativeDistance(distance, minDistance, maxDistance));

                var queryParameters = new QueryParameters(layerMask, hitMultipleFaces: false, hitTriggers: QueryTriggerInteraction.Ignore, hitBackfaces: false);

                for (int i = 0; i < count; i++) {
                    raycastCommands[startIndex + i] = new RaycastCommand(
                        from: soundPos + math.mul(rot, offset * GetOcclusionOffset(i, count, raySector)),
                        dir,
                        queryParameters,
                        distance
                    );
                }
            }
        }
        
        [BurstCompile]
        private struct CalculateOcclusionWeightsJob : IJobParallelFor {

            [ReadOnly] public NativeArray<OcclusionCandidate> candidates;
            [ReadOnly] public NativeArray<RaycastHit> hitsArray;
            [ReadOnly] public OcclusionProfile profile;

            // Results are indexed by sound, while this job is indexed by candidate.
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<OcclusionResultData> resultArray;

            public void Execute(int index) {
                var candidate = candidates[index];

                int rays = profile.rays;
                int maxHits = profile.maxHits;

                int collisions = 0;
                int hitOffset = index * rays * maxHits;

                for (int j = 0; j < rays; j++) {
                    int rayOffset = hitOffset + j * maxHits;

                    for (int r = 0; r < maxHits; r++) {
                        if (hitsArray[rayOffset + r].colliderEntityId == EntityId.None) break;

                        collisions++;
                    }
                }

                resultArray[candidate.soundIndex] = CalculateOcclusionResult(candidate.distance, collisions, profile);
            }
        }
        
        [BurstCompile]
        private struct CalculateResultSoundJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<SoundData> soundDataArray; 
            [ReadOnly] public NativeArray<AudioOptions> soundOptionsArray; 
            [ReadOnly] public NativeArray<AudioVolumeResultData> volumeResultDataArray;
            [ReadOnly] public int volumesEnabled;
            [ReadOnly] public NativeArray<OcclusionResultData> occlusionResultDataArray;
            [ReadOnly] public float timescale;
            [ReadOnly] public float dtScaled;
            [ReadOnly] public float dtUnscaled;
            [ReadOnly] public float smoothing;
            [ReadOnly] public float attenuationDefault;
            [ReadOnly] public float lpCutoff;
            [ReadOnly] public float hpCutoff;
            [ReadOnly] public EasingType lpCutoffEasing;
            [ReadOnly] public EasingType hpCutoffEasing;
            
            [WriteOnly] public NativeArray<SoundResultData> resultArray;
            
            public void Execute(int index) {
                var soundData = soundDataArray[index];
                var options = soundOptionsArray[index];
                var occlusionData = occlusionResultDataArray[index];

                // With volumes disabled the pipeline skips the whole volume pass and its allocations.
                var volumeData = volumesEnabled != 0
                    ? volumeResultDataArray[index]
                    : new AudioVolumeResultData(1f, 1f, attenuationDefault, LpCutoffUpperBound, HpCutoffLowerBound);

                float dt = (options & AudioOptions.AffectedByTimeScale) != 0 ? dtScaled : dtUnscaled;

                resultArray[index] = CalculateSoundResult(
                    soundData, options, volumeData, occlusionData,
                    timescale, dt, smoothing, lpCutoff, hpCutoff, lpCutoffEasing, hpCutoffEasing
                );
            }
        }
        
        #endregion JOBS
        
        #region EDITOR
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showSoundsGizmo;
        [SerializeField] private bool _showSoundsDebugInfo;
        [SerializeField] private bool _showPlayReleaseLogs;
        [SerializeField] private bool _showVolumeInfo;
        [SerializeField] private bool _showOcclusionInfo;
        [SerializeField] private string[] _showSoundsNameFilters;
        [SerializeField] private string[] _showOcclusionNameFilters;

        bool IAudioPool.ShowGizmo => _showSoundsGizmo;
        
        private readonly Dictionary<int, Color> _debugColors = new();

        private void OnValidate() {
            if (_maxDistance < _minDistance) _maxDistance = _minDistance;

            if (Application.isPlaying) {
                FetchIncludeMixerGroupsFromVolumes();
                FetchIgnoreZeroTimescaleMixerGroups();
                CreateReverbParamNames();

                for (int i = 0; i < _elements.Count; i++) {
                    WriteMixerGroupFlags(_elements[i]);
                }
            }
        }

        private void CreateDebugColor(int id, AudioClip clip) {
            _debugColors[id] = ColorUtils.ColorFromHash(clip.GetHashCode());
        }

        private void LogSoundDebugInfo(IAudioElement e, OcclusionResultData occ, AudioVolumeResultData vol, SoundResultData resultData) {
            if (!_showSoundsDebugInfo) return;

            var clip = e.Source.clip;
            string clipName = clip == null ? "<null>" : clip.name;

            if (!MatchesNameFilters(clipName, _showSoundsNameFilters)) return;

            Debug.Log($"AudioPool.ProcessSounds: f {Time.frameCount}, clip {clipName}, " +
                      $"w low {occ.weightLowFreq}, w high {occ.weightHighFreq}, lp {resultData.lpCutoff}, hp {resultData.hpCutoff}, " +
                      $"dist {occ.distance}, collisions {occ.collisions}, dist w {occ.distanceWeight}, coll w {occ.collisionWeight}, " +
                      $"vol occ {vol.occlusion}, vol pitch {vol.pitch}, vol atten {vol.attenuation}, vol lp {vol.lpCutoff}, vol hp {vol.hpCutoff}");
        }

        private void LogVolumeDebugInfo(
            IAudioVolume volume, int volumeIndex, int mask,
            float occlusionListener, float occlusionSound, float pitch, float attenuation, float lpCutoff, float hpCutoff)
        {
            if (!_showVolumeInfo) return;

            var sb = new StringBuilder();

            if (AudioParameter.ListenerOcclusion.InMask(mask)) sb.Append($"{AudioParameter.ListenerOcclusion} ");
            if (AudioParameter.SoundOcclusion.InMask(mask)) sb.Append($"{AudioParameter.SoundOcclusion} ");
            if (AudioParameter.Pitch.InMask(mask)) sb.Append($"{AudioParameter.Pitch} ");
            if (AudioParameter.Attenuation.InMask(mask)) sb.Append($"{AudioParameter.Attenuation} ");
            if (AudioParameter.LpCutoff.InMask(mask)) sb.Append($"{AudioParameter.LpCutoff} ");
            if (AudioParameter.HpCutoff.InMask(mask)) sb.Append($"{AudioParameter.HpCutoff}");

            Debug.Log($"AudioPool.ScheduleAudioVolumes: f {Time.frameCount}, vol #{volumeIndex} {volume}, " +
                      $"list presence {volume.ListenerPresence}, " +
                      $"occ lis {occlusionListener}, occ sound {occlusionSound}, pitch {pitch}, atten {attenuation}, lp {lpCutoff}, hp {hpCutoff}, " +
                      $"changed [{sb}]");
        }

        private void DrawOcclusionDebug(
            NativeArray<float3> positionArray,
            NativeArray<SoundData> soundDataArray,
            NativeArray<OcclusionCandidate> candidates,
            int candidateCount,
            NativeArray<RaycastCommand> raycastCommands)
        {
            if (!_showOcclusionInfo || candidateCount <= 0) return;

            for (int c = 0; c < candidateCount; c++) {
                int soundIndex = candidates[c].soundIndex;
                int id = soundDataArray[soundIndex].id;

                if (_showOcclusionNameFilters?.Length > 0) {
                    var clip = _handleIdToAudioElementMap.TryGetValue(id, out var e) ? e.Source.clip : null;
                    if (!MatchesNameFilters(clip == null ? "<null>" : clip.name, _showOcclusionNameFilters)) continue;
                }

                var pos = positionArray[soundIndex + 1];
                var color = GetDebugColor(id);

                for (int j = 0; j < _rays; j++) {
                    var com = raycastCommands[c * _rays + j];
                    DebugExt.DrawLine(pos, com.from, color);
                    DebugExt.DrawRay(com.from, com.direction * com.distance, color);
                }
            }
        }

        private void LogPlaySound(int id, AudioClip clip, AudioOptions options) {
            if (!_showPlayReleaseLogs || !MatchesNameFilters(clip.name, _showSoundsNameFilters)) return;

            Debug.Log($"AudioPool.Play: f {Time.frameCount}, clip {clip.name}, id {id}, " +
                      $"timescale {Time.timeScale}, length {clip.length:0.###} s, " +
                      $"loop {(options & AudioOptions.Loop) != 0}");
        }

        private void LogReleaseSound(int handleId, IAudioElement e, bool immediate) {
            if (!_showPlayReleaseLogs) return;

            var source = e.Source;
            string clipName = source == null || source.clip == null ? "<null>" : source.clip.name;

            if (!MatchesNameFilters(clipName, _showSoundsNameFilters)) return;

            Debug.Log($"AudioPool.Release: f {Time.frameCount}, clip {clipName}, id {handleId}, " +
                      $"timescale {Time.timeScale}, immediate {immediate}, " +
                      $"length {e.ClipLength:0.###} s, played {(source == null ? 0f : source.time):0.###} s, " +
                      $"fade out {(immediate ? 0f : e.FadeOut):0.###} s");
        }

        private static bool MatchesNameFilters(string clipName, string[] filters) {
            if (filters is not { Length: > 0 }) return true;

            for (int i = 0; i < filters.Length; i++) {
                string filter = filters[i];
                if (!string.IsNullOrWhiteSpace(filter) && clipName.Contains(filter)) return true;
            }

            return false;
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