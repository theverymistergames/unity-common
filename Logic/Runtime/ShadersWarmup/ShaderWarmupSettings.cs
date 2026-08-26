using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.ShadersWarmup {

    [CreateAssetMenu(fileName = nameof(ShaderWarmupSettings), menuName = "MisterGames/Shaders/" + nameof(ShaderWarmupSettings))]
    internal sealed class ShaderWarmupSettings : ScriptableObject {

        [Header("Tracing")]
        [SerializeField] [ReadOnly] private string _filepath = "Shaders/pso_tracing.graphicsstate";
        [SerializeField] private bool _enableTracingInDevBuild = true;
        [SerializeField] [Min(0f)] private float _traceSavePeriod = 10f;
#if UNITY_EDITOR
        [SerializeField] private DefaultAsset[] _searchInFolders;
        [SerializeField] private DefaultAsset[] _searchScenesFolders;
        [SerializeField] private DefaultAsset[] _searchVolumeFolders;
        [SerializeField] private DefaultAsset[] _excludeFolders;
        [SerializeField] private DefaultAsset _generatedContentFolder;
#endif
        [SerializeField] private Shader[] _manualShaders = Array.Empty<Shader>();

#if UNITY_EDITOR
        [Header("Player Logs")]
        // Player logs are the second source of variants for the release collection: tracing writes keywords
        // the way the render loop sees them, and some variants never make it into the trace (keywords enabled
        // by the build keyword filter, for instance). The log lists exactly what the player did compile.
        // A relative path is resolved against persistentDataPath, where a build from this machine writes its log.
        [SerializeField] private string[] _playerLogFiles = { "Player.log", "Player-prev.log" };

        // Skip Hidden/* shaders: engine passes, VFX graph shaders and everything else that is not wanted
        // in the collection.
        [SerializeField] private bool _skipHiddenShadersFromPlayerLogs;
#endif

        [Header("Shaders Warmup Scene")]
        [SerializeField] private SceneReference _warmupScene;
        [SerializeField] private bool _enterShadersWarmupSceneOnBootstrapInDevBuild;
        [SerializeField] private bool _enterShadersWarmupSceneOnBootstrapInEditor;

        [Header("Shaders Warmup Progress View")]
        [SerializeField] [Min(0f)] private float _simulateShadersWarmupInEditorDuration = 5f;
        [SerializeField] [Min(0f)] private float _enableViewDelay = 0.5f;
        [SerializeField] [Min(0f)] private float _disableViewDelay = 0.25f;
        [SerializeField] [Min(0f)] private float _enableViewFader = 0.25f;
        [SerializeField] [Min(0f)] private float _disableViewFader = 0.25f;

        [Header("Warmup Collections")]
        [SerializeField] private GraphicsStateCollection _releaseGraphicsStateCollection;
        [SerializeField] private VisualEffectAsset[] _visualEffectAssets;
        [SerializeField] [Min(0)] private int _warmupStartDelayFrames = 30;
        [SerializeField] [Min(1)] private int _progressiveWarmupBatchCountPso = 128;
        [SerializeField] [Min(1)] private int _progressiveWarmupBatchCountVisualEffectAssets = 32;

        [Header("Addressables")]
        // Warmup collection and vfx asset list reference assets directly, so those assets are built into
        // the player along with the bootstrap scene, while the game loads its own copy from a bundle.
        // These are different objects, and warming up the first one does not help the second.
        // With this option the service takes the same assets through Addressables and warms them up instead.
        [SerializeField] private bool _useAddressablesForWarmup = true;
        [SerializeField] [ReadOnly] private string[] _addressableShaderKeys = Array.Empty<string>();
        [SerializeField] [ReadOnly] private string[] _addressableVisualEffectKeys = Array.Empty<string>();

        private const string VisualEffectAssetFilter = "t:VisualEffectAsset";

        public bool EnableTracingInDevBuild => _enableTracingInDevBuild;
        public int ProgressiveWarmupBatchCountPso => _progressiveWarmupBatchCountPso;
        public int ProgressiveWarmupBatchCountVisualEffectAssets => _progressiveWarmupBatchCountVisualEffectAssets;
        public float SavePeriod => _traceSavePeriod;
        public bool EnterShadersWarmupSceneOnBootstrapInDevBuild => _enterShadersWarmupSceneOnBootstrapInDevBuild;
        public bool EnterShadersWarmupSceneOnBootstrapInEditor => _enterShadersWarmupSceneOnBootstrapInEditor;
        public float SimulateShadersWarmupInEditorDuration => _simulateShadersWarmupInEditorDuration;
        public string WarmupSceneName => _warmupScene.scene;
        public int WarmupStartDelayFrames => _warmupStartDelayFrames;
        public VisualEffectAsset[] VisualEffectAssets => _visualEffectAssets ?? Array.Empty<VisualEffectAsset>();

        public bool UseAddressablesForWarmup => _useAddressablesForWarmup;
        public string[] AddressableShaderKeys => _addressableShaderKeys ?? Array.Empty<string>();
        public string[] AddressableVisualEffectKeys => _addressableVisualEffectKeys ?? Array.Empty<string>();

        public float EnableViewDelay => _enableViewDelay;
        public float DisableViewDelay => _disableViewDelay;
        public float EnableViewFader => _enableViewFader;
        public float DisableViewFader => _disableViewFader;

        public Shader[] ManualShaders => _manualShaders ?? Array.Empty<Shader>();

        public string GetTracingFilePath() {
            return Path.Combine(Application.persistentDataPath, _filepath);
        }

        public GraphicsStateCollection GetReleaseGraphicsStateCollection() {
            return _releaseGraphicsStateCollection ?? new GraphicsStateCollection();
        }

#if UNITY_EDITOR
        private const string ShaderFilter = "t:Shader";
        private const string NameTagName = "Name";
        private const string LightModeTagName = "LightMode";
        private const string HiddenShaderPrefix = "Hidden/";
        private const string NoKeywordsMarker = "<no keywords>";

        // How many names to print in a report before cutting the list off with an ellipsis.
        private const int ReportSampleSize = 10;

        // The line a player prints for every variant it actually compiled. Written while Log Shader Compilation
        // is enabled in Graphics Settings:
        // Uploaded shader variant to the GPU driver: Shader Graphs/Snow (instance 0x4BFECA),
        //     pass: Forward, stage: vertex, keywords USE_LEGACY_LIGHTMAPS, time: 0.11 ms
        private static readonly Regex UploadedVariantRegex = new(
            @"^Uploaded shader variant to the GPU driver: (?<shader>.+?) \(instance [^)]*\), pass: (?<pass>[^,]+), stage: (?<stage>\w+), keywords (?<keywords>.*), time:",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // An unnamed pass is printed as <Unnamed Pass 3>: there is no name to look up, but the caption
        // itself carries the pass index.
        private static readonly Regex UnnamedPassRegex = new(
            @"^<Unnamed Pass (?<index>\d+)>$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public DefaultAsset[] GetContentFolders() {
            return _searchInFolders;
        }

        public DefaultAsset[] GetSceneFolders() {
            return _searchScenesFolders;
        }

        // Volume profiles usually live outside the art folders, hence a separate field.
        // Empty means they are searched in the content folders.
        public DefaultAsset[] GetVolumeFolders() {
            return _searchVolumeFolders;
        }

        public List<string> GetExcludeFolderPaths() {
            var folderPaths = GetFolderPaths(_excludeFolders);
            string generatedContentFolder = GetGeneratedContentFolderPath();

            // Generated materials and meshes are a search result themselves: collecting them back would keep
            // them in the warmup scene even after the shaders they were created for are gone from the project.
            if (!string.IsNullOrEmpty(generatedContentFolder) && !folderPaths.Contains(generatedContentFolder)) {
                folderPaths.Add(generatedContentFolder);
            }

            return folderPaths;
        }

        public string GetGeneratedContentFolderPath() {
            if (_generatedContentFolder == null) return string.Empty;

            string folderPath = AssetDatabase.GetAssetPath(_generatedContentFolder);

            // DefaultAsset refers not only to folders, so a file assigned to this field does not count as a path.
            return AssetDatabase.IsValidFolder(folderPath) ? folderPath : string.Empty;
        }

        [Button]
        internal void SearchVisualEffectAssets() {
            var searchInFolders = GetFolderPaths(_searchInFolders);
            var excludeFolders = GetExcludeFolderPaths();
            var assetPaths = CollectVisualEffectAssetPaths(searchInFolders, excludeFolders);
            var assets = new List<VisualEffectAsset>(assetPaths.Count);

            for (int i = 0; i < assetPaths.Count; i++) {
                var asset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(assetPaths[i]);
                if (asset == null) continue;

                assets.Add(asset);
            }

            int countBefore = _visualEffectAssets?.Length ?? 0;
            _visualEffectAssets = assets.ToArray();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            Debug.Log($"{nameof(ShaderWarmupSettings)}: collected {_visualEffectAssets.Length} VisualEffectAssets " +
                      $"(were {countBefore}) from {searchInFolders.Count} folders (excluded folders: {excludeFolders.Count}).");
        }

        private static List<string> CollectVisualEffectAssetPaths(List<string> searchInFolders, List<string> excludeFolders) {
            string[] guids = AssetDatabase.FindAssets(VisualEffectAssetFilter, searchInFolders.ToArray());
            var assetPaths = new List<string>(guids.Length);

            var visitedPaths = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                assetPaths.Add(assetPath);
            }

            assetPaths.Sort(StringComparer.Ordinal);
            return assetPaths;
        }

        private static bool IsExcluded(string assetPath, List<string> excludeFolders) {
            for (int i = 0; i < excludeFolders.Count; i++) {
                if (assetPath.StartsWith(excludeFolders[i] + "/", StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private static List<string> GetFolderPaths(DefaultAsset[] folders) {
            var folderPaths = new List<string>();
            if (folders == null) return folderPaths;

            for (int i = 0; i < folders.Length; i++) {
                var folder = folders[i];
                if (folder == null) continue;

                string folderPath = AssetDatabase.GetAssetPath(folder);
                if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                if (!folderPaths.Contains(folderPath)) folderPaths.Add(folderPath);
            }

            return folderPaths;
        }

        /// <summary>
        /// Fills the release collection from two sources at once: the tracing file and the player logs.
        /// <para>
        /// The tracing file alone is not enough. The tracer writes the keyword set the way the render loop sees it,
        /// and part of the variants disagree with what the player actually compiles: a keyword enabled by the build
        /// keyword filter (USE_LEGACY_LIGHTMAPS in HDRP) does not get into the traced record, warmup warms up a
        /// twin variant, and the real one is still compiled in a frame. The player log lists exactly what was
        /// compiled, so the missing part is picked up from there.
        /// </para>
        /// </summary>
        [Button]
        internal void AppendTracedShadersToReleaseCollection() {
            if (_releaseGraphicsStateCollection == null) {
                Debug.LogError($"{nameof(ShaderWarmupSettings)}: Release Graphics State Collection should not be null. " +
                               $"Create new collection with Create/Shader/Graphics State Collection and assign it to the Release Graphics State Collection field.");
                return;
            }

            int releaseCountBefore = _releaseGraphicsStateCollection.variantCount;

            string tracingReport = AppendTracingCollection();
            string playerLogReport = AppendPlayerLogVariants();

            int releaseCountAfter = _releaseGraphicsStateCollection.variantCount;

            SaveReleaseCollection();

            Debug.Log($"{nameof(ShaderWarmupSettings)}: added {releaseCountAfter - releaseCountBefore} new variants " +
                      $"to the release collection (total variants now is {releaseCountAfter}). {tracingReport} {playerLogReport} " +
                      $"New variants bring new shaders: collect addressable keys again with " +
                      $"MisterGames/Shaders Warmup, otherwise warmup takes them by direct reference, " +
                      $"that is not the copy the game renders with.");
        }

        /// <summary>
        /// The original source: the tracing file a dev build writes into persistentDataPath.
        /// A missing file no longer aborts the whole button, player logs are parsed either way.
        /// </summary>
        private string AppendTracingCollection() {
            string filePath = GetTracingFilePath();

            if (!File.Exists(filePath)) {
                Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: tracing file not found at [{filePath}]. " +
                                 $"This file contains traced shaders. Lack of this file means shaders were not traced yet or the file was deleted. " +
                                 $"To trace shaders, launch dev build and render needed shaders, " +
                                 $"they will be saved into [{filePath}] during the play session. " +
                                 $"Nothing is added to the release collection from tracing.");

                return $"Tracing file [{filePath}] is not found, nothing added from tracing.";
            }

            var tracingCollection = new GraphicsStateCollection();
            tracingCollection.LoadFromFile(filePath);

            int countBefore = _releaseGraphicsStateCollection.variantCount;
            _releaseGraphicsStateCollection.Append(tracingCollection);
            int added = _releaseGraphicsStateCollection.variantCount - countBefore;

            return $"From tracing file [{filePath}] ({tracingCollection.variantCount} variants) added {added}.";
        }

        /// <summary>
        /// The second source: "Uploaded shader variant to the GPU driver" lines from player logs. They are written
        /// while Log Shader Compilation is enabled in Graphics Settings and contain everything a variant needs:
        /// shader, pass, stage and keywords.
        /// <para>
        /// Vertex and pixel lines of the same pass are deliberately not merged into a single variant: in the log
        /// every stage has its own keyword set (the engine drops the ones that do not affect the stage), and stages
        /// are uploaded separately. Merging by "shader + pass" would mix up different materials and produce sets
        /// that do not exist in the game, so every line is added as its own variant.
        /// </para>
        /// </summary>
        private string AppendPlayerLogVariants() {
            var logFiles = ResolvePlayerLogFiles();

            if (logFiles.Count == 0) {
                return "Player logs were not parsed: no file from Player Log Files is found.";
            }

            var loggedVariants = new Dictionary<string, LoggedVariant>(StringComparer.Ordinal);
            int matchedLines = 0;

            for (int i = 0; i < logFiles.Count; i++) {
                matchedLines += CollectVariantsFromPlayerLog(logFiles[i], loggedVariants);
            }

            // ShaderTagId calls TagToID, so tags are created here and not in a field initializer.
            var nameTag = new ShaderTagId(NameTagName);
            var lightModeTag = new ShaderTagId(LightModeTagName);

            var shaderCache = new Dictionary<string, Shader>(StringComparer.Ordinal);
            var missingShaders = new HashSet<string>(StringComparer.Ordinal);
            var missingPasses = new HashSet<string>(StringComparer.Ordinal);
            var droppedKeywords = new HashSet<string>(StringComparer.Ordinal);

            Dictionary<string, Shader> projectShaders = null;

            int added = 0;
            int alreadyPresent = 0;
            int rejected = 0;
            int skippedHidden = 0;

            foreach (var loggedVariant in loggedVariants.Values) {
                if (_skipHiddenShadersFromPlayerLogs && loggedVariant.shaderName.StartsWith(HiddenShaderPrefix, StringComparison.Ordinal)) {
                    skippedHidden++;
                    continue;
                }

                var shader = ResolveShader(loggedVariant.shaderName, shaderCache, ref projectShaders);

                if (shader == null) {
                    missingShaders.Add(loggedVariant.shaderName);
                    continue;
                }

                if (!TryResolvePass(shader, loggedVariant.passName, nameTag, lightModeTag, out var passId)) {
                    missingPasses.Add($"{loggedVariant.shaderName} -> {loggedVariant.passName}");
                    continue;
                }

                var keywords = ResolveKeywords(shader, loggedVariant.keywordNames, droppedKeywords);

                if (_releaseGraphicsStateCollection.ContainsVariant(shader, passId, keywords)) {
                    alreadyPresent++;
                    continue;
                }

                if (_releaseGraphicsStateCollection.AddVariant(shader, passId, keywords)) added++;
                else rejected++;
            }

            string report = $"From player logs ({string.Join(", ", logFiles)}) parsed {matchedLines} lines, " +
                            $"unique variants {loggedVariants.Count}, added {added}, " +
                            $"already present in the collection {alreadyPresent}.";

            if (skippedHidden > 0) report += $" Skipped hidden shaders: {skippedHidden}.";
            if (rejected > 0) report += $" Rejected by the collection: {rejected}.";

            if (missingShaders.Count > 0) {
                report += $" Shaders not found by name: {missingShaders.Count} ({FormatSample(missingShaders)}).";
            }

            if (missingPasses.Count > 0) {
                report += $" Passes not found by name (matched by Name and LightMode tags): " +
                          $"{missingPasses.Count} ({FormatSample(missingPasses)}).";
            }

            if (droppedKeywords.Count > 0) {
                report += $" Keywords not declared by their shaders and therefore dropped: " +
                          $"{droppedKeywords.Count} ({FormatSample(droppedKeywords)}).";
            }

            return report;
        }

        private List<string> ResolvePlayerLogFiles() {
            var files = new List<string>();
            if (_playerLogFiles == null) return files;

            for (int i = 0; i < _playerLogFiles.Length; i++) {
                string entry = _playerLogFiles[i];
                if (string.IsNullOrWhiteSpace(entry)) continue;

                // A relative path is resolved against persistentDataPath: a build from this machine writes
                // its log there, and the tracing file lands there as well.
                string path = Path.IsPathRooted(entry) ? entry : Path.Combine(Application.persistentDataPath, entry);

                if (!File.Exists(path)) {
                    Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: player log [{path}] is not found, skipped.");
                    continue;
                }

                if (!files.Contains(path)) files.Add(path);
            }

            return files;
        }

        /// <summary> Returns the count of parsed lines, the variants themselves go into <paramref name="variants"/>. </summary>
        private static int CollectVariantsFromPlayerLog(string filePath, Dictionary<string, LoggedVariant> variants) {
            int matched = 0;

            try {
                // FileShare.ReadWrite: the log can be held open by a running game.
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                string line;

                while ((line = reader.ReadLine()) != null) {
                    var match = UploadedVariantRegex.Match(line);
                    if (!match.Success) continue;

                    matched++;

                    string shaderName = match.Groups["shader"].Value;
                    string passName = match.Groups["pass"].Value;
                    var keywordNames = ParseKeywordNames(match.Groups["keywords"].Value);

                    string key = $"{shaderName}|{passName}|{string.Join(" ", keywordNames)}";
                    if (!variants.ContainsKey(key)) variants.Add(key, new LoggedVariant(shaderName, passName, keywordNames));
                }
            }
            catch (IOException exception) {
                Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: cannot read player log [{filePath}]: {exception.Message}. File skipped.");
            }

            return matched;
        }

        /// <summary> Keywords come in arbitrary order in the log, sorting is needed for the dedup key to match. </summary>
        private static string[] ParseKeywordNames(string keywords) {
            if (string.IsNullOrWhiteSpace(keywords) || keywords.StartsWith(NoKeywordsMarker, StringComparison.Ordinal)) {
                return Array.Empty<string>();
            }

            string[] names = keywords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(names, StringComparer.Ordinal);

            return names;
        }

        private static Shader ResolveShader(string shaderName, Dictionary<string, Shader> cache, ref Dictionary<string, Shader> projectShaders) {
            if (cache.TryGetValue(shaderName, out var cached)) return cached;

            var shader = Shader.Find(shaderName);

            // Shader.Find does not see sub asset shaders, VFX graphs compile into such shaders for example.
            // The project index is built lazily: it is not free and it is not always needed.
            if (shader == null) {
                projectShaders ??= BuildProjectShaderIndex();
                projectShaders.TryGetValue(shaderName, out shader);
            }

            cache[shaderName] = shader;
            return shader;
        }

        /// <summary>
        /// Shader name to shader, over all project shaders including sub assets. VFX assets are added separately:
        /// their shaders live inside the .vfx file and do not match the t:Shader filter.
        /// </summary>
        private static Dictionary<string, Shader> BuildProjectShaderIndex() {
            var index = new Dictionary<string, Shader>(StringComparer.Ordinal);

            AddShadersFromAssets(AssetDatabase.FindAssets(ShaderFilter), index);
            AddShadersFromAssets(AssetDatabase.FindAssets(VisualEffectAssetFilter), index);

            return index;
        }

        private static void AddShadersFromAssets(string[] guids, Dictionary<string, Shader> index) {
            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                for (int j = 0; j < assets.Length; j++) {
                    if (assets[j] is Shader shader && !string.IsNullOrEmpty(shader.name)) index[shader.name] = shader;
                }
            }
        }

        /// <summary>
        /// The log prints a pass name, and there is no direct "name to index" in the API. Lookup goes in three steps:
        /// by the Name tag (ShaderLab puts the pass name there), then by the LightMode tag (in HDRP and Shader Graph
        /// it matches the pass name), then by the index from an unnamed pass caption. The passes are separate so that
        /// an exact name match does not lose to an accidental LightMode match.
        /// </summary>
        private static bool TryResolvePass(Shader shader, string passName, ShaderTagId nameTag, ShaderTagId lightModeTag,
            out PassIdentifier passId)
        {
            if (TryResolvePassByTag(shader, passName, nameTag, out passId)) return true;
            if (TryResolvePassByTag(shader, passName, lightModeTag, out passId)) return true;

            var unnamedMatch = UnnamedPassRegex.Match(passName);

            if (unnamedMatch.Success
                && int.TryParse(unnamedMatch.Groups["index"].Value, out int passIndex)
                && shader.subshaderCount > 0
                && passIndex < shader.GetPassCountInSubshader(0))
            {
                passId = new PassIdentifier(0u, (uint) passIndex);
                return true;
            }

            passId = default;
            return false;
        }

        private static bool TryResolvePassByTag(Shader shader, string passName, ShaderTagId tag, out PassIdentifier passId) {
            for (int subshader = 0; subshader < shader.subshaderCount; subshader++) {
                int passCount = shader.GetPassCountInSubshader(subshader);

                for (int pass = 0; pass < passCount; pass++) {
                    string tagValue = shader.FindPassTagValue(subshader, pass, tag).name;

                    if (string.IsNullOrEmpty(tagValue)) continue;
                    if (!string.Equals(tagValue, passName, StringComparison.OrdinalIgnoreCase)) continue;

                    passId = new PassIdentifier((uint) subshader, (uint) pass);
                    return true;
                }
            }

            passId = default;
            return false;
        }

        /// <summary>
        /// A keyword that is not in the shader keyword space is dropped: it was enabled globally in the frame,
        /// but it does not affect variants of this shader, and an invalid LocalKeyword is not accepted
        /// by the collection.
        /// </summary>
        private static LocalKeyword[] ResolveKeywords(Shader shader, string[] keywordNames, HashSet<string> droppedKeywords) {
            if (keywordNames.Length == 0) return Array.Empty<LocalKeyword>();

            var keywordSpace = shader.keywordSpace;
            var keywords = new List<LocalKeyword>(keywordNames.Length);

            for (int i = 0; i < keywordNames.Length; i++) {
                var keyword = keywordSpace.FindKeyword(keywordNames[i]);

                if (!keyword.isValid) {
                    droppedKeywords.Add(keywordNames[i]);
                    continue;
                }

                keywords.Add(keyword);
            }

            return keywords.ToArray();
        }

        private static string FormatSample(HashSet<string> values) {
            var sample = new List<string>(ReportSampleSize);

            foreach (string value in values) {
                if (sample.Count == ReportSampleSize) break;
                sample.Add(value);
            }

            return values.Count > sample.Count ? $"{string.Join(", ", sample)}, ..." : string.Join(", ", sample);
        }

        /// <summary>
        /// Rewrites the release collection without adding anything to it, only to stamp the metadata.
        /// Needed once, to fix a collection built before the stamp existed: after that the metadata is written
        /// on every save by itself.
        /// </summary>
        [Button]
        private void StampReleaseCollection() {
            if (_releaseGraphicsStateCollection == null) {
                Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: Release Graphics State Collection should not be null.");
                return;
            }

            SaveReleaseCollection();

            Debug.Log($"{nameof(ShaderWarmupSettings)}: release collection is stamped with " +
                      $"device [{_releaseGraphicsStateCollection.graphicsDeviceType}], " +
                      $"platform [{_releaseGraphicsStateCollection.runtimePlatform}], " +
                      $"quality level [{_releaseGraphicsStateCollection.qualityLevelName}].");
        }

        private void SaveReleaseCollection() {
            string assetPath = AssetDatabase.GetAssetPath(_releaseGraphicsStateCollection);
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            StampReleaseCollectionForBuildTarget();

            _releaseGraphicsStateCollection.SaveToFile(fullPath);
        }

        /// <summary>
        /// Stamps the collection with the device, platform and quality level it was captured for.
        /// Without it the collection keeps default values (device Null), and the player prints three
        /// mismatch warnings with a stack trace on every warmup batch.
        /// <para>
        /// The quality level is the current editor one: if the player starts at another level, the warning
        /// comes back, and that is an honest signal that the collection was captured in different settings.
        /// </para>
        /// </summary>
        private void StampReleaseCollectionForBuildTarget() {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var graphicsApis = PlayerSettings.GetGraphicsAPIs(buildTarget);

            _releaseGraphicsStateCollection.graphicsDeviceType =
                graphicsApis != null && graphicsApis.Length > 0 ? graphicsApis[0] : SystemInfo.graphicsDeviceType;

            _releaseGraphicsStateCollection.runtimePlatform = GetRuntimePlatform(buildTarget);
            _releaseGraphicsStateCollection.qualityLevelName = GetCurrentQualityLevelName();
        }

        private static RuntimePlatform GetRuntimePlatform(BuildTarget buildTarget) {
            switch (buildTarget) {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return RuntimePlatform.WindowsPlayer;

                case BuildTarget.StandaloneOSX:
                    return RuntimePlatform.OSXPlayer;

                case BuildTarget.StandaloneLinux64:
                    return RuntimePlatform.LinuxPlayer;

                default:
                    return Application.platform;
            }
        }

        private static string GetCurrentQualityLevelName() {
            string[] names = QualitySettings.names;
            int level = QualitySettings.GetQualityLevel();

            return names != null && level >= 0 && level < names.Length ? names[level] : string.Empty;
        }

        /// <summary> Unique shaders of the release collection, used by editor tools that build addressable keys. </summary>
        internal List<Shader> CollectReleaseCollectionShaders() {
            var shaders = new List<Shader>();

            if (_releaseGraphicsStateCollection == null) return shaders;

            var variants = new List<GraphicsStateCollection.ShaderVariant>();
            _releaseGraphicsStateCollection.GetVariants(variants);

            var visited = new HashSet<Shader>();

            for (int i = 0; i < variants.Count; i++) {
                var shader = variants[i].shader;

                if (shader == null || !visited.Add(shader)) continue;

                shaders.Add(shader);
            }

            return shaders;
        }

        /// <summary>
        /// Called by the editor tool that resolves addressable keys: the tool needs the Addressables editor
        /// assembly, which a runtime assembly cannot reference.
        /// </summary>
        internal void SetAddressableKeys(string[] shaderKeys, string[] visualEffectKeys) {
            _addressableShaderKeys = shaderKeys ?? Array.Empty<string>();
            _addressableVisualEffectKeys = visualEffectKeys ?? Array.Empty<string>();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        [Button]
        private void ClearReleaseCollection() {
            if (_releaseGraphicsStateCollection == null) {
                Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: Release Graphics State Collection should not be null.");
                return;
            }

            _releaseGraphicsStateCollection.ClearVariants();

            SaveReleaseCollection();

            Debug.Log($"{nameof(ShaderWarmupSettings)}: release collection was cleared successfully.");
        }

        /// <summary> A variant line from a player log, before it is resolved into shader, pass and keywords. </summary>
        private readonly struct LoggedVariant {

            public readonly string shaderName;
            public readonly string passName;
            public readonly string[] keywordNames;

            public LoggedVariant(string shaderName, string passName, string[] keywordNames) {
                this.shaderName = shaderName;
                this.passName = passName;
                this.keywordNames = keywordNames;
            }
        }
#endif
    }

}
