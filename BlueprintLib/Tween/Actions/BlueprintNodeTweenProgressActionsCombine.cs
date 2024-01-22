using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionsCombine :
        BlueprintSource<BlueprintNodeTweenProgressActionsCombine>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionsCombine, ITweenProgressAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Combine Tween Progress Actions", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionsCombine : IBlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

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

}
