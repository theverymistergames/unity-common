using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTransform :
        BlueprintSource<BlueprintNodeTransform2>,
        BlueprintSources.IOutput<BlueprintNodeTransform2, Vector3>,
        BlueprintSources.IOutput<BlueprintNodeTransform2, Quaternion>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Transform", Category = "Transform", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeTransform2 : IBlueprintNode, IBlueprintOutput2<Vector3>, IBlueprintOutput2<Quaternion> {

        [SerializeField] private bool _useLocal;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Transform>("Transform"));
            meta.AddPort(id, Port.Output<Vector3>("Position"));
            meta.AddPort(id, Port.Output<Vector3>("Euler Angles"));
            meta.AddPort(id, Port.Output<Quaternion>("Rotation"));
            meta.AddPort(id, Port.Output<Vector3>("Scale"));
        }

        Vector3 IBlueprintOutput2<Vector3>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            var t = blueprint.Read<Transform>(token, 0);
            return port switch {
                1 => _useLocal ? t.localPosition : t.position,
                2 => _useLocal ? t.localEulerAngles : t.eulerAngles,
                4 => t.localScale,
                _ => default,
            };
        }

        Quaternion IBlueprintOutput2<Quaternion>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            var t = blueprint.Read<Transform>(token, 0);
            return port switch {
                3 => _useLocal ? t.localRotation : t.rotation,
                _ => default,
            };
        }
    }

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
