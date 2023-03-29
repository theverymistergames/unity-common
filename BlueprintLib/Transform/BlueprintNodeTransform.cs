using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Transform", Category = "Transform", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeTransform : BlueprintNode, IBlueprintOutput<Vector3>, IBlueprintOutput<Quaternion> {

        [SerializeField] private bool _useLocal;

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>("Transform"),
            Port.Output<Vector3>("Position"),
            Port.Output<Vector3>("Euler Angles"),
            Port.Output<Quaternion>("Rotation"),
            Port.Output<Vector3>("Scale"),
        };

        Vector3 IBlueprintOutput<Vector3>.GetOutputPortValue(int port) {
            var t = Ports[0].Get<Transform>();
            return port switch {
                1 => _useLocal ? t.localPosition : t.position,
                2 => _useLocal ? t.localEulerAngles : t.eulerAngles,
                4 => t.localScale,
                _ => default,
            };
        }

        Quaternion IBlueprintOutput<Quaternion>.GetOutputPortValue(int port) {
            var t = Ports[0].Get<Transform>();
            return port switch {
                3 => _useLocal ? t.localRotation : t.rotation,
                _ => default,
            };
        }
    }

}
