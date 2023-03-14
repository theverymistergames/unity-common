using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeNodeFsmTransition :
        BlueprintNode,
        IBlueprintNodeFsmTransition,
        IFsmTransitionCallback,
        IBlueprintAssetValidator
    {
        [SerializeReference] [SubclassSelector] private IFsmTransitionBase _transition;

        private IFsmTransitionCallback _stateCallback;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Create(PortDirection.Output, "Self", typeof(IBlueprintNodeFsmTransition))
                    .Layout(PortLayout.Left)
                    .Capacity(PortCapacity.Single),
                Port.Action(PortDirection.Output, "On Transit"),
            };

            if (_transition is not IFsmTransition t) return ports.ToArray();

            ports.Add(Port.DynamicFunc(PortDirection.Input, returnType: t.DataType));

            return ports.ToArray();
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
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
    }

}
