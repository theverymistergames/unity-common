using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Input Action Key", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionKey : IBlueprintNode, IBlueprintStartCallback {

        [SerializeField] private InputActionKey _inputActionKey;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<InputActionKey>());
            meta.AddPort(id, Port.Exit("On Use"));
            meta.AddPort(id, Port.Exit("On Press"));
            meta.AddPort(id, Port.Exit("On Release"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            _token = token;
            _blueprint = blueprint;
            _inputActionKey = blueprint.Read(token, 0, _inputActionKey);

#if UNITY_EDITOR
            if (!Application.isPlaying && InputUpdater.TryStartEditorInputUpdater(this, out var inputChannel)) {
                inputChannel.AddInputAction(_inputActionKey);
            }
#endif

            _inputActionKey.OnUse += OnUse;
            _inputActionKey.OnPress += OnPress;
            _inputActionKey.OnRelease += OnRelease;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
#if UNITY_EDITOR
            if (!Application.isPlaying && InputUpdater.TryStopInputUpdater(this, out var inputChannel)) {
                inputChannel.RemoveInputAction(_inputActionKey);
            }
#endif

            _inputActionKey.OnUse -= OnUse;
            _inputActionKey.OnPress -= OnPress;
            _inputActionKey.OnRelease -= OnRelease;

            _blueprint = null;
        }

        private void OnUse() {
            _blueprint.Call(_token, 1);
        }

        private void OnPress() {
            _blueprint.Call(_token, 2);
        }

        private void OnRelease() {
            _blueprint.Call(_token, 3);
        }
    }

}
