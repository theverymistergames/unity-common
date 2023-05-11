using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Character Force Zone", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterForceZone :
        BlueprintNode,
        IBlueprintEnter,
        IBlueprintStart,
        IBlueprintOutput<CharacterAccess>,
        IBlueprintOutput<Vector3>
    {
        [SerializeField] private bool _autoSetZoneAtStart;

        private CharacterAccess _characterAccess;
        private CharacterForceZone _forceZone;
        private Vector3 _force;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Set Zone"),
            Port.Input<CharacterForceZone>("Zone"),
            Port.Output<CharacterAccess>(),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
            Port.Exit("On Force Update"),
            Port.Output<Vector3>("Force Vector"),
        };

        public void OnStart() {
            if (!_autoSetZoneAtStart) return;

            _forceZone = Ports[1].Get<CharacterForceZone>();

            _forceZone.OnEnteredZone -= OnEnteredZone;
            _forceZone.OnEnteredZone += OnEnteredZone;

            _forceZone.OnExitedZone -= OnExitedZone;
            _forceZone.OnExitedZone += OnExitedZone;
        }

        public override void OnDeInitialize() {
            if (_forceZone == null) return;

            _forceZone.OnEnteredZone -= OnEnteredZone;
            _forceZone.OnExitedZone -= OnExitedZone;
            _forceZone.OnForceUpdate -= OnForceUpdate;
        }

        public void OnEnterPort(int port) {
            if (port != 0) return;

            _forceZone ??= Ports[1].Get<CharacterForceZone>();

            _forceZone.OnEnteredZone -= OnEnteredZone;
            _forceZone.OnEnteredZone += OnEnteredZone;

            _forceZone.OnExitedZone -= OnExitedZone;
            _forceZone.OnExitedZone += OnExitedZone;
        }

        CharacterAccess IBlueprintOutput<CharacterAccess>.GetOutputPortValue(int port) {
            return port == 2 ? _characterAccess : default;
        }

        Vector3 IBlueprintOutput<Vector3>.GetOutputPortValue(int port) {
            return port == 6 ? _force : default;
        }

        private void OnEnteredZone(CharacterAccess characterAccess) {
            _characterAccess = characterAccess;

            _forceZone.OnForceUpdate -= OnForceUpdate;
            _forceZone.OnForceUpdate += OnForceUpdate;

            Ports[3].Call();
        }

        private void OnExitedZone(CharacterAccess characterAccess) {
            _forceZone.OnForceUpdate -= OnForceUpdate;

            Ports[4].Call();
            _characterAccess = null;
        }

        private void OnForceUpdate(Vector3 force) {
            _force = force;
            Ports[5].Call();
        }
    }

}
