using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Transactions;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeSceneTransaction : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private SceneTransactions _sceneTransactions;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
        };

        protected override void OnInit() { }

        protected override void OnTerminate() { }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            SceneLoader.Instance.CommitTransaction(_sceneTransactions);
        }

        private void OnFinish() {
            Call(1);
        }

        private async UniTaskVoid CommitTransaction() {

        }
    }

}
