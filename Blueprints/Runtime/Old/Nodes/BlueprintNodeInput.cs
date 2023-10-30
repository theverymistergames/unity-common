using System;
using MisterGames.Blueprints.Core;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using UnityEngine;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeInput : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintPortDecorator
        , IBlueprintPortLinksListener
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.DynamicOutput(),
            Port.DynamicInput(_port).External(true).Hide(true),
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
            var linksToOutput = blueprint.BlueprintMeta.GetLinksToNodePort(nodeId, 0);
            if (linksToOutput.Count == 0) return;

            var link = linksToOutput[0];
            var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.DynamicOutput(type: linkedPort.DataType);
            ports[1] = Port.DynamicInput(_port, linkedPort.DataType).External(true).Hide(true);
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
