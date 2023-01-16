using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node data that is used for:
    /// - Blueprint Editor operations;
    /// - Compilation of the runtime blueprint node instance with links to other runtime node instances.
    /// </summary>
    [Serializable]
    public sealed class BlueprintNodeMeta {

        /// <summary>
        /// Reference to the serializable blueprint node implementation,
        /// to be able to store serialized data inside node.
        /// </summary>
        [SerializeReference] private BlueprintNode _node;
        public BlueprintNode Node => _node;

        /// <summary>
        /// Position of the blueprint node in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private Vector2 _position;
        public Vector2 Position {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// Port array created by BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;
        public IReadOnlyList<Port> Ports => _ports;

        public static BlueprintNodeMeta FromType(Type nodeType) {
            var node = (BlueprintNode) Activator.CreateInstance(nodeType);
            return new BlueprintNodeMeta(node);
        }

        private BlueprintNodeMeta() { }

        private BlueprintNodeMeta(BlueprintNode node) {
            _node = node;
            RecreatePorts();
        }

        public void OnValidate(int nodeId, BlueprintAsset owner) {
            if (_node is IBlueprintValidatedNode callback) callback.OnValidate(nodeId, owner);
        }

        public void RecreatePorts() {
            _ports = _node.CreatePorts();
        }
    }
}
