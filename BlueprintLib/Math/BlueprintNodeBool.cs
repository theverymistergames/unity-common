using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceBool :
        BlueprintSource<BlueprintNodeBool2>,
        BlueprintSources.IEnter<BlueprintNodeBool2>,
        BlueprintSources.IOutput<BlueprintNodeBool2, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Add Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeBool2 : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<bool> {

        [SerializeField] private bool _value;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set"));
            meta.AddPort(id, Port.Input<bool>());
            meta.AddPort(id, Port.Output<bool>());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _value = blueprint.Read(token, 1, _value);
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            2 => _value,
            _ => default,
        };
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeBool : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private bool _value;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>(),
        };

        public bool GetOutputPortValue(int port) => port switch {
            0 => _value,
            _ => default,
        };
    }

}
