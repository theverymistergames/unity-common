using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Tweens.Core2;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Tween Player", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenPlayer :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<float>,
        IBlueprintStartCallback
    {
        [SerializeField] private bool _autoSetTweenOnStart;
        [SerializeField] private bool _routeOnCancelledIntoOnFinished;
        [SerializeField] [Range(0f, 1f)] private float _progress;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private YoyoMode _yoyo;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _invertNextPlay;

        private IBlueprint _blueprint;
        private NodeToken _token;
        private CancellationTokenSource _destroyCts;
        private TweenPlayer _tweenPlayer;
        private bool _isFirstPlay;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Tween"));
            meta.AddPort(id, Port.Enter("Play"));
            meta.AddPort(id, Port.Enter("Stop"));

            meta.AddPort(id, Port.Input<float>("Progress"));
            meta.AddPort(id, Port.Input<float>("Speed"));

            meta.AddPort(id, Port.Input<ITween>("Tweens").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Update"));

            meta.AddPort(id, Port.Output<float>("Progress"));
            meta.AddPort(id, Port.Output<float>("Speed"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
            _destroyCts = new CancellationTokenSource();
            _tweenPlayer = new TweenPlayer();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _destroyCts?.Cancel();
            _destroyCts?.Dispose();
            _destroyCts = null;
            _blueprint = null;
            _tweenPlayer = null;
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            _token = token;
            _blueprint = blueprint;

            if (_autoSetTweenOnStart) SetTween();
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            switch (port) {
                case 0:
                    SetTween();
                    break;

                case 1:
                    Play(_destroyCts.Token).Forget();
                    break;

                case 2:
                    _tweenPlayer.Stop();
                    break;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            return port switch {
                10 => _tweenPlayer.Progress,
                11 => _tweenPlayer.Speed,
                _ => default
            };
        }

        private async UniTask Play(CancellationToken cancellationToken = default) {
            if (_tweenPlayer == null) return;

            // Wait one frame to save OnStart and OnFinish callbacks order,
            // because OnStart will be called before last tween finishes and notifies OnFinish.
            if (_tweenPlayer.IsPlaying) {
                _tweenPlayer.Stop();
                await UniTask.Yield();
            }

            if (_invertNextPlay && !_isFirstPlay) _tweenPlayer.Speed = -_tweenPlayer.Speed;
            _isFirstPlay = false;

            if (_tweenPlayer.Speed > 0f && _tweenPlayer.Progress >= 1f ||
                _tweenPlayer.Speed < 0f && _tweenPlayer.Progress <= 0f
            ) {
                return;
            }

            // Notify OnStart
            _blueprint.Call(_token, 6);

            bool finished = await _tweenPlayer.Play(
                this,
                (t, _) => t.ReportProgress(),
                cancellationToken
            );

            // Notify OnFinished or OnCancelled
            _blueprint.Call(_token, finished || _routeOnCancelledIntoOnFinished ? 7 : 8);
        }

        private void ReportProgress() {
            _blueprint.Call(_token, 9);
        }

        private void SetTween() {
            var links = _blueprint.GetLinks(_token, 5);
            ITween tween = null;

            while (links.MoveNext()) {
                if (links.Read<ITween>() is { } t0) {
                    MergeTween(ref tween, t0);
                    continue;
                }

                if (links.Read<ITween[]>() is { } array) {
                    for (int i = 0; i < array.Length; i++) {
                        if (array[i] is { } t1) MergeTween(ref tween, t1);
                    }
                }
            }

            _tweenPlayer.Tween = tween;
            _tweenPlayer.Progress = _blueprint.Read(_token, 3, _progress);
            _tweenPlayer.Speed = _blueprint.Read(_token, 4, _speed);
            _tweenPlayer.Yoyo = _yoyo;
            _tweenPlayer.Loop = _loop;

            _isFirstPlay = true;
        }

        private static void MergeTween(ref ITween dest, ITween tween) {
            if (dest == null) {
                dest = tween;
                return;
            }

            if (dest is TweenGroup { mode: TweenGroup.Mode.Parallel } tweenGroup) {
                tweenGroup.tweens ??= new List<ITween>(1);
                tweenGroup.tweens.Add(tween);
                return;
            }

            dest = new TweenGroup {
                mode = TweenGroup.Mode.Parallel,
                tweens = new List<ITween>(2) {
                    dest,
                    tween
                }
            };
        }
    }

}
