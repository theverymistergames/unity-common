using System;
using MisterGames.Blueprints.Core;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using UnityEngine;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Output", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeOutput : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintPortDecorator
        , IBlueprintPortLinksListener
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private string _port;
        
        public override Port[] CreatePorts() => new[] {
            Port.DynamicInput(),
            Port.DynamicOutput(_port).External(true).Hidden(true),
        };

        public int GetLinkedPorts(int port, out int count) {
            if (port == 0) {
                count = 1;
                return 1;
            }

            if (port == 1) {
                count = 1;
                return 0;
            }

            count = 0;
            return -1;
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var linksFromInput = blueprint.BlueprintMeta.GetLinksFromNodePort(nodeId, 0);
            if (linksFromInput.Count == 0) return;

            var link = linksFromInput[0];
            var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.DynamicInput(type: linkedPort.DataType);
            ports[1] = Port.DynamicOutput(_port, linkedPort.DataType).External(true).Hidden(true);
        }

        public void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex) {
            if (portIndex == 0) blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false, notify: false);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
