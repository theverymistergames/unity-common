using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Utils {

    internal sealed class VirtualBlueprintContainer : ScriptableObject {

        [SerializeField] private BlueprintFactory _factory;
        [SerializeField] private Blackboard _blackboard;

        public void Initialize(BlueprintFactory factory, Blackboard blackboard) {
            _factory = factory;
            _blackboard = blackboard;
        }

        public string GetNodePath(NodeId id) {
            return _factory.TryGetNodePath(id, out int s, out int n)
                ? $"_factory._sources._entries.Array.data[{s}].value._nodeMap._entries.Array.data[{n}].value"
                : null;
        }

    }

}
