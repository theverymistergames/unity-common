using System;
using MisterGames.Blueprints;
using MisterGames.Tweens.Actions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenInstantAction Log", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenInstantActionLog : BlueprintNode, IBlueprintEnter {

        [SerializeField] private TweenInstantActionLog _action;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<GameObject>("GameObject"),
            Port.Input<Vector3>("Position"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {

        }
    }

}
