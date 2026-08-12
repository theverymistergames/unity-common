using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MisterGames.Logic.ShadersWarmup {
    
    internal sealed class ShadersWarmupSceneContentCollector : MonoBehaviour {
        
        [SerializeField] private ShaderWarmupSettings _warmupSettings;

        [Header("Prefabs")]
        [SerializeField] private Transform _prefabParent;
        [SerializeField] private List<GameObject> _prefabs;

        [Header("UI")]
        [SerializeField] [ReadOnly] private List<GameObject> _uiInstances = new();
        [SerializeField] [ReadOnly] private GameObject _uiCanvasRoot;

        [Header("Custom passes")]
        [SerializeField] [ReadOnly] private List<CustomPassVolume> _customPassVolumes = new();

        [Header("Volume profiles")]
        [SerializeField] [ReadOnly] private List<VolumeProfile> _volumeProfiles = new();

        [Header("Materials")]
        [SerializeField] private string[] _decalShaderMarkers = { "Decal" };
        [SerializeField] private string[] _skipShaderMarkers = System.Array.Empty<string>();
        [SerializeField] private string[] _renderStateProperties = {
            "_CullMode", "_CullModeForward", "_DoubleSidedEnable",
            "_SrcBlend", "_DstBlend", "_AlphaSrcBlend", "_AlphaDstBlend", "_BlendMode",
            "_ZWrite", "_TransparentZWrite", "_ZTestDepthEqualForOpaque", "_ZTestTransparent", "_ZTestGBuffer",
            "_StencilRef", "_StencilWriteMask",
            "_StencilRefDepth", "_StencilWriteMaskDepth",
            "_StencilRefMV", "_StencilWriteMaskMV",
            "_StencilRefGBuffer", "_StencilWriteMaskGBuffer",
            "_StencilRefDistortionVec", "_StencilWriteMaskDistortionVec",
            "_ColorMaskTransparentVelOne", "_ColorMaskTransparentVelTwo",
        };

        [Header("Grid")]
        [SerializeField] private Vector3 _gridOrigin = Vector3.zero;
        [SerializeField] [Min(0f)] private float _gridMaxHeight = 100f;
        [SerializeField] [Min(0f)] private float _gridSpacing = 1f;

        [Header("Grid no bounds")]
        [SerializeField] [Min(0f)] private float _noBoundsOffset = 10f;
        [SerializeField] [Min(0f)] private float _noBoundsStep = 3f;

        [Header("Grid info")]
        [SerializeField] [ReadOnly] private List<Bounds> _gridColumns = new();
        [SerializeField] [ReadOnly] private bool _hasNoBoundsRow;
        [SerializeField] [ReadOnly] private Bounds _noBoundsRow;

        public IReadOnlyList<Bounds> GridColumns => _gridColumns;
        public bool HasNoBoundsRow => _hasNoBoundsRow;
        public Bounds NoBoundsRow => _noBoundsRow;
        public IReadOnlyList<GameObject> UiInstances => _uiInstances ?? (IReadOnlyList<GameObject>) System.Array.Empty<GameObject>();
        public IReadOnlyList<CustomPassVolume> CustomPassVolumes => _customPassVolumes ?? (IReadOnlyList<CustomPassVolume>) System.Array.Empty<CustomPassVolume>();
        public IReadOnlyList<VolumeProfile> VolumeProfiles => _volumeProfiles ?? (IReadOnlyList<VolumeProfile>) System.Array.Empty<VolumeProfile>();

#if UNITY_EDITOR
        private const string PrefabExtension = ".prefab";
        private const string PrefabFilter = "t:Prefab";
        private const string ModelFilter = "t:Model";
        private const string SceneExtension = ".unity";
        private const string SceneFilter = "t:Scene";

        private const string CubeAssetName = "WarmupCube";
        private const string UiCanvasName = "WARMUP_UI_CANVAS";
        private const string MaterialExtension = ".mat";
        private const string MaterialFilter = "t:Material";
        private const string VolumeProfileFilter = "t:VolumeProfile";
        private const string MaterialCubePrefix = "MAT_";
        private const string DecalHostPrefix = "DECAL_";
        private const string CustomPassHostPrefix = "CUSTOMPASS_";
        private const string ManualMaterialPrefix = "WarmupMat_";

        private const string SrpBatcherCompatibilityCodeMethodName = "GetSRPBatcherCompatibilityCode";
        private const string DisallowGpuDrivenRenderingTypeName =
            "UnityEngine.Rendering.DisallowGPUDrivenRendering, Unity.RenderPipelines.GPUDriven.Runtime";

        private static Mesh CachedCubeMesh;
        private static ShaderTagId? _lightModeTag;
        private static ShaderTagId LightModeTag {
            get {
                _lightModeTag ??= new ShaderTagId("LightMode");
                return _lightModeTag.Value;
            }
        }
        private static bool SrpBatcherCompatibilityMethodResolved;
        private static System.Reflection.MethodInfo CachedSrpBatcherCompatibilityMethod;

        // Internal, but the only way to tell whether a shader can be drawn by a BatchRendererGroup at all.
        private static System.Reflection.MethodInfo SrpBatcherCompatibilityMethod {
            get {
                if (SrpBatcherCompatibilityMethodResolved) return CachedSrpBatcherCompatibilityMethod;

                SrpBatcherCompatibilityMethodResolved = true;
                CachedSrpBatcherCompatibilityMethod = typeof(ShaderUtil).GetMethod(
                    SrpBatcherCompatibilityCodeMethodName,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    null, new[] { typeof(Shader), typeof(int) }, null);

                if (CachedSrpBatcherCompatibilityMethod == null) {
                    Debug.LogWarning($"{nameof(ShadersWarmupSceneContentCollector)}: " +
                                     $"{nameof(ShaderUtil)}.{SrpBatcherCompatibilityCodeMethodName}(Shader, int) is not found, " +
                                     $"shaders incompatible with GPU driven rendering are not detected.");
                }

                return CachedSrpBatcherCompatibilityMethod;
            }
        }

        private static bool DisallowGpuDrivenRenderingTypeResolved;
        private static System.Type CachedDisallowGpuDrivenRenderingType;

        // Renderer.allowGPUDrivenRendering is internal, this component is the public way to turn it off,
        // and unlike the property it survives scene reloads and builds.
        private static System.Type DisallowGpuDrivenRenderingType {
            get {
                if (DisallowGpuDrivenRenderingTypeResolved) return CachedDisallowGpuDrivenRenderingType;

                DisallowGpuDrivenRenderingTypeResolved = true;
                CachedDisallowGpuDrivenRenderingType = System.Type.GetType(DisallowGpuDrivenRenderingTypeName);

                if (CachedDisallowGpuDrivenRenderingType == null) {
                    Debug.LogWarning($"{nameof(ShadersWarmupSceneContentCollector)}: type [{DisallowGpuDrivenRenderingTypeName}] is not found, " +
                                     $"cubes with shaders incompatible with GPU driven rendering are not excluded from it.");
                }

                return CachedDisallowGpuDrivenRenderingType;
            }
        }

        private static readonly HashSet<string> MeshLightModes = new() {
            "Forward", "ForwardOnly", "GBuffer", "DepthOnly", "DepthForwardOnly",
            "TransparentBackface", "TransparentDepthPrepass", "TransparentDepthPostpass",
            "MotionVectors", "ShadowCaster", "SRPDefaultUnlit", "META",
            "DistortionVectors", "FullScreenDebug", "RayTracingPrepass",
        };

        private enum MaterialHostKind {
            Cube,
            Decal,
            CustomPass,
            Skip,
        }
        
        [Button]
        private void CollectPrefabs() {
            if (_warmupSettings == null) {
                Debug.LogError($"{nameof(ShadersWarmupSceneContentCollector)}: {nameof(ShaderWarmupSettings)} is null. " +
                               $"Create one with Create/MisterGames/Shaders/{nameof(ShaderWarmupSettings)} and assign it to Warmup Settings.");
                return;
            }

            if (_prefabParent == null) {
                Debug.LogError($"{nameof(ShadersWarmupSceneContentCollector)}: prefab parent is null. ");
                return;
            }

            var contentFolders = GetFolderPaths(_warmupSettings.GetContentFolders());
            if (contentFolders.Count == 0) {
                Debug.LogWarning($"{nameof(ShadersWarmupSceneContentCollector)}: no content folders found in {nameof(ShaderWarmupSettings)}.");
                return;
            }

            var excludeFolders = _warmupSettings.GetExcludeFolderPaths();
            var prefabPaths = CollectPrefabPaths(contentFolders, excludeFolders);
            var modelPaths = CollectModelPaths(contentFolders, excludeFolders);
            var scenePaths = CollectScenePaths(excludeFolders);

            var assetPaths = new List<string>(prefabPaths.Count + modelPaths.Count);
            assetPaths.AddRange(prefabPaths);
            assetPaths.AddRange(modelPaths);

            ClearPrefabs();

            int volumeProfiles = CollectVolumeProfiles(contentFolders, excludeFolders);

            _customPassVolumes ??= new List<CustomPassVolume>();

            var materials = new List<Material>();
            var visitedMaterials = new HashSet<Material>();
            var nonMeshMaterials = new HashSet<Material>();

            int skippedCoveredByCubes = 0;

            for (int i = 0; i < assetPaths.Count; i++) {
                string assetPath = assetPaths[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                CollectMeshRendererMaterials(prefab, materials, visitedMaterials);
                CollectNonMeshMaterials(prefab, nonMeshMaterials);

                if (!HasNonMeshShaderContent(prefab)) {
                    skippedCoveredByCubes++;
                    continue;
                }

                if (PrefabUtility.InstantiatePrefab(prefab, _prefabParent) is not GameObject instance) continue;

                StripNonShaderComponents(instance);
                StripMeshRenderers(instance);
                CollectCustomPassVolumes(instance);

                if (TryPrepareUiInstance(instance)) continue;

                _prefabs.Add(instance);
            }

            int prefabInstances = _prefabs.Count;
            int folderMaterials = CollectFolderMaterials(contentFolders, excludeFolders, materials, visitedMaterials, nonMeshMaterials);
            int modelMaterials = CollectModelMaterials(modelPaths, materials, visitedMaterials, nonMeshMaterials);
            int sceneMaterials = CollectSceneMaterials(scenePaths, excludeFolders, materials, visitedMaterials, nonMeshMaterials);
            var coveredShaders = new HashSet<Shader>();

            CollectShaders(materials, coveredShaders);
            CollectShaders(nonMeshMaterials, coveredShaders);

            int visualEffectShaders = CollectVisualEffectShaders(coveredShaders);

            int coveredManualShaders = 0;

            if (_warmupSettings != null) {
                int manualShaders = CollectShaderMaterials(_warmupSettings.ManualShaders, materials, visitedMaterials, coveredShaders);

                coveredManualShaders = _warmupSettings.ManualShaders.Length - manualShaders;
            }

            SpawnMaterialHosts(materials, out int materialCubes, out int decalHosts,
                out int customPassHosts, out int skippedMaterials, out int duplicateVariants, out int notGpuDrivenCubes);

            Debug.Log($"{nameof(ShadersWarmupSceneContentCollector)}: added {materialCubes} cube meshes, " +
                      $"{decalHosts} decal projectors and {customPassHosts} fullscreen passes " +
                      $"(skipped repeated shaders: {duplicateVariants}, " +
                      $"skipped materials with no appropriate target: {skippedMaterials}, " +
                      $"cubes excluded from GPU driven rendering as not SRP Batcher compatible: {notGpuDrivenCubes}); " +
                      $"materials found {materials.Count}, materials in folders: {folderMaterials}, " +
                      $"materials in models: {modelMaterials}, materials in scenes: {sceneMaterials}, " +
                      $"(excluded as already covered: {coveredManualShaders}; " +
                      $"vfx compute shaders: {visualEffectShaders}). " +
                      $"Prefab instances with non-mesh renderers: {prefabInstances}, " +
                      $"plus {_uiInstances.Count} ui instances (Screen Space - Overlay). " +
                      $"Traversed prefabs: {prefabPaths.Count}, models: {modelPaths.Count} and scenes: {scenePaths.Count} " +
                      $"from {contentFolders.Count} folders (excluded folders: {excludeFolders.Count}), " +
                      $"skipped as covered by cube meshes: {skippedCoveredByCubes}. " +
                      $"Collected volume profiles: {volumeProfiles}, custom passes: {_customPassVolumes.Count}.");

            LayoutPrefabs();
        }

        [Button]
        private void LayoutPrefabs() {
            if (_prefabParent == null) {
                Debug.LogError($"{nameof(ShadersWarmupSceneContentCollector)}: Prefab Parent is null.");
                return;
            }

            var origin = _prefabParent.position + _gridOrigin;

            _gridColumns ??= new List<Bounds>();
            _gridColumns.Clear();
            _hasNoBoundsRow = false;
            _noBoundsRow = default;

            float cursorX = 0f;
            float cursorY = 0f;
            float columnWidth = 0f;
            int noBoundsIndex = 0;

            var columnBounds = new Bounds();
            bool hasColumnBounds = false;

            for (int i = 0; i < _prefabs.Count; i++) {
                var instance = _prefabs[i];
                if (instance == null) continue;


                if (!TryGetBounds(instance, out var bounds)) {
                    var noBoundsPosition = origin + new Vector3(noBoundsIndex * _noBoundsStep, -_noBoundsOffset, 0f);
                    instance.transform.position = noBoundsPosition;
                    noBoundsIndex++;

                    if (_hasNoBoundsRow) {
                        _noBoundsRow.Encapsulate(noBoundsPosition);
                    }
                    else {
                        _noBoundsRow = new Bounds(noBoundsPosition, Vector3.zero);
                        _hasNoBoundsRow = true;
                    }

                    continue;
                }

                var size = bounds.size;
                if (cursorY > 0f && cursorY + size.y > _gridMaxHeight) {
                    cursorX += columnWidth + _gridSpacing;
                    cursorY = 0f;
                    columnWidth = 0f;

                    if (hasColumnBounds) _gridColumns.Add(columnBounds);
                    hasColumnBounds = false;
                }

                var targetMin = origin + new Vector3(cursorX, cursorY, 0f);
                instance.transform.position += targetMin - bounds.min;

                var placedBounds = new Bounds(targetMin + bounds.extents, size);

                if (hasColumnBounds) {
                    columnBounds.Encapsulate(placedBounds);
                }
                else {
                    columnBounds = placedBounds;
                    hasColumnBounds = true;
                }

                cursorY += size.y + _gridSpacing;
                columnWidth = Mathf.Max(columnWidth, size.x);
            }

            if (hasColumnBounds) _gridColumns.Add(columnBounds);

            MarkDirty();

            Debug.Log($"{nameof(ShadersWarmupSceneContentCollector)}: layout done: {_gridColumns.Count} columns " +
                      $"with max width {cursorX + columnWidth:F1} and height {_gridMaxHeight}. " +
                      $"Non bound prefabs: {noBoundsIndex}.");
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds) {
            bounds = default;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++) {
                if (renderers[i] == null) continue;

                var rendererBounds = renderers[i].bounds;
                if (rendererBounds.size == Vector3.zero) continue;

                if (hasBounds) {
                    bounds.Encapsulate(rendererBounds);
                }
                else {
                    bounds = rendererBounds;
                    hasBounds = true;
                }
            }

            return hasBounds;
        }

        private static bool HasNonMeshShaderContent(GameObject prefab) {
            var components = prefab.GetComponentsInChildren<Component>(true);

            for (int i = 0; i < components.Length; i++) {
                // Null means a missing script in the prefab, it does not affect checking the other components.
                if (components[i] == null) continue;

                if (!IsShaderComponent(components[i])) continue;
                if (components[i] is MeshRenderer) continue;

                // A component living on top of a MeshRenderer (3D TextMeshPro and alike) is the same mesh
                // rendering: its material is on the renderer and has already gone into cubes.
                if (DependsOnMeshRendering(components[i])) continue;

                return true;
            }

            return false;
        }

        private int CollectVolumeProfiles(List<string> contentFolders, List<string> excludeFolders) {
            _volumeProfiles ??= new List<VolumeProfile>();
            _volumeProfiles.Clear();

            var volumeFolders = GetFolderPaths(_warmupSettings.GetContentFolders());
            if (volumeFolders.Count == 0) volumeFolders = contentFolders;

            string[] guids = AssetDatabase.FindAssets(VolumeProfileFilter, volumeFolders.ToArray());
            var visitedPaths = new HashSet<string>();
            var assetPaths = new List<string>(guids.Length);

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                assetPaths.Add(assetPath);
            }

            assetPaths.Sort(System.StringComparer.Ordinal);

            for (int i = 0; i < assetPaths.Count; i++) {
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(assetPaths[i]);
                if (profile == null) continue;

                _volumeProfiles.Add(profile);
            }

            return _volumeProfiles.Count;
        }

        private static int CollectFolderMaterials(List<string> contentFolders, List<string> excludeFolders,
            List<Material> materials, HashSet<Material> visited,
            HashSet<Material> nonMeshMaterials) {
            string[] guids = AssetDatabase.FindAssets(MaterialFilter, contentFolders.ToArray());
            var visitedPaths = new HashSet<string>();
            int added = 0;

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!assetPath.EndsWith(MaterialExtension, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                if (TryAddMaterial(AssetDatabase.LoadAssetAtPath<Material>(assetPath), materials, visited, nonMeshMaterials)) added++;
            }

            return added;
        }

        private static int CollectModelMaterials(List<string> modelPaths, List<Material> materials,
            HashSet<Material> visited, HashSet<Material> nonMeshMaterials) {
            int added = 0;

            for (int i = 0; i < modelPaths.Count; i++)
                added += CollectEmbeddedMaterials(modelPaths[i], materials, visited, nonMeshMaterials);

            return added;
        }

        private static int CollectSceneMaterials(List<string> scenePaths, List<string> excludeFolders,
            List<Material> materials, HashSet<Material> visited, HashSet<Material> nonMeshMaterials) {
            if (scenePaths.Count == 0) return 0;

            // Scenes are not opened: all their materials, including ones inside nested prefabs and models,
            // are visible as recursive dependencies of the scene asset.
            string[] dependencies = AssetDatabase.GetDependencies(scenePaths.ToArray(), recursive: true);
            System.Array.Sort(dependencies, System.StringComparer.Ordinal);

            var visitedPaths = new HashSet<string>();
            int added = 0;

            for (int i = 0; i < dependencies.Length; i++) {
                string assetPath = dependencies[i];

                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                if (assetPath.EndsWith(MaterialExtension, System.StringComparison.OrdinalIgnoreCase)) {
                    if (TryAddMaterial(AssetDatabase.LoadAssetAtPath<Material>(assetPath), materials, visited, nonMeshMaterials)) added++;
                    continue;
                }

                if (IsModelPath(assetPath)) added += CollectEmbeddedMaterials(assetPath, materials, visited, nonMeshMaterials);
            }

            return added;
        }

        private static int CollectEmbeddedMaterials(string assetPath, List<Material> materials,
            HashSet<Material> visited, HashSet<Material> nonMeshMaterials) {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            int added = 0;

            for (int i = 0; i < subAssets.Length; i++) {
                // Materials embedded into a model are not stored as separate .mat assets in the project
                // and are not found by the material search over folders.
                if (subAssets[i] is Material material &&
                    TryAddMaterial(material, materials, visited, nonMeshMaterials)) {
                    added++;
                }
            }

            return added;
        }

        private static bool TryAddMaterial(Material material, List<Material> materials,
            HashSet<Material> visited, HashSet<Material> nonMeshMaterials) {
            if (material == null) return false;

            if (nonMeshMaterials.Contains(material)) return false;
            if (!visited.Add(material)) return false;

            materials.Add(material);
            return true;
        }

        private static void CollectMeshRendererMaterials(GameObject prefab, List<Material> materials, HashSet<Material> visited) {
            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < meshRenderers.Length; i++) {
                if (meshRenderers[i] == null) continue;

                var sharedMaterials = meshRenderers[i].sharedMaterials;

                for (int j = 0; j < sharedMaterials.Length; j++) {
                    // An empty material slot on a renderer is a common case, there is nothing to draw with it.
                    if (sharedMaterials[j] == null) continue;
                    if (!visited.Add(sharedMaterials[j])) continue;

                    materials.Add(sharedMaterials[j]);
                }
            }
        }

        private static void CollectNonMeshMaterials(GameObject prefab, HashSet<Material> nonMeshMaterials) {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++) {
                if (renderers[i] == null || renderers[i] is MeshRenderer) continue;

                var sharedMaterials = renderers[i].sharedMaterials;

                for (int j = 0; j < sharedMaterials.Length; j++)
                    if (sharedMaterials[j] != null)
                        nonMeshMaterials.Add(sharedMaterials[j]);
            }

            var decalProjectors = prefab.GetComponentsInChildren<DecalProjector>(true);

            for (int i = 0; i < decalProjectors.Length; i++)
                if (decalProjectors[i] != null && decalProjectors[i].material != null)
                    nonMeshMaterials.Add(decalProjectors[i].material);

            var terrains = prefab.GetComponentsInChildren<Terrain>(true);

            for (int i = 0; i < terrains.Length; i++)
                if (terrains[i] != null && terrains[i].materialTemplate != null)
                    nonMeshMaterials.Add(terrains[i].materialTemplate);

            CollectCustomPassMaterials(prefab, nonMeshMaterials);
        }
        
        private static void CollectCustomPassMaterials(GameObject prefab, HashSet<Material> nonMeshMaterials) {
            var customPassVolumes = prefab.GetComponentsInChildren<CustomPassVolume>(true);

            for (int i = 0; i < customPassVolumes.Length; i++) {
                if (customPassVolumes[i] == null || customPassVolumes[i].customPasses == null) continue;

                var customPasses = customPassVolumes[i].customPasses;

                for (int j = 0; j < customPasses.Count; j++)
                    switch (customPasses[j]) {
                        case FullScreenCustomPass fullScreen when fullScreen.fullscreenPassMaterial != null:
                            nonMeshMaterials.Add(fullScreen.fullscreenPassMaterial);
                            break;

                        case DrawRenderersCustomPass drawRenderers when drawRenderers.overrideMaterial != null:
                            nonMeshMaterials.Add(drawRenderers.overrideMaterial);
                            break;
                    }
            }
        }

        private void CollectCustomPassVolumes(GameObject instance) {
            var customPassVolumes = instance.GetComponentsInChildren<CustomPassVolume>(true);

            for (int i = 0; i < customPassVolumes.Length; i++) {
                if (customPassVolumes[i] == null) continue;

                customPassVolumes[i].isGlobal = true;
                customPassVolumes[i].enabled = false;

                _customPassVolumes.Add(customPassVolumes[i]);
            }
        }

        private void SpawnMaterialHosts(List<Material> materials, out int cubes, out int decals,
            out int customPasses, out int skipped, out int duplicates, out int notGpuDriven) {
            cubes = 0;
            decals = 0;
            customPasses = 0;
            skipped = 0;
            duplicates = 0;
            notGpuDriven = 0;

            if (materials.Count == 0) return;

            var cubeMesh = GetUnitCubeMesh();
            if (cubeMesh == null) return;

            materials.Sort(CompareMaterials);

            var visitedVariants = new HashSet<string>();

            for (int i = 0; i < materials.Count; i++) {
                var material = materials[i];
                var hostKind = ClassifyMaterial(material);

                if (hostKind == MaterialHostKind.Skip) {
                    skipped++;
                    continue;
                }

                if (!visitedVariants.Add(GetShaderVariantKey(material))) {
                    duplicates++;
                    continue;
                }

                switch (hostKind) {
                    case MaterialHostKind.Decal:
                        SpawnDecalHost(material);
                        decals++;
                        continue;

                    case MaterialHostKind.CustomPass:
                        if (SpawnCustomPassHost(material)) customPasses++;
                        continue;
                }

                if (SpawnMaterialCube(material, cubeMesh)) notGpuDriven++;
                cubes++;
            }
        }

        private string GetShaderVariantKey(Material material) {
            string[] keywords = material.shaderKeywords;
            System.Array.Sort(keywords, System.StringComparer.Ordinal);

            var key = new System.Text.StringBuilder();

            key.Append(material.shader.GetEntityId().ToString());
            key.Append('|').Append(material.renderQueue);
            key.Append('|').Append(string.Join(" ", keywords));

            if (_renderStateProperties == null) return key.ToString();

            for (int i = 0; i < _renderStateProperties.Length; i++) {
                string property = _renderStateProperties[i];

                if (string.IsNullOrEmpty(property)) continue;
                if (!material.HasFloat(property)) continue;

                key.Append('|').Append(property).Append('=')
                    .Append(material.GetFloat(property).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return key.ToString();
        }

        private bool SpawnMaterialCube(Material material, Mesh cubeMesh) {
            var cube = new GameObject($"{MaterialCubePrefix}{material.name}", typeof(MeshFilter), typeof(MeshRenderer));
            cube.transform.SetParent(_prefabParent, false);

            cube.GetComponent<MeshFilter>().sharedMesh = cubeMesh;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;

            _prefabs.Add(cube);

            // A BatchRendererGroup (GPU Resident Drawer and alike) can only draw SRP Batcher compatible shaders
            // and logs an error per draw command for any other one, so such cubes are drawn the regular way.
            return !IsSrpBatcherCompatible(material.shader) && TryDisallowGpuDrivenRendering(cube);
        }

        private static bool IsSrpBatcherCompatible(Shader shader) {
            if (shader == null) return true;

            var method = SrpBatcherCompatibilityMethod;
            if (method == null) return true;

            var arguments = new object[] { shader, 0 };
            int subShaderCount = Mathf.Max(shader.subshaderCount, 1);

            // Which subshader is picked depends on the platform, so a shader counts as usable
            // only when there is nothing incompatible to fall back to.
            for (int i = 0; i < subShaderCount; i++) {
                arguments[1] = i;

                // Zero code means the subshader is compatible, any other value is an incompatibility reason.
                if ((int) method.Invoke(null, arguments) != 0) return false;
            }

            return true;
        }

        private static bool TryDisallowGpuDrivenRendering(GameObject host) {
            var componentType = DisallowGpuDrivenRenderingType;
            if (componentType == null) return false;

            host.AddComponent(componentType);
            return true;
        }

        private void SpawnDecalHost(Material material) {
            var host = new GameObject($"{DecalHostPrefix}{material.name}", typeof(DecalProjector));
            host.transform.SetParent(_prefabParent, false);

            var projector = host.GetComponent<DecalProjector>();
            projector.material = material;
            projector.size = Vector3.one;

            _prefabs.Add(host);
        }

        private int CollectShaderMaterials(Shader[] shaders, List<Material> materials,
            HashSet<Material> visited, HashSet<Shader> coveredShaders) {
            if (shaders == null) return 0;

            int added = 0;

            for (int i = 0; i < shaders.Length; i++) {
                if (shaders[i] == null) continue;
                if (!coveredShaders.Add(shaders[i])) continue;

                var material = GetOrCreateManualMaterial(shaders[i]);

                if (material == null) continue;
                if (!visited.Add(material)) continue;

                materials.Add(material);
                added++;
            }

            return added;
        }

        private static void CollectShaders(IEnumerable<Material> materials, HashSet<Shader> coveredShaders) {
            foreach (var material in materials)
                if (material != null && material.shader != null)
                    coveredShaders.Add(material.shader);
        }
        
        private int CollectVisualEffectShaders(HashSet<Shader> coveredShaders) {
            if (_warmupSettings == null) return 0;

            var visualEffectAssets = _warmupSettings.VisualEffectAssets;
            int added = 0;

            for (int i = 0; i < visualEffectAssets.Length; i++) {
                if (visualEffectAssets[i] == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(visualEffectAssets[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                for (int j = 0; j < subAssets.Length; j++)
                    if (subAssets[j] is Shader shader && coveredShaders.Add(shader))
                        added++;
            }

            return added;
        }

        private MaterialHostKind ClassifyMaterial(Material material) {
            if (MatchesShaderMarkers(material, _skipShaderMarkers)) return MaterialHostKind.Skip;
            if (MatchesShaderMarkers(material, _decalShaderMarkers)) return MaterialHostKind.Decal;

            if (material.shader == null) return MaterialHostKind.Skip;

            if (HasLightMode(material.shader, IsDecalLightMode)) return MaterialHostKind.Decal;
            if (HasLightMode(material.shader, IsMeshLightMode)) return MaterialHostKind.Cube;

            return MaterialHostKind.CustomPass;
        }

        private static bool HasLightMode(Shader shader, System.Func<string, bool> predicate) {
            for (int i = 0; i < shader.passCount; i++) {
                string lightMode = shader.FindPassTagValue(i, LightModeTag).name;

                if (string.IsNullOrEmpty(lightMode)) continue;
                if (predicate(lightMode)) return true;
            }

            return false;
        }

        private static bool IsMeshLightMode(string lightMode) {
            return MeshLightModes.Contains(lightMode);
        }

        private static bool IsDecalLightMode(string lightMode) {
            return lightMode.StartsWith("DBuffer", System.StringComparison.OrdinalIgnoreCase)
                   || lightMode.IndexOf("Decal", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        private bool SpawnCustomPassHost(Material material) {
            var host = new GameObject($"{CustomPassHostPrefix}{material.name}", typeof(CustomPassVolume));
            host.transform.SetParent(_prefabParent, false);

            var volume = host.GetComponent<CustomPassVolume>();
            volume.isGlobal = true;

            if (volume.AddPassOfType<FullScreenCustomPass>() is not { } fullScreenPass) {
                DestroyImmediate(host);
                return false;
            }

            fullScreenPass.fullscreenPassMaterial = material;

            volume.enabled = false;

            _prefabs.Add(host);
            _customPassVolumes.Add(volume);

            return true;
        }

        private Material GetOrCreateManualMaterial(Shader shader) {
            if (shader == null) return null;

            if (!TryGetGeneratedContentFolder(out string folderPath)) return null;

            string assetPath = $"{folderPath}/{ManualMaterialPrefix}{GetSafeAssetName(shader.name)}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (material != null && material.shader == shader) return material;

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);

            return material;
        }

        private static string GetSafeAssetName(string shaderName) {
            var safeName = new System.Text.StringBuilder(shaderName.Length);
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            for (int i = 0; i < shaderName.Length; i++) {
                char symbol = shaderName[i];
                safeName.Append(System.Array.IndexOf(invalidChars, symbol) >= 0 ? '_' : symbol);
            }

            return safeName.ToString();
        }

        private static bool MatchesShaderMarkers(Material material, string[] markers) {
            if (markers == null || markers.Length == 0 || material.shader == null) return false;

            string shaderName = material.shader.name;

            for (int i = 0; i < markers.Length; i++) {
                if (string.IsNullOrEmpty(markers[i])) continue;
                if (shaderName.IndexOf(markers[i], System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }

            return false;
        }

        private static int CompareMaterials(Material a, Material b) {
            int byPath = string.CompareOrdinal(AssetDatabase.GetAssetPath(a), AssetDatabase.GetAssetPath(b));
            return byPath != 0 ? byPath : string.CompareOrdinal(a.name, b.name);
        }

        private Mesh GetUnitCubeMesh() {
            if (CachedCubeMesh != null) return CachedCubeMesh;

            if (!TryGetGeneratedContentFolder(out string folderPath)) return null;

            string assetPath = $"{folderPath}/{CubeAssetName}.asset";
            var cube = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            if (cube == null) {
                cube = BuildCubeMesh();
                AssetDatabase.CreateAsset(cube, assetPath);
            }

            CachedCubeMesh = cube;
            return cube;
        }

        private static Mesh BuildCubeMesh() {
            var faceNormals = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            var faceRights = new[] { Vector3.back, Vector3.forward, Vector3.right, Vector3.right, Vector3.right, Vector3.left };
            var faceUps = new[] { Vector3.up, Vector3.up, Vector3.back, Vector3.forward, Vector3.up, Vector3.up };

            var vertices = new Vector3[24];
            var normals = new Vector3[24];
            var tangents = new Vector4[24];
            var uv = new Vector2[24];
            var colors = new Color[24];
            int[] triangles = new int[36];

            const float extent = 0.5f;

            for (int face = 0; face < 6; face++) {
                var normal = faceNormals[face];
                var right = faceRights[face] * extent;
                var up = faceUps[face] * extent;
                var center = normal * extent;

                int vertex = face * 4;

                vertices[vertex + 0] = center - right - up;
                vertices[vertex + 1] = center + right - up;
                vertices[vertex + 2] = center + right + up;
                vertices[vertex + 3] = center - right + up;

                uv[vertex + 0] = new Vector2(0f, 0f);
                uv[vertex + 1] = new Vector2(1f, 0f);
                uv[vertex + 2] = new Vector2(1f, 1f);
                uv[vertex + 3] = new Vector2(0f, 1f);

                var tangent = new Vector4(faceRights[face].x, faceRights[face].y, faceRights[face].z, -1f);

                for (int i = 0; i < 4; i++) {
                    normals[vertex + i] = normal;
                    tangents[vertex + i] = tangent;
                    colors[vertex + i] = Color.white;
                }

                int index = face * 6;

                triangles[index + 0] = vertex + 0;
                triangles[index + 1] = vertex + 1;
                triangles[index + 2] = vertex + 2;
                triangles[index + 3] = vertex + 0;
                triangles[index + 4] = vertex + 2;
                triangles[index + 5] = vertex + 3;
            }

            var mesh = new Mesh { name = CubeAssetName };

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uv);
            mesh.SetUVs(1, uv);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateBounds();
            return mesh;
        }

        private bool TryGetGeneratedContentFolder(out string folderPath) {
            folderPath = _warmupSettings.GetGeneratedContentFolderPath();

            if (!string.IsNullOrEmpty(folderPath)) return true;

            Debug.LogError($"{nameof(ShadersWarmupSceneContentCollector)}: generated content folder is not set " +
                           $"in {nameof(ShaderWarmupSettings)} (or the assigned asset is not a folder), " +
                           $"there is no place to put generated assets.");
            return false;
        }

        private bool TryPrepareUiInstance(GameObject instance) {
            if (HasNonUiRenderer(instance)) {
                ExtractUiRoots(instance);
                return false;
            }

            return TryPrepareUiRoot(instance);
        }
        
        private int ExtractUiRoots(GameObject instance) {
            var uiRoots = new List<Transform>();
            CollectUiRoots(instance.transform, uiRoots);

            if (uiRoots.Count == 0) return 0; 
            if (PrefabUtility.IsPartOfPrefabInstance(instance)) PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            int extracted = 0;
            for (int i = 0; i < uiRoots.Count; i++)
                if (TryPrepareUiRoot(uiRoots[i].gameObject))
                    extracted++;

            return extracted;
        }

        private static void CollectUiRoots(Transform node, List<Transform> uiRoots) {
            for (int i = 0; i < node.childCount; i++) {
                var child = node.GetChild(i);

                if (IsPureUiSubtree(child.gameObject)) {
                    uiRoots.Add(child);
                    continue;
                }

                CollectUiRoots(child, uiRoots);
            }
        }

        private static bool IsPureUiSubtree(GameObject candidate) {
            return HasCanvasGraphics(candidate) && !HasNonUiRenderer(candidate);
        }

        private bool TryPrepareUiRoot(GameObject uiRoot) {
            var canvases = uiRoot.GetComponentsInChildren<Canvas>(true);
            Transform parent;

            if (canvases.Length > 0) {
                for (int i = 0; i < canvases.Length; i++)
                    canvases[i].renderMode = RenderMode.ScreenSpaceOverlay;

                parent = _prefabParent;
            }
            else {
                if (!HasCanvasGraphics(uiRoot)) return false;

                var uiCanvas = GetOrCreateUiCanvas();
                if (uiCanvas == null) return false;

                parent = uiCanvas.transform;
            }

            uiRoot.transform.SetParent(parent, false);

            uiRoot.SetActive(false);
            _uiInstances.Add(uiRoot);
            return true;
        }

        private static bool HasNonUiRenderer(GameObject instance) {
            var components = instance.GetComponentsInChildren<Component>(true);

            for (int i = 0; i < components.Length; i++) {
                if (components[i] == null) continue;

                if (!IsShaderComponent(components[i])) continue;
                if (IsCanvasComponent(components[i])) continue;

                return true;
            }

            return false;
        }

        private static bool IsCanvasComponent(Component component) {
            return component is Canvas or CanvasRenderer or Graphic;
        }

        private static bool HasCanvasGraphics(GameObject instance) {
            if (instance.GetComponentInChildren<Graphic>(true) != null) return true;

            return instance.GetComponentInChildren<CanvasRenderer>(true) != null;
        }
        
        private GameObject GetOrCreateUiCanvas() {
            if (_uiCanvasRoot != null) return _uiCanvasRoot;

            if (_prefabParent == null) return null;

            _uiCanvasRoot = new GameObject(UiCanvasName, typeof(Canvas));
            _uiCanvasRoot.transform.SetParent(_prefabParent, false);
            _uiCanvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            return _uiCanvasRoot;
        }
        
        private static bool IsShaderComponent(Component component) {
            return component is Renderer
                or CanvasRenderer
                or Graphic
                or ParticleSystem
                or VisualEffect
                or DecalProjector
                or Terrain
                or CustomPassVolume;
        }

        private static int StripNonShaderComponents(GameObject instance) {
            int removed = 0;

            var transforms = instance.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++) {
                removed += StripGameObject(transforms[i].gameObject, out int missingScripts);
                removed += missingScripts;
            }

            return removed;
        }

        private static int StripMeshRenderers(GameObject instance) {
            var meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            int removed = 0;

            for (int i = 0; i < meshRenderers.Length; i++) {
                if (meshRenderers[i] == null) continue; 
                removed += DestroyMeshRenderingDependents(meshRenderers[i].gameObject);

                if (meshRenderers[i] == null) continue;

                if (meshRenderers[i].TryGetComponent<MeshFilter>(out var meshFilter)) {
                    DestroyImmediate(meshFilter);
                    removed++;
                }

                DestroyImmediate(meshRenderers[i]);
                removed++;
            }

            return removed;
        }

        private static int DestroyMeshRenderingDependents(GameObject gameObject) {
            var components = gameObject.GetComponents<Component>();
            int removed = 0;

            for (int i = 0; i < components.Length; i++) {
                var component = components[i];

                if (component == null || component is Transform) continue;
                if (component is MeshRenderer or MeshFilter) continue;
                if (!DependsOnMeshRendering(component)) continue;

                DestroyImmediate(component);
                removed++;
            }

            return removed;
        }

        private static bool DependsOnMeshRendering(Component component) {
            object[] attributes = component.GetType().GetCustomAttributes(typeof(RequireComponent), true);

            for (int i = 0; i < attributes.Length; i++) {
                var requirement = (RequireComponent) attributes[i];

                if (IsMeshRenderingType(requirement.m_Type0)) return true;
                if (IsMeshRenderingType(requirement.m_Type1)) return true;
                if (IsMeshRenderingType(requirement.m_Type2)) return true;
            }

            return false;
        }

        private static bool IsMeshRenderingType(System.Type requiredType) {
            if (requiredType == null) return false;

            return requiredType.IsAssignableFrom(typeof(MeshRenderer)) || requiredType.IsAssignableFrom(typeof(MeshFilter));
        }

        private static int StripGameObject(GameObject gameObject, out int missingScripts) {
            missingScripts = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

            var components = gameObject.GetComponents<Component>();
            var kept = new List<Component>(components.Length);
            var candidates = new List<Component>(components.Length);

            for (int i = 0; i < components.Length; i++) {
                var component = components[i];
                if (component == null) continue;

                if (component is Transform || IsShaderComponent(component) || IsInfrastructure(component))
                    kept.Add(component);
                else
                    candidates.Add(component);
            }

            bool keptGrew = true;
            while (keptGrew) {
                keptGrew = false;

                for (int i = candidates.Count - 1; i >= 0; i--) {
                    if (!IsRequiredBy(candidates[i], kept)) continue;

                    kept.Add(candidates[i]);
                    candidates.RemoveAt(i);
                    keptGrew = true;
                }
            }

            int removed = 0;

            while (candidates.Count > 0) {
                bool removedAny = false;

                for (int i = candidates.Count - 1; i >= 0; i--) {
                    if (IsRequiredBy(candidates[i], candidates)) continue;

                    DestroyImmediate(candidates[i]);
                    candidates.RemoveAt(i);
                    removed++;
                    removedAny = true;
                }

                if (!removedAny) break;
            }

            return removed;
        }

        private static bool IsInfrastructure(Component component) {
            return component is MeshFilter or Canvas;
        }

        private static bool IsRequiredBy(Component component, List<Component> dependents) {
            var componentType = component.GetType();

            for (int i = 0; i < dependents.Count; i++) {
                var dependent = dependents[i];
                if (dependent == null || dependent == component) continue;

                object[] attributes = dependent.GetType().GetCustomAttributes(typeof(RequireComponent), true);

                for (int j = 0; j < attributes.Length; j++) {
                    var requirement = (RequireComponent) attributes[j];

                    if (Requires(requirement.m_Type0, componentType)) return true;
                    if (Requires(requirement.m_Type1, componentType)) return true;
                    if (Requires(requirement.m_Type2, componentType)) return true;
                }
            }

            return false;
        }

        private static bool Requires(System.Type requiredType, System.Type componentType) {
            return requiredType != null && requiredType.IsAssignableFrom(componentType);
        }

        [Button]
        private void ClearPrefabs() {
            _prefabs ??= new List<GameObject>();
            _uiInstances ??= new List<GameObject>();

            DestroyInstances(_prefabs);
            DestroyInstances(_uiInstances);

            if (_uiCanvasRoot != null) DestroyImmediate(_uiCanvasRoot);
            _uiCanvasRoot = null;

            _volumeProfiles?.Clear();

            _customPassVolumes?.Clear();

            MarkDirty();
        }

        private static void DestroyInstances(List<GameObject> instances) {
            for (int i = instances.Count - 1; i >= 0; i--)
                if (instances[i] != null)
                    DestroyImmediate(instances[i]);

            instances.Clear();
        }

        private List<string> CollectPrefabPaths(List<string> contentFolders, List<string> excludeFolders) {
            string[] guids = AssetDatabase.FindAssets(PrefabFilter, contentFolders.ToArray());
            var prefabPaths = new List<string>(guids.Length);
            var visitedPaths = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!assetPath.EndsWith(PrefabExtension, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                prefabPaths.Add(assetPath);
            }

            prefabPaths.Sort(System.StringComparer.Ordinal);
            return prefabPaths;
        }

        private static List<string> CollectModelPaths(List<string> contentFolders, List<string> excludeFolders) {
            string[] guids = AssetDatabase.FindAssets(ModelFilter, contentFolders.ToArray());
            var modelPaths = new List<string>(guids.Length);
            var visitedPaths = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!IsModelPath(assetPath)) continue;
                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                modelPaths.Add(assetPath);
            }

            modelPaths.Sort(System.StringComparer.Ordinal);
            return modelPaths;
        }

        private static bool IsModelPath(string assetPath) {
            // A model (fbx, obj and other formats) is imported as a GameObject asset and is instantiated like a prefab,
            // so the extension is not checked, instead everything that does not unfold as a model is cut off.
            return !assetPath.EndsWith(PrefabExtension, System.StringComparison.OrdinalIgnoreCase) &&
                   AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(GameObject);
        }

        private List<string> CollectScenePaths(List<string> excludeFolders) {
            var sceneFolders = GetFolderPaths(_warmupSettings.GetSceneFolders());
            if (sceneFolders.Count == 0) return new List<string>();

            string[] guids = AssetDatabase.FindAssets(SceneFilter, sceneFolders.ToArray());
            var scenePaths = new List<string>(guids.Length);
            var visitedPaths = new HashSet<string>();

            // The collector's own scene is not needed: its content is the result of the collecting itself.
            string currentScenePath = gameObject.scene.path;

            for (int i = 0; i < guids.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!assetPath.EndsWith(SceneExtension, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(assetPath, currentScenePath, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (IsExcluded(assetPath, excludeFolders)) continue;
                if (!visitedPaths.Add(assetPath)) continue;

                scenePaths.Add(assetPath);
            }

            scenePaths.Sort(System.StringComparer.Ordinal);
            return scenePaths;
        }

        private static bool IsExcluded(string assetPath, List<string> excludeFolders) {
            for (int i = 0; i < excludeFolders.Count; i++)
                if (assetPath.StartsWith(excludeFolders[i] + "/", System.StringComparison.OrdinalIgnoreCase))
                    return true;

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

        private void MarkDirty() {
            EditorUtility.SetDirty(this);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}