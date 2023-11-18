using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Custom Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCustomTween2 :
        IBlueprintNode,
        IBlueprintNodeTween2,
        IBlueprintOutput2<IBlueprintNodeTween2>,
        IBlueprintOutput2<float>,
        ITween
    {
        public ITween Tween => this;
        public LinkIterator NextLinks => _blueprint.GetLinks(_token, 1);
        public float Progress => _tween?.Progress ?? default;

        private ITween _tween;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Input<ITween>());
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Output<float>("Progress"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _tween?.DeInitialize();
            _tween = null;
            _blueprint = null;
        }

        IBlueprintNodeTween2 IBlueprintOutput2<IBlueprintNodeTween2>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput2<float>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 6 && _tween != null ? _tween.Progress : default;
        }

        public void Initialize(MonoBehaviour owner) {
            _tween = _blueprint.Read<ITween>(_token, 2);
            _tween?.Initialize(owner);
        }

        public void DeInitialize() {
            _tween?.DeInitialize();
            _tween = null;
        }

        public async UniTask Play(CancellationToken token) {
            if (_tween == null) return;

            _blueprint.Call(_token, 3);
            await _tween.Play(token);
            _blueprint.Call(_token, token.IsCancellationRequested ? 4 : 5);
        }

        public void Wind(bool reportProgress = true) {
            _tween?.Wind(reportProgress);
        }

        public void Rewind(bool reportProgress = true) {
            _tween?.Rewind(reportProgress);
        }

        public void Invert(bool isInverted) {
            _tween?.Invert(isInverted);
        }
    }

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Custom Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCustomTween :
        BlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        IBlueprintOutput<float>,
        ITween
    {
        public ITween Tween => this;
        public List<RuntimeLink> NextLinks => Ports[1].links;

        public float Progress => _tween?.Progress ?? default;

        private ITween _tween;

        public override Port[] CreatePorts() => new[] {
            Port.Output<IBlueprintNodeTween>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
            Port.Input<IBlueprintNodeTween>("Next Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Input<ITween>(),
            Port.Exit("On Start"),
            Port.Exit("On Cancelled"),
            Port.Exit("On Finished"),
            Port.Output<float>("Progress"),
        };

        IBlueprintNodeTween IBlueprintOutput<IBlueprintNodeTween>.GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetOutputPortValue(int port) {
            return port == 6 && _tween != null ? _tween.Progress : default;
        }

        public void Initialize(MonoBehaviour owner) {
            _tween = Ports[2].Get<ITween>();
            _tween?.Initialize(owner);
        }

        public void DeInitialize() {
            _tween?.DeInitialize();
            _tween = null;
        }

        public async UniTask Play(CancellationToken token) {
            if (_tween == null) return;

            Ports[3].Call();
            await _tween.Play(token);
            Ports[token.IsCancellationRequested ? 4 : 5].Call();
        }

        public void Wind(bool reportProgress = true) {
            _tween?.Wind(reportProgress);
        }

        public void Rewind(bool reportProgress = true) {
            _tween?.Rewind(reportProgress);
        }

        public void Invert(bool isInverted) {
            _tween?.Invert(isInverted);
        }
    }

}
