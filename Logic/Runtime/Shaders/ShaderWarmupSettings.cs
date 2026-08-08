using System;
using System.Collections.Generic;
using System.IO;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Shaders {
 
    [CreateAssetMenu(fileName = nameof(ShaderWarmupSettings), menuName = "MisterGames/Shaders/" + nameof(ShaderWarmupSettings))]
    internal sealed class ShaderWarmupSettings : ScriptableObject {
        
        [Header("Tracing")]
        [SerializeField] [ReadOnly] private string _filepath = "Shaders/pso_tracing.graphicsstate";
        [SerializeField] [Min(0f)] private float _traceSavePeriod = 10f;
#if UNITY_EDITOR
        [SerializeField] private DefaultAsset[] _searchInFolders;
        [SerializeField] private DefaultAsset[] _searchScenesFolders;
        [SerializeField] private DefaultAsset[] _excludeFolders;
#endif
        [SerializeField] private string _generatedContentFolder;
        [SerializeField] private Shader[] _manualShaders = Array.Empty<Shader>();
        [SerializeField] private Shader[] _hiddenShaders = Array.Empty<Shader>();

        [Header("Shaders Warmup Scene")]
        [SerializeField] private SceneReference _warmupScene;
        [SerializeField] private bool _enterShadersWarmupSceneOnBootstrapInDevBuild;
        [SerializeField] private bool _enterShadersWarmupSceneOnBootstrapInEditor;

        [Header("Warmup Collections")]
        [SerializeField] private GraphicsStateCollection _releaseGraphicsStateCollection;
        [SerializeField] private VisualEffectAsset[] _visualEffectAssets;
        [SerializeField] [Min(1)] private int _progressiveWarmupBatchCountPso = 128;
        [SerializeField] [Min(1)] private int _progressiveWarmupBatchCountVisualEffectAssets = 32;

        private const string VisualEffectAssetFilter = "t:VisualEffectAsset";
        private const string HiddenShaderPrefix = "Hidden/";

        public int ProgressiveWarmupBatchCountPso => _progressiveWarmupBatchCountPso;
        public int ProgressiveWarmupBatchCountVisualEffectAssets => _progressiveWarmupBatchCountVisualEffectAssets;
        public float SavePeriod => _traceSavePeriod;
        public bool EnterShadersWarmupSceneOnBootstrapInDevBuild => _enterShadersWarmupSceneOnBootstrapInDevBuild;
        public bool EnterShadersWarmupSceneOnBootstrapInEditor => _enterShadersWarmupSceneOnBootstrapInEditor;
        public string WarmupSceneName => _warmupScene.scene;
        public VisualEffectAsset[] VisualEffectAssets => _visualEffectAssets ?? Array.Empty<VisualEffectAsset>();

        public Shader[] ManualShaders => _manualShaders ?? Array.Empty<Shader>();
        public Shader[] HiddenShaders => _hiddenShaders ?? Array.Empty<Shader>();
        
        public string GetTracingFilePath() {
            return Path.Combine(Application.persistentDataPath, _filepath);
        }

        public GraphicsStateCollection GetReleaseGraphicsStateCollection() {
            return _releaseGraphicsStateCollection ?? new GraphicsStateCollection();
        }

#if UNITY_EDITOR
        public DefaultAsset[] GetContentFolders() {
            return _searchInFolders;
        }

        public DefaultAsset[] GetSceneFolders() {
            return _searchScenesFolders;
        }

        public DefaultAsset[] GetExcludeFolders() {
            return _excludeFolders;
        }
        
        public string GetGeneratedContentFolderPath() {
            return string.IsNullOrWhiteSpace(_generatedContentFolder)
                ? string.Empty
                : _generatedContentFolder.Trim().Replace('\\', '/').TrimEnd('/');
        }
        
        [Button]
        private void SearchHiddenShaders() {
            var shaderInfos = ShaderUtil.GetAllShaderInfo();
            var shaders = new List<Shader>();

            int skippedUnsupported = 0;
            int skippedNotFound = 0;

            for (int i = 0; i < shaderInfos.Length; i++) {
                if (!shaderInfos[i].name.StartsWith(HiddenShaderPrefix, StringComparison.Ordinal)) continue;

                // Неподдерживаемый на этой платформе шейдер всё равно не скомпилируется — прогревать нечего.
                if (!shaderInfos[i].supported) {
                    skippedUnsupported++;
                    continue;
                }

                var shader = Shader.Find(shaderInfos[i].name);

                if (shader == null) {
                    skippedNotFound++;
                    continue;
                }

                shaders.Add(shader);
            }

            shaders.Sort((first, second) => string.CompareOrdinal(first.name, second.name));

            int countBefore = _hiddenShaders?.Length ?? 0;
            _hiddenShaders = shaders.ToArray();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            Debug.Log($"{nameof(ShaderWarmupSettings)}: collected {_hiddenShaders.Length} hidden shaders (were {countBefore}). " +
                      $"Skipped unsupported: {skippedUnsupported}, skipped not found by name: {skippedNotFound}. " +
                      $"Press recollect prefabs on {nameof(ShadersWarmupSceneContentCollector)} to include these shaders.");
        }

        [Button]
        internal void SearchVisualEffectAssets() {
            var searchInFolders = GetFolderPaths(_searchInFolders);
            var excludeFolders = GetFolderPaths(_excludeFolders);
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

        [Button]
        internal void AppendTracedShadersToReleaseCollection() {
            if (_releaseGraphicsStateCollection == null) {
                Debug.LogError($"{nameof(ShaderWarmupSettings)}: Release Graphics State Collection should not be null. " +
                               $"Create new collection with Create/Shader/Graphics State Collection assign it to the Release Graphics State Collection field.");
                return;
            }

            if (!File.Exists(GetTracingFilePath())) {
                Debug.LogWarning($"{nameof(ShaderWarmupSettings)}: tracing file not found at [{GetTracingFilePath()}]. " +
                                 $"This file contains traced shaders. Lack of this file means shaders were not traced yet or the file was deleted. " +
                                 $"To trace shaders, launch dev build and render needed shaders, " +
                                 $"they will be saved into [{GetTracingFilePath()}] during the play session. " +
                                 $"Nothing is added to the release collection. ");
                return;
            }

            var tracingCollection = new GraphicsStateCollection();
            tracingCollection.LoadFromFile(GetTracingFilePath());

            int tracingCount = tracingCollection.variantCount;
            int releaseCountBefore = _releaseGraphicsStateCollection.variantCount;

            //_releaseGraphicsStateCollection.Append(tracingCollection);
            int releaseCountAfter = _releaseGraphicsStateCollection.variantCount;

            string assetPath = AssetDatabase.GetAssetPath(_releaseGraphicsStateCollection);
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            _releaseGraphicsStateCollection.SaveToFile(fullPath);

            Debug.Log($"{nameof(ShaderWarmupSettings)}: added {releaseCountAfter - releaseCountBefore} new shaders " +
                      $"to the release collection (total shaders now is {releaseCountAfter}) " +
                      $"from tracing file [{GetTracingFilePath()}] that contains {tracingCount} shaders.");
        }
#endif
    }
    
}