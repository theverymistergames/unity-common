using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSetEulerAngles :
        BlueprintSource<BlueprintNodeSetEulerAngles>,
        BlueprintSources.IEnter<BlueprintNodeSetEulerAngles>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Euler Angles", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetEulerAngles : IBlueprintNode, IBlueprintEnter {

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

}
