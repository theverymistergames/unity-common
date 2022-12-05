using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeDelay : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _defaultDuration;

        private CancellationTokenSource _delayCts;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Duration"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _delayCts?.Cancel();
            _delayCts?.Dispose();
        }

        protected override void OnTerminate() {
            _delayCts?.Cancel();
            _delayCts?.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _delayCts?.Cancel();
                _delayCts?.Dispose();
                _delayCts = new CancellationTokenSource();

                float duration = Read(2, _defaultDuration);
                StartDelay(duration, _delayCts.Token).Forget();

                return;
            }

            if (port == 1) {
                _delayCts?.Cancel();
                _delayCts?.Dispose();
            }
        }

        private async UniTaskVoid StartDelay(float duration, CancellationToken token) {
            bool isCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(duration), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCanceled) return;

            Call(port: 3);
        }
    }

}
