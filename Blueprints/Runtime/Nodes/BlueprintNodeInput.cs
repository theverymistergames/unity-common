using System;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Runtime.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeInput :
        BlueprintNode,
        IBlueprintPortDecorator,
        IBlueprintPortLinksListener,
        IBlueprintPortLinker,
        IBlueprintAssetValidator
    {
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.Input(_port).SetExternal(true),
            Port.Output()
        };

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksToOutput = blueprintMeta.GetLinksToNodePort(nodeId, 1);
            if (linksToOutput.Count == 0) return;

            var link = linksToOutput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];
            var dataType = linkedPort.DataType;

            ports[0] = Port.Input(_port, dataType).SetExternal(true);
            ports[1] = Port.Output(dataType.Name, dataType);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 1) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
    }

}
