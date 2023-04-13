using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        BlueprintNode,
        IBlueprintOutput<ICondition>,
        ICondition,
        IConditionCallback,
        IBlueprintAssetValidator,
        IDynamicDataProvider
    {
        [SerializeReference] [SubclassSelector] private ICondition _condition;

        public bool IsMatched => _condition.IsMatched;

        private IConditionCallback _stateCallback;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Output<ICondition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
                Port.Exit("On Transit"),
            };

            if (_condition is IDynamicDataHost host) {
                var types = new HashSet<Type>();
                host.OnSetDataTypes(types);

                var typesArray = new Type[types.Count];
                types.CopyTo(typesArray);

                if (typesArray.Length == 0) return ports.ToArray();

                if (typesArray.Length > 1) {
                    Debug.LogWarning($"{nameof(BlueprintNodeFsmTransition)}: " +
                                     $"more than 1 {nameof(IDynamicDataHost)} data type is not supported");

                    return ports.ToArray();
                }

                ports.Add(Port.DynamicInput(type: typesArray[0]));
            }

            return ports.ToArray();
        }

        public ICondition GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        public T GetData<T>() {
            return Ports[2].Get<T>();
        }

        public void Arm(IConditionCallback callback) {
            if (_condition is IDynamicDataHost host) host.OnSetData(this);

            _stateCallback = callback;
            _condition?.Arm(this);
        }

        public void Disarm() {
            _condition?.Disarm();
            _stateCallback = null;
        }

        public void OnConditionMatch() {
            if (_stateCallback == null) return;

            _stateCallback.OnConditionMatch();
            Ports[1].Call();
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
    }

}
