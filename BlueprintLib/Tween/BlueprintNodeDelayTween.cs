using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Delay Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeDelayTween : BlueprintNode, IBlueprintNodeTween, IBlueprintOutput<IBlueprintNodeTween> {

        [SerializeField] [Min(0f)] private float _duration;

        public ITween Tween => _tween;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        private readonly DelayTween _tween = new DelayTween();

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<float>("Duration"),
        };

        public void SetupTween() {
            _tween.duration = Mathf.Max(0f, Ports[2].Get(_duration));

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
