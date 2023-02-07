﻿using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Runtime.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Output Array", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeOutputArray :
        BlueprintNode,
        IBlueprintPortDecorator,
        IBlueprintPortLinksListener,
        IBlueprintPortLinker,
        IBlueprintAssetValidator
    {
        [SerializeField] private string _port;
        
        public override Port[] CreatePorts() => new[] {
            Port.InputArray(),
            Port.Output(_port).SetExternal(true)
        };

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var linksFromInput = blueprintMeta.GetLinksFromNodePort(nodeId, 0);
            if (linksFromInput.Count == 0) return;

            var link = linksFromInput[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

            var dataType = linkedPort.DataType;
            var arrayDataType = dataType.MakeArrayType();

            ports[0] = Port.InputArray(dataType.Name, dataType);
            ports[1] = Port.Output(_port, arrayDataType).SetExternal(true);
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
