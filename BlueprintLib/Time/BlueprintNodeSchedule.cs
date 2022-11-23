using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] [Min(0f)] private float _startDelay;
        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private IJob _scheduleJob;
        private ITimeSource _timeSource;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Start Delay"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _scheduleJob?.Stop();
        }

        protected override void OnTerminate() {
            _scheduleJob?.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _scheduleJob?.Stop();

                float startDelay = Read(2, _startDelay);
                float period = Read(3, _period);
                int times = Read(4, _times);
                
                _scheduleJob = GetJob(startDelay, period, times).RunFrom(_timeSource);
                return;
            }

            if (port == 1) {
                _scheduleJob?.Stop();
            }
        }

        private void Execute() {
            Call(port: 5);
        }

        private void Finish() {
            _scheduleJob?.Stop();
        }

        private IJob GetJob(float startDelay, float period, int times) {
            var job = JobSequence.Create().Delay(startDelay);

            return _isInfinite
                ? job.Schedule(period, Execute)
                : job.ScheduleTimes(period, times, Execute).Action(Finish);
        }
    }

}
