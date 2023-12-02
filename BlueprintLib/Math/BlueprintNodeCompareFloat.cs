using System;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCompareFloat :
        BlueprintSource<BlueprintNodeCompareFloat>,
        BlueprintSources.IOutput<BlueprintNodeCompareFloat, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Compare Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeCompareFloat : IBlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private float _x;
        [SerializeField] private float _y;
        [SerializeField] private Comparer _mode;

        private enum Comparer {
            Less,
            LessOrEqual,
            Equal,
            MoreOrEqual,
            More
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<float>("X"));
            meta.AddPort(id, Port.Input<float>("Y"));
            meta.AddPort(id, Port.Output<bool>());
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 2) return default;

            float x = blueprint.Read(token, 0, _x);
            float y = blueprint.Read(token, 1, _y);

            return _mode switch {
                Comparer.Less => x < y,
                Comparer.LessOrEqual => x <= y,
                Comparer.Equal => x.IsNearlyEqual(y),
                Comparer.MoreOrEqual => x >= y,
                Comparer.More => x > y,
                _ => throw new NotImplementedException()
            };
        }
    }

}
