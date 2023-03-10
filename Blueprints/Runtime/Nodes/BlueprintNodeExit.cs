using System;
using MisterGames.Blueprints.Core;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using UnityEngine;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Exit", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeExit : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintPortLinksListener
        , IBlueprintPortDecorator
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.AnyAction(PortMode.Input),
            Port.AnyAction(PortMode.Output, _port).External(true),
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
        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksToInput = blueprintMeta.GetLinksToNodePort(nodeId, 0);
            if (linksToInput.Count == 0) return;

            var link = linksToInput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.Create(PortMode.Input, signature: linkedPort.Signature);
            ports[1] = Port.Create(PortMode.Output, _port, linkedPort.Signature).External(true);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 0) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
