using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "DelayTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeDelayTween :
        BlueprintNode, 
        IBlueprintEnter,
        IBlueprintOutput<ITween>
    {
        [SerializeField] [Min(0f)] private float _duration;

        private readonly DelayTween _tween = new DelayTween();

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        private MonoBehaviour _runner;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Play"),
            Port.Enter("Pause"),
            Port.Enter("Wind"),
            Port.Enter("Rewind"),
            Port.Enter("Reset Progress"),
            Port.Enter("Set Inverted"),
            Port.Exit("On Finish"),
            Port.Input<float>("Duration"),
            Port.Input<bool>("Is Inverted"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _runner = host.Runner;
            _destroyCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();

            _tween.DeInitialize();
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _tween.duration = Mathf.Max(0, ReadInputPort(7, _duration));
                    _tween.Initialize(_runner);

                    Play(_destroyCts.Token).Forget();
                    break;
                
                case 1:
                    _pauseCts?.Cancel();
                    break;
                
                case 2:
                    _pauseCts?.Cancel();
                    _tween.Wind();
                    break;
                
                case 3:
                    _pauseCts?.Cancel();
                    _tween.Rewind();
                    break;
                
                case 4: 
                    _tween.ResetProgress();
                    break;
                
                case 5:
                    bool isInverted = ReadInputPort<bool>(8);
                    _tween.Invert(isInverted);
                    break;
            }
        }

        public ITween GetOutputPortValue(int port) {
            if (port != 9) return null;

            _tween.duration = Mathf.Max(0, ReadInputPort(7, _duration));
            _tween.Initialize(_runner);

            return _tween;
        }

        private async UniTask Play(CancellationToken token) {
            _pauseCts?.Cancel();
            _pauseCts?.Dispose();
            _pauseCts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_pauseCts.Token, token);
            await _tween.Play(linkedCts.Token);
            
            if (linkedCts.IsCancellationRequested) return;

            CallExitPort(6);
        }
    }

}
