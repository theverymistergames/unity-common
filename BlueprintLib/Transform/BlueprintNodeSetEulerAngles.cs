using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSetEulerAngles :
        BlueprintSource<BlueprintNodeSetEulerAngles2>,
        BlueprintSources.IEnter<BlueprintNodeSetEulerAngles2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Euler Angles", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetEulerAngles2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private Vector3 _eulers;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Eulers"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var transform = blueprint.Read<Transform>(token, 1);
            var eulers = blueprint.Read(token, 2, _eulers);

            transform.eulerAngles = eulers;

            blueprint.Call(token, 3);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Euler Angles", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetEulerAngles : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _eulers;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Transform>(),
            Port.Input<Vector3>("Eulers"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var transform = Ports[1].Get<Transform>();
            var eulers = Ports[2].Get(_eulers);

            transform.eulerAngles = eulers;

            Ports[3].Call();
        }
    }

}
