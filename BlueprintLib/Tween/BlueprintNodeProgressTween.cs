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
    [BlueprintNodeMeta(Name = "Progress Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween :
        BlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        IBlueprintOutput<float>,
        ITween
    {
        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private AnimationCurve _curve;

        public ITween Tween => this;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        public float Progress => _tween.Progress;

        private readonly ProgressTween _tween = new ProgressTween();

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<float>("Duration"),
            Port.Input<AnimationCurve>("Curve"),
            Port.Input<ITweenProgressAction>(),
            Port.Exit("On Start"),
            Port.Exit("On Cancelled"),
            Port.Exit("On Finished"),
            Port.Output<float>("Progress"),
            Port.Output<float>("Curve T"),
        };

        IBlueprintNodeTween IBlueprintOutput<IBlueprintNodeTween>.GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetOutputPortValue(int port) => port switch {
            8 => _tween.Progress,
            9 => _tween.T,
            _ => default,
        };

        public void Initialize(MonoBehaviour owner) {
            _tween.duration = Mathf.Max(0f, Ports[2].Get(_duration));
            _tween.curve = Ports[3].Get(_curve);
            _tween.action = Ports[4].Get<ITweenProgressAction>();

            _tween.Initialize(owner);
        }

        public void DeInitialize() {
            _tween.DeInitialize();
        }

        public async UniTask Play(CancellationToken token) {
            Ports[5].Call();
            await _tween.Play(token);
            Ports[token.IsCancellationRequested ? 6 : 7].Call();
        }

        public void Wind(bool reportProgress = true) {
            _tween.Wind(reportProgress);
        }

        public void Rewind(bool reportProgress = true) {
            _tween.Rewind(reportProgress);
        }

        public void Invert(bool isInverted) {
            _tween.Invert(isInverted);
        }
    }

}
