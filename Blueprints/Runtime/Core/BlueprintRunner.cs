﻿using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private List<BlueprintAssetBlackboardSceneObjectReferences> _blackboardSceneObjectReferences;

        [Serializable]
        private struct BlueprintAssetBlackboardSceneObjectReferences {
            [ReadOnly] public BlueprintAsset blueprint;
            public List<SceneObjectReference> references;
        }

        [Serializable]
        private struct SceneObjectReference {
            [ReadOnly] public string property;
            [HideInInspector] public int hash;
            public GameObject gameObject;
        }

        public BlueprintAsset BlueprintAsset => _blueprintAsset;
        public Blackboard Blackboard => _blackboard;
        public MonoBehaviour Runner => this;

        private Blackboard _blackboard;
        private RuntimeBlueprint _runtimeBlueprint;

        private void Awake() {
            _runtimeBlueprint = _blueprintAsset.Compile();

            _blackboard = _blueprintAsset.Blackboard.Clone();
            ResolveBlackboardSceneReferences(_blueprintAsset, _blackboard);

            _runtimeBlueprint.Initialize(this);
        }

        private void OnDestroy() {
            _runtimeBlueprint.DeInitialize();
        }

        private void Start() {
            _runtimeBlueprint.Start();
        }

        public void ResolveBlackboardSceneReferences(BlueprintAsset blueprint, Blackboard blackboard) {
            for (int i = 0; i < _blackboardSceneObjectReferences.Count; i++) {
                var entry = _blackboardSceneObjectReferences[i];
                var asset = entry.blueprint;
                if (asset != blueprint) continue;

                var references = entry.references;
                for (int r = 0; r < references.Count; r++) {
                    var reference = references[r];
                    if (reference.gameObject == null) continue;

                    blackboard.SetGameObject(reference.hash, reference.gameObject);
                }
            }
        }

#if UNITY_EDITOR
        internal void FetchBlackboardGameObjectProperties() {
            if (_blueprintAsset == null) {
                _blackboardSceneObjectReferences?.Clear();
                return;
            }

            var blueprintAssets = new List<BlueprintAsset>();
            AddBlueprintAssetAndItsSubgraphAssetsTo(_blueprintAsset, blueprintAssets);

            var assetReferencesMap = new Dictionary<BlueprintAsset, Dictionary<int, GameObject>>(_blackboardSceneObjectReferences.Count);

            for (int i = 0; i < _blackboardSceneObjectReferences.Count; i++) {
                var entry = _blackboardSceneObjectReferences[i];
                var blueprint = entry.blueprint;

                var references = entry.references;
                var referencesMap = new Dictionary<int, GameObject>(references.Count);

                for (int r = 0; r < references.Count; r++) {
                    var reference = references[r];
                    referencesMap[reference.hash] = reference.gameObject;
                }

                assetReferencesMap[blueprint] = referencesMap;
            }

            _blackboardSceneObjectReferences.Clear();

            for (int i = 0; i < blueprintAssets.Count; i++) {
                var blueprintAsset = blueprintAssets[i];
                var blackboardPropertiesMap = blueprintAsset.Blackboard.PropertiesMap;

                var references = new List<SceneObjectReference>();

                if (assetReferencesMap.TryGetValue(blueprintAsset, out var referencesMap)) {
                    foreach ((int hash, var property) in blackboardPropertiesMap) {
                        var propertyType = Blackboard.GetPropertyType(property);
                        if (propertyType != typeof(GameObject)) continue;

                        if (referencesMap.TryGetValue(hash, out var go)) {
                            references.Add(new SceneObjectReference {
                                hash = hash,
                                property = property.name,
                                gameObject = go,
                            });
                        }
                        else {
                            references.Add(new SceneObjectReference {
                                hash = hash,
                                property = property.name
                            });
                        }
                    }
                }
                else {
                    foreach ((int hash, var property) in blackboardPropertiesMap) {
                        var propertyType = Blackboard.GetPropertyType(property);
                        if (propertyType != typeof(GameObject)) continue;

                        references.Add(new SceneObjectReference {
                            hash = hash,
                            property = property.name
                        });
                    }
                }

                if (references.Count == 0) continue;

                _blackboardSceneObjectReferences.Add(new BlueprintAssetBlackboardSceneObjectReferences {
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
