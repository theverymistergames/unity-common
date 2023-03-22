using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "If (Data)", Category = "Flow", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeIfData : BlueprintNode, IBlueprintOutput

#if UNITY_EDITOR
    , IBlueprintPortDecorator
    , IBlueprintPortLinksListener
#endif

    {
        [SerializeField] private bool _condition;

        public override Port[] CreatePorts() => new[] {
            Port.Input<bool>("Condition"),
            Port.DynamicInput("On True"),
            Port.DynamicInput("On False"),
            Port.DynamicOutput(),
        };

        public T GetOutputPortValue<T>(int port) {
            if (port != 3) return default;

            bool condition = Ports[0].Get(_condition);
            return Ports[condition ? 1 : 2].Get<T>();
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var links = blueprint.BlueprintMeta.GetLinksFromNodePort(nodeId, 1);
            if (links.Count == 0) links = blueprint.BlueprintMeta.GetLinksFromNodePort(nodeId, 2);
            if (links.Count == 0) links = blueprint.BlueprintMeta.GetLinksToNodePort(nodeId, 3);
            if (links.Count == 0) return;

            var link = links[0];
            var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];
            var dataType = linkedPort.DataType;

            ports[1] = Port.DynamicInput("On True", dataType);
            ports[2] = Port.DynamicInput("On False", dataType);
            ports[3] = Port.DynamicOutput(type: dataType);
        }

        public void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex) {
            if (portIndex is 1 or 2 or 3) blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false);
        }
#endif
    }

}
