﻿using System;
using MisterGames.Blueprints.Core;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using UnityEngine;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Enter", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeEnter : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintPortLinksListener
        , IBlueprintPortDecorator
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.AnyAction(PortDirection.Output),
            Port.AnyAction(PortDirection.Input, _port).External(true),
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
            var linksFromOutput = blueprintMeta.GetLinksFromNodePort(nodeId, 0);
            if (linksFromOutput.Count == 0) return;

            var link = linksFromOutput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            ports[0] = Port.Create(PortDirection.Output, signature: linkedPort.Signature);
            ports[1] = Port.Create(PortDirection.Input, _port, linkedPort.Signature).External(true);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex == 0) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true, notify: false);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
