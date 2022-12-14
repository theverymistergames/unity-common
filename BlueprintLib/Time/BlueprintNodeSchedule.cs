using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {

        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private Job _scheduleJob;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit(),
        };

        protected override void OnInit() { }

        protected override void OnTerminate() {
            _scheduleJob.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _scheduleJob.Dispose();

                float period = Read(2, _period);
                int times = _isInfinite ? -1 : Read(3, _times);

                _scheduleJob = JobSequence.Create(runner.TimeSourceStage)
                    .Schedule(ScheduleAction, period, times)
                    .Push()
                    .Start();

                return;
            }

            if (port == 1) {
                _scheduleJob.Dispose();
            }
        }

        private void ScheduleAction() {
            Call(port: 5);
        }

    }

}
