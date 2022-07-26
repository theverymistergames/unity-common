using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Subgraph", Category = "Exposed", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeSubgraph : BlueprintNode, IBlueprintHost, IBlueprintEnter, IBlueprintGetter {

        [SerializeField] private Blueprint _blueprint;
        Blueprint IBlueprintHost.Source => _blueprint;
        
        private event Action<BlueprintNode, BlueprintNode> _onFlow = delegate {  };
        event Action<BlueprintNode, BlueprintNode> IBlueprintHost.OnFlow {
            add => _onFlow += value;
            remove => _onFlow -= value;
        }

        private Blueprint _instance;
        Blueprint IBlueprintHost.Instance => _instance;

        internal override bool HasStablePorts => false;

        protected override IReadOnlyList<Port> CreatePorts() {
            if (_blueprint == null) return new Port[0];
            
            var subgraph = _blueprint.AsIBlueprintNode();
            subgraph.InitPorts();
            
            var subgraphPorts = subgraph.Ports;
            var ports = new List<Port>();
            
            for (int i = 0; i < subgraphPorts.Length; i++) {
                var subgraphPort = subgraphPorts[i];
                if (!subgraphPort.IsExposed) continue;
                
                ports.Add(subgraphPort.NotExposed());
            }
            
            return ports;
        }

        protected override void OnInit() {
            _instance.Init(runner, this);
        }

        protected override void OnTerminate() {
            _instance.Terminate();
            _instance = null;
        }

        void IBlueprintHost.OnCalled(BlueprintNode source, BlueprintNode target) {
            target.FlowCount++;
            _onFlow.Invoke(source, target);
        }

        void IBlueprintEnter.Enter(int port) {
            Call(port);
        }

        T IBlueprintGetter.Get<T>(int port) {
            return Read<T>(port);
        }

        internal override void OnResolveLinks(BlueprintNode original, IBlueprintResolver resolver) {
            resolver.Prepare(_blueprint);
            
            _instance = Instantiate(_blueprint);
            _instance.name = $"{_blueprint.name} (Runtime)";
            
            _instance.ResolveLinks(_blueprint, resolver);

            var externalPorts = this.AsIBlueprintNode().Ports;
            var nodes = _instance.AsIBlueprint().Nodes;

            for (int n = 0; n < nodes.Length; n++) {
                var node = nodes[n];
                var iNode = node.AsIBlueprintNode();
                var ports = iNode.Ports;
                
                for (int p = 0; p < ports.Length; p++) {
                    var port = ports[p];
                    
                    if (!port.IsExposed) continue;

                    int ownerPort = port.OwnerPort;
                    
                    if (port.IsOwned) {
                        var externalLinks = externalPorts[ownerPort].Links;
                        for (int l = 0; l < externalLinks.Length; l++) {
                            var link = externalLinks[l];
                            iNode.ConnectPort(p, link.remote, link.remotePort);
                        }
                        continue;
                    }

                    ConnectPort(ownerPort, node, p, true);
                }
            }
        }

        internal override void OnInitPorts() {
            if (_blueprint == null) return;
            _blueprint.AsIBlueprintNode().InitPorts();
        }

        internal override void OnDeInitPorts() {
            if (_blueprint == null) return;
            _blueprint.AsIBlueprintNode().DeInitPorts();
        }

        private void OnValidate() {
            if (_blueprint == m_NodeOwner) {
                Debug.LogWarning($"Subgraph node can not execute its owner Blueprint {m_NodeOwner.name}");
                _blueprint = null;
            }
            
            InvalidatePorts();
        }

    }

}