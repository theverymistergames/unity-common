using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tick.Core;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Transactions;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.Utils;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Scene Transaction", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeSceneTransaction : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private SceneTransactions _sceneTransactions;

        private IJob _loadSceneJob;
        private ITimeSource _timeSource;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
        };

        protected override void OnInit() {
            _loadSceneJob?.Stop();
            _timeSource = runner.TimeSource;
        }

        protected override void OnTerminate() {
            _loadSceneJob?.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            _loadSceneJob?.Stop();

            _loadSceneJob = JobSequence.Create()
                .WaitCompletion(SceneLoader.Instance.CommitTransaction(_sceneTransactions))
                .Action(OnFinish)
                .StartFrom(_timeSource);
        }

        private void OnFinish() {
            Call(1);
        }
    }

}
