using System;
using MisterGames.Blueprints;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Interactive Events", Category = "Interactive", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeInteractiveEvents : BlueprintNode, IBlueprintOutput<bool>, IBlueprintOutput<InteractiveUser> {

        public override Port[] CreatePorts() => new[] {
            Port.Input<Interactive>("Interactive"),
            Port.Exit("On Start Interact"),
            Port.Exit("On Stop Interact"),
            Port.Output<bool>("Is Interacting"),
            Port.Output<InteractiveUser>("User"),
        };

        private Interactive _interactive;
        private InteractiveUser _currentUser;

        public override void OnInitialize(IBlueprintHost host) {
            _interactive = ReadInputPort<GameObject>(0).GetComponent<Interactive>();

            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        public override void OnDeInitialize() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
        }

        private void OnStartInteract(InteractiveUser obj) {
            _currentUser = obj;
        }

        private void OnStopInteract() {
            _currentUser = null;
        }

        bool IBlueprintOutput<bool>.GetOutputPortValue(int port) => port switch {
            3 => _interactive.IsInteracting,
            _ => false,
        };

        InteractiveUser IBlueprintOutput<InteractiveUser>.GetOutputPortValue(int port) => port switch {
            4 => _currentUser,
            _ => null,
        };
    }

}
