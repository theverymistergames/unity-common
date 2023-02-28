using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Progress Action Scale Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionScaleTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction> {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private readonly TweenProgressActionScaleTransform _action = new TweenProgressActionScaleTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>("Transform"),
            Port.Input<Vector3>("Start Position"),
            Port.Input<Vector3>("End Position"),
            Port.Output<ITweenProgressAction>("Action"),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = ReadInputPort(0, _transform);
            _action.startLocalScale = ReadInputPort(1, _startLocalScale);
            _action.endLocalScale = ReadInputPort(2, _endLocalScale);

            return _action;
        }
    }

}
