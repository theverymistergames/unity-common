using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Force Zone", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterForceZone :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintStartCallback,
        IBlueprintOutput<CharacterAccess>,
        IBlueprintOutput<Vector3>
    {
        [SerializeField] private bool _autoSetZoneAtStart;

        private CharacterAccess _characterAccess;
        private CharacterForceZone _forceZone;
        private IBlueprint _blueprint;

        private NodeToken _token;
        private Vector3 _force;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Zone"));
            meta.AddPort(id, Port.Input<CharacterForceZone>("Zone"));
            meta.AddPort(id, Port.Output<CharacterAccess>());
            meta.AddPort(id, Port.Exit("On Enter"));
            meta.AddPort(id, Port.Exit("On Exit"));
            meta.AddPort(id, Port.Exit("On Force Update"));
            meta.AddPort(id, Port.Output<Vector3>("Force Vector"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;

            if (!_autoSetZoneAtStart) return;

            _token = token;
            PrepareZone();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _characterAccess = null;

            if (_forceZone == null) return;

            _forceZone.OnEnteredZone -= OnEnteredZone;
            _forceZone.OnExitedZone -= OnExitedZone;
            _forceZone.OnForceUpdate -= OnForceUpdate;

            _forceZone = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _token = token;
            PrepareZone();
        }

        CharacterAccess IBlueprintOutput<CharacterAccess>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 2 ? _characterAccess : default;
        }

        Vector3 IBlueprintOutput<Vector3>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 6 ? _force : default;
        }

        private void PrepareZone() {
            if (_forceZone != null) {
                _forceZone.OnEnteredZone -= OnEnteredZone;
                _forceZone.OnExitedZone -= OnExitedZone;
            }

            _forceZone = _blueprint.Read<CharacterForceZone>(_token, 1);

            _forceZone.OnEnteredZone += OnEnteredZone;
            _forceZone.OnExitedZone += OnExitedZone;
        }

        private void OnEnteredZone(CharacterAccess characterAccess) {
            _characterAccess = characterAccess;

            _forceZone.OnForceUpdate -= OnForceUpdate;
            _forceZone.OnForceUpdate += OnForceUpdate;

            _blueprint.Call(_token, 3);
        }

        private void OnExitedZone(CharacterAccess characterAccess) {
            _forceZone.OnForceUpdate -= OnForceUpdate;

            _blueprint.Call(_token, 4);
            _characterAccess = null;
        }

        private void OnForceUpdate(Vector3 force) {
            _force = force;
            _blueprint.Call(_token, 5);
        }
    }

}
