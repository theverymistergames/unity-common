using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Interactive Events", Category = "Interactive", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeInteractiveEvents :
        BlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<bool>,
        IBlueprintOutput<IInteractiveUser>,
        IBlueprintStart
    {
        [SerializeField] private bool _autoSetInteractiveOnStart = true;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Set Interactive"),
            Port.Input<Interactive>("Interactive"),
            Port.Exit("On Start Interact"),
            Port.Exit("On Stop Interact"),
            Port.Output<bool>("Is Interacting"),
            Port.Output<IInteractiveUser>(),
        };

        private Interactive _interactive;
        private IInteractiveUser _currentUser;

        public void OnStart() {
            if (!_autoSetInteractiveOnStart) return;

            _interactive = Ports[1].Get<Interactive>();

            if (_interactive != null) {
                _interactive.OnStartInteract += OnStartInteract;
                _interactive.OnStopInteract += OnStopInteract;
            }
        }

        public override void OnDeInitialize() {
            if (_interactive != null) {
                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
            }
        }

        public void OnEnterPort(int port) {
            if (port != 0) return;

            if (_interactive != null) {
                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
            }

            _interactive = Ports[1].Get<Interactive>();

            if (_interactive != null) {
                _interactive.OnStartInteract += OnStartInteract;
                _interactive.OnStopInteract += OnStopInteract;
            }
        }

        private void OnStartInteract(IInteractiveUser user, Vector3 hitPoint) {
            _currentUser = user;
            Ports[2].Call();
        }

        private void OnStopInteract(IInteractiveUser user) {
            _currentUser = null;
            Ports[3].Call();
        }

        bool IBlueprintOutput<bool>.GetOutputPortValue(int port) => port switch {
            4 => _interactive != null && _interactive.IsInteracting,
            _ => false,
        };

        IInteractiveUser IBlueprintOutput<IInteractiveUser>.GetOutputPortValue(int port) => port switch {
            5 => _currentUser,
            _ => null,
        };
    }

}
