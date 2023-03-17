using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Instant Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeInstantTween : BlueprintNode, IBlueprintNodeTween, IBlueprintOutput<IBlueprintNodeTween> {

        public ITween Tween => _tween;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        private readonly InstantTween _tween = new InstantTween();

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<ITweenInstantAction>(),
        };

        public void SetupTween() {
            _tween.action = Ports[2].Get<ITweenInstantAction>();

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
