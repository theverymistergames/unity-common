using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceIf :
        BlueprintSource<BlueprintNodeIf2>,
        BlueprintSources.IEnter<BlueprintNodeIf2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "If", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeIf2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private bool _condition;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<bool>("Condition"));
            meta.AddPort(id, Port.Exit("On True"));
            meta.AddPort(id, Port.Exit("On False"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            bool condition = blueprint.Read(token, 1, _condition);
            blueprint.Call(token, condition ? 2 : 3);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "If", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeIf : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private bool _condition;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<bool>("Condition"),
            Port.Exit("On True"),
            Port.Exit("On False"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            bool condition = Ports[1].Get(_condition);
            Ports[condition ? 2 : 3].Call();
        }
    }

}
