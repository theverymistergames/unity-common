using MisterGames.Blueprints.Compile;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;
        [SerializeField] private SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> _sceneReferencesMap;

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
            if (!_sceneReferencesMap.TryGetValue(blueprint, out var references)) return;

            foreach ((int hash, var gameObjectRef) in references) {
                blackboard.SetGameObject(hash, gameObjectRef);
            }
        }

#if UNITY_EDITOR
        internal bool IsRunningRuntimeBlueprint => _isRunningRuntimeBlueprint;

        internal SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>> SceneReferencesMap =>
            _sceneReferencesMap ??= new SerializedDictionary<BlueprintAsset, SerializedDictionary<int, GameObject>>();

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
