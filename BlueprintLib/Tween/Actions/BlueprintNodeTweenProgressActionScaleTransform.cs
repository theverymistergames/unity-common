using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Action Scale Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionScaleTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private readonly TweenProgressActionScaleTransform  _action = new TweenProgressActionScaleTransform ();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>(),
            Port.Input<Vector3>("Start Scale"),
            Port.Input<Vector3>("End Scale"),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = Ports[0].Get(_transform);
            _action.startLocalScale = Ports[1].Get(_startLocalScale);
            _action.endLocalScale = Ports[2].Get(_endLocalScale);

            return _action;
        }
    }

}
