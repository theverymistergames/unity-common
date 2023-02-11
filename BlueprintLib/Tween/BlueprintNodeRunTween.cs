using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Run Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeRunTween : BlueprintNode, IBlueprintEnter {

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        private bool _isInverted;
        private ITween _tween;
        private MonoBehaviour _runner;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Play"),
            Port.Enter("Pause"),
            Port.Enter("Wind"),
            Port.Enter("Rewind"),
            Port.Enter("Invert"),
            Port.Enter("Set Tween"),
            Port.Input<ITween>("Tween"),
            Port.Exit("On Finish"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _runner = host.Runner;
            _destroyCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();

            _tween?.DeInitialize();
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    if (_tween != null) Play(_destroyCts.Token).Forget();
                    break;
                
                case 1:
                    _pauseCts?.Cancel();
                    break;
                
                case 2:
                    _pauseCts?.Cancel();
                    _tween?.Wind();
                    break;
                
                case 3:
                    _pauseCts?.Cancel();
                    _tween?.Rewind();
                    break;

                case 4:
                    if (_tween != null) {
                        _isInverted = !_isInverted;
                        _tween.Invert(_isInverted);
                    }
                    break;

                case 5:
                    _tween?.DeInitialize();

                    _tween = ReadInputPort<ITween>(6);
                    _tween.Initialize(_runner);
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

            CallExitPort(7);
        }
    }

}
