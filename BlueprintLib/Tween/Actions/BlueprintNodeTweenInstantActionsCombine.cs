using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenInstantActionsCombine :
        BlueprintSource<BlueprintNodeTweenInstantActionsCombine2>,
        BlueprintSources.IOutput<BlueprintNodeTweenInstantActionsCombine2, ITweenInstantAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Combine Tween Instant Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenInstantActionsCombine2 : IBlueprintNode, IBlueprintOutput2<ITweenInstantAction>  {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<ITweenInstantAction>("Actions").Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Output<ITweenInstantAction>());
        }

        public ITweenInstantAction GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 1) return null;

            var links = blueprint.GetLinks(token, 0);
            if (!links.MoveNext()) return null;

            var action = links.Read<ITweenInstantAction>();
            if (!links.MoveNext()) return action;

            var result = new TweenInstantActions { actions = new List<ITweenInstantAction>() };
            if (action != null) result.actions.Add(action);

            action = links.Read<ITweenInstantAction>();
            if (action != null) result.actions.Add(action);

            while (links.MoveNext()) {
                if (links.Read<ITweenInstantAction>() is {} v) {
                    result.actions.Add(v);
                    continue;
                }

                if (links.Read<ITweenInstantAction[]>() is {} actions) {
                    for (int i = 0; i < actions.Length; i++) {
                        if (actions[i] is {} a) result.actions.Add(a);
                    }
                }
            }

            return result;
        }
    }

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
