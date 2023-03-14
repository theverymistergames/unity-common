using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Sequence", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenSequence : BlueprintNode, IBlueprintNodeTween {

        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;

        public ITween Tween => _tweenSequence;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        private TweenSequence _tweenSequence;

        public override Port[] CreatePorts() => new[] {
            Port.Create(PortDirection.Output, "Self", typeof(IBlueprintNodeTween)).Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Create(PortDirection.Input, "Next Tweens", typeof(IBlueprintNodeTween)).Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Create(PortDirection.Input, "Sequence Tweens", typeof(IBlueprintNodeTween)).Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
        };

        public void SetupTween() {
            var t = BlueprintTweenConverter.AsTween(Ports[2].links);
            _tweenSequence = t == null ? null : t as TweenSequence ?? new TweenSequence { tweens = new List<ITween> { t } };

            if (_tweenSequence == null) return;

            _tweenSequence.loop = _loop;
            _tweenSequence.yoyo = _yoyo;
        }
    }
}
