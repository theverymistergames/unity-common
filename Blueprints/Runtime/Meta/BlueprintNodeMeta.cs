using System;
using System.Collections.Generic;
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
        /// Ports array created by the given node in method BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;
        public Port[] Ports => _ports;

        public BlueprintNode CreateNodeInstance() {
            return JsonUtility.FromJson(_nodeJson, SerializedType.FromString(_serializedNodeType)) as BlueprintNode;
        }

        public void SerializeNode(BlueprintNode node) {
            _nodeJson = JsonUtility.ToJson(node);
            _serializedNodeType = SerializedType.ToString(node.GetType());
        }

        public void RecreatePorts(BlueprintNode node) {
            _ports = node.CreatePorts();
        }

        public override string ToString() {
            return $"BlueprintNode#{_nodeId}({_serializedNodeType})";
        }
    }

}
