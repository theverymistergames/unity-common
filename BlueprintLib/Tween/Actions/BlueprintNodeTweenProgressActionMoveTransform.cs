﻿using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenProgressAction Move Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionMoveTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _endPosition;
        [SerializeField] private bool _useLocal = true;

        private readonly TweenProgressActionMoveTransform _action = new TweenProgressActionMoveTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>("Transform"),
            Port.Input<Vector3>("Start Position"),
            Port.Input<Vector3>("End Position"),
            Port.Output<ITweenProgressAction>("Action"),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = ReadInputPort(0, _transform);
            _action.startPosition = ReadInputPort(1, _startPosition);
            _action.endPosition = ReadInputPort(2, _endPosition);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

}