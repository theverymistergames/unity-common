using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner2 : MonoBehaviour, IBlueprintHost2 {

        [SerializeField] private BlueprintAsset2 _blueprintAsset;
        [SerializeField] private SerializedDictionary<BlueprintAsset2, Blackboard2> _blackboardOverridesMap;

        public BlueprintAsset2 BlueprintAsset => _blueprintAsset;
        public MonoBehaviour Runner => this;

        private RuntimeBlueprint2 _runtimeBlueprint;

        public RuntimeBlueprint2 GetOrCompileBlueprint() {
            if (_runtimeBlueprint != null) return _runtimeBlueprint;

            _runtimeBlueprint = _blueprintAsset.Compile(BlueprintFactories.Global, this);
            _runtimeBlueprint.Initialize(this);

            return _runtimeBlueprint;
        }

        private void Awake() {
            GetOrCompileBlueprint();
        }

        private void OnDestroy() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }

        private void OnEnable() {
            _runtimeBlueprint?.SetEnabled(true);
        }

        private void OnDisable() {
            _runtimeBlueprint?.SetEnabled(false);
        }

        private void Start() {
            _runtimeBlueprint?.Start();
        }

        public Blackboard2 GetBlackboard(BlueprintAsset2 blueprint) {
            return _blackboardOverridesMap[blueprint];
        }

#if UNITY_EDITOR
        internal RuntimeBlueprint2 RuntimeBlueprint => _runtimeBlueprint;

        internal SerializedDictionary<BlueprintAsset2, Blackboard2> BlackboardOverridesMap =>
            _blackboardOverridesMap ??= new SerializedDictionary<BlueprintAsset2, Blackboard2>();

        private readonly HashSet<MonoBehaviour> _clients = new HashSet<MonoBehaviour>();

        internal void RestartBlueprint() {
            _blueprintAsset.BlueprintMeta.NodeJsonMap.Clear();
            InterruptRuntimeBlueprint();

            RegisterClient(this);
            GetOrCompileBlueprint().Start();
        }

        internal void InterruptRuntimeBlueprint() {
            UnregisterClient(this);

            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }

        internal void RegisterClient(MonoBehaviour client) {
            _clients.Add(client);
        }

        internal void UnregisterClient(MonoBehaviour client) {
            _clients.Remove(client);
            if (_clients.Count > 0) return;

            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }

#endif
    }

}
