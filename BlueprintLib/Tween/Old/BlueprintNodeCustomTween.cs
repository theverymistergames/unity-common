using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Attributes;
using UnityEngine;
using ITween = MisterGames.Tweens.Core.ITween;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Custom Tween (deprecated)", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCustomTween :
        IBlueprintNode,
        IBlueprintNodeTween,
        IBlueprintOutput<IBlueprintNodeTween>,
        IBlueprintOutput<float>,
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

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _tween?.DeInitialize();
            _tween = null;
            _blueprint = null;
        }

        IBlueprintNodeTween IBlueprintOutput<IBlueprintNodeTween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
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

}
