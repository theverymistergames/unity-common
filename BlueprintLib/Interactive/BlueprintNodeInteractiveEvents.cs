using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Nodes;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Interactive Events", Category = "Interactive", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeInteractiveEvents2 :
        IBlueprintNode,
        IBlueprintEnter2,
        IBlueprintOutput2<bool>,
        IBlueprintOutput2<IInteractiveUser>,
        IBlueprintStartCallback
    {
        [SerializeField] private bool _autoSetInteractiveOnStart = true;

        private Interactive _interactive;
        private IInteractiveUser _lastUser;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Interactive"));
            meta.AddPort(id, Port.Input<Interactive>("Interactive"));
            meta.AddPort(id, Port.Exit("On Start Interact"));
            meta.AddPort(id, Port.Exit("On Stop Interact"));
            meta.AddPort(id, Port.Output<bool>("Is Interacting"));
            meta.AddPort(id, Port.Output<IInteractiveUser>("Last User"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetInteractiveOnStart) return;

            _token = token;
            PrepareInteractive();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            if (_interactive != null) {
                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
            }

            _interactive = null;
            _lastUser = null;
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _token = token;
            PrepareInteractive();
        }

        private void PrepareInteractive() {
            if (_interactive != null) {
                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
            }

            _interactive = _blueprint.Read<Interactive>(_token, 1);

            if (_interactive != null) {
                _interactive.OnStartInteract += OnStartInteract;
                _interactive.OnStopInteract += OnStopInteract;
            }
        }

        private void OnStartInteract(IInteractiveUser user) {
            _lastUser = user;
            _blueprint.Call(_token, 2);
        }

        private void OnStopInteract(IInteractiveUser user) {
            _blueprint.Call(_token, 3);
        }

        bool IBlueprintOutput2<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            4 => _interactive != null && _interactive.IsInteractingWith(_lastUser),
            _ => false,
        };

        IInteractiveUser IBlueprintOutput2<IInteractiveUser>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastUser,
            _ => null,
        };
    }

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
            Port.Output<IInteractiveUser>("Last User"),
        };

        private Interactive _interactive;
        private IInteractiveUser _lastUser;

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

        private void OnStartInteract(IInteractiveUser user) {
            _lastUser = user;
            Ports[2].Call();
        }

        private void OnStopInteract(IInteractiveUser user) {
            Ports[3].Call();
        }

        bool IBlueprintOutput<bool>.GetOutputPortValue(int port) => port switch {
            4 => _interactive != null && _interactive.IsInteractingWith(_lastUser),
            _ => false,
        };

        IInteractiveUser IBlueprintOutput<IInteractiveUser>.GetOutputPortValue(int port) => port switch {
            5 => _lastUser,
            _ => null,
        };
    }

}
