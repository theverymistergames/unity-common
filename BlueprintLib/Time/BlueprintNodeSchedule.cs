using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] [Min(0f)] private float _startDelay;
        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private readonly SingleJobHandler _handler = new SingleJobHandler();
        private TimeDomain _timeDomain;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Start Delay"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _handler.Stop();
            _timeDomain = runner.TimeDomain;
        }

        protected override void OnTerminate() {
            _handler.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _handler.Stop();

                float startDelay = Read(2, _startDelay);
                float period = Read(3, _period);
                int times = Read(4, _times);
                
                _handler.Start(GetJob(startDelay, period, times));    
                return;
            }

            if (port == 1) {
                _handler.Stop();
            }
        }

        private void Execute() {
            Call(port: 5);
        }

        private void Finish() {
            _handler.Stop();
        }
        
        private IJob GetJob(float startDelay, float period, int times) {
            return _isInfinite
                ? _timeDomain.Schedule(startDelay, period, Execute)
                : Jobs
                    .Do(_timeDomain.ScheduleTimes(startDelay, period, times, Execute))
                    .Then(Finish);
        }
    }

}