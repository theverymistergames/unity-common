﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "InstantTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeInstantTween :
        BlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<ITween>,
        ITweenInstantAction
    {
        [SerializeField] private bool _isInverted;

        private readonly InstantTween _tween = new InstantTween();

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
            Port.Input<ITweenInstantAction>("Tween Instant Action"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _destroyCts = new CancellationTokenSource();
            _runner = host.Runner;
        }

        public override void OnDeInitialize() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();

            _tween.DeInitialize();
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _tween.action = ReadInputPort(8, this);
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
                    bool isInverted = ReadInputPort(7, _isInverted);
                    _tween.Invert(isInverted);
                    break;
            }
        }

        public ITween GetOutputPortValue(int port) {
            if (port != 9) return null;

            _tween.action = ReadInputPort(8, this);
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

        void ITweenInstantAction.Initialize(MonoBehaviour owner) { }
        void ITweenInstantAction.DeInitialize() { }
        void ITweenInstantAction.InvokeAction() { }
    }

}
