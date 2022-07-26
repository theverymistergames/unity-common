using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Input", Category = "Exposed", Color = BlueprintColors.Node.Exposed)]
    public sealed class BlueprintNodeInput : BlueprintNode, IBlueprintGetter {

        [SerializeField] private string _parameter;

        internal override bool HasExposedPorts => true;

        protected override IReadOnlyList<Port> CreatePorts() {
            var input = Port.Input(_parameter).Exposed();
            var output = Port.Output();

            var node = this.AsIBlueprintNode();
            bool hasPort = node.Ports.Length > 1;

            if (hasPort && m_NodeOwner.TryGetConnectedPort(this, 1, out var port)) {
                input = input.CopyViewFrom(port);
                output = output.CopyViewFrom(port);
            }
            
            return new List<Port> {
                input,
                output,
            };
        }

        T IBlueprintGetter.Get<T>(int port) {
            return port == 1 ? Read<T>(port: 0) : default;
        }
        
        protected override void OnPortConnected(BlueprintNode node, int port) {
            if (port == 1) InvalidatePorts();
        }

        protected override void OnPortDisconnected(BlueprintNode node, int port) {
            if (port == 1) InvalidatePorts();
        }

    }

}