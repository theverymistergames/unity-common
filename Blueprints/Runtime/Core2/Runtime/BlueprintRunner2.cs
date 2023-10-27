using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintRunner2 : MonoBehaviour, IBlueprintHost2 {

        [SerializeField] private BlueprintAsset2 _blueprintAsset;
        [SerializeField] private SerializedDictionary<BlueprintAsset2, Blackboard> _blackboardOverridesMap;

        public BlueprintAsset2 BlueprintAsset => _blueprintAsset;
        public Blackboard Blackboard => _blackboard;
        public MonoBehaviour Runner => this;

        private Blackboard _blackboard;
        private RuntimeBlueprint2 _runtimeBlueprint;
        private bool _isCompiled;

        public RuntimeBlueprint2 GetOrCompileBlueprint() {
            if (_isCompiled) return _runtimeBlueprint;

            _isCompiled = true;

            _blackboard = GetBlackboard(_blueprintAsset);

            _runtimeBlueprint = _blueprintAsset.Compile(BlueprintFactories.Global);
            _runtimeBlueprint.Initialize(this);

            return _runtimeBlueprint;
        }

        private void Awake() {
            GetOrCompileBlueprint();
        }

        private void OnDestroy() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint?.Destroy();
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

        public Blackboard GetBlackboard(BlueprintAsset2 blueprint) {
            return _blackboardOverridesMap[blueprint];
        }

#if UNITY_EDITOR
        internal bool IsRunningRuntimeBlueprint => _runtimeBlueprint != null;

        internal SerializedDictionary<BlueprintAsset2, Blackboard> BlackboardOverridesMap =>
            _blackboardOverridesMap ??= new SerializedDictionary<BlueprintAsset2, Blackboard>();

        internal void RestartBlueprint() {
            _blackboard = GetBlackboard(_blueprintAsset);
            _runtimeBlueprint = _blueprintAsset.Compile(BlueprintFactories.Global);

            _runtimeBlueprint.Initialize(this);
            _runtimeBlueprint.Start();
        }

        internal void InterruptRuntimeBlueprint() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }
#endif
    }

}
