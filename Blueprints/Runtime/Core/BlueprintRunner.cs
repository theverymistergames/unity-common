using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private List<BlueprintAssetReferences> _blackboardReferences;

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
            if (_blueprintAsset == null) return;

            _runtimeBlueprint = _blueprintAsset.Compile();

            _runtimeBlackboard = CompileBlackboardOf(_blueprintAsset);
            _runtimeBlueprint.Initialize(this);
        }

        private void OnDestroy() {
            if (_blueprintAsset == null) return;

            _runtimeBlueprint.DeInitialize();
        }

        private void Start() {
            if (_blueprintAsset == null) return;

            _runtimeBlueprint.Start();
        }

        public RuntimeBlackboard CompileBlackboardOf(BlueprintAsset blueprintAsset) {
            var runtimeBlackboard = blueprintAsset.Blackboard.Compile();

            for (int i = 0; i < _blackboardReferences.Count; i++) {
                var entry = _blackboardReferences[i];
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

        private static void AddBlueprintAssetAndItsSubgraphAssetsTo(BlueprintAsset asset, List<BlueprintAsset> destination) {
            if (asset == null) return;

            destination.Add(asset);
            foreach (var subgraphAsset in asset.BlueprintMeta.SubgraphReferencesMap.Values) {
                AddBlueprintAssetAndItsSubgraphAssetsTo(subgraphAsset, destination);
            }
        }

        internal void FetchBlackboardGameObjectProperties() {
            if (_blueprintAsset == null) {
                _blackboardReferences.Clear();
                return;
            }

            var blueprintAssets = new List<BlueprintAsset>();
            AddBlueprintAssetAndItsSubgraphAssetsTo(_blueprintAsset, blueprintAssets);

            var assetReferencesMap = new Dictionary<BlueprintAsset, Dictionary<int, GameObject>>(_blackboardReferences.Count);

            for (int i = 0; i < _blackboardReferences.Count; i++) {
                var entry = _blackboardReferences[i];
                var blueprint = entry.blueprint;

                var references = entry.references;
                var referencesMap = new Dictionary<int, GameObject>(references.Count);

                for (int r = 0; r < references.Count; r++) {
                    var reference = references[r];
                    referencesMap[reference.hash] = reference.gameObject;
                }

                assetReferencesMap[blueprint] = referencesMap;
            }

            _blackboardReferences.Clear();

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

                _blackboardReferences.Add(new BlueprintAssetReferences {
                    blueprint = blueprintAsset,
                    references = references
                });
            }
        }

        private void OnValidate() {
            FetchBlackboardGameObjectProperties();
        }
    }

}
