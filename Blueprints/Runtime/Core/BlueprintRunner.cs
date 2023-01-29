using System;
using MisterGames.Blueprints.Compile;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> _sceneReferencesMap;

#if UNITY_EDITOR
        internal SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> SceneReferencesMap =>
            _sceneReferencesMap ??= new SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>>();
#endif

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

        private void OnEnable() {
            _runtimeBlueprint.OnEnable();
        }

        private void OnDisable() {
            _runtimeBlueprint.OnDisable();
        }

        private void Start() {
            _runtimeBlueprint.Start();
        }

        public void ResolveBlackboardSceneReferences(BlueprintAsset blueprint, Blackboard blackboard) {
            if (!_sceneReferencesMap.TryGetValue(blueprint, out var references)) return;

            foreach ((int hash, var gameObjectRef) in references) {
                blackboard.SetGameObject(hash, gameObjectRef);
            }
        }
    }

}
