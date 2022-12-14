using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeDelay : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _defaultDuration;

        private Job _delayJob;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Duration"),
            Port.Exit(),
        };

        protected override void OnInit() { }

        protected override void OnTerminate() {
            _delayJob.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _delayJob.Dispose();

                float duration = Read(2, _defaultDuration);

                _delayJob = JobSequence.Create(runner.TimeSourceStage)
                    .Delay(duration)
                    .Action(OnFinish)
                    .Push()
                    .Start();

                return;
            }

            if (port == 1) {
                _delayJob.Dispose();
            }
        }

        private void OnFinish() {
            Call(port: 3);
        }
    }

}
