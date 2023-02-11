using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenSequence", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenSequence : BlueprintNode, IBlueprintOutput<ITween> {

        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;
        
        private readonly TweenSequence _tweenSequence = new TweenSequence();

        public override Port[] CreatePorts() => new[] {
            Port.InputArray<ITween>("Tweens"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _tweenSequence.loop = _loop;
            _tweenSequence.yoyo = _yoyo;
        }

        public ITween GetOutputPortValue(int port) {
            if (port != 1) return null;

            _tweenSequence.tweens = ReadInputArrayPort<ITween>(0);

            return _tweenSequence;
        }
    }

}
