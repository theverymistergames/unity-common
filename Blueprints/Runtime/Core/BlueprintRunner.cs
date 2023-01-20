using MisterGames.Blueprints.Compile;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private BlueprintAsset _blueprintAsset;

        public BlueprintAsset BlueprintAsset => _blueprintAsset;

        public RuntimeBlackboard Blackboard => _runtimeBlackboard;
        public MonoBehaviour Runner => this;

        private RuntimeBlueprint _runtimeBlueprint;
        private RuntimeBlackboard _runtimeBlackboard;

        private void Awake() {
            _runtimeBlackboard = _blueprintAsset.Blackboard.Compile();

            _runtimeBlueprint = _blueprintAsset.Compile();
            _runtimeBlueprint.Initialize(this);
        }

        private void OnDestroy() {
            _runtimeBlueprint.DeInitialize();
        }

        private void Start() {
            _runtimeBlueprint.Start();
        }
    }

}
