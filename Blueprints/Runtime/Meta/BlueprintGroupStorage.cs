using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {
    
    [Serializable]
    public sealed class BlueprintGroupStorage {

        [SerializeField] private List<BlueprintGroup> _groups;
        [SerializeField] private int _lastId;

        public IReadOnlyList<BlueprintGroup> Groups => _groups;

        public bool TryGetGroupOfNode(NodeId nodeId, out int groupId) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.nodes is not {Count: > 0} nodes) continue;
                
                for (int j = 0; j < nodes.Count; j++) {
                    if (nodes[j] != nodeId) continue;

                    groupId = g.id;
                    return true;
                }
            }

            groupId = default;
            return false;
        }
        
        public bool AddNodeIntoGroup(NodeId nodeId, int groupId) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.id != groupId) continue;

                g.nodes ??= new List<NodeId>();
                g.nodes.Add(nodeId);
                
                return true;
            }

            return false;
        }

        public bool RemoveNodeFromGroups(NodeId nodeId) {
            bool hasNode = false;
            
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.nodes is not {Count: > 0} nodes) continue;
                
                for (int j = nodes.Count - 1; j >= 0; j--) {
                    if (nodes[j] != nodeId) continue;
                    
                    hasNode = true;
                    nodes.RemoveAt(j);
                }

                _groups[i] = g;
            }

            return hasNode;
        }
        
        public BlueprintGroup GetGroup(int id) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.id == id) return g;
            }

            return default;
        }
        
        public bool TryGetGroup(int id, out BlueprintGroup group) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.id == id) {
                    group = g;
                    return true;
                }
            }

            group = default;
            return false;
        }
        
        public int AddGroup(BlueprintGroup group) {
            _groups.Add(new BlueprintGroup { id = ++_lastId, name = group.name, position = group.position, nodes = group.nodes });
            return _lastId;
        }

        public bool RemoveGroup(int id) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.id != id) continue;

                _groups.RemoveAt(i);
                return true;
            }

            return false;
        }

        public void SetGroup(int id, BlueprintGroup group) {
            for (int i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.id != id) continue;

                _groups[i] = group;
                return;
            }
            
            _groups.Add(new BlueprintGroup { id = id, name = group.name, position = group.position, nodes = group.nodes });
        }
    }
    
}