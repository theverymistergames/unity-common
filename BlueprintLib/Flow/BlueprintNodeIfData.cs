using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceIfData :
        BlueprintSource<BlueprintNodeIfData>,
        BlueprintSources.IOutput<BlueprintNodeIfData>,
        BlueprintSources.IConnectionCallback<BlueprintNodeIfData>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "If (data)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeIfData : IBlueprintNode, IBlueprintOutput, IBlueprintConnectionCallback {

        [SerializeField] private bool _condition;

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

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 3) return default;

            bool condition = blueprint.Read(token, 0, _condition);
            return blueprint.Read<T>(token, condition ? 1 : 2);
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port is 1 or 2 or 3) meta.InvalidateNode(id, invalidateLinks: false);
        }
    }

}
