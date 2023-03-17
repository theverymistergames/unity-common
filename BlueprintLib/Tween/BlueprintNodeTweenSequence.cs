using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Tween Sequence", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenSequence :
        BlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        ITween
    {
        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;

        public ITween Tween => this;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        private TweenSequence _tweenSequence;

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<IBlueprintNodeTween>("Sequence Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Exit("On Start"),
            Port.Exit("On Cancelled"),
            Port.Exit("On Finished"),
        };

        public IBlueprintNodeTween GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        public void Initialize(MonoBehaviour owner) {
            var t = BlueprintTweenConverter.AsTween(Ports[2].links);
            _tweenSequence = t == null ? null : t as TweenSequence ?? new TweenSequence { tweens = new List<ITween> { t } };

            if (_tweenSequence != null) {
                _tweenSequence.loop = _loop;
                _tweenSequence.yoyo = _yoyo;
            }

            _tweenSequence?.Initialize(owner);
        }

        public void DeInitialize() {
            _tweenSequence?.DeInitialize();
        }

        public async UniTask Play(CancellationToken token) {
            if (_tweenSequence == null) return;

            Ports[3].Call();
            await _tweenSequence.Play(token);
            Ports[token.IsCancellationRequested ? 4 : 5].Call();
        }

        public void Wind(bool reportProgress = true) {
            _tweenSequence?.Wind(reportProgress);
        }

        public void Rewind(bool reportProgress = true) {
            _tweenSequence?.Rewind(reportProgress);
        }

        public void Invert(bool isInverted) {
            _tweenSequence?.Invert(isInverted);
        }
    }
}
