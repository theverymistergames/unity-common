﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node data that is used for:
    /// - Blueprint Editor operations;
    /// - Compilation of the runtime blueprint node instance with links to other runtime node instances.
    /// </summary>
    public sealed class BlueprintNodeMeta : ScriptableObject {

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

        public static BlueprintNodeMeta Create(BlueprintAsset ownerAsset, Type nodeType) {
            var nodeMeta = CreateInstance<BlueprintNodeMeta>();

            var nodeInstance = (BlueprintNode) Activator.CreateInstance(nodeType);

            nodeMeta._node = nodeInstance;
            nodeMeta.RecreatePorts();

            nodeMeta._ownerAsset = ownerAsset;
            nodeMeta._nodeId = ownerAsset.BlueprintMeta.AddNode(nodeMeta);

            return nodeMeta;
        }

        public void RecreatePorts() {
            _ports = _node.CreatePorts();
        }

        private void OnValidate() {
            if (_node is IBlueprintValidatedNode validatedNode && _ownerAsset != null && _nodeId >= 0) {
                validatedNode.OnValidate(_nodeId, _ownerAsset);
            }
        }
    }

}
