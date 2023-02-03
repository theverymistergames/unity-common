using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "InstantTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeInstantTween : BlueprintNode, IBlueprintEnter {

        [SerializeField] private InstantTween _tween;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            //Port.InputArray<ITweenInstantAction>("Actions"),
            Port.Exit("On Finish"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var gameObject = ReadInputPort<GameObject>(1);
            //var position = ReadInputPort(2, _defaultPosition);
            //gameObject.transform.position = position;

            CallExitPort(3);
        }
    }

}
