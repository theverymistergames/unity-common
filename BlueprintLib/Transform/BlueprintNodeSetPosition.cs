using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSetPosition :
        BlueprintSource<BlueprintNodeSetPosition>,
        BlueprintSources.IEnter<BlueprintNodeSetPosition>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Position", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetPosition : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _position;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Position"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var transform = blueprint.Read(token, 1, _transform);
            var position = blueprint.Read(token, 2, _position);

            transform.position = position;

            blueprint.Call(token, 3);
        }
    }

}
