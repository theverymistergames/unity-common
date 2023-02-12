using System;
using MisterGames.Blueprints;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "InputActionKey", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionKey : BlueprintNode {
        
        [SerializeField] private InputActionKey _inputActionKey;

        public override Port[] CreatePorts() => new[] {
            Port.Exit("On Use"),
            Port.Exit("On Press"),
            Port.Exit("On Release"),
        };

        public override void OnInitialize(IBlueprintHost host) {
#if UNITY_EDITOR
            if (!Application.isPlaying && InputUpdater.TryStartEditorInputUpdater(this, out var inputChannel)) {
                inputChannel.AddInputAction(_inputActionKey);
            }
#endif

            _inputActionKey.OnUse += OnUse;
            _inputActionKey.OnPress += OnPress;
            _inputActionKey.OnRelease += OnRelease;
        }

        public override void OnDeInitialize() {
#if UNITY_EDITOR
            if (!Application.isPlaying && InputUpdater.TryStopInputUpdater(this, out var inputChannel)) {
                inputChannel.RemoveInputAction(_inputActionKey);
            }
#endif

            _inputActionKey.OnUse -= OnUse;
            _inputActionKey.OnPress -= OnPress;
            _inputActionKey.OnRelease -= OnRelease;
        }

        private void OnUse() {
            CallExitPort(0);
        }

        private void OnPress() {
            CallExitPort(1);
        }

        private void OnRelease() {
            CallExitPort(2);
        }
    }

}
