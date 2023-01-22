using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private List<BlueprintAssetReferences> _blackboardProperties;

        [Serializable]
        private struct BlueprintAssetReferences {
            [ReadOnly] public BlueprintAsset blueprint;
            public List<SceneReference> references;
        }

        [Serializable]
        private struct SceneReference {
            [ReadOnly] public string property;
            [HideInInspector] public int hash;
            public GameObject gameObject;
        }

        public BlueprintAsset BlueprintAsset => _blueprintAsset;
        public RuntimeBlackboard Blackboard => _runtimeBlackboard;
        public BlueprintRunner Runner => this;

        private RuntimeBlueprint _runtimeBlueprint;
        private RuntimeBlackboard _runtimeBlackboard;

        private void Awake() {
            _runtimeBlueprint = _blueprintAsset.Compile();

            _runtimeBlackboard = CompileBlackboard(_blueprintAsset);
            _runtimeBlueprint.Initialize(this);
        }

        private void OnDestroy() {
            _runtimeBlueprint.DeInitialize();
        }

        private void Start() {
            _runtimeBlueprint.Start();
        }

        public RuntimeBlackboard CompileBlackboard(BlueprintAsset blueprintAsset) {
            var runtimeBlackboard = blueprintAsset.Blackboard.Compile();

            for (int i = 0; i < _blackboardProperties.Count; i++) {
                var entry = _blackboardProperties[i];
                var asset = entry.blueprint;
                if (asset != blueprintAsset) continue;

                var references = entry.references;
                for (int r = 0; r < references.Count; r++) {
                    var reference = references[r];
                    runtimeBlackboard.Set(reference.hash, reference.gameObject);
                }
            }

            return runtimeBlackboard;
        }

#if UNITY_EDITOR
        internal void FetchBlackboardGameObjectProperties() {
            if (_blueprintAsset == null) {
                _blackboardProperties.Clear();
                return;
            }

            var blueprintAssets = new List<BlueprintAsset>();
            AddBlueprintAssetAndItsSubgraphAssetsTo(_blueprintAsset, blueprintAssets);

            var assetReferencesMap = new Dictionary<BlueprintAsset, Dictionary<int, GameObject>>(_blackboardProperties.Count);

            for (int i = 0; i < _blackboardProperties.Count; i++) {
                var entry = _blackboardProperties[i];
                var blueprint = entry.blueprint;

                var references = entry.references;
                var referencesMap = new Dictionary<int, GameObject>(references.Count);

                for (int r = 0; r < references.Count; r++) {
                    var reference = references[r];
                    referencesMap[reference.hash] = reference.gameObject;
                }

                assetReferencesMap[blueprint] = referencesMap;
            }

            _blackboardProperties.Clear();

            for (int i = 0; i < blueprintAssets.Count; i++) {
                var blueprintAsset = blueprintAssets[i];
                var blackboardPropertiesMap = blueprintAsset.Blackboard.PropertiesMap;

                var references = new List<SceneReference>();

                if (assetReferencesMap.TryGetValue(blueprintAsset, out var referencesMap)) {
                    foreach ((int hash, var property) in blackboardPropertiesMap) {
                        var propertyType = MisterGames.Common.Data.Blackboard.GetPropertyType(property);
                        if (propertyType != typeof(GameObject)) continue;

                        if (referencesMap.TryGetValue(hash, out var go)) {
                            references.Add(new SceneReference {
                                hash = hash,
                                property = property.name,
                                gameObject = go,
                            });
                        }
                        else {
                            references.Add(new SceneReference {
                                hash = hash,
                                property = property.name
                            });
                        }
                    }
                }
                else {
                    foreach ((int hash, var property) in blackboardPropertiesMap) {
                        var propertyType = MisterGames.Common.Data.Blackboard.GetPropertyType(property);
                        if (propertyType != typeof(GameObject)) continue;

                        references.Add(new SceneReference {
                            hash = hash,
                            property = property.name
                        });
                    }
                }

                if (references.Count == 0) continue;

                _blackboardProperties.Add(new BlueprintAssetReferences {
                    blueprint = blueprintAsset,
                    references = references
                });
            }
        }

        private static void AddBlueprintAssetAndItsSubgraphAssetsTo(BlueprintAsset asset, List<BlueprintAsset> destination) {
            if (asset == null) return;

            destination.Add(asset);
            foreach (var subgraphAsset in asset.BlueprintMeta.SubgraphReferencesMap.Values) {
                AddBlueprintAssetAndItsSubgraphAssetsTo(subgraphAsset, destination);
            }
        }

        private void OnValidate() {
            FetchBlackboardGameObjectProperties();
        }
#endif
    }

}
