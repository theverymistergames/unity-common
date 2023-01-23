using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Common.Color;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    /// <summary>
    /// Blueprint node data that is used for:
    /// - Blueprint Editor operations;
    /// - Compilation of the runtime blueprint node instance with links to other runtime node instances.
    /// </summary>
    [Serializable]
    public sealed class BlueprintNodeMeta {

        /// <summary>
        /// NodeId is a key of BlueprintNodeMeta data in the BlueprintAsset nodes storage.
        /// NodeId is provided by BlueprintAsset when BlueprintNodeMeta is added to it.
        /// </summary>
        [SerializeField] private int _nodeId = -1;
        public int NodeId {
            get => _nodeId;
            set => _nodeId = value;
        }

        /// <summary>
        /// String representation of the given node type.
        /// </summary>
        [SerializeField] private string _serializedNodeType;
        public string SerializedNodeType => _serializedNodeType;

        /// <summary>
        /// String representation of the given node.
        /// </summary>
        [SerializeField] private string _nodeJson;
        public string NodeJson => _nodeJson;

        /// <summary>
        /// Position of the blueprint node in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private Vector2 _position;
        public Vector2 Position {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// Display name for the blueprint node view in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private string _nodeName;
        public string NodeName => _nodeName;

        /// <summary>
        /// Display color for the blueprint node view in the Blueprint Editor window.
        /// </summary>
        [SerializeField] private string _nodeColor;
        public Color NodeColor => ColorUtils.HexToColor(_nodeColor);

        /// <summary>
        /// Ports array created by the given node in method BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;
        public IReadOnlyList<Port> Ports => _ports;

        public BlueprintNode CreateNodeInstance() {
            return JsonUtility.FromJson(_nodeJson, SerializedType.FromString(_serializedNodeType)) as BlueprintNode;
        }

        public void SerializeNode(BlueprintNode node) {
            var nodeType = node.GetType();
            var nodeMetaAttr = nodeType.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);

            _nodeJson = JsonUtility.ToJson(node);
            _serializedNodeType = SerializedType.ToString(nodeType);
            _ports = node.CreatePorts();
            _nodeName = string.IsNullOrWhiteSpace(nodeMetaAttr.Name) ? nodeType.Name : nodeMetaAttr.Name.Trim();
            _nodeColor = string.IsNullOrEmpty(nodeMetaAttr.Color) ? BlueprintColors.Node.Default : nodeMetaAttr.Color;
        }

        public void RecreatePorts() {
            _ports = CreateNodeInstance().CreatePorts();
        }

        public override string ToString() {
            return $"{nameof(BlueprintNodeMeta)}(nodeId = {_nodeId}, nodeName = {_nodeName}, nodeType = {_serializedNodeType}, ports = [{string.Join(", ", _ports)}])";
        }
    }

}
