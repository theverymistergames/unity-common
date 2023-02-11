using System;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "EditorInputKey", Category = "Debug", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeEditorInputKey : BlueprintNode {
        
        [SerializeReference] [SubclassSelector] private IKeyBinding _binding;

        private InputActionKey _inputAction;

        public override Port[] CreatePorts() => new[] {
            Port.Exit("On Press"),
            Port.Exit("On Release"),
        };

        public override void OnInitialize(IBlueprintHost host) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                if (InputUpdater.CheckEditorInputUpdaterIsStarted(this, out var inputChannel)) {
                    _inputAction = ScriptableObject.CreateInstance<InputActionKey>();
                    _inputAction.Bindings = new[] {_binding};

                    inputChannel.AddInputAction(_inputAction);

                    _inputAction.OnPress += OnPress;
                    _inputAction.OnRelease += OnRelease;
                }
            }
            else {
                Debug.LogWarning($"Using {nameof(BlueprintNodeEditorInputKey)} " +
                                 $"in blueprint `{((BlueprintRunner) host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                                 $"in blueprint runner `{host.Runner.name}`: " +
                                 $"this node is for debug purposes only, " +
                                 $"it must be removed in the release build.");
            }
#else
            Debug.LogWarning($"Using {nameof(BlueprintNodeEditorInputKey)} " +
                                 $"in blueprint `{((BlueprintRunner) host.Runner).BlueprintAsset.name}` or in its subgraphs " +
                                 $"in blueprint runner `{host.Runner.name}`: " +
                                 $"this node is for debug purposes only, " +
                                 $"it must be removed in the release build.");
#endif
        }

        public override void OnDeInitialize() {
#if UNITY_EDITOR
            if (!Application.isPlaying && InputUpdater.CheckEditorInputUpdaterIsStopped(this, out var inputChannel)) {
                inputChannel.RemoveInputAction(_inputAction);

                _inputAction.OnPress -= OnPress;
                _inputAction.OnRelease -= OnRelease;
            }
#endif
        }

        private void OnPress() {
            CallExitPort(0);
        }

        private void OnRelease() {
            CallExitPort(1);
        }
    }

}
