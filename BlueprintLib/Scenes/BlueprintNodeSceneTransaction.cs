using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Transactions;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeSceneTransaction : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private SceneTransactions _sceneTransactions;

        private Job _loadJob;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
        };

        protected override void OnInit() { }

        protected override void OnTerminate() {
            _loadJob.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            _loadJob.Dispose();

            _loadJob = JobSequence.Create(runner.TimeSourceStage)
                .Wait(SceneLoader.Instance.CommitTransaction(_sceneTransactions))
                .Action(OnFinish)
                .Push()
                .Start();
        }

        private void OnFinish() {
            Call(1);
        }
    }

}
