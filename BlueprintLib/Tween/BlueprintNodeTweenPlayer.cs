using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Tweens;
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
        [SerializeField] private bool _autoSetTweensOnStart;
        [SerializeField] private bool _routeOnCancelledIntoOnFinished;
        [SerializeField] [Range(0f, 1f)] private float _progress;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private YoyoMode _yoyo;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _invertNextPlay;

        private IBlueprint _blueprint;
        private NodeToken _token;
        private CancellationTokenSource _destroyCts;
        private TweenPlayer<IActor, IActorTween> _tweenPlayer;
        private bool _isFirstPlay;
        private bool _isFirstNotifyProgress;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Tweens"));
            meta.AddPort(id, Port.Input<ITween>("Tweens").Capacity(PortCapacity.Multiple).Layout(PortLayout.Right));

            meta.AddPort(id, Port.Enter("Play"));
            meta.AddPort(id, Port.Enter("Stop"));

            meta.AddPort(id, Port.Input<float>("Progress"));
            meta.AddPort(id, Port.Input<float>("Speed"));

            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Update"));

            meta.AddPort(id, Port.Output<float>("Progress"));
            meta.AddPort(id, Port.Output<float>("Speed"));
            
            meta.AddPort(id, Port.Input<IActor>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
            _destroyCts = new CancellationTokenSource();
            _tweenPlayer = new TweenPlayer<IActor, IActorTween>();
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

            if (_autoSetTweensOnStart) SetTween();
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            switch (port) {
                case 0:
                    SetTween();
                    break;

                case 2:
                    Play(_destroyCts.Token).Forget();
                    break;

                case 3:
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

            _tweenPlayer.Stop();

            if (_invertNextPlay && !_isFirstPlay) _tweenPlayer.Speed = -_tweenPlayer.Speed;
            _isFirstPlay = false;
            _isFirstNotifyProgress = true;

            if (_tweenPlayer.Speed > 0f && _tweenPlayer.Progress >= 1f ||
                _tweenPlayer.Speed < 0f && _tweenPlayer.Progress <= 0f
            ) {
                return;
            }

            bool finished = await _tweenPlayer.Play(
                this,
                (t, p, _) => t.ReportProgress(p),
                cancellationToken: cancellationToken
            );

            // Notify OnFinished or OnCancelled
            _blueprint.Call(_token, !finished && !_routeOnCancelledIntoOnFinished ? 8 : 7);
        }

        private void ReportProgress(float progress) {
            // Notify OnStart when ReportProgress is called first time
            // to save OnStart and OnCancelled calls order.
            if (_isFirstNotifyProgress) {
                _isFirstNotifyProgress = false;
                _blueprint.Call(_token, 6);
            }

            _blueprint?.Call(_token, 9);
        }

        private void SetTween() {
            var links = _blueprint.GetLinks(_token, 1);
            IActorTween tween = null;

            while (links.MoveNext()) {
                if (links.Read<IActorTween>() is { } t) {
                    TweenExtensions.MergeTweenIntoParallelGroup<IActor, IActorTween, TweenGroup>(ref tween, t);
                    continue;
                }

                if (links.Read<IActorTween[]>() is { } array) {
                    for (int i = 0; i < array.Length; i++) {
                        TweenExtensions.MergeTweenIntoParallelGroup<IActor, IActorTween, TweenGroup>(ref tween, array[i]);
                    }
                }
            }

            _tweenPlayer.Context = _blueprint.Read<IActor>(_token, 12);
            _tweenPlayer.Tween = tween;
            _tweenPlayer.Progress = _blueprint.Read(_token, 4, _progress);
            _tweenPlayer.Speed = _blueprint.Read(_token, 5, _speed);
            _tweenPlayer.Yoyo = _yoyo;
            _tweenPlayer.Loop = _loop;

            _isFirstPlay = true;
        }
    }

}
