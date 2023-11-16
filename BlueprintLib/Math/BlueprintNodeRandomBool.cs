using System;
using MisterGames.Blueprints;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceRandomBool :
        BlueprintSource<BlueprintNodeRandomBool2>,
        BlueprintSources.IOutput<BlueprintNodeRandomBool2, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Random Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeRandomBool2 : IBlueprintNode, IBlueprintOutput2<bool> {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<bool>());
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Random Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomBool : BlueprintNode, IBlueprintOutput<bool> {

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public bool GetOutputPortValue(int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
    }

}
