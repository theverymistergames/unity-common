using System;
using MisterGames.Blueprints;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Random Bool", Category = "Data", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomBool : BlueprintNode, IBlueprintOutput<bool> {

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public bool GetPortValue(int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
    }

}
