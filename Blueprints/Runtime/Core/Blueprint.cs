﻿using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using UnityEngine;

 namespace MisterGames.Blueprints.Core {

    public interface IBlueprint {
        
        BlueprintNode[] Nodes { get; }
        Blackboard Blackboard { get; }

        void AddNode(BlueprintNode node);
        void RemoveNode(BlueprintNode node);

    }
    
    [CreateAssetMenu(fileName = nameof(Blueprint), menuName = "MisterGames/" + nameof(Blueprint))]
    public sealed class Blueprint : BlueprintNode, IBlueprint {

        [HideInInspector]
        [SerializeField] private List<BlueprintNode> _nodes = new List<BlueprintNode>();
        BlueprintNode[] IBlueprint.Nodes => _nodes.ToArray();

        [HideInInspector]
        [SerializeField] private Blackboard _blackboard;
        public Blackboard Blackboard => _blackboard;

        internal override bool HasStablePorts => false;
        internal override bool HasExposedPorts => true;

        protected override void Awake() {
            base.Awake();
            m_NodeOwner = this;
        }

        protected override IReadOnlyList<Port> CreatePorts() {
            var portNameMap = new Dictionary<string, int>();
            var nodePortMap = new Dictionary<BlueprintNode, int>();
            
            var exposedPorts = new List<Port>();
            int portIndex = 0;
            
            for (int n = 0; n < _nodes.Count; n++) {
                var node = _nodes[n];
                var iNode = node.AsIBlueprintNode();
                
                if (!node.HasStablePorts || node.HasExposedPorts) {
                    iNode.OnPortsUpdated -= InvalidatePorts;
                    iNode.OnPortsUpdated += InvalidatePorts;    
                }
                
                if (!node.HasExposedPorts) continue;
                var ports = iNode.Ports;
                
                for (int p = 0; p < iNode.Ports.Length; p++) {
                    var port = ports[p];
                    if (!port.IsExposed) continue;

                    int index = portIndex;
                    if (portNameMap.ContainsKey(port.Name)) {
                        index = portNameMap[port.Name];
                    }
                    else {
                        var exposedPort = port.CopyWithoutLinks();
                        
                        if (port.IsBuiltIn && exposedPorts.Count > 0) {
                            exposedPorts.Insert(0, exposedPort);
                            index = 0;

                            foreach (var entry in nodePortMap) {
                                var processedNode = entry.Key;
                                int processedNodePort = entry.Value;
                                int writtenOwnerPort = processedNode.AsIBlueprintNode().Ports[processedNodePort].OwnerPort;
                                processedNode.SetOwnerPort(processedNodePort, writtenOwnerPort + 1);
                            }
                        }
                        else {
                            exposedPorts.Add(exposedPort);
                        }
                        
                        portNameMap[port.Name] = index;
                        portIndex++;
                    }

                    nodePortMap[node] = p;
                    node.SetOwnerPort(p, index);
                }
            }
            
            nodePortMap.Clear();
            portNameMap.Clear();
            
            return exposedPorts;
        }

        protected override void OnInit() {
            for (int i = 0; i < _nodes.Count; i++) {
                _nodes[i].Init(runner, host);
            }
        }

        protected override void OnTerminate() {
            for (int i = 0; i < _nodes.Count; i++) {
                _nodes[i].Terminate();
            }
        }
        
        internal void Start() {
            for (int i = 0; i < _nodes.Count; i++) {
                _nodes[i].OnStart();
            }
        }

        void IBlueprint.AddNode(BlueprintNode node) {
            node.m_NodeOwner = this;
            _nodes.Add(node);
            
            node.AsIBlueprintNode().InitPorts();
            InvalidatePorts();
        }

        void IBlueprint.RemoveNode(BlueprintNode node) {
            if (!_nodes.Remove(node)) return;

            var iNode = node.AsIBlueprintNode();
            iNode.OnPortsUpdated -= InvalidatePorts;
            iNode.DeInitPorts();

            DisconnectNode(node);
            
            InvalidatePorts();
        }

        private void DisconnectNode(BlueprintNode other) {
            var iOther = other.AsIBlueprintNode();
            
            iOther.DisconnectAllPorts();
            var otherPorts = iOther.Ports;
            
            for (int n = 0; n < _nodes.Count; n++) {
                var node = _nodes[n].AsIBlueprintNode();
                var ports = node.Ports;
                
                for (int p = 0; p < ports.Length; p++) {
                    for (int o = 0; o < otherPorts.Length; o++) {
                        node.DisconnectPort(p, other, o);    
                    }
                }    
            }
        }
        
        internal override void OnResolveLinks(BlueprintNode original, IBlueprintResolver resolver) {
            var originalNodes = ((Blueprint) original)._nodes;
            int count = originalNodes.Count;
            resolver.Prepare(originalNodes);
            
            _nodes = new List<BlueprintNode>(count);
            for (int i = 0; i < count; i++) {
                var originalNode = originalNodes[i];
                var node = resolver.Resolve(originalNode);
                
                node.m_NodeOwner = this;
                _nodes.Add(node);
            }

            for (int i = 0; i < _nodes.Count; i++) {
                var originalNode = originalNodes[i];
                _nodes[i].ResolveLinks(originalNode, resolver);    
            }
        }

        internal override void OnInitPorts() {
            for (int i = 0; i < _nodes.Count; i++) {
                var node = _nodes[i];
                
                if (node == null) {
                    _nodes.RemoveAt(i--);
                    continue;
                }
                
                node.AsIBlueprintNode().InitPorts();
            }
            
            for (int i = 0; i < _nodes.Count; i++) {
                _nodes[i].AsIBlueprintNode().InitPorts();
            }
        }

        internal override void OnDeInitPorts() {
            for (int i = 0; i < _nodes.Count; i++) {
                var node = _nodes[i].AsIBlueprintNode();
                node.OnPortsUpdated -= InvalidatePorts;
                node.DeInitPorts();
            }
        }

        internal bool TryGetConnectedPort(BlueprintNode fromNode, int fromPort, out Port port) {
            port = default;
            if (!fromNode.HasPort(fromPort)) return false;

            var sourcePort = fromNode.AsIBlueprintNode().Ports[fromPort];
            if (sourcePort.IsOwned) {
                var links = sourcePort.Links;
                if (links.Length == 0) return false;

                var link = links[0];
                var remote = link.remote;
                int remotePort = link.remotePort;

                if (!remote.HasPort(remotePort)) return false;
                
                port = remote.AsIBlueprintNode().Ports[remotePort];
                return true;
            }

            string sourceGuid = fromNode.Guid;
            for (int n = 0; n < _nodes.Count; n++) {
                var remote = _nodes[n];
                if (sourceGuid == remote.Guid) continue;
                
                var ports = remote.AsIBlueprintNode().Ports;
                for (int p = 0; p < ports.Length; p++) {
                    var remotePort = ports[p];
                    if (!remotePort.IsOwned || !remotePort.IsCompatibleWith(remote, sourcePort, fromNode)) {
                        continue;
                    }
                    
                    var links = remotePort.Links;
                    for (int l = 0; l < links.Length; l++) {
                        var link = links[l];
                        if (link.remote.Guid == sourceGuid && link.remotePort == fromPort) {
                            port = remotePort;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }

}
