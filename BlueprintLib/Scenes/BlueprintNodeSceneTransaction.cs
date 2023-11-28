using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Scenes.Transactions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSceneTransaction :
        BlueprintSource<BlueprintNodeSceneTransaction2>,
        BlueprintSources.IEnter<BlueprintNodeSceneTransaction2> { }

    [Serializable]
    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public struct BlueprintNodeSceneTransaction2 : IBlueprintNode, IBlueprintEnter2 {

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
            await _sceneTransaction.Apply(blueprint, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            blueprint.Call(token, 1);
        }
    }

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
