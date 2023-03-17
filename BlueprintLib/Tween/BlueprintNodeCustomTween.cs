using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Custom Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCustomTween : BlueprintNode, IBlueprintNodeTween, IBlueprintOutput<IBlueprintNodeTween> {

        public ITween Tween => Ports[2].Get<ITween>();
        public List<RuntimeLink> NextLinks => Ports[1].links;

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<ITween>(),
        };

        public void SetupTween() {
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
