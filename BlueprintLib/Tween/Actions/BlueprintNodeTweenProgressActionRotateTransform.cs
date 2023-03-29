using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Action Rotate Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionRotateTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startEulerAngles;
        [SerializeField] private Vector3 _endEulerAngles;
        [SerializeField] private bool _useLocal = true;

        private readonly TweenProgressActionRotateTransform _action = new TweenProgressActionRotateTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>(),
            Port.Input<Vector3>("Start Euler Angles"),
            Port.Input<Vector3>("End Euler Angles"),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = Ports[0].Get(_transform);
            _action.startEulerAngles = Ports[1].Get(_startEulerAngles);
            _action.endEulerAngles = Ports[2].Get(_endEulerAngles);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

}
