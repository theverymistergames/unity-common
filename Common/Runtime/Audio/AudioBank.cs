using MisterGames.Common.Attributes;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Audio {

    [CreateAssetMenu(fileName = nameof(AudioBank), menuName = "MisterGames/Audio/" + nameof(AudioBank))]
    public sealed class AudioBank : ScriptableObject {

        [Tooltip("Clips of the bank imported with " + nameof(LoadTime.AssetPreload) + " or " +
                 nameof(LoadTime.BankPreload) + ", collected by the search and the reimport buttons. " +
                 "The clips of the search folders that are not preloaded at all are not referenced by the bank.")]
        [SerializeField] [ReadOnly] private AudioClip[] _preloadClips;

        /// <summary>
        /// When the audio data of a clip is loaded into memory.
        /// </summary>
        public enum LoadTime {

            /// <summary>
            /// Preload option of the clip is off, the audio data is loaded on the first play of the clip.
            /// <see cref="PreloadAudioData"/> of the bank does not touch such a clip.
            /// </summary>
            NoPreload,

            /// <summary>
            /// Preload option of the clip is on, the audio data is loaded by the engine along with the asset.
            /// <see cref="PreloadAudioData"/> of the bank has nothing to do for such a clip.
            /// </summary>
            AssetPreload,

            /// <summary>
            /// Preload option of the clip is off, so that loading the asset does not load the audio data:
            /// the data is loaded by <see cref="PreloadAudioData"/> of the bank, at the moment the bank owner
            /// decides to.
            /// </summary>
            BankPreload,
        }

        /// <summary>
        /// Starts loading the audio data of the clips imported with <see cref="LoadTime.BankPreload"/>.
        /// <para>
        /// Clips imported with load in background start loading asynchronously, so a clip is not ready to play
        /// right after the call: check <see cref="AudioClip.loadState"/> for that.
        /// </para>
        /// </summary>
        /// <returns>Number of clips the load was started for.</returns>
        public int PreloadAudioData() {
            int count = 0;

            for (int i = 0; i < _preloadClips?.Length; i++) {
                var clip = _preloadClips[i];

                if (clip == null) continue;

                // The preload option separates the two load times inside the list: it is on for the clips
                // the engine loads itself, and there is nothing left for the bank to do about them.
                if (clip.preloadAudioData) continue;

                // A streaming clip is read from the file while it is playing and has no audio data to preload.
                if (clip.loadType == AudioClipLoadType.Streaming) continue;
                if (clip.loadState is AudioDataLoadState.Loading or AudioDataLoadState.Loaded) continue;

                if (clip.LoadAudioData()) {
                    count++;
                    continue;
                }

                Debug.LogWarning($"{nameof(AudioBank)} '{name}': can not load audio data of clip '{clip.name}'.", this);
            }

            return count;
        }

        /// <summary>
        /// Unloads the audio data of the clips imported with <see cref="LoadTime.AssetPreload"/> and
        /// <see cref="LoadTime.BankPreload"/>. Clips imported with <see cref="LoadTime.NoPreload"/> keep
        /// their data: nothing asked for it to be in memory, it is loaded by the playback itself.
        /// <para>
        /// Audio data of a playing clip is unloaded as well, and the playback is stopped.
        /// </para>
        /// </summary>
        /// <returns>Number of clips the data was unloaded for.</returns>
        public int UnloadAudioData() {
            int count = 0;

            for (int i = 0; i < _preloadClips?.Length; i++) {
                var clip = _preloadClips[i];

                if (clip == null) continue;
                if (TryUnloadAudioData(clip)) count++;
            }

            return count;
        }

        private bool TryUnloadAudioData(AudioClip clip) {
            if (clip.loadType == AudioClipLoadType.Streaming || clip.loadState == AudioDataLoadState.Unloaded) return false;
            if (clip.UnloadAudioData()) return true;

            Debug.LogWarning($"{nameof(AudioBank)} '{name}': can not unload audio data of clip '{clip.name}'.", this);
            return false;
        }

#if UNITY_EDITOR
        [Tooltip("Total audio data size of the preloaded clips, counted from their import settings: raw PCM " +
                 "samples before the compression, and what the clips take in memory after it. " +
                 "The two are equal for the clips imported as " + nameof(AudioClipLoadType.DecompressOnLoad) + ": " +
                 "such a clip is compressed on disk only and keeps the raw samples in memory. " +
                 "The compressed size of a variable bitrate format is an estimate.")]
        [SerializeField] [ReadOnly] private string _preloadClipsSize;

        [Header("Search")]
        [SerializeField] private DefaultAsset[] _searchFolders;
        [SerializeField] private DefaultAsset[] _excludeFolders;

        [Header("Import Presets")]
        [SerializeField] private Preset _defaultPreset = Preset.Default;
        [SerializeField] private CustomPreset[] _customPresets = CustomPreset.Defaults;
        [SerializeField] private ClipPreset[] _clipPresets;

        /// <summary>
        /// Audio clip import settings applied by <see cref="AudioBankPostprocessor"/> to the clips of the bank.
        /// </summary>
        [Serializable]
        public struct Preset {

            public AudioClipLoadType loadType;
            public AudioCompressionFormat compressionFormat;
            public LoadTime loadTime;
            public bool loadInBackground;

            /// <summary>
            /// Only <see cref="LoadTime.AssetPreload"/> keeps the preload option of the clip on:
            /// <see cref="LoadTime.BankPreload"/> loads the audio data by the bank instead of the engine,
            /// so the option has to be off for it as well as for <see cref="LoadTime.NoPreload"/>.
            /// </summary>
            public bool PreloadAudioData => loadTime == LoadTime.AssetPreload;

            /// <summary>
            /// Settings of a clip that no custom preset covers: it is compressed on disk, decompressed on load
            /// and its audio data is loaded by the first play, without the bank taking part in it.
            /// </summary>
            public static Preset Default => new() {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.Vorbis,
                loadTime = LoadTime.NoPreload,
                loadInBackground = false,
            };
        }

        /// <summary>
        /// A preset for the clips of a certain duration. Custom presets are checked in order, the first one
        /// the clip duration falls into wins, and a clip out of all the ranges is imported with the default preset.
        /// </summary>
        [Serializable]
        public struct CustomPreset {

            [Tooltip("Clip duration in seconds the preset starts at, inclusive.")]
            [Min(0f)] public float minDuration;

            [Tooltip("Clip duration in seconds the preset ends at, exclusive. Zero means no upper bound.")]
            [Min(0f)] public float maxDuration;

            public Preset preset;

            public bool Contains(float duration) {
                return duration >= minDuration && (maxDuration <= 0f || duration < maxDuration);
            }

            /// <summary>
            /// Presets the banks of the project are set up with. The shorter a clip is, the cheaper it is to keep
            /// it raw and ready to play: sounds under a second are kept as PCM, longer ones trade the decoding
            /// cost for the size, and everything above half a minute is streamed from disk instead of being
            /// preloaded at all.
            /// </summary>
            public static CustomPreset[] Defaults => new[] {
                Create(0f, 1f, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM, LoadTime.BankPreload, loadInBackground: true),
                Create(1f, 3f, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.ADPCM, LoadTime.BankPreload, loadInBackground: true),
                Create(3f, 10f, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.Vorbis, LoadTime.BankPreload, loadInBackground: true),
                Create(10f, 30f, AudioClipLoadType.CompressedInMemory, AudioCompressionFormat.Vorbis, LoadTime.BankPreload, loadInBackground: true),
                Create(30f, 0f, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis, LoadTime.NoPreload, loadInBackground: false),
            };

            private static CustomPreset Create(
                float minDuration,
                float maxDuration,
                AudioClipLoadType loadType,
                AudioCompressionFormat compressionFormat,
                LoadTime loadTime,
                bool loadInBackground)
            {
                return new CustomPreset {
                    minDuration = minDuration,
                    maxDuration = maxDuration,
                    preset = new Preset {
                        loadType = loadType,
                        compressionFormat = compressionFormat,
                        loadTime = loadTime,
                        loadInBackground = loadInBackground,
                    },
                };
            }
        }

        /// <summary>
        /// A preset for the clips listed in it, the same as a <see cref="CustomPreset"/> with the clips named
        /// instead of a duration range. Clip presets are checked before the custom presets, so a listed clip
        /// keeps these settings whatever its duration is.
        /// </summary>
        [Serializable]
        public struct ClipPreset {

            [Tooltip("Clips the preset is applied to.")]
            public AudioClip[] clips;

            public Preset preset;

            public bool Contains(AudioClip clip) {
                for (int i = 0; i < clips?.Length; i++) {
                    if (clips[i] == clip) return true;
                }

                return false;
            }
        }

        private const string AudioClipFilter = "t:AudioClip";

        // Decompressed audio data is kept as 16 bit PCM samples.
        private const int PcmBytesPerSample = 2;

        // Compression ratio of the ADPCM format as Unity documents it.
        private const float AdpcmCompressionRatio = 3.5f;

        // Bitrates of the quality scale, in kbit/s, for a clip of ReferenceChannels channels at
        // ReferenceFrequency Hz: the quality slider of the importer maps onto this table.
        private static readonly int[] VorbisBitratesKbps = { 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 500 };
        private const int ReferenceChannels = 2;
        private const float ReferenceFrequency = 44100f;

        [Button]
        private void SearchAudioClips() {
            var clipPaths = CollectClipPaths();
            int countBefore = _preloadClips?.Length ?? 0;

            CollectPreloadClips(clipPaths);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            Debug.Log($"{nameof(AudioBank)} '{name}': found {clipPaths.Count} audio clips in the search folders, " +
                      $"{_preloadClips.Length} of them are preloaded (were {countBefore}), {_preloadClipsSize}.", this);
        }

        /// <summary>
        /// Writes the settings of the preset each clip of the search folders falls under and reimports
        /// the clips that were changed.
        /// </summary>
        [Button]
        private void Reimport() {
            InvalidateBankCache();

            var clipPaths = CollectClipPaths();

            // NoPreload and BankPreload are the same import settings and differ only by who loads the audio data,
            // so the list is rebuilt even when there is nothing to reimport.
            CollectPreloadClips(clipPaths);

            int count = ApplyPresets(this, clipPaths, showProgress: true);

            // The batch of imports is done by now, so the clips are measured with the settings they got.
            UpdatePreloadClipsSize();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            Debug.Log($"{nameof(AudioBank)} '{name}': reimported {count} of {clipPaths.Count} audio clips, " +
                      $"{_preloadClips.Length} of them are preloaded, {_preloadClipsSize}.", this);
        }

        /// <summary>
        /// Measures the preloaded clips again, without touching the collected clips or their import settings.
        /// </summary>
        [Button]
        private void CalculatePreloadClipsSize() {
            UpdatePreloadClipsSize();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            Debug.Log($"{nameof(AudioBank)} '{name}': {_preloadClips?.Length ?? 0} preloaded clips, " +
                      $"{_preloadClipsSize}.", this);
        }

        /// <summary>
        /// Writes the settings of the preset each clip falls under into its importer and reimports the clips
        /// that were changed.
        /// <para>
        /// The settings are not written from OnPreprocessAudio, the way an asset postprocessor usually does it:
        /// the preset is chosen by the clip duration, which is not known until the clip is built, and an import
        /// can run in an import worker process, where nothing the editor process measured and kept in memory
        /// exists. Here, in the editor process and after the import, both the clip and the banks are available.
        /// </para>
        /// </summary>
        /// <param name="bank">Bank whose presets are applied, null to look the bank up for every clip.</param>
        /// <param name="assetPaths">Paths of the audio clips to apply the presets to.</param>
        /// <param name="showProgress">Show the progress bar: a bank can hold thousands of clips.</param>
        /// <returns>Number of clips the settings were changed for.</returns>
        private static int ApplyPresets(AudioBank bank, IReadOnlyList<string> assetPaths, bool showProgress) {
            var importers = new List<AudioImporter>();
            bool assetEditing = false;

            try {
                // The clips and the banks are read before the asset editing starts: the asset database
                // does not answer the same way while it is locked for a batch of imports.
                for (int i = 0; i < assetPaths?.Count; i++) {
                    string assetPath = assetPaths[i];

                    if (showProgress) {
                        EditorUtility.DisplayProgressBar(nameof(AudioBank), $"Reading {assetPath}",
                            (float) i / assetPaths.Count);
                    }

                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (clip == null) continue;

                    Preset preset;

                    if (bank != null
                        ? !bank.TryGetPreset(assetPath, clip, out preset)
                        : !TryGetPresetFromBanks(assetPath, clip, out preset))
                    {
                        continue;
                    }

                    // A clip that already matches its preset has nothing to gain from a reimport.
                    if (AssetImporter.GetAtPath(assetPath) is not AudioImporter importer ||
                        !NeedsPreset(importer, preset))
                    {
                        continue;
                    }

                    ApplyPreset(importer, preset);
                    importers.Add(importer);
                }

                if (importers.Count == 0) return 0;

                AssetDatabase.StartAssetEditing();
                assetEditing = true;

                for (int i = 0; i < importers.Count; i++) {
                    if (showProgress) {
                        EditorUtility.DisplayProgressBar(nameof(AudioBank), $"Reimporting {importers[i].assetPath}",
                            (float) i / importers.Count);
                    }

                    importers[i].SaveAndReimport();
                }
            }
            finally {
                if (assetEditing) AssetDatabase.StopAssetEditing();
                if (showProgress) EditorUtility.ClearProgressBar();
            }

            return importers.Count;
        }

        private List<string> CollectClipPaths() {
            var searchFolders = GetFolderPaths(_searchFolders);
            var excludeFolders = GetFolderPaths(_excludeFolders);
            var assetPaths = new List<string>();

            if (searchFolders.Count == 0) {
                Debug.LogWarning($"{nameof(AudioBank)} '{name}': can not search audio clips, no search folders are set.", this);
                return assetPaths;
            }

            string[] guids = AssetDatabase.FindAssets(AudioClipFilter, searchFolders.ToArray());
            var visitedPaths = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (IsInFolders(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                assetPaths.Add(assetPath);
            }

            return assetPaths;
        }

        /// <summary>
        /// Gathers the clips the presets asked to preload. A bank preloaded clip has its preload option off,
        /// so at runtime it is indistinguishable from a clip that is not preloaded at all, and the list is the
        /// only thing <see cref="PreloadAudioData"/> can rely on. Asset preloaded clips are kept in the list
        /// as well: <see cref="UnloadAudioData"/> needs them, and the reference makes the bank load them.
        /// </summary>
        private void CollectPreloadClips(List<string> clipPaths) {
            var clips = new List<AudioClip>(clipPaths.Count);

            for (int i = 0; i < clipPaths.Count; i++) {
                string assetPath = clipPaths[i];

                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null) continue;

                if (!TryGetPreset(assetPath, clip, out var preset) || preset.loadTime == LoadTime.NoPreload) continue;

                clips.Add(clip);
            }

            _preloadClips = clips.ToArray();

            UpdatePreloadClipsSize();
        }

        /// <summary>
        /// Measures what the preloaded clips cost in memory, before and after the compression the presets ask for.
        /// <para>
        /// The uncompressed size is the raw PCM the clip is built from: samples of every channel, 16 bit each.
        /// The compressed one is what the clip takes in memory as it is imported now, so for a clip decompressed
        /// on load the two are the same — such a clip is compressed on disk only.
        /// </para>
        /// </summary>
        private void UpdatePreloadClipsSize() {
            long uncompressed = 0L;
            long compressed = 0L;

            for (int i = 0; i < _preloadClips?.Length; i++) {
                var clip = _preloadClips[i];
                if (clip == null) continue;

                uncompressed += (long) clip.samples * clip.channels * PcmBytesPerSample;

                if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip)) is AudioImporter importer) {
                    compressed += GetAudioDataSize(clip, importer.defaultSampleSettings);
                }
            }

            _preloadClipsSize = uncompressed > 0L
                ? $"{EditorUtility.FormatBytes(uncompressed)} uncompressed, " +
                  $"~{EditorUtility.FormatBytes(compressed)} compressed ({compressed * 100L / uncompressed}%)"
                : EditorUtility.FormatBytes(0L);
        }

        /// <summary>
        /// Size of the audio data of a clip in memory, counted from its import settings: decompressed samples
        /// for a clip decompressed on load, the compressed data for a clip kept compressed in memory, and
        /// nothing for a streamed one, which reads the file while it plays.
        /// </summary>
        private static long GetAudioDataSize(AudioClip clip, in AudioImporterSampleSettings settings) {
            long samples = (long) clip.samples * clip.channels;

            return settings.loadType switch {
                AudioClipLoadType.Streaming => 0L,
                AudioClipLoadType.DecompressOnLoad => samples * PcmBytesPerSample,
                _ => GetCompressedSize(clip, samples, settings),
            };
        }

        private static long GetCompressedSize(AudioClip clip, long samples, in AudioImporterSampleSettings settings) {
            return settings.compressionFormat switch {
                AudioCompressionFormat.PCM => samples * PcmBytesPerSample,
                AudioCompressionFormat.ADPCM => (long) (samples * PcmBytesPerSample / AdpcmCompressionRatio),
                AudioCompressionFormat.Vorbis or AudioCompressionFormat.MP3 => GetVariableBitrateSize(clip, settings.quality),

                // Console formats are not counted format by format: raw samples are the upper bound for them.
                _ => samples * PcmBytesPerSample,
            };
        }

        /// <summary>
        /// Estimated size of a variable bitrate format. Unlike PCM and ADPCM, it does not follow from the sample
        /// count: the bitrate is taken from the quality the clip is imported with, and scaled by the channels
        /// and the sample rate of the clip, so the number is an estimate rather than a measurement.
        /// </summary>
        private static long GetVariableBitrateSize(AudioClip clip, float quality) {
            float point = Mathf.Clamp01(quality) * (VorbisBitratesKbps.Length - 1);
            int index = Mathf.FloorToInt(point);
            int nextIndex = Mathf.Min(index + 1, VorbisBitratesKbps.Length - 1);

            float bitrate = Mathf.Lerp(VorbisBitratesKbps[index], VorbisBitratesKbps[nextIndex], point - index) * 1000f;
            float scale = clip.channels / (float) ReferenceChannels * clip.frequency / ReferenceFrequency;

            return (long) (clip.length * bitrate * scale / 8f);
        }

        /// <summary>
        /// Picks the preset of a clip: the settings listed for the clip itself win over the preset picked
        /// by the clip duration, and a clip out of all the duration ranges gets the default preset.
        /// </summary>
        private bool TryGetPreset(string assetPath, AudioClip clip, out Preset preset) {
            preset = default;

            if (clip == null ||
                !IsInFolders(assetPath, GetFolderPaths(_searchFolders)) ||
                IsInFolders(assetPath, GetFolderPaths(_excludeFolders))
            ) {
                return false;
            }

            for (int i = 0; i < _clipPresets?.Length; i++) {
                if (!_clipPresets[i].Contains(clip)) continue;

                preset = _clipPresets[i].preset;
                return true;
            }

            for (int i = 0; i < _customPresets?.Length; i++) {
                if (!_customPresets[i].Contains(clip.length)) continue;

                preset = _customPresets[i].preset;
                return true;
            }

            preset = _defaultPreset;
            return true;
        }

        private static bool NeedsPreset(AudioImporter importer, in Preset preset) {
            var sampleSettings = importer.defaultSampleSettings;

            return sampleSettings.loadType != preset.loadType ||
                   sampleSettings.compressionFormat != preset.compressionFormat ||
                   sampleSettings.preloadAudioData != preset.PreloadAudioData ||
                   importer.loadInBackground != preset.loadInBackground;
        }

        private static void ApplyPreset(AudioImporter importer, in Preset preset) {
            var sampleSettings = importer.defaultSampleSettings;

            sampleSettings.loadType = preset.loadType;
            sampleSettings.compressionFormat = preset.compressionFormat;
            sampleSettings.preloadAudioData = preset.PreloadAudioData;

            importer.defaultSampleSettings = sampleSettings;
            importer.loadInBackground = preset.loadInBackground;
        }

        private static List<string> GetFolderPaths(DefaultAsset[] folders) {
            var folderPaths = new List<string>();
            if (folders == null) return folderPaths;

            for (int i = 0; i < folders.Length; i++) {
                var folder = folders[i];
                if (folder == null) continue;

                string folderPath = AssetDatabase.GetAssetPath(folder);

                // DefaultAsset refers not only to folders, so a file assigned to this field does not count as a path.
                if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                if (!folderPaths.Contains(folderPath)) folderPaths.Add(folderPath);
            }

            return folderPaths;
        }

        private static bool IsInFolders(string assetPath, List<string> folderPaths) {
            for (int i = 0; i < folderPaths.Count; i++) {
                if (assetPath.StartsWith(folderPaths[i] + "/", StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private void OnValidate() {
            InvalidateBankCache();
        }

        private static AudioBank[] _bankCache;

        private static void InvalidateBankCache() {
            _bankCache = null;
        }

        private static AudioBank[] GetBanks() {
            if (_bankCache is { Length: > 0 }) return _bankCache;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(AudioBank)}");
            var banks = new List<AudioBank>(guids.Length);

            for (int i = 0; i < guids.Length; i++) {
                var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (bank == null) continue;

                banks.Add(bank);
            }

            // An empty result is not cached: the asset database also answers with nothing when it is queried
            // at a moment it can not be queried at, and a cached emptiness would silence the postprocessor
            // for the rest of the editor session.
            return banks.Count > 0 ? _bankCache = banks.ToArray() : Array.Empty<AudioBank>();
        }

        private static bool TryGetPresetFromBanks(string assetPath, AudioClip clip, out Preset preset) {
            var banks = GetBanks();

            for (int i = 0; i < banks.Length; i++) {
                if (banks[i] != null && banks[i].TryGetPreset(assetPath, clip, out preset)) return true;
            }

            preset = default;
            return false;
        }

        /// <summary>
        /// Applies the presets to the audio clips that were just imported, so that clips added to the bank
        /// folders get the bank settings right away, without waiting for a manual reimport.
        /// </summary>
        private sealed class AudioBankPostprocessor : AssetPostprocessor {

            private static readonly HashSet<string> PendingPaths = new();
            private static bool _applyScheduled;

            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (HasBanks(importedAssets) || HasBanks(movedAssets) ||
                    HasAssets(deletedAssets) || HasAssets(movedFromAssetPaths)
                ) {
                    InvalidateBankCache();
                }

                CollectAudioClips(importedAssets);
                CollectAudioClips(movedAssets);

                if (PendingPaths.Count == 0 || _applyScheduled) return;

                // The presets are applied outside the import callback: writing the settings reimports the clips,
                // and an import can not be started from inside another one.
                _applyScheduled = true;
                EditorApplication.delayCall += ApplyPendingPresets;
            }

            private static void CollectAudioClips(string[] assets) {
                for (int i = 0; i < assets?.Length; i++) {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assets[i]) == typeof(AudioClip)) PendingPaths.Add(assets[i]);
                }
            }

            private static void ApplyPendingPresets() {
                _applyScheduled = false;

                // Applying the presets imports the clips back, and this very callback fills the set again:
                // on that pass the clips already match their presets and nothing is scheduled anymore.
                string[] assetPaths = new string[PendingPaths.Count];
                PendingPaths.CopyTo(assetPaths);
                PendingPaths.Clear();

                ApplyPresets(bank: null, assetPaths, showProgress: false);
            }

            private static bool HasBanks(string[] assets) {
                for (int i = 0; i < assets?.Length; i++) {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assets[i]) == typeof(AudioBank)) return true;
                }

                return false;
            }

            // A deleted or a moved away bank can not be loaded to check its type anymore.
            private static bool HasAssets(string[] assets) {
                for (int i = 0; i < assets?.Length; i++) {
                    if (assets[i]?.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) ?? false) return true;
                }

                return false;
            }
        }
#endif
    }

}
