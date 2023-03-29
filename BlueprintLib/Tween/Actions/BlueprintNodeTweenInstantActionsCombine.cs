using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Combine Tween Instant Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenInstantActionsCombine : BlueprintNode, IBlueprintOutput<ITweenInstantAction>  {

        private readonly TweenInstantActions _actions = new TweenInstantActions();
        
        public override Port[] CreatePorts() => new[] {
            Port.Input<ITweenProgressAction>("Actions").Capacity(PortCapacity.Multiple),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenInstantAction GetOutputPortValue(int port) {
            if (port != 1) return null;

            var links = Ports[0].links;
            _actions.actions = new List<ITweenInstantAction>(links.Count);

            for (int l = 0; l < links.Count; l++) {
                var link = links[l];

                if (link.Get<ITweenInstantAction>() is { } action) {
                    _actions.actions.Add(action);
                    continue;
                }

                if (link.Get<ITweenInstantAction[]>() is { } actions) {
                    for (int i = 0; i < actions.Length; i++) {
                        if (actions[i] is { } a) _actions.actions.Add(a);
                    }
                }
            }

            return _actions;
        }
    }

}
