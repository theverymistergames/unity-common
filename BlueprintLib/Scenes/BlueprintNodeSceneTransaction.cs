using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Scenes.Transactions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSceneTransaction :
        BlueprintSource<BlueprintNodeSceneTransaction>,
        BlueprintSources.IEnter<BlueprintNodeSceneTransaction> { }

    [Serializable]
    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public struct BlueprintNodeSceneTransaction : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private SceneTransaction _sceneTransaction;

        private CancellationTokenSource _terminateCts;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Exit("On Finish"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts.Cancel();
            _terminateCts.Dispose();
            _terminateCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            CommitTransactionAndExitAsync(blueprint, token, _terminateCts.Token).Forget();
        }

        private async UniTaskVoid CommitTransactionAndExitAsync(
            IBlueprint blueprint,
            NodeToken token,
            CancellationToken cancellationToken
        ) {
            await _sceneTransaction.Apply(cancellationToken);
            if (!cancellationToken.IsCancellationRequested) blueprint.Call(token, 1);
        }
    }

}
