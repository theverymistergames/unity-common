using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintPortDecorator
        , IBlueprintPortLinksListener
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] [Range(1, 32)] private int _exits = 1;

        public override Port[] CreatePorts() {
            var ports = new Port[1 + _exits];

            ports[0] = Port.AnyAction(PortDirection.Input);
            for (int p = 1; p < ports.Length; p++) {
                ports[p] = Port.AnyAction(PortDirection.Output);
            }

            return ports;
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }

        public int GetLinkedPorts(int port, out int count) {
            if (port == 0) {
                count = _exits;
                return 1;
            }

            if (port > 0 && port <= _exits) {
                count = 1;
                return 0;
            }

            count = 0;
            return -1;
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var links = blueprintMeta.GetLinksToNodePort(nodeId, 0);
            if (links.Count == 0) {
                for (int i = 1; i < _exits + 1; i++) {
                    links = blueprintMeta.GetLinksFromNodePort(nodeId, i);
                    if (links.Count > 0) break;
                }
            }
            if (links.Count == 0) return;

            var link = links[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.Create(PortDirection.Input, signature: linkedPort.Signature);
            for (int p = 1; p < ports.Length; p++) {
                ports[p] = Port.Create(PortDirection.Output, signature: linkedPort.Signature);
            }
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
