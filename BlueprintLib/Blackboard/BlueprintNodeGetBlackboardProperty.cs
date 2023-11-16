using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints;
using UnityEngine;
using MisterGames.Blueprints.Meta;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetBlackboardProperty :
        BlueprintSource<BlueprintNodeGetBlackboardProperty2>,
        BlueprintSources.IOutput<BlueprintNodeGetBlackboardProperty2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Blackboard Property", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public struct BlueprintNodeGetBlackboardProperty2 : IBlueprintNode, IBlueprintOutput2 {

        [SerializeField] [BlackboardProperty("_blackboard")] private int _property;

        private Blackboard _blackboard;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            var asset = (meta as BlueprintMeta2)?.Owner as BlueprintAsset2;
            Type dataType = null;

            if (asset != null && asset.Blackboard.TryGetProperty(_property, out var property)) {
                dataType = property.type.ToType();
            }

            meta.AddPort(id, Port.DynamicOutput(type: dataType).Hide(dataType == null));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blackboard = blueprint.GetBlackboard(token.caller);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _blackboard = null;
        }

        public T GetPortValue<T>(NodeToken token, int port) => port switch {
            0 => _blackboard.Get<T>(_property),
            _ => default,
        };

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

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
            Port.DynamicOutput().Hide(true),
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
            ports[0] = Port.DynamicOutput(type: dataType).Hide(dataType == null);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
#endif
    }

}
