using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.Utils;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeDelay : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _defaultDuration;

        private IJob _delayJob;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Duration"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _delayJob?.Stop();
        }

        protected override void OnTerminate() {
            _delayJob?.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _delayJob?.Stop();

                float duration = Read(2, _defaultDuration);

                _delayJob = JobSequence.Create()
                    .Delay(duration)
                    .Action(OnDelayFinished)
                    .RunFrom(runner.TimeSource);

                return;
            }

            if (port == 1) {
                _delayJob?.Stop();
            }
        }

        private void OnDelayFinished() {
            Call(port: 3);
        }
    }

}
