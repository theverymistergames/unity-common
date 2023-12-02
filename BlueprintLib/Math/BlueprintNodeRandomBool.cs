using System;
using MisterGames.Blueprints;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceRandomBool :
        BlueprintSource<BlueprintNodeRandomBool>,
        BlueprintSources.IOutput<BlueprintNodeRandomBool, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Random Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeRandomBool : IBlueprintNode, IBlueprintOutput<bool> {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<bool>());
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
    }

}
