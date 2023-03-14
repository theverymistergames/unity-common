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
            Port.Func<bool>(PortDirection.Input, "Condition"),
            Port.DynamicFunc(PortDirection.Input, "On True"),
            Port.DynamicFunc(PortDirection.Input, "On False"),
            Port.DynamicFunc(PortDirection.Output),
        };

        public T GetOutputPortValue<T>(int port) {
            if (port != 3) return default;

            bool condition = Ports[0].Get(_condition);
            return Ports[condition ? 1 : 2].Get<T>();
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            var links = blueprintMeta.GetLinksFromNodePort(nodeId, 1);
            if (links.Count == 0) links = blueprintMeta.GetLinksFromNodePort(nodeId, 2);
            if (links.Count == 0) links = blueprintMeta.GetLinksToNodePort(nodeId, 3);
            if (links.Count == 0) return;

            var link = links[0];
            var linkedPort = blueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];
            var dataType = linkedPort.Signature;

            ports[1] = Port.DynamicFunc(PortDirection.Input, "On True", dataType);
            ports[2] = Port.DynamicFunc(PortDirection.Input, "On False", dataType);
            ports[3] = Port.DynamicFunc(PortDirection.Output, returnType: dataType);
        }

        public void OnPortLinksChanged(BlueprintMeta blueprintMeta, int nodeId, int portIndex) {
            if (portIndex is 1 or 2 or 3) blueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true, notify: false);
        }
#endif
    }

}
