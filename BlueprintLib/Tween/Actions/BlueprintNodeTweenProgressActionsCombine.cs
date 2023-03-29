using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Combine Tween Progress Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionsCombine : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        private readonly TweenProgressActions _actions = new TweenProgressActions();
        
        public override Port[] CreatePorts() => new[] {
            Port.Input<ITweenProgressAction>("Actions").Capacity(PortCapacity.Multiple),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 1) return null;

            var links = Ports[0].links;
            _actions.actions = new List<ITweenProgressAction>(links.Count);

            for (int l = 0; l < links.Count; l++) {
                var link = links[l];

                if (link.Get<ITweenProgressAction>() is { } action) {
                    _actions.actions.Add(action);
                    continue;
                }

                if (link.Get<ITweenProgressAction[]>() is { } actions) {
                    for (int i = 0; i < actions.Length; i++) {
                        if (actions[i] is { } a) _actions.actions.Add(a);
                    }
                }
            }

            return _actions;
        }
    }

}
