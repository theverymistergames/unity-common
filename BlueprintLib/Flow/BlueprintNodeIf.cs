using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceIf :
        BlueprintSource<BlueprintNodeIf>,
        BlueprintSources.IEnter<BlueprintNodeIf>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "If", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeIf : IBlueprintNode, IBlueprintEnter {

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

}
