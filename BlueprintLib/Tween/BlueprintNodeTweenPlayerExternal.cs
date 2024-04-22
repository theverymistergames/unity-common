using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    //[BlueprintNode(Name = "Tween Player External", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenPlayerExternal :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<float>,
        IBlueprintStartCallback
    {
        [SerializeField] private bool _autoSetTweenRunnerOnStart;
        [SerializeField] private bool _routeOnCancelledIntoOnFinished;
        
        private IBlueprint _blueprint;
        private NodeToken _token;
        private CancellationTokenSource _destroyCts;
        private TweenPlayer _tweenPlayer;
        private bool _isFirstPlay;
        private bool _isFirstNotifyProgress;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set TweenRunner"));
            meta.AddPort(id, Port.Input<TweenRunner>());

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

            if (_autoSetTweenRunnerOnStart) SetTweenRunner();
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            switch (port) {
                case 0:
                    SetTweenRunner();
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
            
            _isFirstPlay = false;
            _isFirstNotifyProgress = true;

            bool finished = await _tweenPlayer.Play(
                this,
                (t, p) => t.ReportProgress(p),
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

        private void SetTweenRunner() {
            var tweenRunner = _blueprint.Read<TweenRunner>(_token, 1);
            if (tweenRunner == null) return;

            _tweenPlayer = tweenRunner.TweenPlayer;
            _isFirstPlay = true;
        }
    }

}
