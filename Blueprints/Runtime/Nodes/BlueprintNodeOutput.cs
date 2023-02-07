using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Runtime.Core;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Output", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeOutput :
        BlueprintNode,
        IBlueprintPortDecorator,
        IBlueprintPortLinksListener,
        IBlueprintPortLinker,
        IBlueprintAssetValidator
    {
        [SerializeField] private string _port;
        
        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output(_port).SetExternal(true)
        };

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksFromInput = blueprintMeta.GetLinksFromNodePort(nodeId, 0);
            if (linksFromInput.Count == 0) return;

            var nodesMap = blueprintMeta.NodesMap;

            for (int l = 0; l < linksFromInput.Count; l++) {
                var link = linksFromInput[l];

                var linkedPort = nodesMap[link.nodeId].Ports[link.portIndex];
                if (linkedPort.mode != Port.Mode.Output) continue;

                var dataType = linkedPort.DataType;

                ports[0] = Port.Input(dataType.Name, dataType);
                ports[1] = Port.Output(_port, dataType).SetExternal(true);

                break;
            }
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 0) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
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
