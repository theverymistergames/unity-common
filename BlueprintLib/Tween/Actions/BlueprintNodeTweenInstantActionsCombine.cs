using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenInstantActionsCombine :
        BlueprintSource<BlueprintNodeTweenInstantActionsCombine>,
        BlueprintSources.IOutput<BlueprintNodeTweenInstantActionsCombine, ITweenInstantAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Combine Tween Instant Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenInstantActionsCombine : IBlueprintNode, IBlueprintOutput<ITweenInstantAction>  {

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

}
