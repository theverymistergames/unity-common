using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenProgressAction Rotate Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionRotateTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction> {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startEulerAngles;
        [SerializeField] private Vector3 _endEulerAngles;
        [SerializeField] private bool _useLocal = true;

        private readonly TweenProgressActionRotateTransform _action = new TweenProgressActionRotateTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>("Transform"),
            Port.Input<Vector3>("Start Euler Angles"),
            Port.Input<Vector3>("End Euler Angles"),
            Port.Output<ITweenProgressAction>("Action"),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = ReadInputPort(0, _transform);
            _action.startEulerAngles = ReadInputPort(1, _startEulerAngles);
            _action.endEulerAngles = ReadInputPort(2, _endEulerAngles);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

}
