using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceRunTween :
        BlueprintSource<BlueprintNodeRunTween>,
        BlueprintSources.IEnter<BlueprintNodeRunTween>,
        BlueprintSources.IOutput<BlueprintNodeRunTween, float>,
        BlueprintSources.IStartCallback<BlueprintNodeRunTween>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Run Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeRunTween :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintStartCallback,
        IBlueprintOutput<float>
    {
        [SerializeField] private bool _autoInitOnStart;
        [SerializeField] private bool _autoInvertNextPlay;

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        private bool _isInverted;
        private bool _isPlayCalledOnce;
        private ITween _tween;
        private MonoBehaviour _runner;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _autoInitOnStart = true;
            _autoInvertNextPlay = true;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Init"));
            meta.AddPort(id, Port.Enter("Play"));
            meta.AddPort(id, Port.Enter("Pause"));
            meta.AddPort(id, Port.Enter("Wind"));
            meta.AddPort(id, Port.Enter("Rewind"));
            meta.AddPort(id, Port.Enter("Invert"));
            meta.AddPort(id, Port.Input<IBlueprintNodeTween>("Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Output<float>("Progress"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;

            _runner = blueprint.Host;
            _destroyCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
            _destroyCts = null;

            _tween?.DeInitialize();
            _tween = null;

            _blueprint = null;
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoInitOnStart) return;

            _token = token;
            _tween = BlueprintTweenConverter.AsTween(blueprint.GetLinks(token, 6));
            _tween?.Initialize(_runner);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;

            switch (port) {
                case 0:
                    _tween?.DeInitialize();

                    _tween = BlueprintTweenConverter.AsTween(blueprint.GetLinks(token, 6));
                    _tween?.Initialize(_runner);
                    break;

                case 1:
                    if (_tween != null) {
                        if (_autoInvertNextPlay && _isPlayCalledOnce) {
                            _isInverted = !_isInverted;
                            _tween.Invert(_isInverted);
                        }

                        _isPlayCalledOnce = true;
                        Play(_destroyCts.Token).Forget();
                    }
                    break;

                case 2:
                    _pauseCts?.Cancel();
                    break;

                case 3:
                    _pauseCts?.Cancel();
                    _tween?.Wind();
                    break;

                case 4:
                    _pauseCts?.Cancel();
                    _tween?.Rewind();
                    break;

                case 5:
                    if (_tween != null) {
                        _isInverted = !_isInverted;
                        _tween.Invert(_isInverted);
                    }
                    break;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 10 && _tween != null ? _tween.Progress : default;
        }

        private async UniTask Play(CancellationToken token) {
            _pauseCts?.Cancel();
            _pauseCts?.Dispose();
            _pauseCts = new CancellationTokenSource();

            _blueprint.Call(_token, 7);

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_pauseCts.Token, token);
            await _tween.Play(linkedCts.Token);

            _blueprint.Call(_token, linkedCts.IsCancellationRequested ? 8 : 9);
        }
    }

}
