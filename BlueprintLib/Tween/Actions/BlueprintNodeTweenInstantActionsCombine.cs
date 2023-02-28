using System;
using MisterGames.Blueprints;
using MisterGames.Tweens.Actions;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Combine Tween Instant Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenInstantActionsCombine : BlueprintNode, IBlueprintOutput<ITweenInstantAction> {

        private readonly TweenInstantActions _actions = new TweenInstantActions();

        public override Port[] CreatePorts() => new[] {
            Port.InputArray<ITweenInstantAction>("Actions"),
            Port.Output<ITweenInstantAction>("Action"),
        };

        public ITweenInstantAction GetOutputPortValue(int port) {
            if (port != 1) return null;

            _actions.actions = ReadInputArrayPort(0, Array.Empty<ITweenInstantAction>());

            return _actions;
        }
    }

}
