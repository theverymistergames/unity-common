using System;
using MisterGames.Blueprints;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceRandomFloat :
        BlueprintSource<BlueprintNodeRandomFloat>,
        BlueprintSources.IOutput<BlueprintNodeRandomFloat, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Random Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeRandomFloat : IBlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _from;
        [SerializeField] private float _to;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<float>("From"));
            meta.AddPort(id, Port.Input<float>("To"));
            meta.AddPort(id, Port.Output<float>());
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 2) return 0f;

            float from = blueprint.Read(token, 0, _from);
            float to = blueprint.Read(token, 1, _to);

            return Random.Range(from, to);
        }
    }

}
