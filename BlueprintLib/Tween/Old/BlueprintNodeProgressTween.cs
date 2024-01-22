using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Progress Tween (deprecated)", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween :
        IBlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        IBlueprintOutput<float>,
        ITween
    {
        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private AnimationCurve _curve;

        public ITween Tween => this;
        public LinkIterator NextLinks => _blueprint.GetLinks(_token, 1);
        public float Progress => _tween.Progress;

        private ProgressTween _tween;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Input<float>("Duration"));
            meta.AddPort(id, Port.Input<AnimationCurve>("Curve"));
            meta.AddPort(id, Port.Input<ITweenProgressCallback>());
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Output<float>("Progress"));
            meta.AddPort(id, Port.Output<float>("Curve T"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
            _tween = new ProgressTween();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _tween?.DeInitialize();
            _tween = null;

            _blueprint = null;
        }

        IBlueprintNodeTween IBlueprintOutput<IBlueprintNodeTween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            8 => _tween.Progress,
            9 => _tween.T,
            _ => default,
        };

        public void Initialize(MonoBehaviour owner) {
            _tween.duration = Mathf.Max(0f, _blueprint.Read(_token, 2, _duration));
            _tween.curve = _blueprint.Read(_token, 3, _curve);
            _tween.action = _blueprint.Read<ITweenProgressCallback>(_token, 4);

            _tween.Initialize(owner);
        }

        public void DeInitialize() {
            _tween?.DeInitialize();
        }

        public async UniTask Play(CancellationToken token) {
            _blueprint.Call(_token, 5);
            await _tween.Play(token);
            _blueprint.Call(_token, token.IsCancellationRequested ? 6 : 7);
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
