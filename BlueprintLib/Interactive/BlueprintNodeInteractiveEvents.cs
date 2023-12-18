using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Interactive Events", Category = "Interactive", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeInteractiveEvents :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<bool>,
        IBlueprintOutput<IInteractiveUser>,
        IBlueprintOutput<Transform>,
        IBlueprintOutput<GameObject>,
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
            meta.AddPort(id, Port.Output<Transform>("Last User Transform"));
            meta.AddPort(id, Port.Output<GameObject>("Last User Root"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetInteractiveOnStart) return;

            _token = token;
            _blueprint = blueprint;

            PrepareInteractive();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
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

        bool IBlueprintOutput<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            4 => _interactive != null && _interactive.IsInteractingWith(_lastUser),
            _ => default,
        };

        IInteractiveUser IBlueprintOutput<IInteractiveUser>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastUser,
            _ => default,
        };

        Transform IBlueprintOutput<Transform>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            6 => _lastUser?.Transform,
            _ => default,
        };

        GameObject IBlueprintOutput<GameObject>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            7 => _lastUser?.Root,
            _ => default,
        };
    }

}
