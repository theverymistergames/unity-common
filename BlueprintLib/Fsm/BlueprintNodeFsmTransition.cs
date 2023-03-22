using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        BlueprintNode,
        IBlueprintOutput<IFsmTransitionBase>,
        IFsmTransitionBase,
        IFsmTransitionCallback,
        IBlueprintAssetValidator
    {
        [SerializeReference] [SubclassSelector] private IFsmTransitionBase _transition;

        private IFsmTransitionCallback _stateCallback;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Output<IFsmTransitionBase>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
                Port.Exit("On Transit"),
            };

            if (_transition is IFsmTransition t) ports.Add(Port.DynamicInput(type: t.DataType));

            return ports.ToArray();
        }

        public IFsmTransitionBase GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        public void Arm(IFsmTransitionCallback callback) {
            if (_transition is IFsmTransition t) t.Data = Ports[2].Get<IFsmTransitionData>();

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
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
    }

}
