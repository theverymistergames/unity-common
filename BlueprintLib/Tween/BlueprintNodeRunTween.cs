using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Run Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeRunTween : BlueprintNode, IBlueprintEnter, IBlueprintStart {

        [SerializeField] private bool _autoInitOnStart = true;
        [SerializeField] private bool _autoInvertNextPlay = true;

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        private bool _isInverted;
        private bool _isPlayCalledOnce;
        private ITween _tween;
        private MonoBehaviour _runner;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Init"),
            Port.Enter("Play"),
            Port.Enter("Pause"),
            Port.Enter("Wind"),
            Port.Enter("Rewind"),
            Port.Enter("Invert"),
            Port.Input<IBlueprintNodeTween>("Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Exit("On Finish"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _runner = host.Runner;
            _destroyCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        public void OnStart() {
            if (!_autoInitOnStart) return;

            var links = Ports[6].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IBlueprintNodeTween>()?.SetupTween();
            }

            _tween = BlueprintTweenConverter.AsTween(links);
            _tween?.Initialize(_runner);
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _tween?.DeInitialize();

                    var links = Ports[6].links;
                    for (int i = 0; i < links.Count; i++) {
                        links[i].Get<IBlueprintNodeTween>()?.SetupTween();
                    }

                    _tween = BlueprintTweenConverter.AsTween(links);
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

        private async UniTask Play(CancellationToken token) {
            _pauseCts?.Cancel();
            _pauseCts?.Dispose();
            _pauseCts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_pauseCts.Token, token);
            await _tween.Play(linkedCts.Token);

            if (linkedCts.IsCancellationRequested) return;

            Ports[7].Call();
        }
    }

}
