using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Common.Color;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node data that is used for:
    /// - Blueprint Editor operations;
    /// - Compilation of the runtime blueprint node instance with links to other runtime node instances.
    /// </summary>
    [Serializable]
    public sealed class BlueprintNodeMeta {

        [SerializeField] private int _nodeId = -1;
        public int NodeId {
            get => _nodeId;
            set => _nodeId = value;
        }

        /// <summary>
        /// Reference to the serializable blueprint node implementation,
        /// to be able to store serialized data inside node.
        /// </summary>
        [SerializeReference] private BlueprintNode _node;
        public BlueprintNode Node => _node;

        /// <summary>
        /// Port array created by BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;
        public IReadOnlyList<Port> Ports => _ports;

        /// <summary>
        /// Position of the blueprint node in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private Vector2 _position;
        public Vector2 Position {
            get => _position;
            set => _position = value;
        }

        [SerializeField] private string _nodeName;
        public string NodeName => _nodeName;

        [SerializeField] private string _nodeColor;
        public Color NodeColor => ColorUtils.HexToColor(_nodeColor);

        private BlueprintNodeMeta() { }

        public static BlueprintNodeMeta Create(Type nodeType) {
            var nodeMetaAttr = nodeType.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);

            var nodeMeta = new BlueprintNodeMeta {
                _node = (BlueprintNode) Activator.CreateInstance(nodeType),
                _nodeName = string.IsNullOrWhiteSpace(nodeMetaAttr.Name) ? nodeType.Name : nodeMetaAttr.Name.Trim(),
                _nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color,
            };

            nodeMeta.RecreatePorts();

            return nodeMeta;
        }

        public void RecreatePorts() {
            _ports = _node.CreatePorts();
        }
    }

}
