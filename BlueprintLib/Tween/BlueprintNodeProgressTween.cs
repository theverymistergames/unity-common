using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using Tweens.Easing;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "ProgressTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween :
        BlueprintNode, 
        IBlueprintEnter,
        IBlueprintOutput<ITween>,
        ITweenProgressAction
    {
        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private EasingType _easingType = EasingType.Linear;
        [SerializeField] private bool _useCustomEasingCurve;
        [SerializeField] private AnimationCurve _customEasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private readonly ProgressTween _tween = new ProgressTween();

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
            Port.Input<bool>("Is Inverted"),
            Port.Input<float>("Duration"),
            Port.Input<ITweenProgressAction>("Tween Progress Action"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _destroyCts = new CancellationTokenSource();

            _runner = host.Runner;

            _tween.easingType = _easingType;
            _tween.useCustomEasingCurve = _useCustomEasingCurve;
            _tween.customEasingCurve = _customEasingCurve;
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
            
            _tween.DeInitialize();
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _tween.duration = Mathf.Max(0f, ReadInputPort(8, _duration));
                    _tween.action = ReadInputPort(9, this);
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
                    bool isInverted = ReadInputPort<bool>(7);
                    _tween.Invert(isInverted);
                    break;
            }
        }

        ITween IBlueprintOutput<ITween>.GetOutputPortValue(int port) {
            if (port != 10) return null;

            _tween.duration = Mathf.Max(0f, ReadInputPort(8, _duration));
            _tween.action = ReadInputPort(9, this);
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

        void ITweenProgressAction.Initialize(MonoBehaviour owner) { }

        void ITweenProgressAction.DeInitialize() { }

        void ITweenProgressAction.OnProgressUpdate(float progress) { }
    }

}
