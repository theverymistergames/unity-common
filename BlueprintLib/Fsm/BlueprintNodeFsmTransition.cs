using System;
using System.Collections.Generic;
using MisterGames.BlueprintLib.Fsm;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        BlueprintNode,
        IBlueprintOutput<IBlueprintFsmTransition>,
        IBlueprintFsmTransition,
        IBlueprintFsmTransitionCallback,
        IBlueprintAssetValidator
    {
        [SerializeReference] [SubclassSelector] private IBlueprintFsmTransition _transition;

        private IBlueprintFsmTransitionCallback _stateCallback;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Output<IBlueprintFsmTransition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
                Port.Exit("On Transit"),
            };

            if (_transition is IBlueprintFsmTransitionDynamicData t) ports.Add(Port.DynamicInput(type: t.DataType));

            return ports.ToArray();
        }

        public IBlueprintFsmTransition GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        public void Arm(IBlueprintFsmTransitionCallback callback) {
            if (_transition is IBlueprintFsmTransitionDynamicData t) t.Data = Ports[2].Get<object>();

            _stateCallback = callback;
            _transition?.Arm(this);
        }

        public void Disarm() {
            _transition?.Disarm();

            _transition = null;
            _stateCallback = null;
        }

        public void OnTransitionRequested() {
            _stateCallback?.OnTransitionRequested();
            Ports[1].Call();
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            Debug.Log($"BlueprintNodeFsmTransition.ValidateBlueprint: ");
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
    }

}
