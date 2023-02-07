using System;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input Array", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeInputArray :
        BlueprintNode,
        IBlueprintPortLinker,
        IBlueprintPortDecorator,
        IBlueprintPortLinksListener,
        IBlueprintAssetValidator
    {
        [SerializeField] private string _port;
        
        public override Port[] CreatePorts() => new[] {
            Port.InputArray(_port).SetExternal(true),
            Port.Output(),
        };

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksToOutput = blueprintMeta.GetLinksToNodePort(nodeId, 1);
            if (linksToOutput.Count == 0) return;

            var link = linksToOutput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            var dataType = linkedPort.DataType;
            var arrayDataType = dataType.MakeArrayType();

            ports[0] = Port.InputArray(_port, dataType).SetExternal(true);
            ports[1] = Port.Output(arrayDataType.Name, arrayDataType);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 0) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
    }

}
