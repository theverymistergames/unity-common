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
            Port.DynamicFunc(PortDirection.Output).Enable(false).Layout(PortLayout.Left),
            Port.DynamicFunc(PortDirection.Output).Enable(false),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
        }

        public R GetOutputPortValue<R>(int port) => port switch {
            0 or 1 => _blackboard.Get<R>(_property),
            _ => default,
        };

#if UNITY_EDITOR
        private Type _dataType;

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            _dataType = blueprint.Blackboard.TryGetProperty(_property, out var property) ? property.type : null;
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            ports[0] = Port.DynamicFunc(PortDirection.Output, returnType: _dataType).Enable(_dataType != null).Layout(PortLayout.Left);
            ports[0] = Port.DynamicFunc(PortDirection.Output, returnType: _dataType).Enable(_dataType != null);
        }
#endif
    }

}
