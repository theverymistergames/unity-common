using System;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCompareInt :
        BlueprintSource<BlueprintNodeCompareInt>,
        BlueprintSources.IOutput<BlueprintNodeCompareInt, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Compare Int", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeCompareInt : IBlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private Comparer _mode;

        private enum Comparer {
            Less,
            LessOrEqual,
            Equal,
            MoreOrEqual,
            More
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<int>("X"));
            meta.AddPort(id, Port.Input<int>("Y"));
            meta.AddPort(id, Port.Output<bool>());
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 2) return default;

            int x = blueprint.Read(token, 0, _x);
            int y = blueprint.Read(token, 1, _y);

            return _mode switch {
                Comparer.Less => x < y,
                Comparer.LessOrEqual => x <= y,
                Comparer.Equal => x == y,
                Comparer.MoreOrEqual => x >= y,
                Comparer.More => x > y,
                _ => throw new NotImplementedException()
            };
        }
    }

}
