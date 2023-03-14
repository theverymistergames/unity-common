using System;
using MisterGames.Blueprints;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Random Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomBool : BlueprintNode, IBlueprintOutput<bool> {

        public override Port[] CreatePorts() => new[] {
            Port.Func<bool>(PortDirection.Output)
        };

        public bool GetOutputPortValue(int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
    }

}
