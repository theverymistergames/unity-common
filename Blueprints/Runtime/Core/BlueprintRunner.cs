using MisterGames.Blueprints.Compile;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private SerializedDictionary<BlueprintAsset, Blackboard> _blackboardOverridesMap;

        public BlueprintAsset BlueprintAsset => _blueprintAsset;
        public Blackboard Blackboard => _blackboard;
        public MonoBehaviour Runner => this;

        private Blackboard _blackboard;
        private RuntimeBlueprint _runtimeBlueprint;
        private bool _isRunningRuntimeBlueprint;

        private void Awake() {
            _isRunningRuntimeBlueprint = true;

            _runtimeBlueprint = _blueprintAsset.Compile();

            _blackboard = _blueprintAsset.Blackboard.Clone();
            ResolveBlackboardSceneReferences(_blueprintAsset, _blackboard);

            _runtimeBlueprint.Initialize(this);
        }

        private void OnDestroy() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;

            _isRunningRuntimeBlueprint = false;
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
            if (!_blackboardOverridesMap.TryGetValue(blueprint, out var blackboardOverride)) return;

            blackboard.OverrideValues(blackboardOverride);
        }

#if UNITY_EDITOR
        internal bool IsRunningRuntimeBlueprint => _isRunningRuntimeBlueprint;

        internal SerializedDictionary<BlueprintAsset, Blackboard> BlackboardOverridesMap =>
            _blackboardOverridesMap ??= new SerializedDictionary<BlueprintAsset, Blackboard>();

        internal void CompileAndStartRuntimeBlueprint() {
            _isRunningRuntimeBlueprint = true;

            _runtimeBlueprint = _blueprintAsset.Compile();

            _blackboard = _blueprintAsset.Blackboard.Clone();
            ResolveBlackboardSceneReferences(_blueprintAsset, _blackboard);

            _runtimeBlueprint.Initialize(this);
            _runtimeBlueprint.Start();
        }

        internal void InterruptRuntimeBlueprint() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;

            _isRunningRuntimeBlueprint = false;
        }
#endif
    }

}
