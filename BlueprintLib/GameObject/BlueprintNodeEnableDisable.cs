using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Enable Disable", Category = "GameObject", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeEnableDisable : BlueprintNode, IBlueprintEnter

#if UNITY_EDITOR
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private InputType _input;
        [SerializeField] private bool _isEnabled = true;

        private enum InputType {
            GameObject,
            Behaviour,
        }

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Apply"),
            _input == InputType.GameObject ? Port.Input<GameObject>() : Port.Input<Behaviour>(),
            Port.Input<bool>("Is Enabled"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            bool isEnabled = Ports[2].Get(_isEnabled);

            switch (_input) {
                case InputType.GameObject:
                    var gameObject = Ports[1].Get<GameObject>();
                    gameObject.SetActive(isEnabled);
                    break;

                case InputType.Behaviour:
                    var behaviour = Ports[1].Get<Behaviour>();
                    behaviour.enabled = isEnabled;
                    break;
            }
        }

#if UNITY_EDITOR
        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
#endif
    }

}
