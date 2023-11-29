using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    internal sealed class VirtualBlueprintContainer : ScriptableObject {

        [SerializeField] private BlueprintFactory _factory;
        [SerializeField] private Blackboard _blackboard;

        public BlueprintFactory Factory { get => _factory; set => _factory = value; }
        public Blackboard Blackboard { get => _blackboard; set => _blackboard = value; }

        public string GetNodePath(NodeId id) {
            return _factory.TryGetNodePath(id, out int s, out int n)
                ? $"_factory._sources._entries.Array.data[{s}].value._nodeMap._entries.Array.data[{n}].value"
                : null;
        }

    }

}
