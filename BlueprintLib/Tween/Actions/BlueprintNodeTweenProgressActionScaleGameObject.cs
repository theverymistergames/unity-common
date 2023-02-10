using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenProgressAction Scale GameObject", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionScaleGameObject : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private readonly TweenProgressActionScaleTransform _action = new TweenProgressActionScaleTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<GameObject>("GameObject"),
            Port.Input<Vector3>("Start Position"),
            Port.Input<Vector3>("End Position"),
            Port.Output<ITweenProgressAction>("Action"),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = ReadInputPort(0, _gameObject).transform;
            _action.startLocalScale = ReadInputPort(1, _startLocalScale);
            _action.endLocalScale = ReadInputPort(2, _endLocalScale);

            return _action;
        }
    }

}
