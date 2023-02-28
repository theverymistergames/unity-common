using System;
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
        /// Node instance that is serialized by reference,
        /// used to store data of the concrete BlueprintNode implementation.
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
        /// Ports array created by the given node in method BlueprintNode.CreatePorts().
        /// </summary>
        [SerializeField] private Port[] _ports;
        public Port[] Ports => _ports;

        private bool _hasCachedNodeJson;
        private string _cachedNodeJson;

        private BlueprintNodeMeta() { }

        public BlueprintNodeMeta(BlueprintNode node) {
            _node = node;
        }

        public BlueprintNode CreateNodeInstance() {
            if (!_hasCachedNodeJson) {
                _cachedNodeJson = JsonUtility.ToJson(_node);
                _hasCachedNodeJson = true;
            }

            return (BlueprintNode) JsonUtility.FromJson(_cachedNodeJson, _node.GetType());
        }

        public void RecreatePorts(BlueprintMeta blueprintMeta) {
            var ports = _node.CreatePorts();
            if (_node is IBlueprintPortDecorator decorator) decorator.DecoratePorts(blueprintMeta, _nodeId, ports);
            _ports = ports;
        }

        public void OnValidateNode(BlueprintAsset blueprint) {
            _node.OnValidate();
            if (_node is IBlueprintAssetValidator validator) validator.ValidateBlueprint(blueprint, _nodeId);

            _hasCachedNodeJson = false;
        }

        public override string ToString() {
            return $"BlueprintNode#{_nodeId}({_node})";
        }
    }

}
