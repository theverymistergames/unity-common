using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "DelayTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeDelayTween : BlueprintNode, IBlueprintOutput<ITween> {

        [SerializeField] [Min(0f)] private float _duration;

        private readonly DelayTween _tween = new DelayTween();

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("Duration"),
            Port.Output<ITween>("Tween"),
        };

        public ITween GetOutputPortValue(int port) {
            if (port != 1) return null;

            _tween.duration = Mathf.Max(0, ReadInputPort(0, _duration));

            return _tween;
        }
    }

}
