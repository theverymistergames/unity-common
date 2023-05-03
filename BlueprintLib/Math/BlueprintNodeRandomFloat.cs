using System;
using MisterGames.Blueprints;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Random Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _from;
        [SerializeField] private float _to;

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("From"),
            Port.Input<float>("To"),
            Port.Output<float>()
        };

        public float GetOutputPortValue(int port) {
            var from = Ports[0].Get(defaultValue: _from);
            var to = Ports[1].Get(defaultValue: _to);
            
            return port switch {
                2 => Random.Range(from, to),
                _ => 0f
            };
        }
    }

}
