using System;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

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
