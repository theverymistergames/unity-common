using System;
using MisterGames.Blueprints;
using MisterGames.Tweens.Actions;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenInstantActions Combine", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionsCombine : BlueprintNode, IBlueprintOutput<ITweenProgressAction> {

        private readonly TweenProgressActions _actions = new TweenProgressActions();

        public override Port[] CreatePorts() => new[] {
            Port.InputArray<ITweenProgressAction>("Actions"),
            Port.Output<ITweenProgressAction>("Action"),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 1) return null;

            _actions.actions = ReadInputArrayPort(0, Array.Empty<ITweenProgressAction>());
            
            return _actions;
        }
    }

}
