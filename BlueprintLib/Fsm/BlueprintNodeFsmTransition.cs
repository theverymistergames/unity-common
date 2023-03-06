using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition : BlueprintNode, IBlueprintOutput<IFsmTransition>, IBlueprintAssetValidator {

        [SerializeReference] [SubclassSelector] private IFsmTransition _transition;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Exit("On Transit"),
                Port.Output<IFsmTransition>("Transition"),
            };

            if (_transition == null) return ports.ToArray();

            var transitionType = _transition.GetType();
            var iFsmTransitionTType = transitionType
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IFsmTransition<>));

            if (iFsmTransitionTType == null) return ports.ToArray();

            var genericArgumentType = iFsmTransitionTType.GenericTypeArguments[0];
            ports.Add(Port.Input("Data", genericArgumentType));

            return ports.ToArray();
        }

        public IFsmTransition GetOutputPortValue(int port) {
            if (port != 0) return default;

            return _transition;
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }
    }

}
