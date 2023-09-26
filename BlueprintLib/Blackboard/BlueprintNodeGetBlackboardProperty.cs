using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints;
using UnityEngine;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
#endif

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Blackboard Property", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetBlackboardProperty : BlueprintNode, IBlueprintOutput

#if UNITY_EDITOR
        , IBlueprintAssetValidator
        , IBlueprintPortDecorator
#endif

    {
        [SerializeField] [BlackboardProperty("_blackboard")] private int _property;

        private Blackboard _blackboard;

        public override Port[] CreatePorts() => new[] {
            Port.DynamicOutput().Hidden(true),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
        }

        public R GetOutputPortValue<R>(int port) => port switch {
            0 => _blackboard.Get<R>(_property),
            _ => default,
        };

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var dataType = blueprint.Blackboard.TryGetProperty(_property, out var property) ? property.type.ToType() : null;
            ports[0] = Port.DynamicOutput(type: dataType).Hidden(dataType == null);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
#endif
    }

}
