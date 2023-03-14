using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Custom Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCustomTween : BlueprintNode, IBlueprintNodeTween {

        public ITween Tween => Ports[2].Get<ITween>();
        public List<RuntimeLink> NextLinks => Ports[1].links;

        public override Port[] CreatePorts() => new[] {
            Port.Create(PortDirection.Output, "Self", typeof(IBlueprintNodeTween)).Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Create(PortDirection.Input, "Next Tweens", typeof(IBlueprintNodeTween)).Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Func<ITweenInstantAction>(PortDirection.Input),
        };

        public void SetupTween() { }
    }

}
