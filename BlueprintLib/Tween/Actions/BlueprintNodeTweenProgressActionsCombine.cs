using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionsCombine :
        BlueprintSource<BlueprintNodeTweenProgressActionsCombine2>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionsCombine2, ITweenProgressAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Combine Tween Progress Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionsCombine2 : IBlueprintNode, IBlueprintOutput2<ITweenProgressAction>  {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<ITweenProgressAction>("Actions").Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Output<ITweenProgressAction>());
        }

        public ITweenProgressAction GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 1) return null;

            var links = blueprint.GetLinks(token, 0);
            if (!links.MoveNext()) return null;

            var action = links.Read<ITweenProgressAction>();
            if (!links.MoveNext()) return action;

            var result = new TweenProgressActions { actions = new List<ITweenProgressAction>() };
            if (action != null) result.actions.Add(action);

            action = links.Read<ITweenProgressAction>();
            if (action != null) result.actions.Add(action);

            while (links.MoveNext()) {
                if (links.Read<ITweenProgressAction>() is {} v) {
                    result.actions.Add(v);
                    continue;
                }

                if (links.Read<ITweenProgressAction[]>() is {} actions) {
                    for (int i = 0; i < actions.Length; i++) {
                        if (actions[i] is {} a) result.actions.Add(a);
                    }
                }
            }

            return result;
        }
    }

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
