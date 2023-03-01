using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
#endif

namespace MisterGames.Blueprints.Nodes {

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
            Port.Output()
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
        }

        public T GetOutputPortValue<T>(int port) => port switch {
            0 => _blackboard.Get<T>(_property),
            _ => default,
        };

#if UNITY_EDITOR
        private SerializedType _dataType;

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            _dataType = blueprint.Blackboard.TryGetProperty(_property, out var property) ? property.type : null;
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }

        public void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports) {
            ports[0] = Port.Output(null, _dataType);
        }
#endif
    }

}
