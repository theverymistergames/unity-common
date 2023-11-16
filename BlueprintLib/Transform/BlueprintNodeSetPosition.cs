using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSetPosition :
        BlueprintSource<BlueprintNodeSetPosition2>,
        BlueprintSources.IEnter<BlueprintNodeSetPosition2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Position", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetPosition2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private Vector3 _position;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Position"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var transform = blueprint.Read<Transform>(token, 1);
            var position = blueprint.Read(token, 2, _position);

            transform.position = position;

            blueprint.Call(token, 3);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Position", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetPosition : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _position;

        private bool _isInitialized;
        private Vector3 _initialPosition;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Transform>("Transform"),
            Port.Input<Vector3>("Position"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var transform = Ports[1].Get<Transform>();
            var position = Ports[2].Get(_position);

            if (!_isInitialized) {
                _initialPosition = transform.position;
                _isInitialized = true;
            }

            transform.position = position + _initialPosition;

            Ports[3].Call();
        }
    }

}
