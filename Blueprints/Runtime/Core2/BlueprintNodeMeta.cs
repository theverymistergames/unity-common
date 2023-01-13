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

        /// <summary>
        /// Position of the blueprint node in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private Vector2 _position;

        /// <summary>
        /// Port array created by BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;

        public Type NodeType => _node.GetType();

        public IReadOnlyList<Port> Ports => _ports;

        public Vector2 Position {
            get => _position;
            set => _position = value;
        }

        public static BlueprintNodeMeta FromType(Type nodeType) {
            var node = (BlueprintNode) Activator.CreateInstance(nodeType);
            var ports = node.CreatePorts();
            return new BlueprintNodeMeta(node, ports);
        }

        private BlueprintNodeMeta() { }

        private BlueprintNodeMeta(BlueprintNode node, Port[] ports) {
            _node = node;
            _ports = ports;
        }
    }

}
