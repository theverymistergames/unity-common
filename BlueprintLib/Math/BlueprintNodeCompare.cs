using System;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCompareFloat :
        BlueprintSource<BlueprintNodeCompareFloat>,
        BlueprintSources.IEnter<BlueprintNodeCompareFloat>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Compare Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeCompareFloat : IBlueprintNode, IBlueprintEnter2 {

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
            meta.AddPort(id, Port.Enter("Compare"));
            meta.AddPort(id, Port.Input<float>("X"));
            meta.AddPort(id, Port.Input<float>("Y"));
            meta.AddPort(id, Port.Exit("On True"));
            meta.AddPort(id, Port.Exit("On False"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            float x = blueprint.Read(token, 1, _x);
            float y = blueprint.Read(token, 2, _y);

            bool result = _mode switch {
                Comparer.Less => x < y,
                Comparer.LessOrEqual => x <= y,
                Comparer.Equal => x.IsNearlyEqual(y),
                Comparer.MoreOrEqual => x >= y,
                Comparer.More => x > y,
                _ => throw new NotImplementedException()
            };

            blueprint.Call(token, result ? 3 : 4);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Compare Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeCompare : BlueprintNode {

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

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Compare"),
            Port.Input<float>("X"),
            Port.Input<float>("Y"),
            Port.Exit("On True"),
            Port.Exit("On False"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            float x = Ports[1].Get(_x);
            float y = Ports[2].Get(_y);

            bool result = _mode switch {
                Comparer.Less => x < y,
                Comparer.LessOrEqual => x <= y,
                Comparer.Equal => x.IsNearlyEqual(y),
                Comparer.MoreOrEqual => x >= y,
                Comparer.More => x > y,
                _ => throw new NotImplementedException()
            };

            Ports[result ? 3 : 4].Call();
        }
    }

}
