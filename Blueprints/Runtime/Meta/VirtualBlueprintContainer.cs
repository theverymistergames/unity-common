using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    internal sealed class VirtualBlueprintContainer : ScriptableObject {

        public BlueprintFactory factory;
        public Blackboard blackboard;

        public string GetNodePath(NodeId id) {
            return factory.TryGetNodePath(id, out int s, out int n)
                ? $"factory._sources._entries.Array.data[{s}].value._nodeMap._entries.Array.data[{n}].value"
                : null;
        }

    }

}
