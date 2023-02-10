using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "InputActionKey", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionKey : BlueprintNode, IBlueprintStart {
        
        [SerializeField] private InputActionKey _inputActionKey;

        public override Port[] CreatePorts() => new[] {
            Port.Input<InputActionKey>("InputActionKey"),
            Port.Exit("On Use"),
            Port.Exit("On Press"),
            Port.Exit("On Release"),
        };

        public override void OnDeInitialize() {
            _inputActionKey.OnUse -= OnUse;
            _inputActionKey.OnPress -= OnPress;
            _inputActionKey.OnRelease -= OnRelease;
        }

        public void OnStart() {
            _inputActionKey = ReadInputPort(0, _inputActionKey);

            _inputActionKey.OnUse += OnUse;
            _inputActionKey.OnPress += OnPress;
            _inputActionKey.OnRelease += OnRelease;
        }

        private void OnUse() {
            CallExitPort(1);
        }

        private void OnPress() {
            CallExitPort(2);
        }

        private void OnRelease() {
            CallExitPort(3);
        }
    }

}
