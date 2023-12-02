using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints;
using UnityEngine;
using MisterGames.Blueprints.Meta;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGetBlackboardPropertyLeft :
        BlueprintSource<BlueprintNodeGetBlackboardPropertyLeft>,
        BlueprintSources.IOutput<BlueprintNodeGetBlackboardPropertyLeft>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Get Blackboard Property (left)", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public struct BlueprintNodeGetBlackboardPropertyLeft : IBlueprintNode, IBlueprintOutput {

        [SerializeField] [BlackboardProperty("_blackboard")] private int _property;

        private Blackboard _blackboard;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;

            if (meta.GetBlackboard() is {} b && b.TryGetProperty(_property, out var property)) {
                dataType = property.type.ToType();
            }

            meta.AddPort(id, Port.DynamicOutput(type: dataType).Hide(dataType == null).Layout(PortLayout.Left));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blackboard = blueprint.GetBlackboard(root);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => _blackboard.Get<T>(_property),
            _ => default,
        };

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
