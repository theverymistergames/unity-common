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

#if UNITY_EDITOR
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.Output(),
            Port.Input(_port).SetExternal(true),
        };

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksToOutput = blueprintMeta.GetLinksToNodePort(nodeId, 1);
            if (linksToOutput.Count == 0) return;

            var link = linksToOutput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];
            var dataType = linkedPort.dataType;

            ports[0] = Port.Output(null, dataType);
            ports[1] = Port.Input(_port, dataType).SetExternal(true);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 0) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#else
        public override Port[] CreatePorts() => null;
#endif

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
    }

}
