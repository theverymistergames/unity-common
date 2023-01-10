using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class Blueprint {

        [SerializeReference] [HideInInspector] private List<BlueprintNode> _nodes;
        public IReadOnlyList<BlueprintNode> Nodes => _nodes;

        private readonly Router _router = new Router();

        public void AddNode(BlueprintNode node) {
            _nodes.Add(node);
        }

        public void RemoveNode(BlueprintNode node) {
            _nodes.Remove(node);
        }

        public RuntimeBlueprint CreateInstance() {
            _router.Clear();
            var runtimeNodes = new BlueprintNode[_nodes.Count];

            for (int i = 0; i < _nodes.Count; i++) {
                var serializedNode = _nodes[i];
                var runtimeNode = (BlueprintNode) Activator.CreateInstance(serializedNode.GetType());

                runtimeNodes[i] = runtimeNode;
                _router.Add(i, runtimeNode);
            }

            for (int i = 0; i < runtimeNodes.Length; i++) {
                runtimeNodes[i].ResolveLinks(_router);
            }

            _router.Clear();
            return new RuntimeBlueprint(runtimeNodes);
        }

        private sealed class Router : IBlueprintRouter {

            private readonly Dictionary<int, BlueprintNode> _nodeMap = new Dictionary<int, BlueprintNode>();

            public void Add(int nodeId, BlueprintNode node) {
                _nodeMap.Add(nodeId, node);
            }

            public BlueprintNode GetNode(int nodeId) {
                return _nodeMap[nodeId];
            }

            public void Clear() {
                _nodeMap.Clear();
            }
        }

    }

}
