using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints;
using UnityEngine;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
#endif

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Blackboard Property (left)", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetBlackboardPropertyLeft : BlueprintNode, IBlueprintOutput

#if UNITY_EDITOR
        , IBlueprintAssetValidator
        , IBlueprintPortDecorator
#endif

    {
        [SerializeField] [BlackboardProperty("_blackboard")] private int _property;
        private Blackboard _blackboard;

        public override Port[] CreatePorts() => new[] {
            Port.DynamicOutput().Hidden(true).Layout(PortLayout.Left),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
        }

        public R GetOutputPortValue<R>(int port) => port switch {
            0 => _blackboard.Get<R>(_property),
            _ => default,
        };

#if UNITY_EDITOR
        private Type _dataType;

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            _dataType = blueprint.Blackboard.TryGetProperty(_property, out var property) ? property.type : null;
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            ports[0] = Port.DynamicOutput(type: _dataType).Hidden(_dataType == null).Layout(PortLayout.Left);
        }
#endif
    }

}
