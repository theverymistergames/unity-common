using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Progress", Category = "Time", Color = BlueprintColors.Node.Data)]
    public class BlueprintNodeProgress : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<float> {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private bool _isInverted;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;

        private float _progress;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Stop"));
            meta.AddPort(id, Port.Input<float>("Duration"));
            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Finish"));
            meta.AddPort(id, Port.Exit("On Update"));
            meta.AddPort(id, Port.Output<float>("Progress"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts.Cancel();
            _terminateCts.Dispose();
            _terminateCts = null;

            _cancelCts?.Cancel();
            _cancelCts?.Dispose();
            _cancelCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port == 0) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;

                _progress = 0f;
                float duration = blueprint.Read(token, 2, _duration);

                if (duration <= 0) {
                    _progress = 1f;
                    return;
                }

                _cancelCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                ProgressAsync(blueprint, token, duration, linkedCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
                return;
            }
        }

        private async UniTask ProgressAsync(IBlueprint blueprint, NodeToken token, float duration, CancellationToken cancellationToken) {
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            blueprint.Call(token, 3);
            
            while (!cancellationToken.IsCancellationRequested) {
                _progress = Mathf.Clamp01(_progress + timeSource.DeltaTime / duration);
                blueprint.Call(token, 5);

                if (_progress >= 1f) break;

                await UniTask.Yield();
            }
            
            blueprint.Call(token, 4);
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port == 6) {
                return _isInverted ? 1f - _progress : _progress;
            }

            return default;
        }
    }

}
