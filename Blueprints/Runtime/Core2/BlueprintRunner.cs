using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintRunner : MonoBehaviour {

        [SerializeField] private BlueprintAsset _blueprintAsset;

        private RuntimeBlueprint _runtimeBlueprint;

        private void Awake() {
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
