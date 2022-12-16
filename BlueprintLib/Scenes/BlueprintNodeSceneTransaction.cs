using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Transactions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeSceneTransaction : BlueprintNode, IBlueprintEnter {
        
        [SerializeReference] [SubclassSelector]
        private ISceneTransaction _sceneTransaction;

        private CancellationTokenSource _terminateCts;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
        };

        protected override void OnInit() {
            _terminateCts = new CancellationTokenSource();
        }

        protected override void OnTerminate() {
            _terminateCts.Cancel();
            _terminateCts.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            CommitTransactionAndExitAsync(_terminateCts.Token).Forget();
        }

        private async UniTaskVoid CommitTransactionAndExitAsync(CancellationToken token) {
            await _sceneTransaction.Commit();
            if (token.IsCancellationRequested) return;
            Call(1);
        }
    }

}
