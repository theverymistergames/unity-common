using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "If (Data)", Category = "Flow", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeDataIf : BlueprintNode, IBlueprintOutput

#if UNITY_EDITOR
    , IBlueprintPortDecorator
    , IBlueprintPortLinksListener
#endif

    {
        [SerializeField] private bool _condition;

        public override Port[] CreatePorts() => new[] {
            Port.Input<bool>("Condition"),
            Port.Input("On True"),
            Port.Input("On False"),
            Port.Output(),
        };

        public T GetOutputPortValue<T>(int port) {
            if (port != 3) return default;

            bool condition = ReadInputPort(0, _condition);
            return ReadInputPort<T>(condition ? 1 : 2);
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var links = blueprintMeta.GetLinksFromNodePort(nodeId, 1);
            if (links.Count == 0) links = blueprintMeta.GetLinksFromNodePort(nodeId, 2);
            if (links.Count == 0) links = blueprintMeta.GetLinksToNodePort(nodeId, 3);
            if (links.Count == 0) return;

            var link = links[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];
            var dataType = linkedPort.dataType;

            ports[1] = Port.Input("On True", dataType);
            ports[2] = Port.Input("On False", dataType);
            ports[3] = Port.Output(null, dataType);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex is 1 or 2 or 3) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#endif
    }

}
