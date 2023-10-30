using UnityEngine;

namespace MisterGames.Blueprints.Factory {

    public sealed class BlueprintStorage : MonoBehaviour {

        private IBlueprintFactory _factory;

        private void Awake() {
            _factory = new BlueprintFactory();
            BlueprintFactories.Global = _factory;
        }
    }

}
