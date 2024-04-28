using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetBlackboardProperty :
        BlueprintSource<BlueprintNodeGetBlackboardProperty>, BlueprintSources.IOutput<BlueprintNodeGetBlackboardProperty>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Blackboard Property", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public struct BlueprintNodeGetBlackboardProperty : IBlueprintNode, IBlueprintOutput {

        [SerializeField] [BlackboardProperty("_blackboard")] private int _property;

        private Blackboard _blackboard;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;

            if (meta.GetBlackboard() is {} b && b.TryGetProperty(_property, out var property)) {
                dataType = property.type.ToType();
            }

            meta.AddPort(id, Port.DynamicOutput(type: dataType).Hide(dataType == null));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blackboard = blueprint.GetBlackboard(root);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blackboard = null;
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? _blackboard.Get<T>(_property) : default;
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
