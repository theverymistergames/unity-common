using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Input Action Key", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionKey : IBlueprintNode, IBlueprintStartCallback {

        [SerializeField] private InputActionRef _inputActionKey;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<InputActionRef>());
            meta.AddPort(id, Port.Exit("On Use"));
            meta.AddPort(id, Port.Exit("On Press"));
            meta.AddPort(id, Port.Exit("On Release"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            _token = token;
            _blueprint = blueprint;
            _inputActionKey = blueprint.Read(token, 0, _inputActionKey);

#if UNITY_EDITOR
            if (!Application.isPlaying) InputServices.EnableInputInEditModeForSource(this, enable: true);
#endif

            _inputActionKey.Get().performed += OnUse;
            _inputActionKey.Get().performed += OnPress;
            _inputActionKey.Get().canceled += OnRelease;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
#if UNITY_EDITOR
            if (!Application.isPlaying) InputServices.EnableInputInEditModeForSource(this, enable: false);
#endif

            _inputActionKey.Get().performed -= OnUse;
            _inputActionKey.Get().performed -= OnPress;
            _inputActionKey.Get().canceled -= OnRelease;

            _blueprint = null;
        }

        private void OnUse(InputAction.CallbackContext callbackContext) {
            _blueprint.Call(_token, 1);
        }

        private void OnPress(InputAction.CallbackContext callbackContext) {
            _blueprint.Call(_token, 2);
        }

        private void OnRelease(InputAction.CallbackContext callbackContext) {
            _blueprint.Call(_token, 3);
        }
    }

}
