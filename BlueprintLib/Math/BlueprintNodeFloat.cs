using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceFloat :
        BlueprintSource<BlueprintNodeFloat2>,
        BlueprintSources.IEnter<BlueprintNodeFloat2>,
        BlueprintSources.IOutput<BlueprintNodeFloat2, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeFloat2 : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<float> {

        [SerializeField] private float _value;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set"));
            meta.AddPort(id, Port.Input<float>());
            meta.AddPort(id, Port.Output<float>());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _value = blueprint.Read(token, 1, _value);
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            2 => _value,
            _ => default,
        };
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _value;

        public override Port[] CreatePorts() => new[] {
            Port.Output<float>(),
        };

        public float GetOutputPortValue(int port) => port switch {
            0 => _value,
            _ => default,
        };
    }

}
