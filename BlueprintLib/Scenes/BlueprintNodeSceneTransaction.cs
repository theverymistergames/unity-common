using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Transactions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeSceneTransaction : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private SceneTransaction _sceneTransaction;

        private CancellationTokenSource _terminateCts;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Perform"),
            Port.Exit("On Finish"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _terminateCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _terminateCts.Cancel();
            _terminateCts.Dispose();
        }

        public void OnEnterPort(int port) {
            if (port != 0) return;

            CommitTransactionAndExitAsync(_terminateCts.Token).Forget();
        }

        private async UniTaskVoid CommitTransactionAndExitAsync(CancellationToken token) {
            await _sceneTransaction.Apply(this, token);
            if (token.IsCancellationRequested) return;

            Ports[1].Call();
        }
    }

}
