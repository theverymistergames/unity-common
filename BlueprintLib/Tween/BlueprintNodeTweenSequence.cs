using System;
using System.Collections.Generic;
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
    [BlueprintNode(Name = "Tween Sequence", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenSequence :
        IBlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        IBlueprintOutput<float>,
        ITween
    {
        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;

        public ITween Tween => this;
        public LinkIterator NextLinks => _blueprint.GetLinks(_token, 1);
        public float Progress => _tweenSequence?.Progress ?? 0f;

        private NodeToken _token;
        private IBlueprint _blueprint;
        private TweenSequence _tweenSequence;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Input<IBlueprintNodeTween>("Sequence Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Output<float>("Progress"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _tweenSequence?.DeInitialize();
            _tweenSequence = null;

            _blueprint = null;
        }

        public void Initialize(MonoBehaviour owner) {
            var t = BlueprintTweenConverter.AsTween(_blueprint.GetLinks(_token, 2));
            _tweenSequence = t == null ? null : t as TweenSequence ?? new TweenSequence { tweens = new List<ITween> { t } };

            if (_tweenSequence != null) {
                _tweenSequence.loop = _loop;
                _tweenSequence.yoyo = _yoyo;
            }

            _tweenSequence?.Initialize(_blueprint.Host);
        }

        public void DeInitialize() {
            _tweenSequence?.DeInitialize();
            _tweenSequence = null;
        }

        IBlueprintNodeTween IBlueprintOutput<IBlueprintNodeTween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 6 && _tweenSequence != null ? _tweenSequence.Progress : default;
        }

        public async UniTask Play(CancellationToken token) {
            if (_tweenSequence == null) return;

            _blueprint.Call(_token, 3);

            await _tweenSequence.Play(token);

            _blueprint.Call(_token, token.IsCancellationRequested ? 4 : 5);
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
