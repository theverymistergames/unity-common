using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input Action Key", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionKey : BlueprintNode, IBlueprintStart {
        
        [SerializeField] private InputActionKey _inputActionKey;

        public override Port[] CreatePorts() => new[] {
            Port.Input<InputActionKey>(),
            Port.Exit("On Use"),
            Port.Exit("On Press"),
            Port.Exit("On Release"),
        };

        public void OnStart() {
            _inputActionKey = Ports[0].Get(_inputActionKey);

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
            Ports[1].Call();
        }

        private void OnPress() {
            Ports[2].Call();
        }

        private void OnRelease() {
            Ports[3].Call();
        }
    }

}
