using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "TweenSequence", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenSequence : BlueprintNode, IBlueprintEnter, IBlueprintOutput<ITween> {

        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;
        
        private readonly TweenSequence _tweenSequence = new TweenSequence();

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _pauseCts;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Play"),
            Port.Enter("Pause"),
            Port.Enter("Wind"),
            Port.Enter("Rewind"),
            Port.Enter("Reset Progress"),
            Port.Enter("Set Inverted"),
            Port.Exit("On Finish"),
            Port.InputArray<ITween>("Tweens"),
            Port.Input<bool>("Is Inverted"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _destroyCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _tweenSequence.loop = _loop;
                    _tweenSequence.yoyo = _yoyo;
                    _tweenSequence.tweens = ReadInputArrayPort<ITween>(7);

                    Play(_destroyCts.Token).Forget();
                    break;
                
                case 1:
                    _pauseCts?.Cancel();
                    break;
                
                case 2:
                    _pauseCts?.Cancel();
                    _tweenSequence.Wind();
                    break;
                
                case 3:
                    _pauseCts?.Cancel();
                    _tweenSequence.Rewind();
                    break;
                
                case 4: 
                    _tweenSequence.ResetProgress();
                    break;
                
                case 5:
                    bool isInverted = ReadInputPort<bool>(8);
                    _tweenSequence.Invert(isInverted);
                    break;
            }
        }

        public ITween GetOutputPortValue(int port) {
            if (port != 9) return null;

            _tweenSequence.loop = _loop;
            _tweenSequence.yoyo = _yoyo;
            _tweenSequence.tweens = ReadInputArrayPort<ITween>(7);

            return _tweenSequence;
        }

        private async UniTask Play(CancellationToken token) {
            _pauseCts?.Cancel();
            _pauseCts?.Dispose();
            _pauseCts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_pauseCts.Token, token);
            await _tweenSequence.Play(linkedCts.Token);
            
            if (linkedCts.IsCancellationRequested) return;
            
            CallExitPort(6);
        }
    }

}
