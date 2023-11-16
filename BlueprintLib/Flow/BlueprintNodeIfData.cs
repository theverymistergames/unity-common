﻿using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceIfData :
        BlueprintSource<BlueprintNodeIfData2>,
        BlueprintSources.IOutput<BlueprintNodeIfData2>,
        BlueprintSources.IConnectionCallback<BlueprintNodeIfData2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "If (data)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeIfData2 : IBlueprintNode, IBlueprintOutput2, IBlueprintConnectionCallback {

        [SerializeField] private bool _condition;

        private IBlueprint _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<bool>("Condition"));

            Type dataType = null;

            if (meta.TryGetLinksFrom(id, 1, out int l) ||
                meta.TryGetLinksFrom(id, 2, out l) ||
                meta.TryGetLinksTo(id, 3, out l)
            ) {
                var link = meta.GetLink(l);
                dataType = meta.GetPort(link.id, link.port).DataType;
            }

            meta.AddPort(id, Port.DynamicInput("On True", dataType));
            meta.AddPort(id, Port.DynamicInput("On False", dataType));
            meta.AddPort(id, Port.DynamicOutput(type: dataType));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = null;
        }

        public T GetPortValue<T>(NodeToken token, int port) {
            if (port != 3) return default;

            bool condition = _blueprint.Read(token, 0, _condition);
            return _blueprint.Read<T>(token, condition ? 1 : 2);
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port is 1 or 2 or 3) meta.InvalidateNode(id, invalidateLinks: false);
        }
    }

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
