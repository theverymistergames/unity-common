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

        [SerializeField] private int _nodeId = -1;
        public int NodeId => _nodeId;

        [SerializeField] private BlueprintAsset _ownerAsset;

        [SerializeField] private string _nodeName;
        public string NodeName => _nodeName;

        [SerializeField] private string _nodeColor;
        public Color NodeColor => ColorUtils.HexToColor(_nodeColor);

        private BlueprintNodeMeta() { }

        public BlueprintNodeMeta(BlueprintNode node, int nodeId, BlueprintAsset ownerAsset) {
            _node = node;
            _nodeId = nodeId;
            _ownerAsset = ownerAsset;

            var nodeType = node.GetType();
            var nodeMetaAttr = GetBlueprintNodeMetaAttribute(nodeType);

            _nodeName = string.IsNullOrEmpty(nodeMetaAttr.Name) ? nodeType.Name : nodeMetaAttr.Name;
            _nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;
        }

        public void RecreatePorts() {
            _ports = _node.CreatePorts();
        }

        private void OnValidate() {
            if (_node is IBlueprintValidatedNode validatedNode && _nodeId >= 0 && _ownerAsset != null) {
                validatedNode.OnValidate(_nodeId, _ownerAsset);
            }
        }

        private static BlueprintNodeMetaAttribute GetBlueprintNodeMetaAttribute(Type type) {
            return type.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);
        }
    }

}
