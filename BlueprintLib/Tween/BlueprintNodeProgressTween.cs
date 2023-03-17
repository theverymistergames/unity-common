using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Progress Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : BlueprintNode, IBlueprintNodeTween, IBlueprintOutput<IBlueprintNodeTween> {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private AnimationCurve _curve;

        public ITween Tween => _tween;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        private readonly ProgressTween _tween = new ProgressTween();

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<float>("Duration"),
            Port.Input<AnimationCurve>("Curve"),
            Port.Input<ITweenProgressAction>(),
        };

        public void SetupTween() {
            _tween.duration = Mathf.Max(0f, Ports[0].Get(_duration));
            _tween.curve = Ports[1].Get(_curve);
            _tween.action = Ports[2].Get<ITweenProgressAction>();

            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IBlueprintNodeTween>()?.SetupTween();
            }
        }

        public IBlueprintNodeTween GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }
    }

}
